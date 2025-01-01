using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Models;
using Npgsql;

namespace MonsterTradingCardsGame.Database
{
    internal class UserService(DatabaseManager databaseManager)
    {
        private readonly DatabaseManager _databaseManager = databaseManager;
        public async Task<User> GetUserAsync(string username)
        {
            using var connection = _databaseManager.GetConnection();
            connection.Open();

            var query = @"SELECT * FROM users WHERE username = @username";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("username", username);

            using var reader = await command.ExecuteReaderAsync();

            if (!reader.Read())
                throw new Exception("User not found");

            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Password = reader.GetString(2),
                Coins = reader.GetInt32(3),
                Elo = reader.GetInt32(4)
            };
        }

        public async Task RegisterUserAsync(User user)
        {
            using var connection = _databaseManager.GetConnection();
            connection.Open();
            var query = @"INSERT INTO users (username, password) VALUES (@username, @password)";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("username", user.Username);
            command.Parameters.AddWithValue("password", user.Password);
            await command.ExecuteNonQueryAsync();
        }
    }
}
