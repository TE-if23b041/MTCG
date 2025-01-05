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

        public async Task<string> RegisterUserAsync(HTTPRequest request)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<User>(request.Content);
                await _userService.RegisterUserAsync(user);
                Console.WriteLine("User registered");

                return ResponseBuilder.CreatePlainTextReponse(201, "OK");
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                Console.WriteLine("User already exists");
                return ResponseBuilder.CreatePlainTextReponse(409, "User already exists");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResponseBuilder.CreatePlainTextReponse(500, "Internal Server Error");
            }
        }


        public async Task<string> GetUserAsync(HTTPRequest request)
        { // GET /users/kienboec
            try
            {
                var username = request.Headers["Authorization"].Split(" ")[1].Split("-")[0];
                var user = await _userService.GetUserAsync(username);
                if (user.Username != request.Path.Split("/").Last())
                    throw new InvalidOperationException("Unauthorized");
                return ResponseBuilder.CreateJSONResponse(200, user);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                return ResponseBuilder.CreatePlainTextReponse(401, ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResponseBuilder.CreatePlainTextReponse(500, "Internal server error");
            }

            // /users/kienboec => /users/.*
        }

        public async Task<string> UpdateUserAsync(HTTPRequest request)
        {
            // curl -i -X PUT http://localhost:10001/users/kienboec --header "Content-Type: application/json" --header "Authorization: Bearer kienboec-mtcgToken" -d "{\"Name\": \"Kienboeck\",  \"Bio\": \"me playin...\", \"Image\": \":-)\"}"
            try
            {
                var username = request.Headers["Authorization"].Split(" ")[1].Split("-")[0];
                var user = await _userService.GetUserAsync(username);
                if (user.Username != request.Path.Split("/").Last())
                    throw new InvalidOperationException("Unauthorized");

                var information = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Content) ?? throw new Exception("Invalid input");
                await _userService.UpdateUserAync(user, information);


                return ResponseBuilder.CreatePlainTextReponse(200, "OK");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                return ResponseBuilder.CreatePlainTextReponse(401, ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return ResponseBuilder.CreatePlainTextReponse(500, "Internal server error");
            }
        }

        public async Task<string> GetUserStatsAsync(HTTPRequest request)
        { // /stats no username just token
            try
            {
                var username = request.Headers["Authorization"].Split(" ")[1].Split("-")[0];
                var user = await _userService.GetUserAsync(username);

                return ResponseBuilder.CreateJSONResponse(200, new
                {
                    user.Username,
                    user.Coins,
                    user.Elo
                });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                return ResponseBuilder.CreatePlainTextReponse(401, ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResponseBuilder.CreatePlainTextReponse(500, "Internal server error");
            }
        }

        public async Task<string> GetScoreboardAsync(HTTPRequest request)
        {
            try
            {
                var users = await _userService.GetUsersAsync();
                var scoreboard = users
                    .Where(x => x.Username != "admin") // Filter´to exclude admin
                    .Select(x => new
                {
                    x.Username,
                    x.Elo
                }).OrderByDescending(x => x.Elo).ToList();
                return ResponseBuilder.CreateJSONResponse(200, scoreboard);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResponseBuilder.CreatePlainTextReponse(500, "Internal server error");
            }
        }
    }
}
