using System;

namespace JetKarmaBot
{
    public class App
    {
        public static void Main(string[] args)
        {
            Current = new App(new Config("karma.cfg.json"));

            Console.ReadKey();
        }

        public static App Current { get; private set; }

        public App(Config cfg)
        {
            Config = cfg;
            Db = new Db(Config);
            Watcher = new JetKarmaBot(Config, Db);
            Console.WriteLine("JatKarmaBot started!");
        }

        Config Config { get; }
        Db Db { get; }
        JetKarmaBot Watcher { get; }
    }
}
