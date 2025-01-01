using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MonsterTradingCardsGame.Controllers;
using MonsterTradingCardsGame.Database;
using MonsterTradingCardsGame.Enums;

namespace MonsterTradingCardsGame.Server
{
    internal class HTTPServer
    {
        private readonly TcpListener _listener;
        private readonly Router _router;

        public HTTPServer(IPAddress ip, int port, string connectionString)
        {
            _listener = new TcpListener(ip, port);

            var services = new ServiceCollection();


            var databaseManager = new DatabaseManager(connectionString);
            services.AddSingleton<DatabaseManager>(databaseManager);

            services.AddSingleton<UserService>();
            services.AddSingleton<UserController>();

            var serviceProvider = services.BuildServiceProvider();
            _router = new Router(serviceProvider);


        }

        public void RegisterRoute(Router router)
        {
            router.RegisterRoute<UserController>("/users", HTTPMethod.GET, (controller, request) => controller.GetUserAsync(request));
            //router.RegisterRoute<UserController>("/users", HTTPMethod.POST, (controller, request) => controller.RegisterUserAsync(request));

        }

        public async Task Start()
        {
            _listener.Start();
            while (true)
            {
                var client = _listener.AcceptTcpClient();
                var requestHandler = new RequestHandler();
                using var networkStream = client.GetStream();
                await requestHandler.HandleRequestAsync(networkStream);
            }
        }



    }

}
