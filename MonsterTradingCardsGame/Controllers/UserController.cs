using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Database;
using MonsterTradingCardsGame.Models;
using Newtonsoft.Json;

namespace MonsterTradingCardsGame.Controllers
{
    // business logic
    internal class UserController(UserService _userService)
    {
        private readonly UserService _userService = _userService;
        public async Task<string> GetUserAsync(HTTPRequest request)
        {
            var user = await _userService.GetUserAsync(request.QueryParameters["Username"]);
            return "asdfasdf";
        }

        public async Task<string> RegisterUserAsync(HTTPRequest request)
        {
            var user = JsonConvert.DeserializeObject<User>(request.Content);
            await _userService.RegisterUserAsync(user);
            return "asdfasdf";
        }
    }
}
