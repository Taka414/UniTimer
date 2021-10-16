#pragma warning disable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Takap.Utility.Timers.Core
{
    /// <summary>
    /// バックグラウンドでタイマーを実行するコンポーネントを表します。
    /// </summary>
    internal partial class UniTimerCore : MonoBehaviour
    {
        // Fields
        // - - - - - - - - - - - - - - - - - - - -

        // 登録されたタイマーを記録しておくためのテーブル
        private readonly Dictionary<UniTimerHandleImpl, UniTimerHandleImpl> mapForUpdate = new Dictionary<UniTimerHandleImpl, UniTimerHandleImpl>();
        // LastUpdate 用のタイマーテーブル
        private readonly Dictionary<UniTimerHandleImpl, UniTimerHandleImpl> mapforLastUpdate = new Dictionary<UniTimerHandleImpl, UniTimerHandleImpl>();
        // 処理中に見つかった無効なタイマーのオブジェクトを記録しておくためのリスト
        private readonly List<UniTimerHandleImpl> invalidObjects = new List<UniTimerHandleImpl>(128);

        //
        // Runtime impl
        // - - - - - - - - - - - - - - - - - - - -

        private void Awake()
        {
            this.enabled = false; // 初期状態は待機、登録されたら開始する
        }

        private void LateUpdate()
        {
            this.updateCore(this.mapforLastUpdate);
        }

        private void Update()
        {
            this.updateCore(this.mapForUpdate);
        }

        private void OnDestroy()
        {
            this.mapForUpdate.Clear();
            this.mapforLastUpdate.Clear();
        }

        //
        // Singleton impl
        // - - - - - - - - - - - - - - - - - - - -

        private static UniTimerCore instance;
        public static UniTimerCore Instance
        {
            get
            {
                // 無ければ作る
                if (instance == null)
                {
                    UniTimerCore core = FindObjectOfType<UniTimerCore>();
                    if (core == null)
                    {
                        var go = new GameObject("UniTimer(singleton)")
                        {
                            //hideFlags = HideFlags.HideInHierarchy
                        };
                        instance = go.AddComponent<UniTimerCore>();

                        DontDestroyOnLoad(go);
                    }
                    else
                    {
                        instance = core;
                    }
                }

                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnDidReloadScripts()
        {
            instance = null; // for Domain Reload (Enter Play Mode Settings)
        }

        //
        // Public Methods
        // - - - - - - - - - - - - - - - - - - - -

        /// <summary>
        /// タイマーを登録します。
        /// </summary>
        /// <remarks>
        /// このレイヤーは登録するだけで後はハンドルから設定を変更する。自動で開始とかも呼び出し側に任せる。
        /// </remarks>
        public IUniTimerHandle AddTimer(float interval, MonoBehaviour scope, UniTimerCallback callback, bool useLastUpdate)
        {
            var key = new UniTimerHandleImpl(interval, scope ?? throw new ArgumentNullException(nameof(scope)));
            key.UseLastUpdate = useLastUpdate;
            key.ChangeElapsedHanlder(callback);
            key.Start();

            if (useLastUpdate)
            {
                this.mapforLastUpdate[key] = key; // LastUpdate用の登録
            }
            else
            {
                this.mapForUpdate[key] = key; // Update用の登録
            }

            this.enabled = true;
            Debug.Log("start");

            return key;
        }

        /// <summary>
        /// 指定したスコープに関連するタイマーを全て取得します。
        /// </summary>
        public IUniTimerHandle[] GetTimers(MonoBehaviour scope)
        {
            IEnumerable<IUniTimerHandle> f()
            {
                foreach (var item in this.mapForUpdate)
                {
                    if (item.Key.Scope == scope)
                    {
                        yield return item.Key;
                    }
                }
                foreach (var item in this.mapforLastUpdate)
                {
                    if (item.Key.Scope == scope)
                    {
                        yield return item.Key;
                    }
                }
            }
            return f().ToArray(); // <- TODO: so slow and lots of alloc
        }

        //
        // Other Methods
        // - - - - - - - - - - - - - - - - - - - -

        /// <summary>
        /// テーブルの内容に従ってタイマーを実行します。
        /// </summary>
        private void updateCore(Dictionary<UniTimerHandleImpl, UniTimerHandleImpl> map)
        {
            float unsclale = Time.deltaTime / Time.timeScale;

            foreach (var pair in map)
            {
                try
                {
                    UniTimerHandleImpl key = pair.Key;

                    if (!key.Scope || !key.Scope.gameObject || key.IsDisposed)
                    {
                        this.invalidObjects.Add(key);
                    }
                    else if (!key.Scope.enabled || !key.Scope.gameObject.activeInHierarchy || !key.IsEnabled)
                    {
                        continue;
                    }
                    else
                    {
                        key.Elapsed +=
                            key.IgnoreTimeScale ?
                                unsclale : 
                                Time.deltaTime;

                        if (key.Elapsed < key.Interval) continue;
                        
                        pair.Key.CallElapsed();
                    }

                    if (key.IsCounterCompleted) // 実行回数超過
                    {
                        key.CallComplete();

                        if (key.IsCounterCompleted)
                        {
                            this.invalidObjects.Add(key); // Completeハンドラ内で回数を変更された場合の処理
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex);
                }
            }

            // 無効なオブジェクトを削除
            if (this.invalidObjects.Count != 0)
            {
                for (int i = 0; i < invalidObjects.Count; i++)
                {
                    UniTimerHandleImpl key = this.invalidObjects[i];
                    map.Remove(key);

                    //key.CallComplete();

                    using (key) { }

                    Debug.Log("deleted");
                }
                this.invalidObjects.Clear();
            }

            // 登録がなくなったら待機状態に戻る
            if (this.mapForUpdate.Count == 0 && this.mapforLastUpdate.Count == 0)
            {
                Debug.Log("stoped.");
                this.enabled = false;
            }
        }
    }
}