using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace MonsterTradingCardsGame.Database
{
    internal class DatabaseManager(string connectionString)
    {
        private readonly string _connectionString = connectionString;

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public void InitializeDatabase()
        {
            using var connection = GetConnection();
            connection.Open();
            using var command = new NpgsqlCommand(
                                @"CREATE TABLE IF NOT EXISTS users (
                                    id SERIAL PRIMARY KEY,
                                    username TEXT NOT NULL UNIQUE,
                                    password TEXT NOT NULL,
                                    coins INT NOT NULL DEFAULT 20,
                                    elo INT NOT NULL DEFAULT 100
                                );", connection);

            command.ExecuteNonQuery();
        }
    }
}
