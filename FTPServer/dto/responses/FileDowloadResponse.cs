using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer.dto.responses
{
    internal class FileDowloadResponse
    {
        private int _status;
        private string _message;
        private long _fileSize;

        public int Status { get => _status; set => _status = value; }
        public string Message { get => _message; set => _message = value; }
        public long FileSize { get => _fileSize; set => _fileSize = value; }
    }
}
