using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer.dto.requests
{
    internal class SignupResponse
    {
        private int _status;
        private string _message;

        public int Status { get => _status; set => _status = value; }
        public string Message { get => _message; set => _message = value; }
    }
}
