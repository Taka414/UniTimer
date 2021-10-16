#pragma warning disable

using System;

namespace Takap.Utility.Timers
{
    /// <summary>
    /// UniTimer タイマーに登録するハンドラの形式を表します。
    /// </summary>
    public delegate void UniTimerCallback(IUniTimerHandle h);

    /// <summary>
    /// タイマーのハンドルを表します。
    /// </summary>
    public interface IUniTimerHandle : IDisposable
    {
        //
        // Props
        // - - - - - - - - - - - - - - - - - - - -

        /// <summary>
        /// 処理が呼び出された回数を取得します。（初回呼び出し時にハンドラ内で参照すると1）
        /// </summary>
        long CurrentExecCount { get; }

        /// <summary>
        /// タイマーの実行間隔を取得します。
        /// </summary>
        float Interval { get; }

        /// <summary>
        /// タイマー動作中かどうかのフラグを取得します。
        /// <para>
        /// true: 動作中 / false: 停止中
        /// </para>
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// LastUpdate のループを使用してタイマーを実行するかどうかのフラグを取得します。
        /// true: LastUpdate を使用している / false: Update を使用している。
        /// </summary>
        bool UseLastUpdate { get; }

        /// <summary>
        /// タイマーの実行回数を取得します。
        /// </summary>
        /// <remarks>
        /// -1 は回数指定なしを表します。
        /// </remarks>
        long ExecCount { get; }

        /// <summary>
        /// 最後のタイマー実行かどうか取得します。
        /// </summary>
        bool IsLastExec { get; }

        /// <summary>
        /// タイムスケールを無視するかどうかのフラグを取得します。
        /// <para>
        /// true: 無視する / false: 無視しない(規定値)
        /// </para>
        /// </summary>
        bool IgnoreTimeScale { get; }

        /// <summary>
        /// オブジェクトが Dispose されたかどうかを取得します。
        /// <para>
        /// true: Dispose された / false: それ以外
        /// </para>
        /// </summary>
        bool IsDisposed { get; }

        //
        // Methods
        // - - - - - - - - - - - - - - - - - - - -

        /// <summary>
        /// タイマーの実行間隔を変更します。
        /// </summary>
        /// <param name="interval">実行間隔(秒)</param>
        /// <remarks>
        /// 既存のタイマーの経過時間は設定時にリセットされます。
        /// </remarks>
        IUniTimerHandle ChangeInterval(float interval);

        /// <summary>
        /// 新しいタイマーの実行回数を設定します。
        /// </summary>
        /// <param name="count">実行回数</param>
        /// <remarks>
        /// 現在10回実行済みの場合、5を指定した場合はタイマーが即座に停止します。
        /// <</remarks>
        IUniTimerHandle SetExecCount(long count);

        /// <summary>
        /// タイマー処理を指定回数分延長します。
        /// </summary>
        /// <param name="count">追加する回数</param>
        /// <remarks>
        /// OnComplete ハンドラ内で回数を追加することもできます。その場合追加回数実行後に再度OnCompleteが発生します。
        /// <para>回数未設定で開始した場合はこの操作は無視されます。回数を変更したい場合 <see cref="SetExecCount(long)"/> を使用します。</para>
        /// </remarks>
        IUniTimerHandle AddExecCount(long count);

        /// <summary>
        /// タイムスケールを無視するかどうかのフラグを設定します。タイマー動作中に変更すると経過時間はリセットされます。
        /// </summary>
        /// <param name="flag">true: 無視する / false: 無視しない(規定値)</param>
        IUniTimerHandle SetIgnoreTimeScale(bool flag);

        /// <summary>
        /// タイマーを開始します。
        /// </summary>
        IUniTimerHandle Start();

        /// <summary>
        /// タイマーを一時停止します。<see cref="Start"/> で再開可能です。
        /// </summary>
        /// <remarks>
        /// 完全に破棄するには <see cref="IDisposable.Dispose"/> を呼び出してください。
        /// </remarks>
        IUniTimerHandle Suspend();

        /// <summary>
        /// 定周期で呼び出されるハンドラを指定したものに変更します。
        /// </summary>
        IUniTimerHandle ChangeElapsedHanlder(UniTimerCallback callback);

        /// <summary>
        /// 実行回数を指定している場合で処理が正常終了したときに発生するイベントを登録します。
        /// </summary>
        /// <remarks>
        /// Dispose したときは発生しません。回数未指定でStopした場合も発生しません。
        /// </remarks>
        IUniTimerHandle OnComplete(UniTimerCallback callback);
    }
}