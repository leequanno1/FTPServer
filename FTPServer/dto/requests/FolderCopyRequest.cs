using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer.dto.requests
{
    internal class FolderCopyRequest
    {
        // id folder 
        private string _folderPath;
        // copy vào đâu?
        private string _destinationPath;

        public string DestinationPath { get => _destinationPath; set => _destinationPath = value; }
        public string FolderPath { get => _folderPath; set => _folderPath = value; }
    }
}
