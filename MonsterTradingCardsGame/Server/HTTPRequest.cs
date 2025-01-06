using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Enums;

namespace MonsterTradingCardsGame.Server
{
    internal struct HTTPRequest(
        HTTPMethod method,
        string httpVersion,
        string path,
        string content,
        Dictionary<string, string> queryParameters,
        Dictionary<string, string> headers)
    {
        // The HTTP method (e.g., GET, POST, PUT, DELETE)
        public HTTPMethod Method { get; private set; } = method;

        // The HTTP version (e.g., HTTP/1.1)
        public string HTTPVersion { get; private set; } = httpVersion;

        // The path of the requested resource (e.g., /users)
        public string Path { get; private set; } = path;

        // The content of the request (e.g., JSON payload)
        public string Content { get; private set; } = content;

        // The headers of the request (e.g., Content-Type, Authorization)
        public Dictionary<string, string> Headers { get; set; } = headers;

        // The query parameters of the request (e.g., {\"Username\":\"kienboec\", \"Password\":\"daniel\"})
        public readonly Dictionary<string, string> QueryParameters = queryParameters;

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"Method: {Method}");
            sb.AppendLine($"HTTPVersion: {HTTPVersion}");
            sb.AppendLine($"Path: {Path}");
            sb.AppendLine($"Content: {Content}");
            sb.AppendLine("Headers:");
            foreach (var header in Headers)
            {
                sb.AppendLine($"\t{header.Key}: {header.Value}");
            }
            sb.AppendLine("QueryParameters:");
            foreach (var queryParameter in QueryParameters)
            {
                sb.AppendLine($"\t{queryParameter.Key}: {queryParameter.Value}");
            }
            return sb.ToString();
        }
    }
}
