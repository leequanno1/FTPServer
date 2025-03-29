using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace lib
{
    public class GlobalRequest
    {
        private string route;
        private string authentToken;
        private Object requestObject;

        public string Route { get => route; set => route = value; }
        public object RequestObject { get => requestObject; set => requestObject = value; }
        public string AuthentToken { get => authentToken; set => authentToken = value; }
    }
}
