using System;
using System.Threading;

namespace BOG.SwissArmyKnife.Demo
{
    partial class Program
    {
        private enum DemoItem : int
        {
            BabbleOn = 0,
            CipherUtility = 1
        }

        static void Help()
        {
            Console.WriteLine("Invalid or missing argument.");
            Console.WriteLine("A string argument is needed to specify the demo to run.  It is one of:");
            foreach (var name in System.Enum.GetNames(typeof(DemoItem)))
            {
                Console.WriteLine($"  {name}");
            }
#if DEBUG
            Console.WriteLine("Press ENTER to close this window.");
            Console.ReadLine();
#endif
            System.Environment.Exit(1);
        }

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Help();
            }
            if (!System.Enum.TryParse<DemoItem>(args[0], out DemoItem demoThis))
            {
                Help();
            }

            try
            {
                switch (demoThis)
                {
                    case DemoItem.BabbleOn:
                        Console.WriteLine("Starting BabbleOn... use Telnet to connect to the listening port (e.g. telnet localhost 65200)");
                        Console.WriteLine("Runs for two minutes, or until a key is pressed in this window.");
                        Console.WriteLine("In the telnet window, pressing a key will reset the client timeout.");
                        var babble = new BabbleOnDemo();
                        var stopAt = DateTime.Now.AddMinutes(2);
                        try
                        {
                            babble.StartBabbling(1);
                            while (DateTime.Now < stopAt && !Console.KeyAvailable)
                            {
                                Thread.Sleep(200);
                            }
                        }
                        finally
                        {
                            babble.StopBabbling();
                        }
                        break;

                    case DemoItem.CipherUtility:
                        Console.WriteLine("Starting CipherUtility...");
                        new CipherUtilityDemo().Demos();
                        break;

                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(DetailedException.WithEnterpriseContent(ref err, "Oops...", "That was bad!!"));
            }
            Console.WriteLine("Press ENTER to close this window");
            Console.ReadLine();
        }
    }
}
