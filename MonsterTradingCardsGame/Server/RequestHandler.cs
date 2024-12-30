using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Server
{
    internal class RequestHandler
    {
        public async Task HandleRequestAsync(Stream networkStream)
        {

            using var reader = new StreamReader(networkStream, Encoding.UTF8, leaveOpen: true);

            // 1.1 first line in HTTP contains the method, path and HTTP version
            string? line = reader.ReadLine();

            Console.WriteLine(line);

            string[]? firstLineParts = line?.Split(' ');
            Method = (HTTPMethod)Enum.Parse(typeof(HTTPMethod), firstLineParts?[0] ?? "GET");
            string[] pathAndQuery = firstLineParts?[1].Split('?') ?? Array.Empty<string>();
            Path = pathAndQuery[0].Split('/', StringSplitOptions.RemoveEmptyEntries);


            if (Path.Length == 2 && Path[1] == "tradings")
            {
                Console.WriteLine();
            }

            if (pathAndQuery.Length > 1)
            {
                string[] queryParams = pathAndQuery[1].Split('&');
                foreach (var queryParam in queryParams)
                {
                    string[] queryParamParts = queryParam.Split('=');

                    if (queryParamParts.Length >= 1)
                        QueryParameters[queryParamParts[0]] = (queryParamParts.Length == 2) ? queryParamParts[1] : "";
                }
            }
            HttpVersion = firstLineParts?[2] ?? "";

            // 1.2 read the HTTP-headers (in HTTP after the first line, until the empy line)
            int contentLength = 0; // we need the content_length later, to be able to read the HTTP-content
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(line);
                if (line == "")
                    break;  // empty line indicates the end of the HTTP-headers

                // Parse the header
                string[] parts = line.Split(':');
                if (parts.Length == 2)
                {
                    Headers[parts[0]] = parts[1].Trim();
                    if (parts[0] == "Content-Length")
                    {
                        contentLength = int.Parse(parts[1].Trim());
                    }
                }
            }

            // 1.3 read the body if existing
            if (contentLength > 0 && Headers.ContainsKey("Content-Type"))
            {
                var data = new StringBuilder(200);
                char[] chars = new char[1024];
                int bytesReadTotal = 0;

                while (bytesReadTotal < contentLength)
                {
                    try
                    {
                        var bytesRead = reader.Read(chars, 0, 1024);
                        bytesReadTotal += bytesRead;
                        if (bytesRead == 0) break;
                        data.Append(chars, 0, bytesRead);
                    }
                    // IOException can occur when there is a mismatch of the 'Content-Length'
                    // because a different encoding is used
                    // Sending a 'plain/text' payload with special characters (äüö...) is
                    // an example of this
                    catch (IOException) { break; }
                    catch (Exception) { break; }
                }
                Content = data.ToString();
                Console.WriteLine(data.ToString());
            }
        }
    }

}
