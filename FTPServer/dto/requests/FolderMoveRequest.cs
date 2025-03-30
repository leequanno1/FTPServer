using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer.dto.requests
{
    internal class FolderMoveRequest
    {
        private string _folderPath;
        private string _folderNewPath;

        public string FolderPath { get => _folderPath; set => _folderPath = value; }
        public string FolderNewPath { get => _folderNewPath; set => _folderNewPath = value; }
    }
}
