using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace MonsterTradingCardsGame.Database
{
    internal class DatabaseManager
    {
        private readonly string _connectionString;

        public DatabaseManager(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty");
            }
            _connectionString = connectionString;
        }
        public NpgsqlConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty");
            }

            Console.WriteLine($"Connection String: {_connectionString}");
            return new NpgsqlConnection(_connectionString);
        }

        public void InitializeDatabase()
        {
            using var connection = GetConnection();
            connection.Open();

            // Create the "users" table
            using (var command = new NpgsqlCommand(
                @"CREATE TABLE IF NOT EXISTS users (
                id SERIAL PRIMARY KEY,
                username TEXT NOT NULL UNIQUE,
                password TEXT NOT NULL,
                coins INT NOT NULL DEFAULT 20,
                elo INT NOT NULL DEFAULT 100,
                deck VARCHAR(50)[]
            );", connection))
            {
                command.ExecuteNonQuery();
            }

            // Create the "packages" table
            using (var command = new NpgsqlCommand(
                @"CREATE TABLE IF NOT EXISTS packages (
                    id          SERIAL PRIMARY KEY
                );", connection))
            {
                command.ExecuteNonQuery();
            }

            // Create the "cards" table
            using (var command = new NpgsqlCommand(
                @"CREATE TABLE IF NOT EXISTS cards (
                    id          VARCHAR(50) PRIMARY KEY,
                    name        VARCHAR(100) NOT NULL,
                    damage      DECIMAL(10, 2) NOT NULL,
                    element_type     VARCHAR(20) NOT NULL,
                    card_type    VARCHAR(20) NOT NULL,
                    monster_type     VARCHAR(20) NOT NULL,
                    package_id  INTEGER NULL,
                    user_id     INTEGER NULL,
                    FOREIGN KEY (package_id) REFERENCES packages(id) ON DELETE SET NULL,
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL,
                    CHECK       (package_id IS NULL OR user_id IS NULL)
                );", connection))
            {
                command.ExecuteNonQuery();
            }

        }
    }
}
