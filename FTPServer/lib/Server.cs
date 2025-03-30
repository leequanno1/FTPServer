using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace lib
{
    public class Server
    {
        public delegate void Controller(Socket socket, GlobalRequest dto);
        private static Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static Socket fileServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static Dictionary<string, Controller> route;
        private static Dictionary<string, Socket> clientFileTranferSockets = new Dictionary<string, Socket>();
        private static object locker = new object();

        public static void Run(IPEndPoint requestEp, IPEndPoint fileEp, Dictionary<string, Controller> appRoute)
        {
            // mở kết nối
            route = appRoute;
            server.Bind(requestEp);
            server.Listen(1000);
            fileServerSocket.Bind(fileEp);
            fileServerSocket.Listen(1000);
            Console.WriteLine($"Server running on port {requestEp.Port}");
            // accept message request flow
            server.BeginAccept(new AsyncCallback(HandleConnection), null);
            // accept file tranfer flow
            fileServerSocket.BeginAccept(new AsyncCallback(HandleFileConnection), null);
            while (true)
            {
                if (Console.ReadLine().ToLower().Trim() == "exit") break;
            }
            server.Close();
            fileServerSocket.Close();
        }

        public static Socket GetFileTranferClientSocket(string ipEndpoint)
        {
            Socket socket = null;
            lock(locker)
            {
                socket = clientFileTranferSockets[ipEndpoint];
            }
            return socket;
        }

        public static void RemoveSocket(string ipEnpoint)
        {
            lock(locker)
            {
                if(clientFileTranferSockets.ContainsKey(ipEnpoint)) clientFileTranferSockets.Remove(ipEnpoint);
            }
        }

        static void HandleConnection(IAsyncResult ar)
        {
            // mở luồn xử lý client
            Socket client = server.EndAccept(ar);
            Thread thread = new Thread(ClientHandler);
            thread.IsBackground = true;
            thread.Start(client);
            // đón kết nối mới
            server.BeginAccept(new AsyncCallback(HandleConnection), null);
        }

        static void HandleFileConnection(IAsyncResult ar)
        {
            // nhận kết nối và thêm vào dictionary với key là IPEndpoint
            Socket clientSocket = server.EndAccept(ar);
            fileServerSocket.BeginAccept(new AsyncCallback(HandleFileConnection), null);
            lock (locker)
            {
                clientFileTranferSockets.Add(clientSocket.RemoteEndPoint.ToString(), clientSocket);
            }
        }

        static void ClientHandler(object clientObject)
        {
            Socket clientSocket = clientObject as Socket;
            Console.WriteLine($"{clientSocket.RemoteEndPoint} connected.");
            try
            {
                GlobalRequest request;
                while (true)
                {
                    TcpProtocol.Receive<GlobalRequest>(clientSocket, out request);
                    // mở luồn xử lý
                    Thread responseHandler = new Thread(ResponseHandler);
                    responseHandler.IsBackground = true;
                    responseHandler.Start(new RequestEncapsulation { Request = request, ClientSocket = clientSocket });
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"{clientSocket.RemoteEndPoint} disconnected!!!");
            }
        }

        static void ResponseHandler(object pairOb)
        {
            // phân giải request và chuyển đến controller chính xác.
            RequestEncapsulation requestEncapsulation = pairOb as RequestEncapsulation;
            Controller controller;
            if (route.TryGetValue(requestEncapsulation.Request.Route, out controller))
            {
                try
                {
                    Console.WriteLine($"Request to: {requestEncapsulation.Request.Route} at {DateTime.Now.ToString()}");
                    controller(requestEncapsulation.ClientSocket, requestEncapsulation.Request);
                } catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                Console.WriteLine($"Route '{requestEncapsulation.Request.Route}' doesnt existed.");
            }
        }
    }
}