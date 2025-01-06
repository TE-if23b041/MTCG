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
    // Handles business logic for user-related actions
    internal class UserController(UserService userService)
    {
        private readonly UserService _userService = userService; // Dependency injection of UserService

        // Handles user login
        // ": Bearer admin-mtcgToken"
        public async Task<string> LoginUserAsync(HTTPRequest request)
        {
            try
            {
                // Parse credentials from the request body
                // -d "{\"Username\":\"kienboec\", \"Password\":\"daniel\"}"
                var credentials = JsonConvert.DeserializeObject<Credentials>(request.Content) ?? throw new Exception("Invalid input");
                var user = await _userService.GetUserAsync(credentials.Username);

                // Check if the provided password matches the stored password
                if (user.Password != credentials.Password)
                    throw new Exception("Invalid credentials");

                // Return a token if login is successful
                return ResponseBuilder.CreatePlainTextReponse(200, $"{user.Username}-mtcgToken");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return ResponseBuilder.CreatePlainTextReponse(401, "Login failed");
            }
        }

        // Handles user registration
        public virtual async Task<string> RegisterUserAsync(HTTPRequest request)
        {   
            try
            {
                // Parse user details from the request body
                var user = JsonConvert.DeserializeObject<User>(request.Content);
                await _userService.RegisterUserAsync(user); // Register the user
                Console.WriteLine("User registered");

                return ResponseBuilder.CreatePlainTextReponse(201, "OK"); // Return success response
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // Handle duplicate username
            {
                Console.WriteLine("User already exists");
                return ResponseBuilder.CreatePlainTextReponse(409, "User already exists");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResponseBuilder.CreatePlainTextReponse(500, "Internal Server Error");
            }

            // /users/kienboec => /users/.*
        }

        // Retrieves user details
        public async Task<string> GetUserAsync(HTTPRequest request)
        {   // GET /users/kienboec
            try
            {
                // Extract username from the token in the Authorization header
                var username = request.Headers["Authorization"].Split(" ")[1].Split("-")[0];
                var user = await _userService.GetUserAsync(username);

                // Check if the username in the path matches the token's username
                if (user.Username != request.Path.Split("/").Last())
                    throw new InvalidOperationException("Unauthorized");

                return ResponseBuilder.CreateJSONResponse(200, user); // Return user details as JSON
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

        // Updates user information
        public async Task<string> UpdateUserAsync(HTTPRequest request)
        {
            // curl -i -X PUT http://localhost:10001/users/kienboec --header "Content-Type: application/json"
            // --header "Authorization: Bearer kienboec-mtcgToken"
            // -d "{\"Name\": \"Kienboeck\",  \"Bio\": \"me playin...\", \"Image\": \":-)\"}"
            try
            {
                // Extract username from the token in the Authorization header
                var username = request.Headers["Authorization"].Split(" ")[1].Split("-")[0];
                var user = await _userService.GetUserAsync(username);

                // Check if the username in the path matches the token's username
                if (user.Username != request.Path.Split("/").Last())
                    throw new InvalidOperationException("Unauthorized");

                // Parse updated information from the request body
                var information = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Content) ?? throw new Exception("Invalid input");
                await _userService.UpdateUserAync(user, information);

                return ResponseBuilder.CreatePlainTextReponse(200, "OK"); // Return success response
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

        // Retrieves user statistics (ELO, coins, etc.)
        public async Task<string> GetUserStatsAsync(HTTPRequest request)
        {      // /stats no username just token
            try
            {
                // Extract username from the token in the Authorization header
                var username = request.Headers["Authorization"].Split(" ")[1].Split("-")[0];
                var user = await _userService.GetUserAsync(username);

                // Return user statistics as JSON
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

        // Retrieves the scoreboard, sorted by ELO
        public async Task<string> GetScoreboardAsync(HTTPRequest request)
        {
            try
            {
                var users = await _userService.GetUsersAsync();

                // Filter out the admin user and sort by ELO in descending order
                var scoreboard = users
                    .Where(x => x.Username != "admin") // Filter´to exclude admin
                    .Select(x => new
                    {
                        x.Username,
                        x.Elo
                    }).OrderByDescending(x => x.Elo).ToList();

                return ResponseBuilder.CreateJSONResponse(200, scoreboard); // Return the scoreboard as JSON
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResponseBuilder.CreatePlainTextReponse(500, "Internal server error");
            }
        }
    }
}
