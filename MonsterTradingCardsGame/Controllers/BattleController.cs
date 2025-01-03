using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Enums;

namespace MonsterTradingCardsGame.Controllers
{
    internal class BattleController
    {
        private const int MaxRounds = 100;

        // Lists or only Cards?
        public async Task<string> StartBattleAsync(List<Card> deck1, List<Card> deck2)
        {
            try
            {
                int round = 0;

                while (deck1.Count > 0 && deck2.Count > 0 && round < MaxRounds)
                {
                    round++;
                    var card1 = SelectRandomCard(deck1);
                    var card2 = SelectRandomCard(deck2);

                    double damage1 = CalculateDamage(card1, card2);
                    double damage2 = CalculateDamage(card2, card1);

                    if (damage1 > damage2)
                    {
                        deck1.Add(card2);
                        deck2.Remove(card2);
                    }
                    else if (damage2 > damage1)
                    {
                        deck2.Add(card1);
                        deck1.Remove(card1);
                    }

                }

                if (deck1.Count > deck2.Count)
                    return ResponseBuilder.CreatePlainTextReponse(200, "Player 1 wins the battle");

                else if (deck2.Count > deck1.Count)
                    return ResponseBuilder.CreatePlainTextReponse(200, "Player 2 wins the battle");

                else
                    return ResponseBuilder.CreatePlainTextReponse(200, "It's a draw");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResponseBuilder.CreatePlainTextReponse(500, "Internal server error");
            }
        }

        private Card SelectRandomCard(List<Card> deck)
        {
            Random random = new Random();
            int index = random.Next(deck.Count);
            return deck[index];
        }

        private double CalculateDamage(Card attacker, Card defender)
        {

            if (SpecialtyCheck(attacker, defender))
                return double.MaxValue;

            if (attacker.CardType == CardType.Spell || defender.CardType == CardType.Spell)
            {
                double multiplier = GetElementMultiplier(attacker.ElementType, defender.ElementType);
                return attacker.Damage * multiplier;
            }

            // monster vs monster - ignore element type
            return attacker.Damage;
        }

        private bool SpecialtyCheck(Card attacker, Card defender)
        {
            if (attacker.MonsterType == MonsterType.Goblin && defender.MonsterType == MonsterType.Dragon)
                return true;

            if (attacker.MonsterType == MonsterType.Wizard && defender.MonsterType == MonsterType.Ork)
                return true;

            if (attacker.MonsterType == MonsterType.Knight && defender.ElementType == ElementType.Water)
                return true;

            if (attacker.MonsterType == MonsterType.Kraken && defender.CardType == CardType.Spell)
                return true;

            if (attacker.MonsterType == MonsterType.Elf && defender.MonsterType == MonsterType.Dragon)
                return false;

            return false;
        }

        private double GetElementMultiplier(ElementType attacker, ElementType defender)
        {
            if (attacker == ElementType.Water && defender == ElementType.Fire)
                return 2.0;

            if (attacker == ElementType.Fire && defender == ElementType.Normal)
                return 2.0;

            if (attacker == ElementType.Normal && defender == ElementType.Water)
                return 2.0;

            if (defender == ElementType.Water && attacker == ElementType.Fire)
                return 0.5;

            if (defender == ElementType.Fire && attacker == ElementType.Normal)
                return 0.5;

            if (defender == ElementType.Normal && attacker == ElementType.Water)
                return 0.5;

            return 1.0;
        }
    }
}
