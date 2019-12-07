using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using JetKarmaBot.Models;
using Microsoft.EntityFrameworkCore;
using NLog;
using Perfusion;

namespace JetKarmaBot
{
    public static class Program
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public enum ExitCode : int
        {
            Ok = 0,
            ErrorNotStarted = 0x80,
            ErrorRunning = 0x81,
            ErrorException = 0x82,
            ErrorInvalidCommandLine = 0x100
        };

        public static int Main(string[] args)
        {
            log.Info("Starting JetKarmaBot.");
            Container c = new Container();
            var cfg = new Config("karma.cfg.json");
            c.AddInstance(cfg);

            var dbOptions = new DbContextOptionsBuilder<KarmaContext>()
                .UseMySql(cfg.ConnectionString + (cfg.ConnectionString.EndsWith(";") ? "" : ";") + "TreatTinyAsBoolean=false");
            c.AddInfo<Logger>(new LogInfo());
            if (cfg.SqlDebug)
            {
                dbOptions = dbOptions.UseLoggerFactory(c.GetInstance<NLoggerFactory>());
            }
            c.AddTransient(() => new KarmaContext(dbOptions.Options));
            c.Add<JetKarmaBot>();

            var bot = c.GetInstance<JetKarmaBot>();

            try
            {
                bot.Init().Wait();
                log.Info("JetKarmaBot started. Press Ctrl-C to exit...");
                Environment.ExitCode = (int)ExitCode.ErrorRunning;
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
                AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
                {
                    log.Info("Received stop request, waiting for exit...");
                    bot?.Stop()?.Wait();
                };
            }
            catch { }

            quitEvent.WaitOne(Timeout.Infinite);
            log.Info("Waiting for exit...");
            bot?.Stop()?.Wait();

            return (int)ExitCode.Ok;
        }
    }
}
