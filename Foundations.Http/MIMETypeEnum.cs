using Foundations.Attributes;

namespace Foundations.Http
{
    //TODO: add all mime types
    public enum MimeTypeEnum
    {
        [Description("application/json")]
        Json,
        [Description("text/plain")]
        Text,
        [Description("text/html")]
        Html,
        [Description("text/xml")]
        Xml,
    }
}
