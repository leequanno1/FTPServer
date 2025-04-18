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
        private string _folderId;
        // copy vào đâu?
        private string _destinationPath;

        public string FolderId { get => _folderId; set => _folderId = value; }
        public string DestinationPath { get => _destinationPath; set => _destinationPath = value; }
    }
}
