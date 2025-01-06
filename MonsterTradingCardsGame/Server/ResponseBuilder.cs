using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MonsterTradingCardsGame.Server
{
    internal class ResponseBuilder
    {
        // Creates a generic HTTP response with the given status code, data, and content type
        public static string CreateResponse(int statusCode, string data, string contentType)
        {
            return $"HTTP/1.1 {statusCode}" +
                $"\r\nContent-Type: {contentType}" +
                $"\r\nContent-Length: {data.Length} " +
                $"\r\n\r\n{data}";
        }

        // Creates a JSON-formatted HTTP response
        public static string CreateJSONResponse(int statusCode, object data)
        {
            return CreateResponse(statusCode, JsonConvert.SerializeObject(data), "application/json");
        }

        // Creates a plain text HTTP response
        public static string CreatePlainTextReponse(int statusCode, string data)
        {
            return CreateResponse(statusCode, data, "text/plain");
        }
    }
}
