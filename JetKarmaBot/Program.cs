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
            Console.ReadKey();
        }

        public static App Current { get; private set; }
        [Inject(true)]
        public void Run(Container c)
        {
            Watcher = c.GetInstance<JetKarmaBot>();
            Console.WriteLine("JetKarmaBot started!");
        }

        JetKarmaBot Watcher { get; set; }
    }
}
