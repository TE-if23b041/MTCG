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
            databaseManager.InitializeDatabase();
            services.AddSingleton<DatabaseManager>(databaseManager);
            
            services.AddSingleton<UserController>();
            services.AddSingleton<CardController>();
            services.AddSingleton<BattleController>();

            services.AddSingleton<CardService>();
            services.AddSingleton<UserService>();

            var serviceProvider = services.BuildServiceProvider();
            _router = new Router(serviceProvider);
            RegisterRoute(_router);

        }

        public static void RegisterRoute(Router router)
        {
            // users
            //router.RegisterRoute<UserController>("/users", HTTPMethod.GET, (controller, request) => controller.GetUserAsync(request));
            router.RegisterRoute<UserController>("/users", HTTPMethod.POST, (controller, request) => controller.RegisterUserAsync(request));

            // session
            router.RegisterRoute<UserController>("/sessions", HTTPMethod.POST, (controller, request) => controller.LoginUserAsync(request));

            // packages
            router.RegisterRoute<CardController>("/packages", HTTPMethod.POST, (controller, request) => controller.CreatePackageAsync(request));
            router.RegisterRoute<CardController>("/transactions/packages", HTTPMethod.POST, (controller, request) => controller.AcquirePackageAsync(request));

            // cards
            router.RegisterRoute<CardController>("/cards", HTTPMethod.GET, (controller, request) => controller.GetCardsAsync(request));

            // deck 
            router.RegisterRoute<CardController>("/deck", HTTPMethod.GET, (controller, request) => controller.GetDeckAsync(request));
            router.RegisterRoute<CardController>("/deck", HTTPMethod.PUT, (controller, request) => controller.ConfigureDeckAsync(request));
            router.RegisterRoute<CardController>("/deck", HTTPMethod.POST, (controller, request) => controller.ConfigureDeckAsync(request));

            // configure user
            router.RegisterRoute<UserController>("/users/.*", HTTPMethod.PUT, (controller, request) => controller.UpdateUserAsync(request));
            router.RegisterRoute<UserController>("/users/.*", HTTPMethod.GET, (controller, request) => controller.GetUserAsync(request));

            // stats
            router.RegisterRoute<UserController>("/stats", HTTPMethod.GET, (controller, request) => controller.GetUserStatsAsync(request));

            // scoreboard
            router.RegisterRoute<UserController>("/scoreboard", HTTPMethod.GET, (controller, request) => controller.GetScoreboardAsync(request));

            // battle
            router.RegisterRoute<BattleController>("/battles", HTTPMethod.POST, (controller, request) => controller.BattleAsync(request));
        }

        public async Task Start()
        {
            _listener.Start();
            while (true) 
            {
                var client = _listener.AcceptTcpClient();
                var requestHandler = new RequestHandler(_router);
                using var networkStream = client.GetStream();
                await requestHandler.HandleRequestAsync(networkStream);
            }
        }

        

    }

}
