using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Foundations.Extensions;

namespace Foundations.Http
{
    public class ServerResponse
    {
        private readonly HttpListenerResponse _response;
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
        private readonly StringBuilder _responseBody = new StringBuilder();

        private int _statusCode;

        private byte[] _responseBuffer => 
            Encoding.UTF8.GetBytes(_responseBody.ToString());

        public ServerResponse(HttpListenerResponse response)
        {
            _response = response;
        }

        public void WriteHead(HttpStatusCode statusCode)
        {
            WriteHead((int) statusCode);
        }

        public void WriteHead(int statusCode)
        {
            _statusCode = statusCode;
        }

        public void WriteHead(
            HttpRequestHeader header, 
            MimeTypeEnum headerContent)
        {
            WriteHead(header, headerContent.EnumToString());
        }

        public void WriteHead(HttpRequestHeader header, string headerContent)
        {
            switch (header)
            {
                case HttpRequestHeader.ContentType: 
                    WriteHead("Content-Type", headerContent);
                    break;
                default:
                    throw new Exception();
            }
        }

        public void WriteHead(
            string headerType, 
            string headerContent)
        {
            WriteHead(new HeaderPair(headerType, headerContent));
        }

        public void WriteHead(params HeaderPair[] headers)
        {
            foreach (var header in headers)
            {
                _headers.Add(header.Key, header.Value);
            }
        }

        public void Write(string data)
        {
            _responseBody.Append(data);
        }

        public void WriteHtml(string fileName)
        {
            WriteHead(HttpStatusCode.OK);
            WriteHead(HttpRequestHeader.ContentType, "text/html");
            //TODO: what if file.readalltext doesnt work???
            Write(File.ReadAllText(fileName));
            End();
        }

        public void End(bool failSilently = true)
        {
            try
            {
                foreach (var header in _headers)
                {
                    _response.AddHeader(header.Key, header.Value);
                }
                _response.StatusCode = _statusCode;
                _response.OutputStream.Write(_responseBuffer, 0, _responseBuffer.Length);
                _response.Close();
            }
            catch (HttpListenerException)
            {
                if (!failSilently)
                {
                    throw;
                }
            }
        }
    }

    public class HeaderPair
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public HeaderPair(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
