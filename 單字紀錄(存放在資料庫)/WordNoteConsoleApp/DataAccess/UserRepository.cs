using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using WordNoteConsoleApp.Models;

namespace WordNoteConsoleApp.DataAccess
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // 註冊帳號
        public bool Register(User user, string plainPassword)
        {
            string salt = GenerateSalt();
            string hash = HashPassword(plainPassword, salt);

            string sql = @"INSERT INTO Users (Name, Account, PasswordHash, Salt)
                           VALUES (@Name, @Account, @PasswordHash, @Salt)";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", user.Name);
                    cmd.Parameters.AddWithValue("@Account", user.Account);
                    cmd.Parameters.AddWithValue("@PasswordHash", hash);
                    cmd.Parameters.AddWithValue("@Salt", salt);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"註冊帳號錯誤：{ex.Message}");
                return false;
            }
        }

        // 登入驗證
        public User Login(string account, string password)
        {
            string sql = "SELECT * FROM Users WHERE Account = @Account";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Account", account);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string salt = reader["Salt"]?.ToString() ?? "";
                            string hashedInput = HashPassword(password, salt);
                            string storedHash = reader["PasswordHash"]?.ToString() ?? "";

                            if (hashedInput == storedHash)
                            {
                                return new User
                                {
                                    UserId = (int)reader["UserId"],
                                    Name = reader["Name"].ToString(),
                                    Account = reader["Account"].ToString(),
                                    PasswordHash = storedHash,
                                    Salt = salt
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"登入時資料庫錯誤：{ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"登入時發生例外：{ex.Message}");
            }

            return null;
        }

        // 密碼雜湊處理（SHA256 + Salt）
        private string HashPassword(string password, string salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] combined = Encoding.UTF8.GetBytes(password + salt);
                byte[] hash = sha256.ComputeHash(combined);
                return Convert.ToBase64String(hash);
            }
        }

        // 產生隨機 Salt（16 bytes）
        private string GenerateSalt()
        {
            byte[] buffer = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
                return Convert.ToBase64String(buffer);
            }
        }
    }
}
