using System.Reflection.Metadata.Ecma335;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Models;
using Npgsql;

namespace MonsterTradingCardsGame.Database
{
    internal class CardService(DatabaseManager databaseManager)
    {
        private readonly DatabaseManager _databaseManager = databaseManager;



        public async Task InsertPackageAsync(List<Card> package)
        {
            using var connection = _databaseManager.GetConnection();
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                var packageQuery = @"INSERT INTO packages DEFAULT VALUES RETURNING id";
                using var packageCommand = new NpgsqlCommand(packageQuery, connection, transaction);
                var packageId = await packageCommand.ExecuteScalarAsync() ?? throw new Exception("Failed to insert package");

                var cardQuery = @"INSERT INTO cards (id, name, damage, element_type, card_type, monster_type, package_id) VALUES
                                (@id, @name, @damage, @element_type, @card_type, @monster_type, @package_id)";
                foreach (var card in package)
                {
                    using var cardCommand = new NpgsqlCommand(cardQuery, connection, transaction);
                    cardCommand.Parameters.AddWithValue("id", card.Id);
                    cardCommand.Parameters.AddWithValue("name", card.Name);
                    cardCommand.Parameters.AddWithValue("damage", card.Damage);
                    cardCommand.Parameters.AddWithValue("element_type", card.ElementType.ToString());
                    cardCommand.Parameters.AddWithValue("card_type", card.CardType.ToString());
                    cardCommand.Parameters.AddWithValue("monster_type", card.MonsterType.ToString());
                    cardCommand.Parameters.AddWithValue("package_id", packageId);
                    await cardCommand.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }



        public async Task AcquriePackageAsync(User user)
        {
            using var connection = _databaseManager.GetConnection();
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                var checkCoinsQuery = @"SELECT coins FROM users WHERE id = @id";
                using var checkCoinsCommand = new NpgsqlCommand(checkCoinsQuery, connection, transaction);
                checkCoinsCommand.Parameters.AddWithValue("id", user.Id);
                var coins = await checkCoinsCommand.ExecuteScalarAsync() ?? throw new InvalidOperationException("User not found");
                if ((int)coins < 5)
                    throw new InvalidOperationException("Not enough money");

                var packageQuery = @"SELECT id FROM packages ORDER BY id LIMIT 1;";
                using var packageCommand = new NpgsqlCommand(packageQuery, connection, transaction);
                var packageId = await packageCommand.ExecuteScalarAsync() ?? throw new InvalidOperationException("No packages available");

                var updateCardsQuery = @"UPDATE cards SET user_id = @user_id, package_id = NULL WHERE package_id = @package_id";
                using var updateCardsCommand = new NpgsqlCommand(updateCardsQuery, connection, transaction);
                updateCardsCommand.Parameters.AddWithValue("user_id", user.Id);
                updateCardsCommand.Parameters.AddWithValue("package_id", Convert.ToInt32(packageId));
                await updateCardsCommand.ExecuteNonQueryAsync();

                var updateCoinsQuery = @"UPDATE users SET coins = coins - 5 WHERE id = @id";
                using var updateCoinsCommand = new NpgsqlCommand(updateCoinsQuery, connection, transaction);
                updateCoinsCommand.Parameters.AddWithValue("id", user.Id);
                await updateCoinsCommand.ExecuteNonQueryAsync();

                var deletePackageQuery = "DELETE FROM packages WHERE id = @packageId;";
                using var deletePackageCommand = new NpgsqlCommand(deletePackageQuery, connection, transaction);
                deletePackageCommand.Parameters.AddWithValue("@packageId", packageId);
                await deletePackageCommand.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }




        public async Task<List<Card>> GetCardsAsync(User user)
        {
            using var connection = _databaseManager.GetConnection();
            await connection.OpenAsync();
            var query = @"SELECT * FROM cards WHERE user_id = @user_id";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("user_id", user.Id);
            using var reader = await command.ExecuteReaderAsync();

            var cards = new List<Card>();
            while (reader.Read())
            {
                cards.Add(new Card
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1),
                    Damage = reader.GetDouble(2),
                    CardType = Enum.Parse<CardType>(reader.GetString(4)),
                    ElementType = Enum.Parse<ElementType>(reader.GetString(3)),
                    MonsterType = Enum.Parse<MonsterType>(reader.GetString(5))
                });
            }
            return cards;
        }


        public async Task<List<Card>> GetDeckAsync(User user)
        {
            using var connection = _databaseManager.GetConnection();
            await connection.OpenAsync();

            var query = @"
            SELECT * FROM cards 
            WHERE id = ANY((SELECT deck FROM users WHERE id = @user_id)::VARCHAR[]);
            ";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("user_id", user.Id);

            var cards = new List<Card>();
            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                cards.Add(new Card
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1),
                    Damage = reader.GetDouble(2),
                    CardType = Enum.Parse<CardType>(reader.GetString(4)),
                    ElementType = Enum.Parse<ElementType>(reader.GetString(3)),
                    MonsterType = Enum.Parse<MonsterType>(reader.GetString(5))
                });
            }

            return cards;
        }


        public async Task ConfigureDeckAsync(User user, List<string> cards)
        {
            using var connection = _databaseManager.GetConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            try
            {
                var configureDeckQuery = @"
                WITH valid_cards AS (
                    SELECT * FROM cards WHERE id = ANY(@deck) AND user_id = @user_id
                )
                UPDATE users
                SET deck = (SELECT ARRAY_AGG(id) FROM valid_cards)
                WHERE id = @user_id
                RETURNING (SELECT COUNT(*)::INT FROM valid_cards);
                ";

                using var command = new NpgsqlCommand(configureDeckQuery, connection, transaction);
                command.Parameters.AddWithValue("deck", cards.ToArray());
                command.Parameters.AddWithValue("user_id", user.Id);

                var count = await command.ExecuteScalarAsync() ?? throw new InvalidOperationException("Invalid card id");
                if ((int)count != 4)
                    throw new InvalidOperationException("Error");

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

    }
}
