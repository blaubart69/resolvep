using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace resolvep
{
    public static class TasksHelper
    {
        public static void StartAllAndWaitForThem(IEnumerable<Task> tasksToStart)
        {
            long counter = 1; // !!!!
            long error = 0;

            using (ManualResetEvent finished = new ManualResetEvent(false))
            {
                foreach (Task t in tasksToStart)
                {
                    Interlocked.Increment(ref counter);

                    t
                    .ContinueWith((Task workingTask) =>
                    {
                        if (workingTask.Exception != null)
                        {
                            Interlocked.Increment(ref error);
                        }

                        if (Interlocked.Decrement(ref counter) == 0)
                        {
                            finished.Set();
                        }
                    })
                    .ConfigureAwait(false);
                }

                if (Interlocked.Decrement(ref counter) != 0)
                {
                    finished.WaitOne();
                }
            }
        }

        public static void RunMaxParallel(IEnumerable<Task> TaskGenerator, int MaxParallel)
        {
            long counter = 1; // !!!! importante !!!!
            long error = 0;
            long done = 0;

            using (ManualResetEvent finished = new ManualResetEvent(false))
            {
                IEnumerator<Task> tasksEnum = TaskGenerator.GetEnumerator();

                Action<Task> afterWork = null;
                afterWork = (Task workingTask) =>
                {
                    Interlocked.Increment(ref done);

                    if (workingTask.Exception != null)
                    {
                        Interlocked.Increment(ref error);
                    }

                    lock (tasksEnum)
                    {
                        if (tasksEnum.MoveNext())
                        {
                            Interlocked.Increment(ref counter);
                            tasksEnum.Current.ContinueWith(afterWork);
                        }
                    }

                    if (Interlocked.Decrement(ref counter) == 0)
                    {
                        finished.Set();
                    }

                    //Console.Error.WriteLine($"counter: {counter} done: {done}");
                };

                for (int i=0; i<MaxParallel; ++i)
                {
                    lock (tasksEnum)
                    {
                        if (tasksEnum.MoveNext())
                        {
                            Interlocked.Increment(ref counter);
                            tasksEnum.Current
                                .ContinueWith(afterWork)
                                .ConfigureAwait(false);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (Interlocked.Decrement(ref counter) != 0)
                {
                    finished.WaitOne();
                }
            }
        }
    }
}
