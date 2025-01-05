using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace MonsterTradingCardsGame.Database
{
    public class DatabaseManager(string connectionString)
    {
        private readonly string _connectionString = connectionString;

        public virtual NpgsqlConnection GetConnection()
        {
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
                name VARCHAR(100) NOT NULL DEFAULT 'no name',
                bio VARCHAR(100) NOT NULL DEFAULT 'no bio',
                image VARCHAR(100) NOT NULL DEFAULT 'no image',
                deck VARCHAR(50)[] DEFAULT '{}'::VARCHAR[]
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
