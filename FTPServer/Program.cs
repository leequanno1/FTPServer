using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using lib;

namespace FTPServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // route
            Dictionary<string, Server.Controller> route = new Dictionary<string, Server.Controller>();
            route.Add("/login", Controller.LoginController);
            route.Add("/signup", Controller.SignUpController);
            route.Add("/list", Controller.ListController);
            route.Add("/folder-add", Controller.AddFolder);
            route.Add("/folder-update", Controller.UpdateFolderName);
            route.Add("/folder-delete", Controller.DeleteFolder);
            route.Add("/file-add", Controller.AddFile);
            route.Add("/file-update", Controller.UpdateFileName);
            route.Add("/file-delete", Controller.DeleteFile);
            route.Add("/file-download", Controller.DownloadFile);

            Server.Run(new IPEndPoint(IPAddress.Loopback, 9000), new IPEndPoint(IPAddress.Loopback, 9001), route);
        }
    }
}
