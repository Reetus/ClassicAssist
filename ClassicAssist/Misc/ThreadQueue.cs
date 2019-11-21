using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ClassicAssist.Misc
{
    public class ThreadQueue<T> : IDisposable
    {
        private readonly Action<T> _onAction;
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);
        private readonly Thread _workerThread;

        public ThreadQueue(Action<T> onAction)
        {
            _onAction = onAction;
            _workerThread = new Thread(ProcessQueue) { IsBackground = true };
            _workerThread.Start();
        }

        private void ProcessQueue()
        {
            while (_workerThread.IsAlive)
            {
                if (_queue.TryDequeue(out T queueItem))
                {
                    if (queueItem == null)
                        return;

                    _onAction(queueItem);
                }
                else
                {
                    _wh.WaitOne();
                }
            }
        }

        public void Dispose()
        {
            StopThread();
        }

        public void Enqueue(T queueItem)
        {
            _queue.Enqueue(queueItem);

            try
            {
                _wh.Set();
            }
            catch (ObjectDisposedException)
            {

            }
        }

        private void StopThread()
        {
            _queue.Enqueue(default);

            try
            {
                _wh.Set();
            }
            catch (ObjectDisposedException)
            {

            }

            _workerThread.Join();
            _wh.Close();
        }
    }
}