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
        public static int Main(String[] args)
        {
            bool running = true;
            bool working = false;
            Thread Listener = new Thread(listen);

            while (running)
            {
                Console.Clear();
                Console.WriteLine("Chatserver v1.0");
                Console.WriteLine("(C)Stephan Schuster");
                Console.WriteLine("*******************");
                Console.WriteLine("0 - Programmende");
                if(!working)
                    Console.WriteLine("1 - Starte Chatserver");
                else
                    Console.WriteLine("1 - Stoppe Chatserver");
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
            return 0;
        }

        private static void listen(Object obj)
        {
            TcpListener serverSocket = new TcpListener(11000);
            TcpClient clientSocket = default(TcpClient);
            int clNo = 0;
            int requestCount = 0;
            byte[] bytesFrom = new byte[65536];
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            string rCount = null;
            requestCount = 0;

            serverSocket.Start();
            clientSocket = serverSocket.AcceptTcpClient();

            while ((true))
            {
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    Console.WriteLine(" >> " + "From client-" + clNo.ToString() + dataFromClient);

                    rCount = Convert.ToString(requestCount);
                    serverResponse = "Server to clinet(" + clNo + ") " + rCount;
                    sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    networkStream.Flush();
                    Console.WriteLine(" >> " + serverResponse);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                }
            }
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
