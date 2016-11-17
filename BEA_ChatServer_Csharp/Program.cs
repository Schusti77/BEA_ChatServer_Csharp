using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
//using System.Threading.Tasks;

using System.Net.Sockets;


namespace BEA_ChatServer_Csharp
{
    class chatserver
    {
        static List<chatclient> ClientDB;
        //static Int16 port;
        static IPEndPoint ServerEP = null;
        static UInt64 MsgSent = 0;
        static UInt64 FramesReceived = 0;
        static UInt64 FramesSent = 0;
        static Thread Listener;
        static bool working;
        static Socket s_listen;
        static int sendport = 11001;
        static int recport = 11000;

        public static int Main(String[] args)
        {
            bool running = true;
            working = false;
            ClientDB = new List<chatclient>();

            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());

            //nach einer IPv4 Adresse suchen
            foreach (IPAddress ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ServerEP = new IPEndPoint(ip, recport);
                    break;
                }
            }

            if (ServerEP == null)
            {
                //Server-Rechner hat keine IP-Adresse
                //entweder keine aktive Netzwerkkarte oder irgendwas faul
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Fehler]:Dieser Rechner hat keine Verbindung zu einem Netzwerk");
                return 1;
            }

            while (running)
            {
                ausgabe();

                string antwort = Console.ReadLine();
                switch (antwort)
                {
                    case "0": //Programmende
                        {
                            running = false; //while schleife verlassen
                            working = false; //Empfangsthread beenden
                            break;
                        }
                    case "1":
                        {

                            if (!working)
                            {
                                working = true;
                                s_listen = new Socket(ServerEP.Address.AddressFamily,
                                    SocketType.Dgram,
                                    ProtocolType.Udp);
                                //IPEndPoint ListenerEP = new IPEndPoint(IPAddress.Parse("192.168.178.20"), 11000);
                                s_listen.Bind(ServerEP);
                                Listener = new Thread(listen);
                                Listener.IsBackground = true;
                                Listener.Start();
                                System.Timers.Timer StatusTimer = new System.Timers.Timer(30000);
                                StatusTimer.Elapsed += OnTimedEvent;
                                StatusTimer.AutoReset = true;
                                StatusTimer.Enabled = true;
                            }
                            else
                            {
                                working = false;
                                if (s_listen != null)
                                    s_listen.Close();
                            }
                            break;
                        }
                    default: break;
                }
            }
            ClientDB.Clear(); //Aufräumen
            return 0;
        }

        private static void ausgabe()
        {
            Console.Clear();
            Console.WriteLine("*************************");
            Console.WriteLine("* Chatserver v1.0       *");
            Console.WriteLine("* (C)Stephan Schuster   *");
            Console.WriteLine("*************************");
            Console.WriteLine("Menü:");
            Console.WriteLine("0 - Programmende");
            if (Listener != null)
                if (!Listener.IsAlive)
                    Console.WriteLine("1 - Starte Chatserver");
                else
                    Console.WriteLine("1 - Stoppe Chatserver");
            else
                Console.WriteLine("1 - Starte Chatserver");
            Console.WriteLine("*************************");
            Console.WriteLine("Status:");
            Console.WriteLine("angemeldete Clients: {0}", ClientDB.Count);
            Console.WriteLine("gesendete Chatnachrichten: {0}", MsgSent);
            Console.WriteLine("empfangene Rahmen: {0}", FramesReceived);
            Console.WriteLine("gesendete Rahmen: {0}", FramesSent);
            Console.WriteLine("*************************");
        }

        private static void listen(Object obj)
        {
            Console.WriteLine("listener running - lausche auf {0}:{1}", ServerEP.Address, ServerEP.Port);
            // Creates an IpEndPoint to capture the identity of the sending host.
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)sender;
            byte[] msg = new Byte[1 + 32 + 256];

            while (working)
            {
                try
                {
                    s_listen.ReceiveFrom(msg, 0, msg.Length, SocketFlags.None, ref senderRemote);

                    Console.WriteLine(System.Text.Encoding.UTF8.GetString(msg).TrimEnd('\0'));
                    MsgToProcess MTP = new MsgToProcess();
                    MTP.Message = System.Text.Encoding.UTF8.GetString(msg).TrimEnd('\0');
                    MTP.EP = (IPEndPoint)senderRemote;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessMessage), MTP);
                    FramesReceived++;
                    ausgabe();
                }
                catch(SocketException e)
                {
                    Console.WriteLine("SocketException aufgetreten. Errorcode: {0}, Fehlermeldung: {1}", e.ErrorCode, e.Message);
                }
            }
        }

        //Statusabfrage an angemeldete clients
        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            Socket s = new Socket(AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp);
            //allen angemeldeten clients eine Statusabfrage schicken
            if (ClientDB.Count > 0)
            {
                foreach (chatclient user in ClientDB)
                {
                    if (user.Retry >= 4)
                        ClientDB.Remove(user);
                    else
                    {
                        byte[] msg = Encoding.ASCII.GetBytes("S" + user.IDS.PadRight(32) + ClientDB.Count.ToString().PadRight(256));
                        IPEndPoint statusEP = new IPEndPoint(user.EP.Address, sendport);
                        s.SendTo(msg, statusEP);
                        //s.SendTo(msg, ClientEP);
                        user.Retry++;
                    }
                }
            }
            s.Close();
        }

        static void ProcessMessage(object obj)
        {
            MsgToProcess MTP = obj as MsgToProcess;
            String Nachrichtentyp = MTP.Message.Substring(0, 1);
            String Argument1 = MTP.Message.Substring(1, 32);
            String Argument2 = MTP.Message.Substring(33, 256);
            switch (Nachrichtentyp)
            {
                case "A":
                    {
                        //Client anmelden
                        //Argument1 = Benutzername
                        //Argument2 = leer
                        chatclient user = AddClient(Argument1, MTP.EP);
                        String msg = "A" + user.IDS.PadRight(32) + "".PadRight(256);
                        SendMsgToClient(msg, user.EP);
                        break;
                    }
                case "Q":
                    {
                        //Client abmelden
                        RemoveClient(Argument1);
                        break;
                    }
                case "T":
                    {
                        //Textnachricht empfangen
                        //Argument1 = IDS
                        //Argument2 = Textnachricht
                        SendMsgToClients(Argument1, Argument2);
                        MsgSent++;
                        ausgabe();
                        break;
                    }
                case "S":
                    {
                        //Clientstatus empfangen
                        SetClientActive(Argument1);
                        break;
                    }
                case "U":
                    {
                        //Benutzernamen angefragt
                        SendUserNames(Argument1, MTP.EP);
                        break;
                    }
                default:
                    {
                        //unbekannte Nachricht empfangen
                        //verwerfen
                        break;
                    }
            }
        }

        static chatclient AddClient(String benutzername, IPEndPoint ep)
        {
            ClientDB.Add(new chatclient(benutzername, ep));
            //hier raceconditions möglich, aber bei so einem kleinem chatserver
            //ignoriere ich das mal. soviele clients werden sich nicht gleichzeitig anmelden
            return (ClientDB.Last());
        }

        static void RemoveClient(String IDS)
        {
            chatclient UserToDel = ClientDB.Find(x => x.IDS == IDS);
            if (UserToDel != null)
            {
                String UsernameToDel = UserToDel.IDS;
                ClientDB.Remove(UserToDel);
            }
        }

        static void SetClientActive(String IDS)
        {
            chatclient UserToProcess = ClientDB.Find(x => x.IDS == IDS);
            if (UserToProcess != null)
                UserToProcess.Retry = 0;
        }

        static void SendMsgToClient(String Msg, IPEndPoint ClientEP)
        {
            Socket s = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);

            byte[] msg = Encoding.ASCII.GetBytes(Msg);
            IPEndPoint sendeEP = new IPEndPoint(ClientEP.Address, sendport);
            s.SendTo(msg, sendeEP);
            //s.SendTo(msg, ClientEP);
            s.Close();
            FramesSent++;
            ausgabe();
        }

        static void SendMsgToClients(String IDS, String Msg)
        {
            chatclient Sender = ClientDB.Find(x => x.IDS == IDS);
            if (Sender != null)
            {
                //sender bekannt
                for (int i = 0; i < ClientDB.Count; i++)
                {
                    String Nachricht = "T" + Sender.Username.PadRight(32) + Msg.PadRight(256);
                    SendMsgToClient(Nachricht, new IPEndPoint(ClientDB[i].EP.Address, sendport));
                }
            }
        }

        static void SendUserNames(String IDS, IPEndPoint ep)
        {
            chatclient Sender = ClientDB.Find(x => x.IDS == IDS);
            if (Sender != null)
            {
                //sender bekannt
                for (int i = 0; i < ClientDB.Count; i++)
                {
                    String Nachricht = "U" + ClientDB[i].Username.PadRight(32) + "".PadRight(256);
                    SendMsgToClient(Nachricht, ep);
                }
            }
        }

        //Benutzerdaten speicher Klasse
        class chatclient
        {
            public String IDS { get; private set; }
            public IPEndPoint EP { get; private set; }
            public String Username { get; private set; }
            public UInt16 Retry { get; set; }

            /* konstruktor */
            public chatclient(String benutzername, IPEndPoint endpoint)
            {
                this.Username = benutzername;
                EP = endpoint;
                IDS = hash(32);
                Retry = 0;
            }

            private string hash(int Länge)
            {
                string ret = string.Empty;
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                string Content = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvw";
                Random rnd = new Random();
                for (int i = 0; i < Länge; i++)
                    SB.Append(Content[rnd.Next(Content.Length)]);
                return SB.ToString();
            }
        }

        class MsgToProcess
        {
            public String Message { get; set; }
            public IPEndPoint EP { get; set; }
        }
    }
}

