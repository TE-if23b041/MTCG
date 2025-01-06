using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MonsterTradingCardsGame.Enums;

namespace MonsterTradingCardsGame.Server
{
    // The Router class handles HTTP routing by mapping paths and methods to specific controller methods.
    internal class Router(IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider; // Dependency injection service provider
        private readonly Dictionary<string, Dictionary<HTTPMethod, Func<HTTPRequest, Task<string>>>> _routes = []; // Route definitions

        // Registers a route for a specific path, HTTP method, and handler
        public void RegisterRoute<TController>(string path, HTTPMethod method, Func<TController, HTTPRequest, Task<string>> handler) where TController : class
        {
            // If the path is not already registered, initialize it
            if (!_routes.ContainsKey(path))
                _routes[path] = [];

            // Assign the handler for the specific HTTP method
            _routes[path][method] = async (request) =>
            {
                // Retrieve the controller instance from the service provider
                var controller = _serviceProvider.GetService<TController>() ?? throw new Exception("Controller not found");
                // Execute the handler and return the response
                return await handler(controller, request);
            };
        }

        // Routes an incoming HTTP request to the appropriate handler
        public async Task<string> RouteAsync(HTTPRequest request) // e.g., /user/.* where the user sends: /user/huber
        {
            // Find a route that matches the request path using regular expressions
            var matchRoute = _routes.Keys.FirstOrDefault(x => new Regex($"^{x}$").IsMatch(request.Path));

            // If a matching route and handler for the HTTP method are found, execute the handler
            if (matchRoute != null && _routes[matchRoute].TryGetValue(request.Method, out var handler))
                return await handler(request);

            // If no matching route is found, return a 404 Not Found response
            Console.WriteLine($"{request.Path} {request.Method}: 404 Not Found");
            return "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\nContent-Length: 9\r\n\r\nNot Found";
        }

        // Example routes:
        // POST /users
        // GET /users
    }
}
