using System;
using Microsoft.Data.SqlClient;
using WordNoteConsoleApp.Models;

namespace WordNoteConsoleApp.DataAccess
{
    public class WordMeaningRepository
    {
        private readonly string _connectionString;
        public WordMeaningRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public OperationResult<bool> InsertWordMeaning(WordMeaning meaning, SqlConnection conn = null, SqlTransaction trans = null)
        {
            string sql = @"
                INSERT INTO WordMeanings (UsageId, MeaningCn, ExampleSentence, Note, CreatedAt)
                VALUES (@UsageId, @MeaningCn, @ExampleSentence, @Note, GETDATE())";

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

                    cmd.Parameters.AddWithValue("@UsageId", meaning.UsageId);
                    cmd.Parameters.AddWithValue("@MeaningCn", meaning.MeaningCn);
                    cmd.Parameters.AddWithValue("@ExampleSentence", string.IsNullOrWhiteSpace(meaning.ExampleSentence) ? DBNull.Value : meaning.ExampleSentence);
                    cmd.Parameters.AddWithValue("@Note", string.IsNullOrWhiteSpace(meaning.Note) ? DBNull.Value : meaning.Note);

                    int result = cmd.ExecuteNonQuery();
                    if (ownConnection) conn.Close();

                    return result > 0
                        ? OperationResult<bool>.Ok(true)
                        : OperationResult<bool>.Fail("插入失敗，無資料受影響");
                }
            }
            catch (SqlException ex)
            {
                if (ownConnection && conn != null) conn.Close();
                Console.WriteLine($"插入失敗: {ex.Message}");
                return OperationResult<bool>.Fail($"插入失敗: {ex.Message}");
            }
            catch (Exception ex)
            {
                if (ownConnection && conn != null) conn.Close();
                Console.WriteLine($"插入例外: {ex.Message}");
                return OperationResult<bool>.Fail($"插入失敗: {ex.Message}");
            }
        }

        public bool UpdateWordMeaning(WordMeaning meaning, SqlConnection conn, SqlTransaction trans)
        {
            string sql = @"
                UPDATE WordMeanings
                SET MeaningCn = @MeaningCn,
                    ExampleSentence = @ExampleSentence,
                    Note = @Note
                WHERE Id = @Id";

            try
            {
                using (var cmd = new SqlCommand(sql, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@Id", meaning.Id);
                    cmd.Parameters.AddWithValue("@MeaningCn", meaning.MeaningCn);
                    cmd.Parameters.AddWithValue("@ExampleSentence", string.IsNullOrWhiteSpace(meaning.ExampleSentence) ? DBNull.Value : meaning.ExampleSentence);
                    cmd.Parameters.AddWithValue("@Note", string.IsNullOrWhiteSpace(meaning.Note) ? DBNull.Value : meaning.Note);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新失敗: {ex.Message}");
                return false;
            }
        }

        public bool DeleteWordMeaningsByUsageId(int usageId, SqlConnection conn, SqlTransaction trans)
        {
            string sql = "DELETE FROM WordMeanings WHERE UsageId = @UsageId";

            try
            {
                using (var cmd = new SqlCommand(sql, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@UsageId", usageId);
                    return cmd.ExecuteNonQuery() >= 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刪除意思失敗: {ex.Message}");
                return false;
            }
        }
    }
}
