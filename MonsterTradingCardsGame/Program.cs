using System;
using System.Net;
using MonsterTradingCardsGame.Server;

namespace MonsterTradingCardsGame
{

    public class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Los gehts");

            var server = new HTTPServer(IPAddress.Any, 10001, BuildConnectionStringFromEnv());
            Console.WriteLine($"Server läuft auf URL: {IPAddress.Any}");

            await server.Start();

        }

        static string BuildConnectionStringFromEnv()
        {
            var host = Environment.GetEnvironmentVariable("DB_HOST");
            var port = Environment.GetEnvironmentVariable("DB_PORT");
            var user = Environment.GetEnvironmentVariable("DB_USER");
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var name = Environment.GetEnvironmentVariable("DB_NAME");
            return $"Host={host};Port={port};Username={user};Password={password};Database={name}";
        }
    }
}
