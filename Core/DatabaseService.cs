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
        private readonly string _connectionString = $"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "accounts.db")}";
        public DatabaseService()
        {
            InitializeDatabase();
        }
        public async Task<List<Account>> GetAccountsAsync()
        {
            var result = new List<Account>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT id, name, tag, region, login, password, FolderId FROM accounts";

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
                    FolderId = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),

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
        INSERT INTO accounts (name, tag, region, login, password, FolderId)
        VALUES ($name, $tag, $region, $login, $password, $folderId)
    """;

            cmd.Parameters.AddWithValue("$name", acc.Name);
            cmd.Parameters.AddWithValue("$tag", acc.Tag);
            cmd.Parameters.AddWithValue("$region", acc.Region);
            cmd.Parameters.AddWithValue("$login", acc.Login);
            cmd.Parameters.AddWithValue("$password", acc.Password);
            cmd.Parameters.AddWithValue("$folderId", (object)acc.FolderId ?? DBNull.Value);

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
        
        public async Task UpdateAccountFolderAsync(int accountId, int? folderId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE accounts SET FolderId = $folderId WHERE id = $id";
            cmd.Parameters.AddWithValue("$id", accountId);
            cmd.Parameters.AddWithValue("$folderId", (object)folderId ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }
        
        public async Task<List<AccountFolder>> GetFoldersAsync()
        {
            var result = new List<AccountFolder>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, IsDefault FROM Folders";

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new AccountFolder
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    IsDefault = reader.GetInt32(2) == 1
                });
            }

            return result;
        }
        
        public async Task<int> InsertFolderAsync(AccountFolder folder)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = """
        INSERT INTO Folders (Name, IsDefault)
        VALUES ($name, $def);
        SELECT last_insert_rowid();
    """;

            cmd.Parameters.AddWithValue("$name", folder.Name);
            cmd.Parameters.AddWithValue("$def", folder.IsDefault ? 1 : 0);

            var id = (long)await cmd.ExecuteScalarAsync();
            return (int)id;
        }
        private void InitializeDatabase()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataDir))
                Directory.CreateDirectory(dataDir);

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS accounts (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            tag TEXT NOT NULL,
            region TEXT NOT NULL,
            login TEXT NOT NULL,
            password TEXT NOT NULL,
            FolderId INTEGER
        );

        CREATE TABLE IF NOT EXISTS Folders (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            IsDefault INTEGER NOT NULL
        );
    """;

            cmd.ExecuteNonQuery();
        }
    }
}
