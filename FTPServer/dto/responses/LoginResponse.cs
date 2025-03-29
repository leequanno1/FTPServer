using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer.dto.responses
{
    internal class LoginResponse
    {
        private string _token;

        public string Token { get => _token; set => _token = value; }
    }
}
