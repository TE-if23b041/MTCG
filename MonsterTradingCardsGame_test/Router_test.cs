using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Controllers;
using MonsterTradingCardsGame.Database;

namespace MonsterTradingCardsGame_test.Server
{
    [TestFixture]
    public class RouterTests
    {
        // Tests if the correct route handler is invoked for a valid route and method.
        [Test]
        public async Task Router_RouteAsync_ShouldInvokeCorrectHandler()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var databaseManagerMock = new Mock<DatabaseManager>("MockConnectionString");

            var userServiceMock = new Mock<UserService>(databaseManagerMock.Object);
            var userController = new UserController(userServiceMock.Object);

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(UserController)))
                .Returns(userController);

            var router = new Router(mockServiceProvider.Object);

            router.RegisterRoute<UserController>("/users", HTTPMethod.GET, (controller, request) => Task.FromResult("User fetched"));

            var request = new HTTPRequest(
                HTTPMethod.GET,
                "HTTP/1.1",
                "/users",
                string.Empty,
                new Dictionary<string, string>(),
                new Dictionary<string, string>()
            );

            var response = await router.RouteAsync(request);

            Assert.That(response, Is.EqualTo("User fetched"));
        }

        // Tests if a 404 Not Found response is returned for an invalid route.
        [Test]
        public async Task Router_RouteAsync_ShouldReturn404NotFoundForInvalidRoute()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var router = new Router(mockServiceProvider.Object);

            var request = new HTTPRequest(
                HTTPMethod.GET,
                "HTTP/1.1",
                "/invalid-route",
                string.Empty,
                new Dictionary<string, string>(),
                new Dictionary<string, string>()
            );

            var response = await router.RouteAsync(request);

            Assert.That(response, Contains.Substring("404 Not Found"));
        }

        // Tests if a 404 Not Found response is returned for an invalid method on a valid route.
        [Test]
        public async Task Router_RouteAsync_ShouldReturn404NotFoundForInvalidMethod()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var router = new Router(mockServiceProvider.Object);

            router.RegisterRoute<UserController>("/users", HTTPMethod.GET, (controller, request) => Task.FromResult("User fetched"));

            var request = new HTTPRequest(
                HTTPMethod.POST,
                "HTTP/1.1",
                "/users",
                string.Empty,
                new Dictionary<string, string>(),
                new Dictionary<string, string>()
            );

            var response = await router.RouteAsync(request);

            Assert.That(response, Contains.Substring("404 Not Found"));
        }
    }
}
