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
        public static string CreateResponse(int statusCode, string data, string contentType)
        {
            return $"HTTP/1.1 {statusCode}" +
                $"\r\nContent-Type: {contentType}" +
                $"\r\nContent-Length: {data.Length} " +
                $"\r\n\r\n{data}";
        }

        public static string CreateJSONResponse(int statusCode, object data)
        {
            return CreateResponse(statusCode, JsonConvert.SerializeObject(data), "application/json");
        }

        public static string CreatePlainTextReponse(int statusCode, string data)
        {
            return CreateResponse(statusCode, data, "text/plain");
        }
    }
}
