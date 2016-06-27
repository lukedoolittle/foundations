using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Foundations.Http
{
    public class IncommingMessage
    {
        private readonly HttpListenerRequest _request;

        public Uri Uri => _request.Url;

        public string Url => _request.RawUrl;

        public ILookup<string, string> Query => _request.QueryString.ToLookup();

        public ILookup<string, string> Headers => _request.Headers.ToLookup();

        public IList<Cookie> Cookies => _request.Cookies.ToList();

        public IncommingMessage(HttpListenerRequest request)
        {
            _request = request;
        }
    }
}
