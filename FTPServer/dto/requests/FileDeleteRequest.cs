using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer.dto.requests
{
    internal class FileDeleteRequest
    {
        private string _filePath;

        public string FilePath { get => _filePath; set => _filePath = value; }
    }
}
