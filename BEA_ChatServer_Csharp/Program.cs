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

        public static int Main(String[] args)
        {
            bool running = true;
            bool working = false;
            Thread Listener = new Thread(listen);
            ClientDB = new List<chatclient>();


            while (running)
            {
                Console.Clear();
                Console.WriteLine("Chatserver v1.0");
                Console.WriteLine("(C)Stephan Schuster");
                Console.WriteLine("*******************");
                Console.WriteLine("Chaträume:");
                Console.WriteLine("0 - Lobby");
                Console.WriteLine("*******************");
                Console.WriteLine("0 - Programmende");
                if (!working)
                    Console.WriteLine("1 - Starte Chatserver");
                else
                    Console.WriteLine("1 - Stoppe Chatserver");
                Console.WriteLine("2 - Chatraum hinzufügen");
                Console.WriteLine("3 - Chatraum löschen");
                Console.WriteLine("2 - Chatraum hinzufügen");
                Console.WriteLine("*******************");
                string antwort = Console.ReadLine();
                switch (antwort)
                {
                    case "0": running = false; break;
                    case "1":
                        {
                            working = !working;
                            if (working)
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
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPEndPoint endPoint = new IPEndPoint(hostEntry.AddressList[2], 11000);

            Socket s = new Socket(endPoint.Address.AddressFamily,
                SocketType.Dgram,
                ProtocolType.Udp);

            // Creates an IpEndPoint to capture the identity of the sending host.
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)sender;

            // Binding is required with ReceiveFrom calls.
            s.Bind(endPoint);
            byte[] msg = new Byte[256];
            Console.WriteLine("Waiting to receive datagrams from client...");
            // This call blocks.  
            s.ReceiveFrom(msg, 0, msg.Length, SocketFlags.None, ref senderRemote);
            s.Close();
        }

        void AddClient(String benutzername, Object connectdingens)
        {
            ClientDB.Add(new chatclient(benutzername, connectdingens));
            Console.WriteLine("Client hinzugefügt: {0}", benutzername);
        }

        void RemoveClient(String IDS)
        {
            chatclient UserToDel = ClientDB.Find(x => x.IDS == IDS);
            String UsernameToDel = UserToDel.IDS;
            ClientDB.Remove(UserToDel);
            Console.WriteLine("Client entfernt: {0}", UsernameToDel);
        }

        void ClientSwitchChannel(String IDS, int ChannelId)
        {
            chatclient UserToDel = ClientDB.Find(x => x.IDS == IDS);
            String UsernameToDel = UserToDel.IDS;
            ClientDB.Remove(UserToDel);
            Console.WriteLine("Client entfernt: {0}", UsernameToDel);
        }

        void SendMsgToChannel(int Channel)
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
            private String ids;
            private Object changeme;//irgendwas zum connecten, ip oder socket etc.
            private Int16 channel;
            private String username;

            /* konstruktor */
            public chatclient(String benutzername, Object connectdingens)
            {
                this.Username = benutzername;
                changeme = connectdingens;
                ids = hash(32);
                Channel = 0; //lobby channel als default
            }

            public string Username
            {
                get
                {
                    return username;
                }
                protected set
                {
                    username = value;
                }
            }

            public string IDS
            {
                get
                {
                    return ids;
                }

                protected set
                {
                    ids = value;
                }
            }

            public short Channel
            {
                get
                {
                    return channel;
                }

                protected set
                {
                    channel = value;
                }
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
    }
}









    ////State object for reading client data asynchronously
    //public class StateObject
    //{
    //    // Client  socket.
    //    public Socket workSocket = null;
    //    // Size of receive buffer.
    //    public const int BufferSize = 1024;
    //    // Receive buffer.
    //    public byte[] buffer = new byte[BufferSize];
    //    // Received data string.
    //    public StringBuilder sb = new StringBuilder();
    //}

    //public class AsynchronousSocketListener
    //{
    //    // Thread signal.
    //    public static ManualResetEvent allDone = new ManualResetEvent(false);

    //    public AsynchronousSocketListener()
    //    {
    //    }

    //    public static void StartListening()
    //    {
    //        int port = 11000;
    //        // Data buffer for incoming data.
    //        byte[] bytes = new Byte[1024];

    //        // Establish the local endpoint for the socket.
    //        // The DNS name of the computer
    //        // running the listener is "host.contoso.com".
    //        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
    //        IPAddress ipAddress = ipHostInfo.AddressList[0];
    //        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

    //        // Create a TCP/IP socket.
    //        Socket listener = new Socket(AddressFamily.InterNetwork,
    //            SocketType.Stream, ProtocolType.Tcp);

    //        // Bind the socket to the local endpoint and listen for incoming connections.
    //        try
    //        {
    //            listener.Bind(localEndPoint);
    //            listener.Listen(100);

    //            while (true)
    //            {
    //                // Set the event to nonsignaled state.
    //                allDone.Reset();

    //                // Start an asynchronous socket to listen for connections.
    //                Console.WriteLine("Waiting for a connection...");
    //                listener.BeginAccept(
    //                    new AsyncCallback(AcceptCallback),
    //                    listener);

    //                // Wait until a connection is made before continuing.
    //                allDone.WaitOne();
    //            }

    //        }
    //        catch (Exception e)
    //        {
    //            Console.WriteLine(e.ToString());
    //        }

    //        Console.WriteLine("\nPress ENTER to continue...");
    //        Console.Read();

    //    }

    //    public static void AcceptCallback(IAsyncResult ar)
    //    {
    //        // Signal the main thread to continue.
    //        allDone.Set();

    //        // Get the socket that handles the client request.
    //        Socket listener = (Socket)ar.AsyncState;
    //        Socket handler = listener.EndAccept(ar);

    //        // Create the state object.
    //        StateObject state = new StateObject();
    //        state.workSocket = handler;
    //        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
    //            new AsyncCallback(ReadCallback), state);
    //    }

    //    public static void ReadCallback(IAsyncResult ar)
    //    {
    //        String content = String.Empty;

    //        // Retrieve the state object and the handler socket
    //        // from the asynchronous state object.
    //        StateObject state = (StateObject)ar.AsyncState;
    //        Socket handler = state.workSocket;

    //        // Read data from the client socket. 
    //        int bytesRead = handler.EndReceive(ar);

    //        if (bytesRead > 0)
    //        {
    //            // There  might be more data, so store the data received so far.
    //            state.sb.Append(Encoding.ASCII.GetString(
    //                state.buffer, 0, bytesRead));

    //            // Check for end-of-file tag. If it is not there, read 
    //            // more data.
    //            content = state.sb.ToString();
    //            if (content.IndexOf("<EOF>") > -1)
    //            {
    //                // All the data has been read from the 
    //                // client. Display it on the console.
    //                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
    //                    content.Length, content);
    //                // Echo the data back to the client.
    //                Send(handler, content);
    //            }
    //            else
    //            {
    //                // Not all data received. Get more.
    //                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
    //                new AsyncCallback(ReadCallback), state);
    //            }
    //        }
    //    }

    //    private static void Send(Socket handler, String data)
    //    {
    //        // Convert the string data to byte data using ASCII encoding.
    //        byte[] byteData = Encoding.ASCII.GetBytes(data);

    //        // Begin sending the data to the remote device.
    //        handler.BeginSend(byteData, 0, byteData.Length, 0,
    //            new AsyncCallback(SendCallback), handler);
    //    }

    //    private static void SendCallback(IAsyncResult ar)
    //    {
    //        try
    //        {
    //            // Retrieve the socket from the state object.
    //            Socket handler = (Socket)ar.AsyncState;

    //            // Complete sending the data to the remote device.
    //            int bytesSent = handler.EndSend(ar);
    //            Console.WriteLine("Sent {0} bytes to client.", bytesSent);

    //            handler.Shutdown(SocketShutdown.Both);
    //            handler.Close();

    //        }
    //        catch (Exception e)
    //        {
    //            Console.WriteLine(e.ToString());
    //        }
    //    }


    //    public static int Main(String[] args)
    //    {
    //        StartListening();
    //        return 0;
    //    }
    //}

    //Class to handle each client request separatly
