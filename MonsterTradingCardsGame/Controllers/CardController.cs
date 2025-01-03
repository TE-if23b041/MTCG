﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Database;
using MonsterTradingCardsGame.Server;
using Newtonsoft.Json;
using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Controllers
{
    internal class CardController(UserService userService, CardService cardService)
    {
        private readonly UserService _userService = userService;
        private readonly CardService _cardService = cardService;

        public async Task<string> CreatePackageAsync(HTTPRequest request)
        {
            try
            {
                // --header "Authorization: Bearer admin-mtcgToken"
                // -d "[{\"Id\":\"b2237eca-0271-43bd-87f6-b22f70d42ca4\", \"Name\":\"WaterGoblin\", \"Damage\": 11.0}, {\"Id\":\"9e8238a4-8a7a-487f-9f7d-a8c97899eb48\", \"Name\":\"Dragon\", \"Damage\": 70.0}, {\"Id\":\"d60...
                var username = request.Headers["Authorization"].Split(" ")[1].Split("-")[0];
                // 0: Bearer 1: admin-mtcgToken
                // 0:admin-1:mtcgToken
                var user = await _userService.GetUserAsync(username);


                if (user.Username != "admin")
                    throw new InvalidOperationException("Unauthorized");

                var package = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(request.Content) ?? throw new Exception("Invalid input");

                var cards = package.Select(x => new Card // List<Card> mit 5 cards
                {
                    Id = x["Id"],
                    Name = x["Name"],
                    Damage = double.Parse(x["Damage"]),
                    ElementType = Card.DetermineElementType(x["Name"]),
                    MonsterType = Card.DetermineMonsterType(x["Name"]),
                    CardType = Card.DetermineCardType(x["Name"])
                });

                await _cardService.InsertPackageAsync(cards.ToList());

                return ResponseBuilder.CreatePlainTextReponse(201, "Package created");
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

        public async Task<string> AcquirePackageAsync(HTTPRequest request)
        {
            try
            {
                // --header "Authorization
                var username = request.Headers["Authorization"].Split(" ")[1].Split("-")[0];
                var user = await _userService.GetUserAsync(username);
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

        public async Task<string> GetCardsAsync(HTTPRequest request)
        {
            try
            {
                var username = request.Headers["Authorization"].Split(" ")[1].Split("-")[0];
                var user = await _userService.GetUserAsync(username);
                var cards = await _cardService.GetCardsAsync(user);

                return ResponseBuilder.CreateJSONResponse(200, cards);
            }
            catch
            {
                return ResponseBuilder.CreatePlainTextReponse(500, "Internal server error");

            }
        }
    }
}