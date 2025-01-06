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

        // Constructor to initialize the HTTP server
        public HTTPServer(IPAddress ip, int port, string connectionString)
        {
            // Set up the TCP listener on the specified IP address and port
            _listener = new TcpListener(ip, port);

            // Create a service collection for dependency injection
            var services = new ServiceCollection();

            // Initialize the database manager with the provided connection string
            var databaseManager = new DatabaseManager(connectionString);
            databaseManager.InitializeDatabase();
            services.AddSingleton<DatabaseManager>(databaseManager);

            // Register controllers for dependency injection
            services.AddSingleton<UserController>();
            services.AddSingleton<CardController>();
            services.AddSingleton<BattleController>();

            // Register services for dependency injection
            services.AddSingleton<CardService>();
            services.AddSingleton<UserService>();

            // Build the service provider to resolve dependencies
            var serviceProvider = services.BuildServiceProvider();
            _router = new Router(serviceProvider);

            // Register all the routes for the application
            RegisterRoute(_router);
        }

        // Method to register all HTTP routes and map them to specific controller methods
        public static void RegisterRoute(Router router)
        {
            // User routes
            router.RegisterRoute<UserController>("/users", HTTPMethod.POST, (controller, request) => controller.RegisterUserAsync(request));

            // Session routes (e.g., login)
            router.RegisterRoute<UserController>("/sessions", HTTPMethod.POST, (controller, request) => controller.LoginUserAsync(request));

            // Package-related routes
            router.RegisterRoute<CardController>("/packages", HTTPMethod.POST, (controller, request) => controller.CreatePackageAsync(request));
            router.RegisterRoute<CardController>("/transactions/packages", HTTPMethod.POST, (controller, request) => controller.AcquirePackageAsync(request));

            // Card routes
            router.RegisterRoute<CardController>("/cards", HTTPMethod.GET, (controller, request) => controller.GetCardsAsync(request));

            // Deck routes
            router.RegisterRoute<CardController>("/deck", HTTPMethod.GET, (controller, request) => controller.GetDeckAsync(request));
            router.RegisterRoute<CardController>("/deck", HTTPMethod.PUT, (controller, request) => controller.ConfigureDeckAsync(request));
            router.RegisterRoute<CardController>("/deck", HTTPMethod.POST, (controller, request) => controller.ConfigureDeckAsync(request));

            // User configuration routes
            router.RegisterRoute<UserController>("/users/.*", HTTPMethod.PUT, (controller, request) => controller.UpdateUserAsync(request));
            router.RegisterRoute<UserController>("/users/.*", HTTPMethod.GET, (controller, request) => controller.GetUserAsync(request));

            // Stats route
            router.RegisterRoute<UserController>("/stats", HTTPMethod.GET, (controller, request) => controller.GetUserStatsAsync(request));

            // Scoreboard route
            router.RegisterRoute<UserController>("/scoreboard", HTTPMethod.GET, (controller, request) => controller.GetScoreboardAsync(request));

            // Battle route
            router.RegisterRoute<BattleController>("/battles", HTTPMethod.POST, (controller, request) => controller.BattleAsync(request));
        }

        // Method to start the server and handle incoming client requests
        public async Task Start()
        {
            _listener.Start(); // Start the TCP listener
            while (true)
            {
                var client = _listener.AcceptTcpClient(); // Accept an incoming client connection
                var requestHandler = new RequestHandler(_router); // Create a handler for processing requests
                using var networkStream = client.GetStream(); // Get the network stream for the client
                await requestHandler.HandleRequestAsync(networkStream); // Handle the client's request
            }
        }
    }
}
