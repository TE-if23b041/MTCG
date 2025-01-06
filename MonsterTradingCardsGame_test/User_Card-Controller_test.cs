using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using MonsterTradingCardsGame.Controllers;
using MonsterTradingCardsGame.Database;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Enums;

namespace MonsterTradingCardsGame_test.Controllers
{
    [TestFixture]
    public class User_CardController_test
    {
        private CardController _cardController;
        private UserController _userController;
        private Mock<UserService> _mockUserService;
        private FakeCardService _fakeCardService;

        [SetUp]
        public void SetUp()
        {
            _mockUserService = new Mock<UserService>(MockBehavior.Strict, new DatabaseManager(""));
            _fakeCardService = new FakeCardService(new DatabaseManager(""));
            _cardController = new CardController(_mockUserService.Object, _fakeCardService);
            _userController = new UserController(_mockUserService.Object);
        }

        // Ensures the deck is configured correctly and returns HTTP 201.
        [Test]
        public async Task ConfigureDeckAsync_ShouldReturn201_WhenValidDeckIsProvided()
        {
            var user = new User { Username = "testuser", Id = 1 };
            var request = new HTTPRequest(
                HTTPMethod.PUT,
                "HTTP/1.1",
                "/deck",
                JsonConvert.SerializeObject(new List<string> { "card1", "card2", "card3", "card4" }),
                new Dictionary<string, string>(),
                new Dictionary<string, string> { { "Authorization", "Bearer testuser-mtcgToken" } }
            );

            _mockUserService.Setup(s => s.GetUserAsync("testuser")).ReturnsAsync(user);

            var response = await _cardController.ConfigureDeckAsync(request);

            Assert.That(response, Is.EqualTo("HTTP/1.1 201\r\nContent-Type: text/plain\r\nContent-Length: 2 \r\n\r\nOK"));
        }

        // Verifies an admin can successfully create a package and returns HTTP 201.
        [Test]
        public async Task CreatePackageAsync_ShouldReturn201_WhenAdminCreatesPackage()
        {
            var adminUser = new User { Username = "admin" };
            var packageContent = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> { { "Id", "card1" }, { "Name", "WaterGoblin" }, { "Damage", "10.0" } },
                new Dictionary<string, string> { { "Id", "card2" }, { "Name", "FireSpell" }, { "Damage", "20.0" } },
                new Dictionary<string, string> { { "Id", "card3" }, { "Name", "Dragon" }, { "Damage", "50.0" } },
                new Dictionary<string, string> { { "Id", "card4" }, { "Name", "RegularSpell" }, { "Damage", "40.0" } }
            };

            var request = new HTTPRequest(
                HTTPMethod.POST,
                "HTTP/1.1",
                "/packages",
                JsonConvert.SerializeObject(packageContent),
                new Dictionary<string, string>(),
                new Dictionary<string, string> { { "Authorization", "Bearer admin-mtcgToken" } }
            );

            _mockUserService.Setup(s => s.GetUserAsync("admin")).ReturnsAsync(adminUser);

            var response = await _cardController.CreatePackageAsync(request);

            Assert.That(response, Is.EqualTo("HTTP/1.1 201\r\nContent-Type: text/plain\r\nContent-Length: 15 \r\n\r\nPackage created"));
        }

        // Tests if a user can successfully acquire a package and returns HTTP 201.
        [Test]
        public async Task AcquirePackageAsync_ShouldReturn201_WhenPackageIsAvailable()
        {
            var user = new User { Username = "testuser", Id = 1 };
            var request = new HTTPRequest(
                HTTPMethod.POST,
                "HTTP/1.1",
                "/transactions/packages",
                "",
                new Dictionary<string, string>(),
                new Dictionary<string, string> { { "Authorization", "Bearer testuser-mtcgToken" } }
            );

            _mockUserService.Setup(s => s.GetUserAsync("testuser")).ReturnsAsync(user);

            var response = await _cardController.AcquirePackageAsync(request);

            Assert.That(response, Is.EqualTo("HTTP/1.1 201\r\nContent-Type: text/plain\r\nContent-Length: 2 \r\n\r\nOK"));
        }

        // Ensures a new user registration returns HTTP 201.
        [Test]
        public async Task RegisterUser_ShouldReturn201_WhenNewUserIsCreated()
        {
            var user = new User { Username = "testuser", Password = "password" };
            var request = new HTTPRequest(
                HTTPMethod.POST,
                "HTTP/1.1",
                "/users",
                JsonConvert.SerializeObject(user),
                new Dictionary<string, string>(),
                new Dictionary<string, string>()
            );

            _mockUserService.Setup(s => s.RegisterUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var response = await _userController.RegisterUserAsync(request);

            Assert.That(response, Is.EqualTo("HTTP/1.1 201\r\nContent-Type: text/plain\r\nContent-Length: 2 \r\n\r\nOK"));
        }

        // Checks if a user can successfully log in and receive a valid token.
        [Test]
        public async Task LoginUser_ShouldReturn200_WhenCredentialsAreValid()
        {
            var user = new User { Username = "testuser", Password = "password" };
            var credentials = new Credentials { Username = "testuser", Password = "password" };
            var request = new HTTPRequest(
                HTTPMethod.POST,
                "HTTP/1.1",
                "/sessions",
                JsonConvert.SerializeObject(credentials),
                new Dictionary<string, string>(),
                new Dictionary<string, string>()
            );

            _mockUserService.Setup(s => s.GetUserAsync("testuser")).ReturnsAsync(user);

            var response = await _userController.LoginUserAsync(request);

            Assert.That(response, Does.Contain("testuser-mtcgToken"));
        }

        // Validates user stats retrieval and ensures correct stats are returned.
        [Test]
        public async Task GetUserStats_ShouldReturn200_WithCorrectStats()
        {
            var user = new User { Username = "testuser", Coins = 10, Elo = 100 };
            var request = new HTTPRequest(
                HTTPMethod.GET,
                "HTTP/1.1",
                "/stats",
                "",
                new Dictionary<string, string>(),
                new Dictionary<string, string> { { "Authorization", "Bearer testuser-mtcgToken" } }
            );

            _mockUserService.Setup(s => s.GetUserAsync("testuser")).ReturnsAsync(user);

            var response = await _userController.GetUserStatsAsync(request);

            Assert.That(response, Does.Contain("testuser"));
            Assert.That(response, Does.Contain("10"));
            Assert.That(response, Does.Contain("100"));
        }

        internal class FakeCardService : CardService
        {
            public FakeCardService(DatabaseManager databaseManager) : base(databaseManager) { }

            public override async Task AcquriePackageAsync(User user)
            {
                await Task.CompletedTask;
            }

            public override async Task ConfigureDeckAsync(User user, List<string> cards)
            {
                await Task.CompletedTask;
            }

            public override async Task InsertPackageAsync(List<Card> package)
            {
                await Task.CompletedTask;
            }
        }
    }
}
