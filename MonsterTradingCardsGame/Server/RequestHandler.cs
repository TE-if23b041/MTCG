using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Enums;
using Npgsql.Internal;

namespace MonsterTradingCardsGame.Server
{
    internal class RequestHandler(Router router)
    {
        private readonly Router _router = router;

        public async Task HandleRequestAsync(Stream networkStream)
        {
            // curl -i -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{"Username":"kienboec", "Password":"daniel"}"

            // POST /user/ HTTP/1.1
            // Content-Type: application/json
            // Content-Length: 123
            //
            // { "Username": "kienboec", "Password": daniel }



            using var reader = new StreamReader(networkStream, Encoding.UTF8, leaveOpen: true);
            var httpRequest = await ParseRequestAsync(reader);

            Console.WriteLine(httpRequest);

            var response = await _router.RouteAsync(httpRequest);
            await networkStream.WriteAsync(Encoding.UTF8.GetBytes(response));
        }

        public static async Task<HTTPRequest> ParseRequestAsync(StreamReader reader)
        {
            var request = new StringBuilder();
            string? line;

            while (!string.IsNullOrWhiteSpace(line = await reader.ReadLineAsync()))
                request.AppendLine(line);

            var lines = request.ToString().Split('\n');
            var requestLine = lines[0].Split(' ');
            var method = requestLine[0];
            var pathAndQuery = requestLine[1];
            var httpVersion = requestLine[2];

            var header = new Dictionary<string, string>();
            header = lines.Skip(1).TakeWhile(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Split(": ")).ToDictionary(x => x[0], x => x[1]);

            var path = pathAndQuery;
            var query = new Dictionary<string, string>();

            if (pathAndQuery.Contains('?'))
            {
                var parts = pathAndQuery.Split('?');
                path = parts[0];
                query = parts[1].Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0], x => x[1]);
            }

            var body = string.Empty;

            if (header.TryGetValue("Content-Length", out var contentLengthStr) && int.TryParse(contentLengthStr, out var contentLength) && contentLength > 0)
            {
                char[] buffer = new char[contentLength];
                await reader.ReadAsync(buffer, 0, contentLength);
                body = new string(buffer);
            }

            return new HTTPRequest(method: Enum.Parse<HTTPMethod>(method), httpVersion: httpVersion, path: path, queryParameters: query, headers: header, content: body);
        }
    }
}
