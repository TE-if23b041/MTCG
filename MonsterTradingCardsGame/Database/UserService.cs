using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Models;
using Npgsql;

namespace MonsterTradingCardsGame.Database
{
    // Provides user-related operations on the database
    public class UserService(DatabaseManager databaseManager)
    {
        private readonly DatabaseManager _databaseManager = databaseManager; // Database manager instance

        // Updates the ELO score of a user in the database
        public virtual async Task UpdateUserAsync(User user)
        {
            using var connection = _databaseManager.GetConnection(); // Get a database connection
            connection.Open(); // Open the connection
            var query = @"UPDATE users SET elo = @elo WHERE id = @id"; // Query to update user's ELO
            using var command = new NpgsqlCommand(query, connection); // Create command with query
            command.Parameters.AddWithValue("elo", user.Elo); // Set ELO parameter
            command.Parameters.AddWithValue("id", user.Id); // Set ID parameter
            await command.ExecuteNonQueryAsync(); // Execute the query
        }

        // Retrieves a user by username from the database
        public virtual async Task<User> GetUserAsync(string username)
        {
            using var connection = _databaseManager.GetConnection(); // Get a database connection
            connection.Open(); // Open the connection
            var query = @"SELECT * FROM users WHERE username = @username"; // Query to fetch user details

            using var command = new NpgsqlCommand(query, connection); // Create command with query
            command.Parameters.AddWithValue("username", username); // Set username parameter

            using var reader = await command.ExecuteReaderAsync(); // Execute query and read data

            if (!reader.Read()) // If no user is found, throw an exception
                throw new Exception("User not found");

            // Map the database row to a User object
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

        // Registers a new user in the database
        public virtual async Task RegisterUserAsync(User user)
        {
            using var connection = _databaseManager.GetConnection(); // Get a database connection
            connection.Open(); // Open the connection
            var query = @"INSERT INTO users (username, password) VALUES (@username, @password)"; // Insert query
            using var command = new NpgsqlCommand(query, connection); // Create command with query
            command.Parameters.AddWithValue("username", user.Username); // Set username parameter
            command.Parameters.AddWithValue("password", user.Password); // Set password parameter
            await command.ExecuteNonQueryAsync(); // Execute the query
        }

        // Updates user information such as name, bio, and image
        public async Task UpdateUserAync(User user, Dictionary<string, string> information)
        {
            using var connection = _databaseManager.GetConnection(); // Get a database connection
            connection.Open(); // Open the connection

            // Normalize dictionary keys to lowercase for consistent access
            var normalizedInformation = information.ToDictionary(
                kvp => kvp.Key.ToLower(),
                kvp => kvp.Value
            );

            // Ensure required keys are present in the dictionary
            if (!normalizedInformation.TryGetValue("name", out var name) ||
                !normalizedInformation.TryGetValue("bio", out var bio) ||
                !normalizedInformation.TryGetValue("image", out var image))
            {
                throw new Exception("Missing required keys: name, bio, or image.");
            }

            // Query to update user details
            var query = @"UPDATE users SET name = @name, bio = @bio, image = @image WHERE id = @id";
            using var command = new NpgsqlCommand(query, connection); // Create command with query

            command.Parameters.AddWithValue("name", name); // Set name parameter
            command.Parameters.AddWithValue("bio", bio); // Set bio parameter
            command.Parameters.AddWithValue("image", image); // Set image parameter
            command.Parameters.AddWithValue("id", user.Id); // Set ID parameter

            await command.ExecuteNonQueryAsync(); // Execute the query
        }

        // Retrieves all users from the database
        public async Task<List<User>> GetUsersAsync()
        {
            using var connection = _databaseManager.GetConnection(); // Get a database connection
            await connection.OpenAsync(); // Open the connection asynchronously
            var query = @"SELECT * FROM users"; // Query to fetch all users
            using var command = new NpgsqlCommand(query, connection); // Create command with query
            using var reader = await command.ExecuteReaderAsync(); // Execute query and read data
            var users = new List<User>(); // List to store users

            // Iterate through the result set and map each row to a User object
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

            return users; // Return the list of users
        }
    }
}
