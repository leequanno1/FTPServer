using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer.dto.requests
{
    internal class FolderAddRequest
    {
        private string _parrentPath;
        private string _folderName;

        public string ParrentPath { get => _parrentPath; set => _parrentPath = value; }
        public string FolderName { get => _folderName; set => _folderName = value; }
    }
}
