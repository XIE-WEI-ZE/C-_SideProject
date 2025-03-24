using System;
using Microsoft.Data.SqlClient;
using WordNoteConsoleApp.Models;

namespace WordNoteConsoleApp.DataAccess
{
    public class WordUsageRepository
    {
        private readonly string _connectionString;
        public WordUsageRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ✅ 主要給 WordService 用（只插入詞性，搭配 WordMeaning 再補意思）
        public int InsertUsage(string partOfSpeech, int wordNoteId, SqlConnection conn, SqlTransaction trans)
        {
            string sql = @"
                INSERT INTO WordUsages (WordNoteId, PartOfSpeech, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES (@WordNoteId, @PartOfSpeech, GETDATE())";

            using (var cmd = new SqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("@WordNoteId", wordNoteId);
                cmd.Parameters.AddWithValue("@PartOfSpeech", partOfSpeech ?? (object)DBNull.Value);
                return (int)cmd.ExecuteScalar();
            }
        }

        // ✅ WordService 更新用：整批刪除詞性（再一併刪除子意思）
        public bool DeleteUsagesByWordNoteId(int wordNoteId, SqlConnection conn, SqlTransaction trans)
        {
            string sql = "DELETE FROM WordUsages WHERE WordNoteId = @WordNoteId";

            using (var cmd = new SqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("@WordNoteId", wordNoteId);
                return cmd.ExecuteNonQuery() >= 0;
            }
        }

        // ✅ 目前未用，但可保留進階用途
        public int InsertWordUsage(WordUsage usage, SqlConnection conn = null, SqlTransaction trans = null)
        {
            string sql = @"
                INSERT INTO WordUsages (WordNoteId, PartOfSpeech, MeaningCn, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES (@WordNoteId, @PartOfSpeech, @MeaningCn, GETDATE())";

            bool ownConnection = conn == null;
            try
            {
                if (ownConnection)
                {
                    conn = new SqlConnection(_connectionString);
                    conn.Open();
                }

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (trans != null) cmd.Transaction = trans;

                    cmd.Parameters.AddWithValue("@WordNoteId", usage.WordNoteId);
                    cmd.Parameters.AddWithValue("@PartOfSpeech", usage.PartOfSpeech ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MeaningCn", usage.MeaningCn ?? (object)DBNull.Value);

                    var result = cmd.ExecuteScalar();
                    if (ownConnection) conn.Close();

                    return result != null && result != DBNull.Value
                        ? Convert.ToInt32(result)
                        : -1;
                }
            }
            catch (SqlException ex)
            {
                if (ownConnection && conn != null) conn.Close();
                Console.WriteLine($"插入詞性失敗: {ex.Message}");
                return -1;
            }
        }

        public bool UpdateWordUsage(WordUsage usage, SqlConnection conn, SqlTransaction trans)
        {
            string sql = @"
                UPDATE WordUsages
                SET PartOfSpeech = @PartOfSpeech,
                    MeaningCn = @MeaningCn
                WHERE Id = @Id";

            try
            {
                using (var cmd = new SqlCommand(sql, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@Id", usage.Id);
                    cmd.Parameters.AddWithValue("@PartOfSpeech", usage.PartOfSpeech ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MeaningCn", usage.MeaningCn ?? (object)DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新詞性失敗: {ex.Message}");
                return false;
            }
        }

        public bool DeleteWordUsage(int id, SqlConnection conn, SqlTransaction trans)
        {
            string sql = "DELETE FROM WordUsages WHERE Id = @Id";

            try
            {
                using (var cmd = new SqlCommand(sql, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刪除詞性失敗: {ex.Message}");
                return false;
            }
        }
    }
}
