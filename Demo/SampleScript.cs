#pragma warning disable

using System;
using UnityEngine;

namespace Takap.Utility.Timers.Demo
{
    public class SampleScript : MonoBehaviour
    {
        [SerializeField, Range(0.5f, 2f)] float timeScale = 1f;

        private void Start()
        {
            //
            // パッケージを取り込むと Monobehavior に以下の 4つのメソッドが追加される
            // 
            // (1) RegisterTimer: タイマーを待機状態で登録
            // (2) StartTimer: タイマーを開始した状態で登録
            // (3) DelayOnce: 指定した時間遅延して処理を1回実行する
            // (4) GetTimers: このコンポーネントから登録したタイマーを全て取得する
            // 

            MyLog.Log("Start timers.");

            Time.timeScale = this.timeScale;

            // ---------- Case.1 ----------
            // 1秒間隔で実行されるタイマーを登録した後に開始する
            IUniTimerHandle h1 = this.RegisterTimer(1f, _ => MyLog.Log("Case.1"));
            h1.Start();

            // ---------- Case.2 ----------
            // 1秒間隔で実行するタイマー登録して即座に開始する
            IUniTimerHandle h2 = this.StartTimer(1f, _ => MyLog.Log("Case.2"));

            // ---------- Case.3 ----------
            // LastUpdate で実行されるタイマー登録を行う
            // 通常は Update でタイマーが実行される
            IUniTimerHandle h3 = this.StartTimer(1f, _ => MyLog.Log("Case.3"), true);

            // ---------- Case.4 ----------
            // 1秒間隔で実行されるタイマーを登録して各種オプションを設定する
            IUniTimerHandle h4 = 
                this.StartTimer(1f, _ => MyLog.Log("Case.4"))
                    // Time.timeScale を無視するタイマーに変更する
                    .SetIgnoreTimeScale(true)
                    // 5回だけ実行するように実行回数を指定する
                    .SetExecCount(5)
                    // 10回実行が終わったときにコールバックを呼び出す
                    .OnComplete(_ => MyLog.Log("Case.4 complete."));

            // ---------- Case.5 ----------
            // 1秒間隔で実行されるタイマーを登録して途中から実行間隔を変更する
            IUniTimerHandle h5 = this.StartTimer(1f, h =>
            {
                MyLog.Log("Case.5(1)");

                // 3回実行されたらインターバルを2秒間隔に変更して
                // 2回実行したら完了イベントを受け取るように変更する
                if (h.CurrentExecCount >= 3)
                {
                    h.ChangeInterval(2f)
                        .SetExecCount(2)
                        .ChangeElapsedHanlder(_ => MyLog.Log("Case.5(2)"))
                        .OnComplete(_ => MyLog.Log("Case.5(Complete)")); // 全て実行が完了したら完了通知を行う
                }
            });

            // ---------- Case.6 ----------
            // 2秒後に指定した処理を1度だけ実行する
            IUniTimerHandle h6 =
                this.DelayOnce(2f, _ => MyLog.Log("Case.6(Once)"))
                    .OnComplete(_ => MyLog.Log("Case.6(Complete)"));

            // ---------- Case.7 ----------
            // タイムスケールを無視して2秒後に指定した処理を1度だけ実行する
            IUniTimerHandle h7 =
                this.DelayOnce(2f, _ => MyLog.Log("Case.7(Once)"))
                    .SetIgnoreTimeScale(true)
                    .OnComplete(_ => MyLog.Log("Case.7(Complete)"));

            IUniTimerHandle[] timers = this.GetTimers();
            Debug.Log("TimerCount=" + timers.Length);

            // 
            // 補足:
            // 
            // (★1)
            // 登録したタイマーは Component や GameObject が破棄されたら同時に破棄されるので
            // OnDestroy に破棄するコードなどは書かなくてよい
            // 
            // (★2) 
            // また、コンポーネントの enabled や gameObject.activeInHierarchy によって停止・再開は自動で行われるため
            // OnEnable に最下位処理は書かなくてよい
            // 
            // (★3)
            // 登録したときに得られる IUniTimerHandle 経由でタイマーの設定を後から変更できるため
            // 操作が発生する場合は戻り値のオブジェクトを状況に応じてフィールドに保存しておく
            // 
            // (★4)
            // 登録後に即座に実行するケースには対応しないので、自分で登録する前に1度呼び出してください
            // 

            //
            // 特記:
            // 
            // このタイマーライブラリで対応しない事:
            //   * FixedUpdate のサポートは対応しない
            //   * フレーム単位の実行のタイマーは対応しない
            //   * 途中から変更 Update ⇔ FixedUpdate の区分変更はできない
            //   * コルーチンの入れ子と同等機能のサポートはしない
            //     * 但し OnComplete で概ね代替している
            //
        }

        // Called by UnityEvent
        public void DeleteTimers()
        {
            foreach (IUniTimerHandle hnd in this.GetTimers())
            {
                using (hnd)
                {
                    // all delete
                }
            }
        }
    }

    public static class MyLog
    {
        public static void Log(string msg) => Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] {msg}");
    }
}