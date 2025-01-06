using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Database;
using MonsterTradingCardsGame.Server;
using Newtonsoft.Json;
using MonsterTradingCardsGame.Models;
using Npgsql;

namespace MonsterTradingCardsGame.Controllers
{
    // Controller to handle card-related business logic
    internal class CardController(UserService userService, CardService cardService)
    {
        private readonly UserService _userService = userService; // Dependency injection for user-related operations
        private readonly CardService _cardService = cardService; // Dependency injection for card-related operations

        // Configures a deck for a user
        public virtual async Task<string> ConfigureDeckAsync(HTTPRequest request)
        {   // 2 errors: 1. either wrong cards or 2. invalid input (e.g. 3 cards not 4)
            try
            {
                // Extract username from Authorization header
                // --header "Authorization
                var username = request.Headers["Authorization"].Split(" ")[1].Split("-")[0];
                var user = await _userService.GetUserAsync(username);

                // Parse deck configuration from request content
                var deck = JsonConvert.DeserializeObject<List<string>>(request.Content) ?? throw new Exception("Invalid input");

                // Ensure the deck contains exactly 4 cards
                if (deck.Count != 4)
                    throw new InvalidOperationException("Bad request - Invalid deck size");

                // Configure the user's deck
                await _cardService.ConfigureDeckAsync(user, deck);
                return ResponseBuilder.CreatePlainTextReponse(201, "OK");
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

        // Creates a new package of cards
        public virtual async Task<string> CreatePackageAsync(HTTPRequest request)
        {
            try
            {
                // Validate that the request is made by the admin
                // --header "Authorization: Bearer admin-mtcgToken"
                // -d "[{\"Id\":\"b2237eca-0271-43bd-87f6-b22f70d42ca4\", \"Name\":\"WaterGoblin\", \"Damage\": 11.0}, {\"Id\":\"9e8238a4-8a7a-487f-9f7d-a8c97899eb48\", \"Name\":\"Dragon\", \"Damage\": 70.0}, {\"Id\":\"d60...
                var username = request.Headers["Authorization"].Split(" ")[1].Split("-")[0];
                var user = await _userService.GetUserAsync(username);
                if (user.Username != "admin")
                    throw new InvalidOperationException("Unauthorized");

                // Parse the package of cards from the request content
                var package = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(request.Content) ?? throw new Exception("Invalid input");

                // Create card objects from the package
                var cards = package.Select(x => new Card    // List<Card> mit 5 cards
                {
                    Id = x["Id"],
                    Name = x["Name"],
                    Damage = double.Parse(x["Damage"]),
                    ElementType = Card.DetermineElementType(x["Name"]),
                    MonsterType = Card.DetermineMonsterType(x["Name"]),
                    CardType = Card.DetermineCardType(x["Name"])
                });

                // Insert the package into the database
                await _cardService.InsertPackageAsync(cards.ToList());

                return ResponseBuilder.CreatePlainTextReponse(201, "Package created");
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // Handle database constraint violations (e.g., duplicate card ID)
            {
                Console.WriteLine($"Database constraint violation: {ex.Message}");
                return ResponseBuilder.CreatePlainTextReponse(400, "Bad Request - Card already exists");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
                return ResponseBuilder.CreatePlainTextReponse(401, "Unauthorized");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return ResponseBuilder.CreatePlainTextReponse(500, "Internal server error");
            }
        }

        // Allows a user to acquire a package of cards
        public virtual async Task<string> AcquirePackageAsync(HTTPRequest request)
        {
            try
            {
                // Extract username from Authorization header
                // --header "Authorization
                var username = request.Headers["Authorization"].Split(" ")[1].Split("-")[0];
                var user = await _userService.GetUserAsync(username);

                // Attempt to acquire a package for the user
                await _cardService.AcquriePackageAsync(user);
                return ResponseBuilder.CreatePlainTextReponse(201, "OK");
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

        // Retrieves all cards owned by the user
        public virtual async Task<string> GetCardsAsync(HTTPRequest request)
        {
            try
            {
                // Check if the Authorization header is present
                if (!request.Headers.ContainsKey("Authorization"))
                    throw new InvalidOperationException("Unauthorized");

                // Extract username and fetch the user's cards
                var username = request.Headers["Authorization"].Split(" ")[1].Split("-")[0];
                var user = await _userService.GetUserAsync(username);
                var cards = await _cardService.GetCardsAsync(user);

                return ResponseBuilder.CreateJSONResponse(200, cards);
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

        // Retrieves the user's deck (formatted as JSON or plain text)
        public virtual async Task<string> GetDeckAsync(HTTPRequest request)
        {
            try
            {
                // Extract username and fetch the user's deck
                var username = request.Headers["Authorization"].Split(" ")[1].Split("-")[0];
                var user = await _userService.GetUserAsync(username);
                var deck = await _cardService.GetDeckAsync(user);

                // Check query parameters for the response format
                if (request.QueryParameters.TryGetValue("format", out string? format) && format == "plain")
                {
                    var formattedDeck = deck.Select(card => $"{card.Name} ({card.Damage} Damage, {card.MonsterType}, {card.ElementType})");
                    return ResponseBuilder.CreatePlainTextReponse(200, string.Join("\n", formattedDeck));
                }

                // Default response format is JSON
                return ResponseBuilder.CreateJSONResponse(200, deck);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResponseBuilder.CreatePlainTextReponse(500, "Internal server error");
            }
        }
    }
}
