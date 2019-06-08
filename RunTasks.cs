using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace resolvep
{
    class RunTasks
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
    }
}
