using System.Net;
using MonsterTradingCardsGame.Server;

namespace MonsterTradingCardsGame;

class Program
{
    static async void Main(string[] args)
    {
        Console.WriteLine("Los gehts");

        var server = new HTTPServer(IPAddress.Any, 10001);

        
        await server.Start();
        Console.WriteLine($"Server läuft auf URL: {IPAddress.Any} ...  zum beenden, beliebige Taste drücken");
        Console.ReadKey();
        server.Stop();

        Console.ReadKey();
    }
}



