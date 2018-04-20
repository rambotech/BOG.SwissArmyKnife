using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BOG.SwissArmyKnife.Demo.Support;

namespace BOG.SwissArmyKnife.Demo
{
    public class BabbleOnDemo : IDisposable
    {
        Dictionary<int, BabbleOn> _BabbleOn = new Dictionary<int, BabbleOn>();
        BabbleFinder _BabbleFinder;
        Queue<string> _Messages = new Queue<string>();
        PhraseGenerator p = new PhraseGenerator();
        Timer tmrMessageSender = null;
        bool IsBabbling = false;
        AutoResetEvent autoEvent = new AutoResetEvent(true);

        public BabbleOnDemo()
        {
            tmrMessageSender = new Timer(tmrMessageSender_FireEvent, autoEvent, 0, 1000);
            Console.WriteLine("BabbleOnDemo() .. intialized");
        }

        public void Dispose()
        {
            foreach (int index in _BabbleOn.Keys)
            {
                try { _BabbleOn[index].Stop(); } catch { }
            }
        }

        public void StartBabbling(int numberOfListeners)
        {
            if (IsBabbling) return;
            if (numberOfListeners < 1 || numberOfListeners > 20) return;

            _BabbleOn.Clear();
            int lowPort = 70000;
            int highPort = 0;
            for (int listenerIndex = 0; listenerIndex < numberOfListeners; listenerIndex++)
            {
                var fingerPrint = $"BabbleOn listener #{listenerIndex}";
                _BabbleOn.Add(listenerIndex, new BabbleOn(fingerPrint));
                _BabbleOn[listenerIndex].Start();
                highPort = _BabbleOn[listenerIndex].ListeningPort;
                if (highPort < lowPort)
                    lowPort = highPort;
                Console.WriteLine($"Started {fingerPrint} on port {highPort}");
            }
            Console.WriteLine("BabbleOnDemo() .. started listener(s)");
            IsBabbling = true;
        }

        public void StopBabbling()
        {
            if (!IsBabbling) return;
            foreach (int listenerIndex in _BabbleOn.Keys)
                _BabbleOn[listenerIndex].Stop();
            _Messages.Clear();
            IsBabbling = false;
            Console.WriteLine("BabbleOnDemo() .. stopped listener(s)");
        }

        public void FindBabblers(string hostname)
        {
            _BabbleFinder = new BabbleFinder(hostname);
            List<ScannedPort> PortList = _BabbleFinder.ScanPorts(true);
            int ActivePortCount = 0;
            foreach (ScannedPort p in PortList)
            {
                if (!p.Found)
                    continue;
                ActivePortCount++;
                Console.WriteLine(string.Format(
                    "{0}:Port={1}:ListeningSince={2}:CurrentConnectionCount={3}:MaxConnectionCount{4}:PID={5}:\r\n",
                    p.AppSignature,
                    p.Port,
                    p.StartTime,
                    p.ActiveConnections,
                    p.MaxConnections,
                    p.PID));
            }
            Console.WriteLine(string.Format(
                "{0} port{1} scanned, {2} active listener{3}",
                PortList.Count, PortList.Count != 1 ? "s" : string.Empty,
                ActivePortCount, ActivePortCount != 1 ? "s" : string.Empty));
        }

        private void tmrMessageSender_FireEvent(object stateInfo)
        {
            if (_Messages.Count == 0)
                foreach (string s in p.GetPhrase(10))
                    _Messages.Enqueue(s);
            string m = _Messages.Dequeue();
            foreach (int listenerIndex in _BabbleOn.Keys)
                _BabbleOn[listenerIndex].WriteLine(m);
        }
    }
}