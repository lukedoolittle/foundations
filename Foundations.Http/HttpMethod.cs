using Foundations.Attributes;

namespace Foundations.Http
{
    public enum HttpMethodEnum
    {
        [Description("GET")]
        Get,
        [Description("HEAD")]
        Head,
        [Description("POST")]
        Post,
        [Description("PUT")]
        Put,
        [Description("DELETE")]
        Delete,
        [Description("CONNECT")]
        Connect,
        [Description("OPTIONS")]
        Options,
        [Description("TRACE")]
        Trace
    }
}
