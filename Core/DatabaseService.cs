using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ValorantAccountManager.Core;
using ValorantAccountManager.MVVM.Model;

namespace ValorantAccountManager.Core
{
    public class DatabaseService
    {
        private readonly string _connectionString =$"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Data","accounts.db")}";
        public async Task<List<Account>> GetAccountsAsync()
        {
            var result = new List<Account>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT id, name, tag, region, login, password FROM accounts";

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new Account
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Tag = reader.GetString(2),
                    Region = reader.GetString(3),
                    Login = reader.GetString(4),
                    Password = reader.GetString(5),

                    Rank = "Loading...",
                    Elo = 0,
                    RankIcon = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Data",
                        "rank_png",
                        "Unranked_Rank.png"
                    )
                });
            }

            return result;
        }
        public async Task InsertAccountAsync(Account acc)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = """
        INSERT INTO accounts (name, tag, region, login, password)
        VALUES ($name, $tag, $region, $login, $password)
    """;

            cmd.Parameters.AddWithValue("$name", acc.Name);
            cmd.Parameters.AddWithValue("$tag", acc.Tag);
            cmd.Parameters.AddWithValue("$region", acc.Region);
            cmd.Parameters.AddWithValue("$login", acc.Login);
            cmd.Parameters.AddWithValue("$password", acc.Password);

            await cmd.ExecuteNonQueryAsync();
        }
        public async Task DeleteAccountAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM accounts WHERE id = $id";
            cmd.Parameters.AddWithValue("$id", id);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}