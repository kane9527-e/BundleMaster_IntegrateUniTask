using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace BM
{
    public static class TimeAwaitHelper
    {
        internal static readonly Queue<TimerAwait> TimerFactoryQueue = new Queue<TimerAwait>();

//         /// <summary>
//         /// 等待一定时间
//         /// </summary>
//         /// <param name="time">单位 秒</param>
//         /// <param name="cancellationToken">异步取消锁</param>
//         public static UniTask AwaitTime(float time/*, CancellationTokenSource cancellationToken = null*/)
//         {
//             TimerAwait timerAwait;
//             if (TimerFactoryQueue.Count > 0)
//             {
//                 timerAwait = TimerFactoryQueue.Dequeue();
//             }
//             else
//             {
//                 timerAwait = new TimerAwait();
//             }
//             UniTaskCompletionSource tcs = new UniTaskCompletionSource();
//             timerAwait.Init(time, tcs);
//             cancellationToken?.(() =>
//             {
//                 timerAwait.Cancel();
//                 tcs.SetResult();
//             });
//             return tcs;
//         }
        /// <summary>
        /// 等待一定时间
        /// </summary>
        /// <param name="time">单位 秒</param>
        public static UniTask AwaitTime(float time)
        {
            TimerAwait timerAwait;
            if (TimerFactoryQueue.Count > 0)
            {
                timerAwait = TimerFactoryQueue.Dequeue();
            }
            else
            {
                timerAwait = new TimerAwait();
            }
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            timerAwait.Init(time, tcs);
            return tcs.Task;
        }
    }
    

    internal class TimerAwait
    {
        private float remainingTime = 0;
        private UniTaskCompletionSource tcs;
        private bool cancelTimer = false;

        internal void Init(float time, UniTaskCompletionSource task)
        {
            this.remainingTime = time;
            this.tcs = task;
            AssetComponent.TimerAwaitQueue.Enqueue(this);
            cancelTimer = false;
        }
        
        internal bool CalcSubTime(float time)
        {
            remainingTime -= time;
            if (remainingTime > 0)
            {
                return false;
            }
            if (!cancelTimer)
            {
                tcs.TrySetResult();
            }
            tcs = null;
            remainingTime = 0;
            TimeAwaitHelper.TimerFactoryQueue.Enqueue(this);
            return true;
        }
        
        internal void Cancel()
        {
            cancelTimer = true;
        }
    }
}