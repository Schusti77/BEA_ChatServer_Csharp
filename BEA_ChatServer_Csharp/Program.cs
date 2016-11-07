using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
//using System.Threading.Tasks;

using System.Net.Sockets;


namespace BEA_ChatServer_Csharp
{
    class chatserver_ui
    {
        static List<chatclient> ClientDB;
        static Int16 port;

        public static int Main(String[] args)
        {
            bool running = true;
            bool working = false;
            Thread Listener = new Thread(listen);
            ClientDB = new List<chatclient>();

            //wenn kein Port übergeben wurde, dann 11000 benutzen
            port = -1;
            if (args.Count() > 0)
                Int16.TryParse(args[1], out port);
            if(port == -1)
                    port = 11000;

            while (running)
            {
                Console.Clear();
                Console.WriteLine("Chatserver v1.0");
                Console.WriteLine("(C)Stephan Schuster");
                Console.WriteLine("*******************");
                //Console.WriteLine("Chaträume:");
                //Console.WriteLine("0 - Lobby");
                //Console.WriteLine("*******************");
                Console.WriteLine("0 - Programmende");
                if (!Listener.IsAlive)
                    Console.WriteLine("1 - Starte Chatserver");
                else
                    Console.WriteLine("1 - Stoppe Chatserver");
                Console.WriteLine("2 - Chatraum hinzufügen");
                Console.WriteLine("*******************");
                string antwort = Console.ReadLine();
                switch (antwort)
                {
                    case "0":
                        {
                            running = false;
                            if(working)
                                Listener.Abort();
                            break;
                        }
                    case "1":
                        {
                            if (!Listener.IsAlive)
                                Listener.Start();
                            else
                                Listener.Abort();
                            break;
                        }
                    default: break;
                }
            }
            ClientDB.Clear();
            return 0;
        }

        private static void listen(Object obj)
        {
            Console.Write("Starte Chatserver");
            while (true)
            { 
                IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                IPEndPoint endPoint = null;

                //nach einer IPv4 Adresse suchen
                foreach (IPAddress ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        endPoint = new IPEndPoint(ip, port);
                        break;
                    }
                }

                //nach einer IPv6 Adresse suchen
                if (endPoint == null)
                {
                    foreach (IPAddress ip in hostEntry.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            endPoint = new IPEndPoint(ip, port);
                            break;
                        }
                    }
                }

                if (endPoint == null)
                {
                    //Server-Rechner hat keine IP-Adresse
                    //entweder keine aktive Netzwerkkarte oder irgendwas faul
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Fehler]:Dieser Rechner hat keine verbindung zu einem Netzwerk");
                }

                Socket s = new Socket(endPoint.Address.AddressFamily,
                SocketType.Dgram,
                ProtocolType.Udp);
                s.ReceiveTimeout = 1000;

                // Creates an IpEndPoint to capture the identity of the sending host.
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint senderRemote = (EndPoint)sender;

                // Binding is required with ReceiveFrom calls.
                s.Bind(endPoint);
                byte[] msg = new Byte[1+32+256];
                //Console.WriteLine("Waiting to receive datagrams from client...");
                // This call blocks.  
                try
                {
                    s.ReceiveFrom(msg, 0, msg.Length, SocketFlags.None, ref senderRemote);
                    
                    Console.WriteLine(System.Text.Encoding.UTF8.GetString(msg).TrimEnd('\0'));
                    MsgToProcess MTP = new MsgToProcess();
                    MTP.Message = System.Text.Encoding.UTF8.GetString(msg).TrimEnd('\0');
                    MTP.IP = sender.Address;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessMessage), MTP);
                }
                catch(System.Net.Sockets.SocketException)
                {
                    //innerhalb des Socket.ReceiveTimeout keine Nachricht empfangen
                    //nichts machen, einfach noch einen Loop in der While-Schleife
                }
                finally
                {
                    //socket immer schließen
                    s.Close();
                }
            }
        }

        static void ProcessMessage(object obj)
        {
            MsgToProcess MTP = obj as MsgToProcess;
            Console.WriteLine("Nachricht verarbeiten: {0}", MTP.Message);
            String Nachrichtentyp = MTP.Message.Substring(0, 1);
            String Argument1 = MTP.Message.Substring(1, 32);
            String Argument2 = MTP.Message.Substring(33, 256);

            switch(Nachrichtentyp)
            {
                case "A":
                    {
                        //Client anmelden
                        //Argument1 = Benutzername
                        //Argument2 = leer
                        AddClient(Argument1, MTP.IP);
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
                        break;
                    }
                case "S":
                    {
                        //Clientstatus empfangen
                        break;
                    }
                case "U":
                    {
                        //Benutzernamen angefragt
                        break;
                    }
                default:
                    {
                        //unbekannte Nachricht empfangen
                        //verwerfen
                        break;
                    }
            }

            Console.WriteLine("Nachricht verarbeitet: {0}", MTP.Message);
        }

        static void AddClient(String benutzername, IPAddress ip)
        {
            ClientDB.Add(new chatclient(benutzername, ip));
            Console.WriteLine("Client hinzugefügt: {0}", benutzername);
        }

        static void RemoveClient(String IDS)
        {
            chatclient UserToDel = ClientDB.Find(x => x.IDS == IDS);
            String UsernameToDel = UserToDel.IDS;
            ClientDB.Remove(UserToDel);
            Console.WriteLine("Client entfernt: {0}", UsernameToDel);
        }

        static void ClientSwitchChannel(String IDS, int ChannelId)
        {
            chatclient UserToDel = ClientDB.Find(x => x.IDS == IDS);
            String UsernameToDel = UserToDel.IDS;
            ClientDB.Remove(UserToDel);
            Console.WriteLine("Client entfernt: {0}", UsernameToDel);
        }

        static void SendMsgToChannel(int Channel)
        {
            foreach (chatclient x in ClientDB)
            {
                if (x.Channel == Channel)
                {
                    //sende text
                }
            }
        }

        //Benutzerdaten speicher Klasse
        class chatclient
        {
            public String IDS { get; private set; }
            public IPAddress IP { get; private set; }
            public Int16 Channel { get; private set; }
            private String Username;

            /* konstruktor */
            public chatclient(String benutzername, IPAddress ip_adress)
            {
                this.Username = benutzername;
                IP = ip_adress;
                IDS = hash(32);
                Channel = 0; //lobby channel als default
            }

            private string hash(int Länge)
            {
                string ret = string.Empty;
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                string Content = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvw!öäüÖÄÜß\"§$%&/()=?*#-";
                Random rnd = new Random();
                for (int i = 0; i < Länge; i++)
                    SB.Append(Content[rnd.Next(Content.Length)]);
                return SB.ToString();
            }
        }
        class chatraum
        {
            private Int16 id;
            private String name;
        }

        class MsgToProcess
        {
            public String Message { get; set; }
            public IPAddress IP { get; set; }
        }
    }
}

