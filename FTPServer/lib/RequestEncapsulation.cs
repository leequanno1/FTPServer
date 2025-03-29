using lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace lib
{
    public class RequestEncapsulation
    {
        private GlobalRequest request;
        private Socket clientSocket;
        private Socket serverSocket;

        public GlobalRequest Request { get => request; set => request = value; }
        public Socket ClientSocket { get => clientSocket; set => clientSocket = value; }
        public Socket ServerSocket { get => serverSocket; set => serverSocket = value; }
    }
}
