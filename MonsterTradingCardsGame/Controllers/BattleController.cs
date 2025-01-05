﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Database;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Enums;

namespace MonsterTradingCardsGame.Controllers
{
    internal class BattleController(UserService userService, CardService cardService)
    {
        private readonly UserService _userService = userService;
        private readonly CardService _cardService = cardService;
        private static readonly ConcurrentQueue<(User, List<Card>)> BatlleQueue = new();
        private static readonly SemaphoreSlim BattleSemaphore = new(0);


        public async Task<string> BattleAsync(HTTPRequest request)
        {
            var user = await _userService.GetUserAsync(request.Headers["Authorization"].Split(" ")[1].Split("-")[0]);
            var deck = await _cardService.GetDeckAsync(user);

            if (deck.Count != 4)
                return ResponseBuilder.CreatePlainTextReponse(400, "Invalid deck size");

            BatlleQueue.Enqueue((user, deck));
            Console.WriteLine($"{user.Username} added to battle queue. Queue size: {BatlleQueue.Count}");

            // singnals that the player is ready
            BattleSemaphore.Release();

            // waiting for another player
            await BattleSemaphore.WaitAsync();

            (User, List<Card>) player1 = default;
            (User, List<Card>) player2 = default;

            lock (BatlleQueue)
            {
                if (BatlleQueue.Count >= 2)
                {
                    BatlleQueue.TryDequeue(out player1);
                    BatlleQueue.TryDequeue(out player2);
                }
            }

            // check if both players are available
            if (!player1.Item1.Equals(default(User)) && !player2.Item1.Equals(default(User)))
            {
                var log = StartBattle(player1, player2);
                await UpdatePlayerStats(player1, player2);
                return ResponseBuilder.CreatePlainTextReponse(200, string.Join("\n", log));
            }

            return ResponseBuilder.CreatePlainTextReponse(200, "Waiting for opponent");
        }

        private List<string> StartBattle((User User, List<Card> Cards) player1, (User User, List<Card> Cards) player2)
        {
            var log = new List<string>();
            var rnd = new Random();

            var rounds = 100;
            for (int i = 0; i < rounds; i++)
            {
                if (player1.Cards.Count == 0 || player2.Cards.Count == 0)
                    break;

                var card1 = player1.Cards[rnd.Next(player1.Cards.Count)];
                var card2 = player2.Cards[rnd.Next(player2.Cards.Count)];

                log.Add($"Round {i+1}: {card1.Name} vs {card2.Name}");

                var damage1 = CalculateDamage(card1, card2);
                var damage2 = CalculateDamage(card2, card1);

                log.Add($"Damage: {damage1} vs {damage2}");

                if (damage1 > damage2)
                {
                    player2.Cards.Remove(card2);
                    player1.Cards.Add(card2);
                    log.Add($"{player1.User.Username} wins with {card1.Name}");
                }
                else if (damage1 < damage2)
                {
                    player1.Cards.Remove(card1);
                    player2.Cards.Add(card1);
                    log.Add($"{player2.User.Username} wins with {card2.Name}");
                }
                else
                {
                    log.Add("The Round is a Draw");
                }
            }

            log.Add("Battle finished");
            log.Add($"{player1.User.Username} has {player1.Cards.Count} cards left");
            log.Add($"{player2.User.Username} has {player2.Cards.Count} cards left");
            
            if(player1.Cards.Count == 0)
            {
                log.Add($"{player2.User.Username} wins the battle");
            }
            else if (player2.Cards.Count == 0)
            {
                log.Add($"{player1.User.Username} wins the battle");
            }
            else
            {
                log.Add("-->| The Battle is a Draw |<--");
            }

            return log;
        }

        private static double CalculateDamage(Card attacker, Card defender)
        {
            if (attacker.MonsterType == MonsterType.Goblin && defender.MonsterType == MonsterType.Dragon)
                return 0;
            if (attacker.MonsterType == MonsterType.Ork && defender.MonsterType == MonsterType.Wizard)
                return 0;
            if (attacker.MonsterType == MonsterType.Knight && defender.CardType == CardType.Spell && defender.ElementType == ElementType.Water)
                return 0;
            if (attacker.CardType == CardType.Spell && defender.MonsterType == MonsterType.Kraken)
                return 0;
            if (attacker.MonsterType == MonsterType.Dragon && defender.MonsterType == MonsterType.Elf && defender.ElementType == ElementType.Fire)
                return 0;

            double multiplier = 1.0;

            if (attacker.ElementType == ElementType.Water && defender.ElementType == ElementType.Fire)
                multiplier = 2.0;
            if (attacker.ElementType == ElementType.Fire && defender.ElementType == ElementType.Normal)
                multiplier = 2.0;
            if (attacker.ElementType == ElementType.Normal && defender.ElementType == ElementType.Water)
                multiplier = 2.0;


            if (attacker.ElementType == ElementType.Fire && defender.ElementType == ElementType.Water)
                multiplier = 0.5;
            if (attacker.ElementType == ElementType.Normal && defender.ElementType == ElementType.Fire)
                multiplier = 0.5;
            if (attacker.ElementType == ElementType.Water && defender.ElementType == ElementType.Normal)
                multiplier = 0.5;

            if (attacker.CardType == CardType.Monster && defender.CardType == CardType.Monster)
                multiplier = 1.0;

            return attacker.Damage * multiplier;
        }

        private async Task UpdatePlayerStats((User User, List<Card> Cards) player1, (User User, List<Card> Cards) player2)
        {
            if (player2.Cards.Count == 0)
            {
                player1.User.Elo += 3;
                player2.User.Elo -= 5;
            }
            else if (player1.Cards.Count == 0)
            {
                player2.User.Elo += 3;
                player1.User.Elo -= 5;
            }

            await _userService.UpdateUserAsync(player1.User);
            await _userService.UpdateUserAsync(player2.User);
        }
    }
}
