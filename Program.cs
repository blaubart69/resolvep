using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace resolvep
{
    static class Program
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
                if (   String.Compare("-h",     args[0], ignoreCase: true) == 0
                    || String.Compare("--help", args[0], ignoreCase: true) == 0)
                {
                    PrintUsage();
                    return;
                }
                else
                {
                    string filename = args[0];
                    hoststream = new StreamReader(filename);
                }
            }

            using (hoststream)
            {
                new MaxTasks().Start(
                    tasks:          ReadLines(hoststream).Select(hostname => resolveAsync(hostname.Trim())),
                    MaxParallel:    128)
                .Wait();
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine(
                  "usage: resolvep [-h] [filename]"
              + "\nlookup hostnames in DNS." 
              + "\neither you specify a filename with one host per line. Otherwise hostnames will be read via stdin."
            + "\n\nOptions:"
              + "\n  -h, --help                 show this message and exit"
              );
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
