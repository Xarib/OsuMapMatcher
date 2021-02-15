using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OMM.Desktop
{
    public static class ClientExtension
    {
        public static void Init(this HttpClient client)
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("app");
        }
    }
}
