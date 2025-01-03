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
using Npgsql;

namespace MonsterTradingCardsGame.Controllers
{
    // business logic
    internal class UserController(UserService userService)
    {
        private readonly UserService _userService = userService;

        // login, ": Bearer admin-mtcgToken"
        public async Task<string> LoginUserAsync(HTTPRequest request)
        {
            try
            {
                // -d "{\"Username\":\"kienboec\", \"Password\":\"daniel\"}"
                var credentials = JsonConvert.DeserializeObject<Credentials>(request.Content) ?? throw new Exception("Invalid input");
                var user = await _userService.GetUserAsync(credentials.Username);

                if (user.Password != credentials.Password)
                    throw new Exception("Invalid credentials");

                return ResponseBuilder.CreatePlainTextReponse(200, $"{user.Username}-mtcgToken");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return ResponseBuilder.CreatePlainTextReponse(401, "Login failed");
            }
        }


        public async Task<string> GetUserAsync(HTTPRequest request)
        {
           var user = await _userService.GetUserAsync(request.QueryParameters["Username"]);
           return ResponseBuilder.CreateJSONResponse(200, user);
        }

        public async Task<string> RegisterUserAsync(HTTPRequest request)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<User>(request.Content);
                await _userService.RegisterUserAsync(user);
                Console.WriteLine("User registered");

                return ResponseBuilder.CreatePlainTextReponse(201, "OK");
            }
            catch(PostgresException ex) when (ex.SqlState == "23505")
            {
                Console.WriteLine("User already exists");
                return ResponseBuilder.CreatePlainTextReponse(409, "User already exists");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return ResponseBuilder.CreatePlainTextReponse(500, "Internal Server Error");
            }
        }
    }
}
