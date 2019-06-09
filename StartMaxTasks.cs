using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace resolvep
{
    public class StartMaxTasks
    {
        ManualResetEvent _finished;
        IEnumerator<Task> _taskEnum;

        long _counter;
        long _error;
        long _done;

        public void Run(IEnumerable<Task> tasks, int MaxParallel)
        {
            _taskEnum = tasks.GetEnumerator();
            _counter = 1; // !!!!! Mike's way :-)

            using (_finished = new ManualResetEvent(false))
            {
                for (int i = 0; i < MaxParallel; ++i)
                {
                    if (StartNextTask() == false)
                    {
                        break;
                    }
                }

                if (Interlocked.Decrement(ref _counter) != 0)
                {
                    _finished.WaitOne();
                }
            }
        }

        private void PostWork(Task workingTask)
        {
            Interlocked.Increment(ref _done);

            if (workingTask.Exception != null)
            {
                Interlocked.Increment(ref _error);
            }

            StartNextTask();

            if (Interlocked.Decrement(ref _counter) == 0)
            {
                _finished.Set();
            }
        }

        private bool StartNextTask()
        {
            bool hasMore;

            lock (_taskEnum)
            {
                hasMore = _taskEnum.MoveNext();
                if (hasMore )
                {
                    Interlocked.Increment(ref _counter);
                    _taskEnum
                        .Current
                        .ContinueWith(PostWork)
                        .ConfigureAwait(false);
                }
            }

            return hasMore;
        }
    }
}
