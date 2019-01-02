using System;
using System.Threading;
using JetKarmaBot.Models;
using Microsoft.EntityFrameworkCore;
using Perfusion;

namespace JetKarmaBot
{
    public static class Program
    {
        public enum ExitCode : int
        {
            Ok = 0,
            ErrorNotStarted = 0x80,
            ErrorRunning = 0x81,
            ErrorException = 0x82,
            ErrorInvalidCommandLine = 0x100
        };

#if DEBUG
        public const bool Debug = true;
#else
        public const bool Debug = false;
#endif
        public static int Main(string[] args)
        {
            Container c = new Container();
            var cfg = new Config("karma.cfg.json");
            c.AddInstance(cfg);

            var dbOptions = new DbContextOptionsBuilder<KarmaContext>()
                .UseMySql(cfg.ConnectionString);

            c.Add(() => new KarmaContext(dbOptions.Options));
            c.Add<JetKarmaBot>();

            var bot = c.GetInstance<JetKarmaBot>();

            try
            {
                bot.Init().Wait();
                Console.WriteLine("JetKarmaBot started. Press Ctrl-C to exit...");
                Environment.ExitCode = (int)ExitCode.ErrorRunning;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Environment.ExitCode = (int)ExitCode.ErrorException;
            }
            ManualResetEvent quitEvent = new ManualResetEvent(false);
            try
            {
                Console.CancelKeyPress += (sender, eArgs) => // ctrl-c
                {
                    eArgs.Cancel = true;
                    quitEvent.Set();
                };
            }
            catch { }

            quitEvent.WaitOne(Timeout.Infinite);
            Console.WriteLine("Waiting for exit...");
            bot?.Stop()?.Wait();

            return (int)ExitCode.Ok;
        }
    }
}
