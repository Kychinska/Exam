using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace P2
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Write:");
                int threads = Convert.ToInt32(Console.ReadLine());
                var scheduler = new BoundedThreadNumberTaskScheduler(threads);
                var tasks = new List<Task>();

                var factory = new TaskFactory(scheduler);

                var watch = Stopwatch.StartNew();

                for (int i = 0; i < 100; i++)
                {
                    tasks.Add(factory.StartNew(() => Find10000PrimeNumbers()));
                }

                Task.WaitAll(tasks.ToArray());
                watch.Stop();
                Console.WriteLine("Time: " + watch.ElapsedMilliseconds + "ms");
            }
        }
        public static void Find10000PrimeNumbers()
        {
            for(int i=1; i<=10000; i++)
            {
                IsPrimeNumber(i);
            }
        }
        public static bool IsPrimeNumber(int n)
        {
            var result = true;
            if (n > 1)
            {
                for (var i = 2u; i < n; i++)
                {
                    if (n % i == 0)
                    {
                        result = false;
                        break;
                    }
                }
            }
            else
            {
                result = false;
            }
            return result;
        }
    }

    public class BoundedThreadNumberTaskScheduler : TaskScheduler
    {
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>();   
        private readonly int _maxDegreeOfParallelism;
        private int _delegatesQueuedOrRunning = 0;
        public BoundedThreadNumberTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }
        protected sealed override void QueueTask(Task task)
        {
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                _currentThreadIsProcessingItems = true;
                try
                {                   
                    while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                            if (_tasks.Count == 0)
                            {
                                --_delegatesQueuedOrRunning;
                                break;
                            }
                            item = _tasks.First.Value;
                            _tasks.RemoveFirst();
                        }
                        base.TryExecuteTask(item);
                    }
                }
                finally { _currentThreadIsProcessingItems = false; }
            }, null);
        }
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (!_currentThreadIsProcessingItems) return false;
            if (taskWasPreviouslyQueued)
                if (TryDequeue(task))
                    return base.TryExecuteTask(task);
                else
                    return false;
            else
                return base.TryExecuteTask(task);
        }
        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks) return _tasks.Remove(task);
        }
        public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken) return _tasks;
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }
    }

}
