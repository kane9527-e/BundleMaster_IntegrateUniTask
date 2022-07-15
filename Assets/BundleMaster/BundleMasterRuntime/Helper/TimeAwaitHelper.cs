using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace BM
{
    public static class TimeAwaitHelper
    {
        internal static readonly Queue<TimerAwait> TimerFactoryQueue = new Queue<TimerAwait>();

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

        internal void Init(float time, UniTaskCompletionSource task)
        {
            this.remainingTime = time;
            this.tcs = task;
            AssetComponent.TimerAwaitQueue.Enqueue(this);
        }
        
        internal bool CalcSubTime(float time)
        {
            remainingTime -= time;
            if (remainingTime > 0)
            {
                return false;
            }
            tcs.TrySetResult();
            tcs = null;
            remainingTime = 0;
            TimeAwaitHelper.TimerFactoryQueue.Enqueue(this);
            return true;
        }
    }
}