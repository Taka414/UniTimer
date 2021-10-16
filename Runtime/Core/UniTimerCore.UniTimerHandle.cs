#pragma warning disable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Takap.Utility.Timers.Core
{
    internal partial class UniTimerCore
    {
        //
        // Inner Types
        // - - - - - - - - - - - - - - - - - - - -

        /// <summary>
        /// 制御用のハンドルの実体を表します。
        /// </summary>
        private class UniTimerHandleImpl : IUniTimerHandle
        {
            //
            // Fields
            // - - - - - - - - - - - - - - - - - - - -

            // 実行間隔(秒)
            private float interval;
            // タイマーが有効かどうかのフラグ
            // true: 有効 / false : 無効
            private bool isEnabled;
            // タイマー実行するハンドラ
            private UniTimerCallback ev_elapsed;
            // 終了通知用
            private UniTimerCallback ev_comoleted;

            //
            // Props
            // - - - - - - - - - - - - - - - - - - - -

            /// <summary>
            /// この処理に関連付けられたコンポーネントを取得します。
            /// </summary>
            /// <remarks>
            /// このオブジェクトが死んだかどうかが関連付けられているタイマーの実行と連動する。
            /// </remarks>
            public MonoBehaviour Scope { get; }

            /// <summary>
            /// <see cref="IUniTimerHandle.CurrentExecCount"/> を実装します。
            /// </summary>
            public long CurrentExecCount { get; set; }

            /// <summary>
            /// <see cref="IUniTimerHandle.Interval"/> を実装します。
            /// </summary>
            public float Interval
            {
                get => this.interval;
                set
                {
                    this.Elapsed = 0; // インターバルを変更した場合最初からやり直し
                    this.interval = value;
                }
            }

            /// <summary>
            /// 経過時間を設定または取得します。
            /// </summary>
            public float Elapsed { get; set; }

            /// <summary>
            /// <see cref="IUniTimerHandle.IsEnabled"/> を実装します。
            /// </summary>
            public bool IsEnabled
            {
                get => this.isEnabled;
                set
                {
                    this.Elapsed = 0; // 有効無効を変更した場合最初からやり直し
                    this.isEnabled = value;
                }
            }

            /// <summary>
            /// <see cref="IUniTimerHandle.UseLastUpdate"/> を実装します。
            /// </summary>
            public bool UseLastUpdate { get; set; }

            /// <summary>
            /// <see cref="IUniTimerHandle.ExecCount"/> を実装します。
            /// </summary>
            public long ExecCount { get; private set; } = -1;

            /// <summary>
            /// <see cref="IUniTimerHandle.IsDisposed"/> を実装します。
            /// </summary>
            public bool IsDisposed { get; set; }

            /// <summary>
            /// <see cref="IUniTimerHandle.IsLastExec"/> を実装します。
            /// </summary>
            public bool IsLastExec => this.ExecCount != -1 && this.CurrentExecCount == this.ExecCount;

            /// <summary>
            /// タイマー実行回数を指定している場合に実行回数が上限を超えているかどうかを取得します。
            /// <para>
            /// true: 超えている / false: まだ
            /// </para>
            /// </summary>
            public bool IsCounterCompleted => this.ExecCount != -1 && this.CurrentExecCount >= this.ExecCount;

            /// <summary>
            /// <see cref="IUniTimerHandle.IgnoreTimeScale"/> を実装します。
            /// </summary>
            public bool IgnoreTimeScale { get; private set; }

            //
            // Constructors
            // - - - - - - - - - - - - - - - - - - - -

            public UniTimerHandleImpl(float interval, MonoBehaviour source)
            {
                this.interval = interval;
                this.Scope = source;
            }

            //
            // Methods
            // - - - - - - - - - - - - - - - - - - - -

            /// <summary>
            /// <see cref="IDisposable.Dispose"/> を実装します。
            /// </summary>
            public void Dispose()
            {
                this.IsDisposed = true;
                this.IsEnabled = false;
                this.ev_comoleted = null;
            }

            /// <summary>
            /// <see cref="IUniTimerHandle.ChangeInterval"/> を実装します。
            /// </summary>
            public IUniTimerHandle ChangeInterval(float interval)
            {
                this.Interval = interval;
                return this;
            }

            /// <summary>
            /// <see cref="IUniTimerHandle.SetExecCount"/> を実装します。
            /// </summary>
            public IUniTimerHandle SetExecCount(long count)
            {
                //this.CurrentExecCount = 0;
                this.ExecCount = count;
                return this;
            }

            /// <summary>
            /// <see cref="IUniTimerHandle.AddExecCount"/> を実装します。
            /// </summary>
            public IUniTimerHandle AddExecCount(long count)
            {
                if (this.ExecCount != -1) // 回数指定なしの場合無視
                {
                    this.ExecCount += count;
                }
                return this;
            }

            /// <summary>
            /// <see cref="IUniTimerHandle.SetIgnoreTimeScale"/> を実装します。
            /// </summary>
            public IUniTimerHandle SetIgnoreTimeScale(bool flag)
            {
                this.Elapsed = 0;
                this.IgnoreTimeScale = flag;
                return this;
            }

            /// <summary>
            /// <see cref="IUniTimerHandle.Start"/> を実装します。
            /// </summary>
            public IUniTimerHandle Start()
            {
                this.IsEnabled = true;
                return this;
            }

            /// <summary>
            /// <see cref="IUniTimerHandle.Suspend"/> を実装します。
            /// </summary>
            public IUniTimerHandle Suspend()
            {
                this.IsEnabled = false;
                return this;
            }

            /// <summary>
            /// 定周期で呼び出されるハンドラを指定したものに変更します。
            /// </summary>
            public IUniTimerHandle ChangeElapsedHanlder(UniTimerCallback callback)
            {
                this.ev_elapsed = callback ?? throw new ArgumentNullException(nameof(callback));
                return this;
            }

            /// <summary>
            /// <see cref="IUniTimerHandle.OnComplete"/> を実装します。
            /// </summary>
            public IUniTimerHandle OnComplete(UniTimerCallback handler)
            {
                this.ev_comoleted = handler;
                return this;
            }

            /// <summary>
            /// タイマーイベントを呼び出します。
            /// </summary>
            public void CallElapsed()
            {
                try
                {
                    this.CurrentExecCount++;
                    this.ev_elapsed(this);
                }
                finally
                {
                    this.Elapsed = 0;
                }
            }

            /// <summary>
            /// 終了通知を呼び出します。
            /// </summary>
            public void CallComplete()
            {
                if (!this.IsCounterCompleted)
                {
                    return; // 回数指定で実行したときに最終回終了したときのみ通知
                }
                this.ev_comoleted?.Invoke(this);
            }
        }
    }
}