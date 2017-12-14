using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DWARFLib
{
    class EnhancedWebClient : WebClient
    {
        int timeout = 0;

        public EnhancedWebClient(int timeout)
        {
            if (timeout == 0)
                this.timeout = System.Threading.Timeout.Infinite;
            else
                this.timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).KeepAlive = false;
                (request as HttpWebRequest).Timeout = timeout; //(tried different values)
            }
            return request;
        }
    }
}
