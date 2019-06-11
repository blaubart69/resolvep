﻿using System;
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

            using (hoststream)
            {
                RunMax(hoststream);
            }
        }
        static void RunMax(TextReader hoststream)
        {
            new StartMaxTasks().Run(
                tasks: ReadLines(hoststream).Select(hostname => resolveAsync(hostname.Trim())),
                MaxParallel: 128);
        }
        static void RunV3(TextReader hoststream)
        {
            TasksHelper.RunMaxParallel(
                ReadLines(hoststream)
                .Select(hostname => resolveAsync(hostname.Trim()) ),
                MaxParallel: 16);
        }
        static void RunV2(TextReader hoststream)
        {
            TasksHelper.StartAllAndWaitForThem(
                ReadLines(hoststream)
                .Select(hostname => resolveAsync(hostname.Trim())));
        }
        static void RunV1(TextReader hoststream)
        {
            long counter = 0;
            int error = 0;
            ManualResetEvent finished = new ManualResetEvent(false);

            using (hoststream)
            {
                Interlocked.Increment(ref counter); // !!!
                foreach (string host in ReadLines(hoststream))
                {
                    Interlocked.Increment(ref counter);
                    resolveAsync(host.Trim())
                        .ContinueWith((Task t) =>
                        {
                            if (t.Exception != null)
                            {
                                Interlocked.Increment(ref error);
                            }
                            if (Interlocked.Decrement(ref counter) == 0)
                            {
                                finished.Set();
                            }
                        });
                }
            }

            if (Interlocked.Decrement(ref counter) != 0)
            {
                finished.WaitOne();
            }
        }
        static async Task resolveAsync(string hostname)
        {
            string ips;
            try
            {
                IPHostEntry entry = await Dns.GetHostEntryAsync(hostname);
                ips = String.Join(" ", entry.AddressList.Select(i => i.ToString()));
            }
            catch ( SocketException sox )
            {
                ips = sox.Message.Replace(' ','_');
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
