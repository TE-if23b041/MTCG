using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Controllers;
using System;

namespace MonsterTradingCardsGame_test.Server
{
    [TestFixture]
    public class HTTPServerTests
    {
        private const string TestConnectionString = "Host=localhost;Port=5432;Username=mtcg_user;Password=mtcg_password;Database=mtcg_db";

        // Tests if the HTTPServer initializes without throwing an exception.
        [Test]
        public void HTTPServer_Initialization_ShouldNotThrowException()
        {
            TestDelegate act = () => new HTTPServer(IPAddress.Loopback, 8080, TestConnectionString);

            Assert.That(act, Throws.Nothing);
        }

        // Tests if the HTTPServer can start and handle client connections.
        [Test]
        public async Task HTTPServer_Start_ShouldHandleConnections()
        {
            var server = new HTTPServer(IPAddress.Loopback, 8080, TestConnectionString);
            _ = Task.Run(async () => await server.Start()); // Starts the server in a background task

            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, 8080);

            Assert.That(client.Connected, Is.True);
            client.Close();
        }

        // Tests if an invalid connection string causes the HTTPServer to throw an exception.
        [Test]
        public void HTTPServer_InvalidConnectionString_ShouldThrowException()
        {
            string invalidConnectionString = "InvalidConnectionString";

            TestDelegate act = () => new HTTPServer(IPAddress.Loopback, 8080, invalidConnectionString);
            Assert.That(act, Throws.Exception);
        }
    }
}
