using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer.dto.requests
{
    internal class FileDownloadRequest
    {
        private string _filePath;
        private string _clientEndpoint;

        public string FilePath { get => _filePath; set => _filePath = value; }
        public string ClientEndpoint { get => _clientEndpoint; set => _clientEndpoint = value; }
    }
}
