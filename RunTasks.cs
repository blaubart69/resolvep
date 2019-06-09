using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace resolvep
{
    static class RunTasks
    {

        public static void RunAndWaitForTasks(IEnumerable<Task> TaskGenerator)
        {
            long counter = 1; // !!!!
            long error = 0;

            using (ManualResetEvent finished = new ManualResetEvent(false))
            {
                foreach (Task t in TaskGenerator)
                {
                    Interlocked.Increment(ref counter);

                    t.ContinueWith((Task tx) =>
                       {
                           if (tx.Exception != null)
                           {
                               Interlocked.Increment(ref error);
                           }

                           if (Interlocked.Decrement(ref counter) == 0)
                           {
                               finished.Set();
                           }
                       });
                }

                if (Interlocked.Decrement(ref counter) != 0)
                {
                    finished.WaitOne();
                }
            }
        }
        public static void RunAndWaitMaxTasks(IEnumerable<Task> TaskGenerator, int MaxParallel)
        {
            long counter = 1; // !!!! importante !!!!
            long error = 0;
            long done = 0;

            using (ManualResetEvent finished = new ManualResetEvent(false))
            {
                var tasksEnum = TaskGenerator.GetEnumerator();

                Action<Task> continueWith = null;
                continueWith = (Task workingTask) =>
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
                            tasksEnum.Current.ContinueWith(continueWith);
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
                            tasksEnum.Current.ContinueWith(continueWith);
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
