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

            // Parse the incoming HTTP request
            var httpRequest = await ParseRequestAsync(reader);

            Console.WriteLine(httpRequest);

            // Route the request to the appropriate controller
            var response = await _router.RouteAsync(httpRequest);

            // Write the response back to the client
            await networkStream.WriteAsync(Encoding.UTF8.GetBytes(response));
        }

        public static async Task<HTTPRequest> ParseRequestAsync(StreamReader reader)
        {
            var request = new StringBuilder();
            string? line;

            // Read the HTTP request line by line until an empty line is encountered
            while (!string.IsNullOrWhiteSpace(line = await reader.ReadLineAsync()))
                request.AppendLine(line);

            // Split the request into lines
            var lines = request.ToString().Split('\n');
            var requestLine = lines[0].Split(' '); // The first line contains the method, path, and HTTP version
            var method = requestLine[0]; // HTTP method (e.g., GET, POST)
            var pathAndQuery = requestLine[1]; // Path and query string (e.g., "/users?key=value")
            var httpVersion = requestLine[2]; // HTTP version (e.g., HTTP/1.1)

            // Parse headers into a dictionary
            var header = new Dictionary<string, string>();
            header = lines.Skip(1) // Skip the request line
                          .TakeWhile(x => !string.IsNullOrWhiteSpace(x)) // Read until the first empty line
                          .Select(x => x.Split(": ")) // Split each header into key-value pairs
                          .ToDictionary(x => x[0], x => x[1]);

            // Extract the path and query parameters from the pathAndQuery string
            var path = pathAndQuery;
            var query = new Dictionary<string, string>();
            if (pathAndQuery.Contains('?'))
            {
                var parts = pathAndQuery.Split('?');
                path = parts[0]; // The path without query parameters
                query = parts[1] // Parse the query parameters into a dictionary
                         .Split('&')
                         .Select(x => x.Split('='))
                         .ToDictionary(x => x[0], x => x[1]);
            }

            // Read the body content if Content-Length is specified
            var body = string.Empty;
            if (header.TryGetValue("Content-Length", out var contentLengthStr) &&
                int.TryParse(contentLengthStr, out var contentLength) && contentLength > 0)
            {
                char[] buffer = new char[contentLength];
                await reader.ReadAsync(buffer, 0, contentLength); // Read the body content based on Content-Length
                body = new string(buffer);
            }

            // Create and return an HTTPRequest object
            return new HTTPRequest(
                method: Enum.Parse<HTTPMethod>(method), // Parse the HTTP method into an enum
                httpVersion: httpVersion,
                path: path,
                queryParameters: query,
                headers: header,
                content: body
            );
        }
    }
}

