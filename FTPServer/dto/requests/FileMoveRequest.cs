using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer.dto.requests
{
    public class FileMoveRequest
    {
        private string _filePath;
        private string _fileNewPath;

        public string FilePath { get => _filePath; set => _filePath = value; }
        public string FileNewPath { get => _fileNewPath; set => _fileNewPath = value; }
    }
}
