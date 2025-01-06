using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using MonsterTradingCardsGame.Controllers;
using MonsterTradingCardsGame.Database;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Enums;

namespace MonsterTradingCardsGame_test.Controllers
{
    [TestFixture]
    public class BattleControllerTests
    {
        // Tests if damage calculation works with specific weather and element modifiers (case 1).
        [Test]
        public void CalculateDamage_ShouldReturnCorrectDamage_WithWeatherAndElementModifiers1()
        {
            var attacker = new Card { Name = "FireSpell", Damage = 30, ElementType = ElementType.Fire, CardType = CardType.Spell };
            var defender = new Card { Name = "WaterMonster", Damage = 50, ElementType = ElementType.Water, CardType = CardType.Monster };
            var weather = WeatherType.Rain;

            var damage = BattleController.CalculateDamage(attacker, defender, weather);

            Assert.That(damage, Is.EqualTo(30 * 0.5 * 1));
        }

        // Tests if damage calculation works with specific weather and element modifiers (case 2).
        [Test]
        public void CalculateDamage_ShouldReturnCorrectDamage_WithWeatherAndElementModifiers2()
        {
            var attacker = new Card { Name = "WaterSpell", Damage = 50, ElementType = ElementType.Water, CardType = CardType.Spell };
            var defender = new Card { Name = "FireMonster", Damage = 30, ElementType = ElementType.Fire, CardType = CardType.Monster };
            var weather = WeatherType.Rain;

            var damage = BattleController.CalculateDamage(attacker, defender, weather);

            Assert.That(damage, Is.EqualTo(50 * 2.0 * 2));
        }

        // Tests if the battle declares a winner when one player has no cards left.
        [Test]
        public void StartBattle_ShouldDeclareWinner_WhenOnePlayerHasNoCardsLeft()
        {
            var player1 = (new User { Username = "Player1" }, new List<Card> { new Card { Name = "Dragon", Damage = 50, ElementType = ElementType.Fire, CardType = CardType.Monster } });
            var player2 = (new User { Username = "Player2" }, new List<Card> { new Card { Name = "Goblin", Damage = 10, ElementType = ElementType.Normal, CardType = CardType.Monster } });

            var battleController = new BattleController(null, null);
            var log = battleController.StartBattle(player1, player2);

            Assert.That(log, Contains.Item("-->| Player1 wins the battle |<--"));
        }

        // Tests if the battle ends in a draw when both players have equal cards left.
        [Test]
        public void StartBattle_ShouldDeclareDraw_WhenBothPlayersHaveEqualCardsLeft()
        {
            var player1 = (new User { Username = "Player1" }, new List<Card> { new Card { Name = "Dragon", Damage = 50, ElementType = ElementType.Fire, CardType = CardType.Monster } });
            var player2 = (new User { Username = "Player2" }, new List<Card> { new Card { Name = "Dragon", Damage = 50, ElementType = ElementType.Fire, CardType = CardType.Monster } });

            var battleController = new BattleController(null, null);
            var log = battleController.StartBattle(player1, player2);

            Assert.That(log, Contains.Item("-->| The Battle is a Draw |<--"));
        }

        // Tests if the damage calculation applies special rules correctly (e.g., Goblin vs Dragon).
        [Test]
        public void CalculateDamage_ShouldReturnZero_WhenSpecialRulesApply()
        {
            var attacker = new Card { Name = "Goblin", Damage = 20, MonsterType = MonsterType.Goblin, CardType = CardType.Monster };
            var defender = new Card { Name = "Dragon", Damage = 50, MonsterType = MonsterType.Dragon, CardType = CardType.Monster };
            var weather = WeatherType.Sunny;

            var damage = BattleController.CalculateDamage(attacker, defender, weather);

            Assert.That(damage, Is.EqualTo(0));
        }

        // Tests if ELO points are updated correctly after a battle (win/lose scenario).
        [Test]
        public async Task UpdatePlayerStats_ShouldIncreaseWinnerEloAndDecreaseLoserElo_AfterBattle()
        {
            var userServiceMock = new Mock<UserService>(null);
            var player1 = (new User { Username = "Player1", Elo = 100 }, new List<Card> { new Card() });
            var player2 = (new User { Username = "Player2", Elo = 100 }, new List<Card>());
            var battleController = new BattleController(userServiceMock.Object, null);

            await battleController.UpdatePlayerStats(player1, player2);

            userServiceMock.Verify(service => service.UpdateUserAsync(It.Is<User>(u => u.Username == "Player1" && u.Elo == 103)), Times.Once);
            userServiceMock.Verify(service => service.UpdateUserAsync(It.Is<User>(u => u.Username == "Player2" && u.Elo == 95)), Times.Once);
        }

        // Tests if ELO points are correctly updated when one player loses.
        [Test]
        public async Task UpdatePlayerStats_ShouldDecreaseElo_WhenPlayerLoses()
        {
            var userServiceMock = new Mock<UserService>(null);
            var player1 = (new User { Username = "Player1", Elo = 120 }, new List<Card>());
            var player2 = (new User { Username = "Player2", Elo = 150 }, new List<Card> { new Card() });
            var battleController = new BattleController(userServiceMock.Object, null);

            await battleController.UpdatePlayerStats(player1, player2);

            userServiceMock.Verify(service => service.UpdateUserAsync(It.Is<User>(u => u.Username == "Player1" && u.Elo == 115)), Times.Once);
            userServiceMock.Verify(service => service.UpdateUserAsync(It.Is<User>(u => u.Username == "Player2" && u.Elo == 153)), Times.Once);
        }

        // Tests if the StartBattle method generates a log with battle details.
        [Test]
        public void StartBattle_ShouldReturnLog_WhenBattleIsPlayed()
        {
            var player1 = (new User { Username = "Player1" }, new List<Card> { new Card { Name = "Dragon", Damage = 50, ElementType = ElementType.Fire, CardType = CardType.Monster }, new Card { Name = "Goblin", Damage = 20, ElementType = ElementType.Normal, CardType = CardType.Monster } });
            var player2 = (new User { Username = "Player2" }, new List<Card> { new Card { Name = "Elf", Damage = 40, ElementType = ElementType.Normal, CardType = CardType.Monster }, new Card { Name = "Knight", Damage = 30, ElementType = ElementType.Normal, CardType = CardType.Monster } });

            var battleController = new BattleController(null, null);
            var log = battleController.StartBattle(player1, player2);

            Assert.That(log, Is.Not.Empty);
            Assert.That(log, Contains.Item($"Player1 has {player1.Item2.Count} cards left"));
            Assert.That(log, Contains.Item($"Player2 has {player2.Item2.Count} cards left"));
        }
    }
}
