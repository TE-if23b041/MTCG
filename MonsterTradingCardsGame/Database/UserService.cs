using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Models;
using Npgsql;

namespace MonsterTradingCardsGame.Database
{
    public class UserService(DatabaseManager databaseManager)
    {
        private readonly DatabaseManager _databaseManager = databaseManager;

        public async Task UpdateUserAsync(User user)
        {
            using var connection = _databaseManager.GetConnection();
            connection.Open();
            var query = @"UPDATE users SET elo = @elo WHERE id = @id";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("elo", user.Elo);
            command.Parameters.AddWithValue("id", user.Id);
            await command.ExecuteNonQueryAsync();
        }
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
                Elo = reader.GetInt32(4),
                Name = reader.GetString(5),
                Bio = reader.GetString(6),
                Image = reader.GetString(7)
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

        public async Task UpdateUserAync(User user, Dictionary<string, string> information)
        {
            using var connection = _databaseManager.GetConnection();
            connection.Open();

            // Normalize the keys to lowercase
            var normalizedInformation = information.ToDictionary(
                kvp => kvp.Key.ToLower(),
                kvp => kvp.Value
            );

            // Check if the required keys are present
            if (!normalizedInformation.TryGetValue("name", out var name) ||
                !normalizedInformation.TryGetValue("bio", out var bio) ||
                !normalizedInformation.TryGetValue("image", out var image))
            {
                throw new Exception("Missing required keys: name, bio, or image.");
            }

            var query = @"UPDATE users SET name = @name, bio = @bio, image = @image WHERE id = @id";
            using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("bio", bio);
            command.Parameters.AddWithValue("image", image);
            command.Parameters.AddWithValue("id", user.Id);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<User>> GetUsersAsync()
        {
            using var connection = _databaseManager.GetConnection();
            await connection.OpenAsync();
            var query = @"SELECT * FROM users";
            using var command = new NpgsqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            var users = new List<User>();
            while (reader.Read())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Coins = reader.GetInt32(3),
                    Elo = reader.GetInt32(4),
                    Name = reader.GetString(5),
                    Bio = reader.GetString(6),
                    Image = reader.GetString(7)
                });
            }
            return users;
        }
    }
}
