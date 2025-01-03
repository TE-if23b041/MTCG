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
                    cardCommand.Parameters.AddWithValue("element_type", card.ElementType);
                    cardCommand.Parameters.AddWithValue("card_type", card.CardType);
                    cardCommand.Parameters.AddWithValue("monster_type", card.MonsterType);
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
                var coins = await checkCoinsCommand.ExecuteScalarAsync() ?? throw new Exception("User not found");
                if ((int)coins < 5)
                    throw new Exception("Not enough money");

                var packageQuery = @"SELECT id FROM packages ORDER BY id LIMIT 1;";
                using var packageCommand = new NpgsqlCommand(packageQuery, connection, transaction);
                var packageId = await packageCommand.ExecuteScalarAsync() ?? throw new Exception("No packages available");

                var updateCardsQuery = @"UPDATE cards SET user_id = @user_id, package_id = NULL WHERE package_id = @package_id";
                using var updateCardsCommand = new NpgsqlCommand(updateCardsQuery, connection, transaction);
                updateCardsCommand.Parameters.AddWithValue("user_id", user.Id);
                updateCardsCommand.Parameters.AddWithValue("package_id", Convert.ToInt32(packageId));
                await updateCardsCommand.ExecuteNonQueryAsync();

                var updateCoinsQuery = @"UPDATE users SET coins = coins - 5 WHERE id = @id";
                using var updateCoinsCommand = new NpgsqlCommand(updateCoinsQuery, connection, transaction);
                updateCoinsCommand.Parameters.AddWithValue("id", user.Id);
                await updateCoinsCommand.ExecuteNonQueryAsync();

                // TODO maybe delete package?

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
        /*
        public async Task<List<Card>> GetDeckAsync(User user)
        {
            using var connection = _databaseManager.GetConnection();
            await connection.OpenAsync();
            var query = @"SELECT deck FROM users WHERE id = @user_id";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("user_id", user.Id);
            using var reader = await command.ExecuteReaderAsync();

            if (!reader.Read())
                throw new Exception("User not found");

            var deck = reader.GetValue(0) as string[];

            var cards = new List<Card>();
            while (reader.Read())
            {



            }
        }
        */
    }
}
