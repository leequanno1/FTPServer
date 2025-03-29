using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer.dto.requests
{
    internal class FileAddRequest
    {
        private string _fileName;
        private string _folderPath;
        private long _size;
        private string _ipEndPoint;

        public string FileName { get => _fileName; set => _fileName = value; }
        public string FolderPath { get => _folderPath; set => _folderPath = value; }
        public long Size { get => _size; set => _size = value; }
        public string IpEndPoint { get => _ipEndPoint; set => _ipEndPoint = value; }
    }
}
