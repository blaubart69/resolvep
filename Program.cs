using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

namespace resolvep
{
    class Program
    {
        static void Main(string[] args)
        {
            TextReader hoststream;
            if (args.Length == 0)
            {
                hoststream = Console.In;
            }
            else
            {
                string filename = args[0];
                hoststream = new StreamReader(filename);
            }

            int all = 0;
            int done = 0;
            int error = 0;
            AutoResetEvent oneResolved = new AutoResetEvent(false);

            using (hoststream)
            {
                foreach (string host in ReadLines(hoststream))
                {
                    all += 1;
                    resolveAsync(host)
                        .ContinueWith((Task t) =>
                            {
                                if ( t.Exception != null )
                                {
                                    Interlocked.Increment(ref error);
                                }
                                Interlocked.Increment(ref done);
                                oneResolved.Set();
                            });
                }
            }

            while (all != done)
            {
                oneResolved.WaitOne();
            }
            Console.Error.WriteLine($"all: {all}, done: {done}, error: {error}");
        }
        static async Task resolveAsync(string hostname)
        {
            string ips;
            try
            {
                IPHostEntry entry = await Dns.GetHostEntryAsync(hostname);
                ips = String.Join(",", entry.AddressList.Select(i => i.ToString()));
            }
            catch ( SocketException sox )
            {
                ips = sox.Message;
            }

            Console.WriteLine($"{hostname}\t{ips}");
        }
        static IEnumerable<string> ReadLines(TextReader reader)
        {
            string line;
            while ( (line=reader.ReadLine()) != null )
            {
                yield return line;
            }
        }
    }
}
