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
            string filename = args[0];

            int all = 0;
            int done = 0;
            AutoResetEvent oneResolved = new AutoResetEvent(false);

            foreach (string host in File.ReadLines(filename) )
            {
                all += 1;
                resolveAsync(host).ContinueWith((Task t) => { Interlocked.Increment(ref done); oneResolved.Set(); });
            }

            while (all != done)
            {
                oneResolved.WaitOne();
                Console.WriteLine($"all: {all}, done: {done}");
            }

        }
        static async Task resolveAsync(string hostname)
        {
            string ips;
            try
            {
                Console.WriteLine($"querying [{hostname}]...");
                IPHostEntry entry = await Dns.GetHostEntryAsync(hostname);

                ips = String.Join(",", entry.AddressList.Select(i => i.ToString()));

                throw new Exception("exi");
            }
            catch ( SocketException sox )
            {
                ips = sox.Message;
            }

            Console.WriteLine($"hostname: {hostname}, ips: {ips}");
        }
    }
}
