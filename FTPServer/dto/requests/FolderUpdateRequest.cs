using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer.dto.requests
{
    internal class FolderUpdateRequest
    {
        private string _folderPath;
        private string _folderName;
        public string FolderPath { get => _folderPath; set => _folderPath = value; }
        public string FolderName { get => _folderName; set => _folderName = value; }
    }
}
