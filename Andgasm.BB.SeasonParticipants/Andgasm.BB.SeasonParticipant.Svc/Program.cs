using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Andgasm.BB.SeasonParticipant.Extractor.Svc
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "SeasonParticipantExtractor.Svc";
            MainAsync().GetAwaiter().GetResult();
        }

        public static async Task MainAsync()
        {
            Console.Title = "SeasonParticipantExtractor.Svc";
            var boot = new Startup();
            await boot.Host.RunConsoleAsync();
            Console.ReadKey();
        }
    }
}
