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
    internal class Router(IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly Dictionary<string, Dictionary<HTTPMethod, Func<HTTPRequest, Task<string>>>> _routes = [];

        public void RegisterRoute<TController>(string path, HTTPMethod method, Func<TController, HTTPRequest, Task<string>> handler) where TController : class
        {
            if (!_routes.ContainsKey(path))
                _routes[path] = new Dictionary<HTTPMethod, Func<HTTPRequest, Task<string>>>();

            _routes[path][method] = async (request) =>
            {
                var controller = _serviceProvider.GetService<TController>() ?? throw new Exception("Controller not found");
                return await handler(controller, request);
            };
        }


        public async Task<string> RouteAsync(HTTPRequest request) // /user/.* user schickt: /user/huber
        {
            var matchRoute = _routes.Keys.FirstOrDefault(x => new Regex($"^{x}$").IsMatch(request.Path));
            if (matchRoute != null && _routes[matchRoute].TryGetValue(request.Method, out var handler)) // return UserController.GetUser(request);
                return await handler(request);


            return "HTTP1.1 404 Not Found";
        }

        // POST /users
        // GET /users
    }
}
