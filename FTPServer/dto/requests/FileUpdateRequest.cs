using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer.dto.requests
{
    internal class FileUpdateRequest
    {
        private string _filePath;
        private string _fileName;

        public string FilePath { get => _filePath; set => _filePath = value; }
        public string FileName { get => _fileName; set => _fileName = value; }
    }
}
