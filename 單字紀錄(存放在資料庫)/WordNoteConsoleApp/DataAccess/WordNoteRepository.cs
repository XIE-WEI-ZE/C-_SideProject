using System;
using Microsoft.Data.SqlClient;
using WordNoteConsoleApp.Models;

namespace WordNoteConsoleApp.DataAccess
{
    public class WordNoteRepository
    {
        private readonly string _connectionString;

        public WordNoteRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int InsertWordNote(WordNote note, SqlConnection conn, SqlTransaction trans)
        {
            string sql = @"
                INSERT INTO WordNotes (UserId, Word, KK, LevelCode, Note, AudioUrl, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES (@UserId, @Word, @KK, @LevelCode, @Note, @AudioUrl, GETDATE())";

            using (var cmd = new SqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("@UserId", note.UserId);
                cmd.Parameters.AddWithValue("@Word", note.Word);
                cmd.Parameters.AddWithValue("@KK", string.IsNullOrWhiteSpace(note.KK) ? DBNull.Value : note.KK);
                cmd.Parameters.AddWithValue("@LevelCode", note.LevelCode);
                cmd.Parameters.AddWithValue("@Note", string.IsNullOrWhiteSpace(note.Note) ? DBNull.Value : note.Note);
                cmd.Parameters.AddWithValue("@AudioUrl", DBNull.Value);

                return (int)cmd.ExecuteScalar();
            }
        }

        public bool UpdateWordNote(WordNote note, SqlConnection conn, SqlTransaction trans)
        {
            string sql = @"
                UPDATE WordNotes
                SET Word = @Word,
                    KK = @KK,
                    LevelCode = @LevelCode,
                    Note = @Note
                WHERE Id = @Id";

            using (var cmd = new SqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("@Id", note.Id);
                cmd.Parameters.AddWithValue("@Word", note.Word);
                cmd.Parameters.AddWithValue("@KK", string.IsNullOrWhiteSpace(note.KK) ? DBNull.Value : note.KK);
                cmd.Parameters.AddWithValue("@LevelCode", note.LevelCode);
                cmd.Parameters.AddWithValue("@Note", string.IsNullOrWhiteSpace(note.Note) ? DBNull.Value : note.Note);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public WordNote? GetWordNoteByWord(string word, int userId)
        {
            string sql = "SELECT * FROM WordNotes WHERE Word = @Word AND UserId = @UserId";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Word", word);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new WordNote
                        {
                            Id = (int)reader["Id"],
                            UserId = (int)reader["UserId"],
                            Word = reader["Word"].ToString(),
                            KK = reader["KK"]?.ToString(),
                            LevelCode = (int)reader["LevelCode"],
                            Note = reader["Note"]?.ToString(),
                            AudioUrl = reader["AudioUrl"]?.ToString(),
                            CreatedAt = (DateTime)reader["CreatedAt"]
                        };
                    }
                }
            }

            return null;
        }

        public OperationResult<bool> DeleteWordNoteByWord(string word, int userId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        return DeleteWordNoteByWord(word, userId, conn, trans);
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return OperationResult<bool>.Fail($"刪除單字失敗：{ex.Message}");
                    }
                }
            }
        }

        public OperationResult<bool> DeleteWordNoteByWord(string word, int userId, SqlConnection conn, SqlTransaction trans)
        {
            string getIdSql = "SELECT Id FROM WordNotes WHERE Word = @Word AND UserId = @UserId";
            int wordNoteId = -1;

            using (var getIdCmd = new SqlCommand(getIdSql, conn, trans))
            {
                getIdCmd.Parameters.AddWithValue("@Word", word);
                getIdCmd.Parameters.AddWithValue("@UserId", userId);

                var result = getIdCmd.ExecuteScalar();
                if (result == null)
                    return OperationResult<bool>.Fail("找不到對應的單字");

                wordNoteId = (int)result;
            }

            // 先刪除 WordMeanings
            string deleteMeanings = @"
                DELETE FROM WordMeanings 
                WHERE UsageId IN (SELECT Id FROM WordUsages WHERE WordNoteId = @WordNoteId)";

            using (var cmd = new SqlCommand(deleteMeanings, conn, trans))
            {
                cmd.Parameters.AddWithValue("@WordNoteId", wordNoteId);
                cmd.ExecuteNonQuery();
            }

            // 再刪除 WordUsages
            string deleteUsages = "DELETE FROM WordUsages WHERE WordNoteId = @WordNoteId";
            using (var cmd = new SqlCommand(deleteUsages, conn, trans))
            {
                cmd.Parameters.AddWithValue("@WordNoteId", wordNoteId);
                cmd.ExecuteNonQuery();
            }

            // 最後刪除 WordNote
            string deleteNote = "DELETE FROM WordNotes WHERE Id = @Id";
            using (var cmd = new SqlCommand(deleteNote, conn, trans))
            {
                cmd.Parameters.AddWithValue("@Id", wordNoteId);
                int affected = cmd.ExecuteNonQuery();
                if (affected == 0)
                {
                    trans.Rollback();
                    return OperationResult<bool>.Fail("刪除資料失敗");
                }
            }

            trans.Commit();
            return OperationResult<bool>.Ok(true);
        }
    }
}
