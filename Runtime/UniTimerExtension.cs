#pragma warning disable

using Takap.Utility.Timers.Core;
using UnityEngine;

namespace Takap.Utility.Timers
{
    /// <summary>
    /// <see cref="UniTimerCore"/> へアクセスを簡単にするためのメソッドを提供します。
    /// </summary>
    public static class UniTimerExtension
    {
        /// <summary>
        /// <paramref name="callback"/> を <paramref name="interval"/> 秒ごとに実行するようにタイマーを登録します。
        /// </summary>
        public static IUniTimerHandle StartTimer(this MonoBehaviour self, float interval, UniTimerCallback callback, bool useLastUpdate = false)
        {
            var handle = UniTimerCore.Instance.AddTimer(interval, self, callback, useLastUpdate);
            handle.Start();
            return handle;
        }

        /// <summary>
        /// <paramref name="callback"/> を <paramref name="interval"/> 秒ごとに実行するようにタイマーを登録します。
        /// <para>
        /// 登録するだけでタイマー動作は開始しないのでタイマーを開始するには明示的に <see cref="IUniTimerHandle.Start"/> を呼び出してください。
        /// </para>
        /// </summary>
        public static IUniTimerHandle RegisterTimer(this MonoBehaviour self, float interval, UniTimerCallback callback, bool useLastUpdate = false)
        {
            return UniTimerCore.Instance.AddTimer(interval, self, callback, useLastUpdate);
        }

        /// <summary>
        /// <paramref name="callback"/> を <paramref name="delay"/> 秒後に1回だけ実行します。
        /// </summary>
        public static IUniTimerHandle DelayOnce(this MonoBehaviour self, float delay, UniTimerCallback callback, bool useLastUpdate = false)
        {
            var handle = UniTimerCore.Instance.AddTimer(delay, self, callback, useLastUpdate);
            handle.SetExecCount(1);
            handle.Start();
            return handle;
        }

        /// <summary>
        /// このコンポーネントに関連付けられたアクティブなタイマーを取得します。
        /// </summary>
        public static IUniTimerHandle[] GetTimers(this MonoBehaviour self)
        {
            return UniTimerCore.Instance.GetTimers(self);
        }
    }
}