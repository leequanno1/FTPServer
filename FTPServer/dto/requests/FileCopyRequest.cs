using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer.dto.requests
{
    internal class FileCopyRequest
    {
        // Cần copy file nào?
        private string _filePath;
        // Copy vào thư mục nào?
        private string _folderPath;

        public string FolderPath { get => _folderPath; set => _folderPath = value; }
        public string FilePath { get => _filePath; set => _filePath = value; }
    }
}
