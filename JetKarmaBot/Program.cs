using System;
using Perfusion;

namespace JetKarmaBot
{
    public class App
    {
        public static void Main(string[] args)
        {
            Container c = new Container();
            c.AddInstance(new Config("karma.cfg.json"));
            Current = c.GetInstance(typeof(App)) as App;
            Current.Run();
            Console.ReadKey();
        }

        public static App Current { get; private set; }

        public void Run()
        {
            Console.WriteLine("JatKarmaBot started!");
        }

        [Inject(true)] JetKarmaBot Watcher { get; }
    }
}
