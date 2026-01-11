using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace BOG.SwissArmyKnife
{
    internal class EavesDropper
    {
        public DateTime StartedOn = DateTime.MinValue;
        public Thread Worker = null;
        public DateTime TimeoutOn = DateTime.MinValue;
        public TcpClient TcpClient = null;
        public NetworkStream ClientStream = null;
        public Queue<string> Messages = new();
        public bool Running = true;
        public bool StopRequested = false;
        public int KeepAliveRequests = 0;
    }

    /// <summary>
    /// This class is intended to be integrated into a windows/web service, but can be used in console
    /// or windows form applications.  It establishes a TCP listener on a port, and allows one or more 
    /// telnet clients to connect and eavesdrop on the code's activity/inactivity. It is intended for 
    /// debugging/troubleshooting a service's activity, without permanent logging.
    /// TCP listener low port defaults to 65200, and the high port defaults to 65299.  This can be 
    /// overridden by creating appSettings in a config file of BabbleOn.LowPort and BabbleOn.HighPort.
    /// The ports available for monitoring can be determined with the Babbler console application.
    /// </summary>
    public class BabbleOn
    {
        private string _IdentificationFingerprint = string.Empty;
        private string _appSignature = string.Empty;
        private int _LowListenPort = 65200;
        private int _HighListenPort = 65209;
        private int _ListeningPort = 0;
        private int _MaxListeners = 5;
        private int _TimeoutSeconds = 60;

        private TcpListener MyTcpListener;
        private Thread MyListenThread;
        private Dictionary<Guid, EavesDropper> Listeners = new();
        private DateTime StartTime = DateTime.Now;
        private bool Running = false;
        private bool Listening = false;
        private bool StopIsRequested = false;

        /// <summary>
        /// 
        /// </summary>
        public int MaxListeners
        {
            get { return _MaxListeners; }
            private set { _MaxListeners = value; }
        }

        /// <summary>
        /// The number of seconds after a connection, without keep alives, before it is forcibly 
        /// disconnected.
        /// </summary>
        public int TimeoutSeconds
        {
            get { return _TimeoutSeconds; }
            private set { _TimeoutSeconds = value; }
        }

        /// <summary>
        /// The port assigned to this listener.
        /// </summary>
        public int ListeningPort
        {
            get { return _ListeningPort; }
        }

        /// <summary>
        /// The lowest port number where a listener can be established
        /// </summary>
        public int LowListenPort
        {
            get { return _LowListenPort; }
        }

        /// <summary>
        /// The highest port number where a listener can be established
        /// </summary>
        public int HighListenPort
        {
            get { return _HighListenPort; }
        }

        /// <summary>
        /// Instantiate with defaults
        /// </summary>
        public BabbleOn()
        {
            _IdentificationFingerprint = string.Empty;
        }

        /// <summary>
        /// Instantiate with fingerprint authentication string
        /// </summary>
        /// <param name="identificationFingerprint"></param>
        public BabbleOn(string identificationFingerprint)
        {
            _IdentificationFingerprint = identificationFingerprint;
        }

        /// <summary>
        /// Instantiate with some overrides.
        /// </summary>
        /// <param name="maxListeners"></param>
        /// <param name="timeoutSeconds"></param>
        public BabbleOn(int maxListeners, int timeoutSeconds)
        {
            _MaxListeners = maxListeners;
            if (timeoutSeconds >= 0)
            {
                _TimeoutSeconds = timeoutSeconds;
            }
        }

        /// <summary>
        /// Instantiate with explicit arguments.
        /// </summary>
        /// <param name="identificationFingerprint"></param>
        /// <param name="maxListeners"></param>
        /// <param name="timeoutSeconds"></param>
        public BabbleOn(string identificationFingerprint, int maxListeners, int timeoutSeconds)
        {
            _IdentificationFingerprint = identificationFingerprint;
            _MaxListeners = maxListeners;
            if (timeoutSeconds >= 0)
            {
                _TimeoutSeconds = timeoutSeconds;
            }
        }

        /// <summary>
        /// Ensures the stop command is executed before dispose commences.
        /// </summary>
        ~BabbleOn()
        {
            StopIsRequested = true;
            Stop();
        }

        public void RequestStop()
        {
            StopIsRequested = true;
        }

        /// <summary>
        /// Establishes the listening port and accepts listener connections.
        /// </summary>
        public void Start()
        {
            if (Running == false && !StopIsRequested)
            {
                EstablishListener();
                StartTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Disconnects listeners, stops the listener and frees up the listening port.
        /// </summary>
        public void Stop()
        {
            if (Running)
            {
                Running = false;
                while (Listening && MyListenThread.ThreadState == ThreadState.Running)
                {
                    Thread.Sleep(100);
                }
                while (Listeners.Count > 0)
                {
                    Guid g = Listeners.Keys.ElementAt(0);
                    DropListener(g);
                }
                MyListenThread = null;
            }
        }

        private void EstablishListener()
        {
            if (!StopIsRequested)
            {
                for (_ListeningPort = _LowListenPort; _ListeningPort <= _HighListenPort; _ListeningPort++)
                {
                    try
                    {
                        this.MyTcpListener = new TcpListener(IPAddress.Any, _ListeningPort);
                        this.MyTcpListener.ExclusiveAddressUse = true;
                        this.MyTcpListener.Start();
                        this.MyTcpListener.Stop();
                        this.MyListenThread = new Thread(new ThreadStart(ListenForClients));
                        this.MyListenThread.Start();
                        Running = true;
                        break;
                    }
                    catch (SocketException)
                    {
                        // this is the only error we trap and ignore.
                    }
                }
                if (Running == false)
                {
                    throw new Exception(string.Format(
                        "BabbleOn was not able to find an available port for the listener in the range {0} .. {1}",
                        _LowListenPort,
                        _HighListenPort));
                }
                _appSignature = string.Format(
                    "#APP: $/{0}/{1}/{2:u}/$\r\n",
                    _IdentificationFingerprint, _MaxListeners, StartTime);
            }
        }

        private void DropListener(Guid g)
        {
            if (this.Listeners.ContainsKey(g))
            {
                try
                {
                    DateTime ForceDropTime = DateTime.Now.AddSeconds(2);
                    this.Listeners[g].StopRequested = true;
                    while (this.Listeners[g].TcpClient != null && this.Listeners[g].TcpClient.Connected && this.Listeners[g].Running && DateTime.Now < ForceDropTime)
                    {
                        Thread.Sleep(20);
                    }
                    // We assume that all TCP connections and Client Streams are flushed, closed, etc, by the members own code.
                    this.Listeners[g].Messages.Clear();
                    this.Listeners[g].ClientStream.Flush();
                    this.Listeners[g].ClientStream.Close();
                    this.Listeners[g].ClientStream.Dispose();
                    this.Listeners[g].TcpClient.Close();
                    this.Listeners[g].TcpClient = null;
                    this.Listeners[g].Worker = null;
                    this.Listeners.Remove(g);
                }
                catch
                {
                }
            }
        }

        private void ListenForClients()
        {
            this.MyTcpListener.Start();
            Listening = true;
            Guid key = Guid.NewGuid();
            bool NeedCleanup = false;

            while (Running && ! StopIsRequested)
            {
                if (this.MyTcpListener.Pending())
                {
                    EavesDropper NewGuy = new();
                    NewGuy.StartedOn = DateTime.Now;
                    NewGuy.TimeoutOn = _TimeoutSeconds <= 0 ? DateTime.MaxValue : NewGuy.StartedOn.AddSeconds(_TimeoutSeconds);
                    NewGuy.TcpClient = this.MyTcpListener.AcceptTcpClient();
                    // create a thread to handle communication with connected client.
                    NewGuy.Worker = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    NewGuy.Running = true;
                    Guid g = Guid.NewGuid();
                    Listeners.Add(g, NewGuy);
                    this.Listeners[g].Worker.Start(g);
                    continue;
                }
                NeedCleanup = false;
                foreach (Guid g in this.Listeners.Keys)
                {
                    if (this.Listeners[g].Running == false)
                    {
                        NeedCleanup = true;
                        key = g;
                        break;
                    }
                    if (this.Listeners[g].TimeoutOn <= DateTime.Now && this.Listeners[g].StopRequested == false)
                    {
                        this.Listeners[g].Messages.Enqueue(string.Format("# Time limit of {0} seconds has been reached, disconnecting **", _TimeoutSeconds));
                        this.Listeners[g].StopRequested = true;
                        break;
                    }
                }
                if (NeedCleanup)
                {
                    DropListener(key);
                    continue;
                }
                Thread.Sleep(250);
            }
            this.MyTcpListener.Stop();
            this.MyTcpListener = null;
            Listening = false;
        }

        private void HandleClientComm(object client)
        {
            Guid myListener = (Guid)client;
            this.Listeners[myListener].ClientStream = this.Listeners[myListener].TcpClient.GetStream();
            System.Text.ASCIIEncoding encoding = new();

            // send the signal string as the first line, in case the caller is BabbleFinder building a list of available
            // connections.
            this.Listeners[myListener].Messages.Enqueue(_appSignature);
            if (this.Listeners.Count > _MaxListeners)
            {
                this.Listeners[myListener].Messages.Enqueue("** Sorry, maximum number of listeners has been reached. Try later. **\r\n");
                this.Listeners[myListener].StopRequested = true;
            }
            else
            {
                // now send a list of current connections, which are not the current connection.  Again, BabbleFinder will use
                // it to build a list of clients connected.
                foreach (Guid g in this.Listeners.Keys)
                {
                    if (this.Listeners[g].TcpClient != null && this.Listeners[g].TcpClient.Connected)
                    {
                        this.Listeners[myListener].Messages.Enqueue(
                            string.Format("#LISTENER: {0},ConnectTime={1:u},KeepAliveRequest={2},Timeout={3:u},{4}\r\n",
                            this.Listeners[g].TcpClient.Client.RemoteEndPoint.ToString(),
                            this.Listeners[g].StartedOn,
                            this.Listeners[g].KeepAliveRequests,
                            this.Listeners[g].TimeoutOn,
                            g != myListener ? "(remote)" : "(me)"
                            ));
                    }
                }
                this.Listeners[myListener].Messages.Enqueue(string.Format("# Activity follows\r\n", this.Listeners[myListener].TcpClient.Client.RemoteEndPoint.ToString()));
                Thread.Sleep(1000);
            }

            while (this.Listeners.ContainsKey(myListener) && this.Listeners[myListener].TcpClient.Connected && this.Listeners[myListener].Running)
            {
                while (this.Listeners[myListener].TcpClient.Connected && this.Listeners[myListener].Messages.Count > 0)
                {
                    try
                    {
                        Byte[] bytes = encoding.GetBytes(this.Listeners[myListener].Messages.Dequeue());
                        this.Listeners[myListener].ClientStream.Write(bytes, 0, bytes.Length);
                    }
                    catch
                    {
                    }
                }
                if (this.Listeners[myListener].TcpClient.Connected && this.Listeners[myListener].ClientStream.DataAvailable == true)
                {
                    // A keypress of X will break the connection.
                    // Any other keypress will extend the timeout period.
                    if (StopIsRequested)
                    {
                        string ConnectionEntry = string.Format("# Disconnecting (listener is shutting down)\r\n", this.Listeners[myListener].TcpClient.Client.RemoteEndPoint.ToString());
                        Byte[] bytes = encoding.GetBytes(ConnectionEntry);
                        this.Listeners[myListener].ClientStream.Write(bytes, 0, bytes.Length);
                        this.Listeners[myListener].ClientStream.Flush();
                        this.Listeners[myListener].StopRequested = true;
                        break;
                    }

                    int keyPressed = this.Listeners[myListener].ClientStream.ReadByte();
                    if (keyPressed == 0x58 || keyPressed == 0x78)
                    {
                        string ConnectionEntry = string.Format("# Disconnecting (operator keypress)\r\n", this.Listeners[myListener].TcpClient.Client.RemoteEndPoint.ToString());
                        Byte[] bytes = encoding.GetBytes(ConnectionEntry);
                        this.Listeners[myListener].ClientStream.Write(bytes, 0, bytes.Length);
                        break;
                    }
                    else if (keyPressed == 0x0D) // reset the timeout period when ENTER is pressed.
                    {
                        this.Listeners[myListener].TimeoutOn = _TimeoutSeconds <= 0 ? DateTime.MaxValue : DateTime.Now.AddSeconds(_TimeoutSeconds);
                        this.Listeners[myListener].Messages.Enqueue("# Keep alive\r\n");
                        this.Listeners[myListener].KeepAliveRequests++;
                    }
                }
                if (this.Listeners[myListener].StopRequested)
                {
                    break;
                }
                Thread.Sleep(250);
            }
            if (this.Listeners.ContainsKey(myListener))
                this.Listeners[myListener].TcpClient.Close();
            DropListener(myListener);
        }

        /// <summary>
        /// Queues a message to pass to all listeners.  The message is sent verbatim.
        /// </summary>
        /// <param name="message"></param>
        public void Write(string message)
        {
            if (this.Listeners.Count == 0)
                return;

            List<Guid> DeadConnections = new();
            foreach (Guid g in Listeners.Keys)
            {
                if (this.Listeners[g].TcpClient.Connected)
                {
                    this.Listeners[g].Messages.Enqueue(message);
                }
                else // connection is no longer listed as active.
                {
                    DeadConnections.Add(g);
                    break;
                }
            }
            foreach (Guid g in DeadConnections)
            {
                DropListener(g);
            }
        }

        /// <summary>
        /// Queues a message to pass to all listeners.  The message is appended with a line terminator.
        /// </summary>
        /// <param name="message"></param>
        public void WriteLine(string message)
        {
            Write(message + "\r\n");
        }
    }

    /// <summary>
    /// A structure to hold the results of a successful port scan which detected a BabbleOn() listener.
    /// </summary>
    public class ScannedPort
    {
        /// <summary>
        /// 
        /// </summary>
        public int Port = 0;
        /// <summary>
        /// 
        /// </summary>
        public bool Error = false;
        /// <summary>
        /// 
        /// </summary>
        public bool Found = false;
        /// <summary>
        /// 
        /// </summary>
        public string AppSignature = "*none*";
        /// <summary>
        /// 
        /// </summary>
        public int PID = 0;
        /// <summary>
        /// 
        /// </summary>
        public int BasePriority = 0;
        /// <summary>
        /// 
        /// </summary>
        public int MaxConnections = 0;
        /// <summary>
        /// 
        /// </summary>
        public int ActiveConnections = 0;
        /// <summary>
        /// 
        /// </summary>
        public DateTime StartTime = DateTime.MinValue;
    }

    /// <summary>
    /// A class which scans a range of ports for BabbleOn listeners, and provides a list of the listeners found.
    /// </summary>
    public class BabbleFinder
    {
        private static string _Host = "localhost";
        private int _LowListenPort = 65200;
        private int _HighListenPort = 65299;

        private static Dictionary<int, ScannedPort> _Answers = new();

        /// <summary>
        /// Instantiation with default localhost and default port range.
        /// </summary>
        public BabbleFinder()
        {
        }

        /// <summary>
        /// Instantiation to scan a system other than the localhost on the default port range.
        /// </summary>
        public BabbleFinder(string host)
        {
            _Host = host;
        }

        /// <summary>
        /// Instantiation to scan localhost on a specific port range.
        /// </summary>
        public BabbleFinder(int lowPort, int highPort)
        {
            _LowListenPort = lowPort;
            _HighListenPort = highPort;
        }

        /// <summary>
        /// Instantiation to scan a system other than the localhost on a specific port range.
        /// </summary>
        public BabbleFinder(string host, int lowPort, int highPort)
        {
            _Host = host;
            _LowListenPort = lowPort;
            _HighListenPort = highPort;
        }

        /// <summary>
        /// Scan the ports for listeners and return details.
        /// </summary>
        /// <returns></returns>
        public List<ScannedPort> ScanPorts(bool includeNotFound)
        {
            List<ScannedPort> result = new();
            _Answers.Clear();
            Dictionary<int, Thread> workers = new();
            for (int portNumber = _LowListenPort; portNumber <= _HighListenPort; portNumber++)
            {
                workers.Add(portNumber, new Thread(PollPort));
                workers[portNumber].Start(portNumber);
            }
            while (workers.Count > _Answers.Count)
                Thread.Sleep(100);
            for (int portNumber = _LowListenPort; portNumber <= _HighListenPort; portNumber++)
            {
                workers[portNumber] = null;
                workers.Remove(portNumber);
            }
            for (int portNumber = _LowListenPort; portNumber <= _HighListenPort; portNumber++)
            {
                if (includeNotFound || _Answers[portNumber].Found)
                    result.Add(_Answers[portNumber]);
            }
            return result;
        }

        // thread safe: internal use only.
        private static void PollPort(object PortNumberObject)
        {
            int portNumber = (int)PortNumberObject;
            TcpClient c = new();
            ScannedPort p = new();

            p.Port = portNumber;
            c.ReceiveTimeout = 5;
            c.NoDelay = true;
            c.Client.ReceiveBufferSize = 2048;
            try
            {
                c.Client.LingerState = new LingerOption(false, 2);
                c.Connect(_Host, portNumber);
                DateTime Timeout = DateTime.Now.AddSeconds(4);
                while (DateTime.Now <= Timeout && c.Client.Connected == false)
                    ;  // wait for connection or timeout.
                if (c.Client.Connected)
                {
                    bool HaveFrame = false;
                    StringBuilder lines = new();
                    byte[] RecvBuffer = new byte[2048];
                    while (lines.Length < 500)
                    {
                        Timeout = DateTime.Now.AddSeconds(2);
                        while (c.Client.Available == 0 && DateTime.Now <= Timeout)
                            ;
                        if (c.Client.Available == 0)
                            break;  // no recv data in 2 seconds?  Give up.
                        int ByteCount = c.Client.Receive(RecvBuffer);
                        int ByteIndex = 0;
                        for (ByteIndex = 0; ByteIndex < ByteCount; ByteIndex++)
                        {
                            lines.Append((char)RecvBuffer[ByteIndex]);
                        }
                        if (lines.ToString().IndexOf("# Activity follows") >= 0 && lines.ToString().IndexOf("\r\n") > 0)
                        {
                            HaveFrame = true;
                            break;
                        }
                    }
                    if (HaveFrame)
                    {
                        string[] LineSet = lines.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        int ClientCount = 0;
                        foreach (string s in LineSet)
                        {
                            if (s.IndexOf("#APP: $") == 0)
                            {
                                string[] AppSig = LineSet[0].Split(new string[] { "/" }, StringSplitOptions.None);
                                //  APP: $/com.mydomain.mywebservice/2380/80/5/{starttime}$
                                if (AppSig.Length == 7 && AppSig[0] == "#APP: $" && AppSig[6] == "$")
                                {
                                    p.AppSignature = AppSig[1];
                                    p.PID = int.Parse(AppSig[2]);
                                    p.BasePriority = int.Parse(AppSig[3]);
                                    p.MaxConnections = int.Parse(AppSig[4]);
                                    p.StartTime = DateTime.Parse(AppSig[5]);
                                    p.Found = true;
                                }
                            }
                            else if (s.IndexOf("#LISTENER: ") == 0)
                            {
                                ClientCount++;
                            }
                        }
                        p.ActiveConnections = ClientCount - 1;  // don't count the finder in the total.
                    }
                }
            }
            catch
            {
                p.Error = true;
            }
            finally
            {
                try
                {
                    c.Close();
                    c = null;
                }
                catch { }
            }
            lock (_Answers)
            {
                _Answers.Add(portNumber, p);
            }
        }
    }
}