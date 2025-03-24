using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using WordNoteConsoleApp.DataAccess;
using WordNoteConsoleApp.Models;

namespace WordNoteConsoleApp.Services
{
    public class WordService
    {
        private readonly WordNoteRepository _noteRepo;
        private readonly WordUsageRepository _usageRepo;
        private readonly WordMeaningRepository _meaningRepo;
        private readonly string _connStr;

        public WordService(WordNoteRepository noteRepo, WordUsageRepository usageRepo, WordMeaningRepository meaningRepo, string connStr)
        {
            _noteRepo = noteRepo;
            _usageRepo = usageRepo;
            _meaningRepo = meaningRepo;
            _connStr = connStr;
        }

        public string GetRawConnectionString() => _connStr;

        // 分頁查詢單字列表（修正名稱重複）
        public (List<(WordNote Word, string PartOfSpeech, string MeaningCn, string ExampleSentence, string MeaningNote)> Results, int TotalCount)
            GetWordListByPage(int userId, int pageNumber, int pageSize)
        {
            var results = new List<(WordNote, string, string, string, string)>();
            int totalCount = 0;

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();

                // 查詢總筆數
                string countSql = "SELECT COUNT(*) FROM WordNotes WHERE UserId = @UserId";
                using (var countCmd = new SqlCommand(countSql, conn))
                {
                    countCmd.Parameters.AddWithValue("@UserId", userId);
                    totalCount = (int)countCmd.ExecuteScalar();
                }

                // 分頁查詢資料
                string sql = @"
                    SELECT W.Id AS WordId, W.Word, W.KK, W.LevelCode, W.CreatedAt,
                           U.PartOfSpeech, M.MeaningCn, M.ExampleSentence, M.Note
                    FROM WordNotes W
                    LEFT JOIN WordUsages U ON W.Id = U.WordNoteId
                    LEFT JOIN WordMeanings M ON U.Id = M.UsageId
                    WHERE W.UserId = @UserId
                    ORDER BY W.CreatedAt DESC, U.PartOfSpeech
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var wordNote = new WordNote
                            {
                                Id = (int)reader["WordId"],
                                UserId = userId,
                                Word = reader["Word"].ToString(),
                                KK = reader["KK"]?.ToString(),
                                LevelCode = (int)reader["LevelCode"],
                                Note = reader["Note"]?.ToString(),
                                CreatedAt = (DateTime)reader["CreatedAt"]
                            };

                            results.Add((
                                wordNote,
                                reader["PartOfSpeech"]?.ToString() ?? "",
                                reader["MeaningCn"]?.ToString() ?? "",
                                reader["ExampleSentence"]?.ToString() ?? "",
                                reader["Note"]?.ToString() ?? ""  // 注意這邊對應的是 MeaningNote
                            ));
                        }
                    }
                }
            }

            return (results, totalCount);
        }

        
        public OperationResult<bool> AddWord(WordNote note, List<(string PartOfSpeech, List<WordMeaning> Meanings)> usageList)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        int wordNoteId = _noteRepo.InsertWordNote(note, conn, trans);
                        foreach (var (partOfSpeech, meanings) in usageList)
                        {
                            int usageId = _usageRepo.InsertUsage(partOfSpeech, wordNoteId, conn, trans);
                            foreach (var meaning in meanings)
                            {
                                meaning.UsageId = usageId;
                                var insertResult = _meaningRepo.InsertWordMeaning(meaning, conn, trans);
                                if (!insertResult.Success)
                                {
                                    trans.Rollback();
                                    return OperationResult<bool>.Fail(insertResult.Message);
                                }
                            }
                        }

                        trans.Commit();
                        return OperationResult<bool>.Ok(true);
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return OperationResult<bool>.Fail($"新增失敗：{ex.Message}");
                    }
                }
            }
        }

       
        public WordNote? FindWord(string word, int userId)
        {
            return _noteRepo.GetWordNoteByWord(word, userId);
        }

        
        public OperationResult<bool> UpdateWord(WordNote note, List<(string PartOfSpeech, List<WordMeaning>)> usageList)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        bool updated = _noteRepo.UpdateWordNote(note, conn, trans);
                        if (!updated)
                        {
                            trans.Rollback();
                            return OperationResult<bool>.Fail("更新失敗");
                        }

                        bool deletedUsages = _usageRepo.DeleteUsagesByWordNoteId(note.Id, conn, trans);
                        if (!deletedUsages)
                        {
                            trans.Rollback();
                            return OperationResult<bool>.Fail("刪除舊詞性失敗");
                        }

                        foreach (var (partOfSpeech, meanings) in usageList)
                        {
                            int usageId = _usageRepo.InsertUsage(partOfSpeech, note.Id, conn, trans);
                            foreach (var meaning in meanings)
                            {
                                meaning.UsageId = usageId;
                                var insertResult = _meaningRepo.InsertWordMeaning(meaning, conn, trans);
                                if (!insertResult.Success)
                                {
                                    trans.Rollback();
                                    return OperationResult<bool>.Fail(insertResult.Message);
                                }
                            }
                        }

                        trans.Commit();
                        return OperationResult<bool>.Ok(true);
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return OperationResult<bool>.Fail($"更新失敗：{ex.Message}");
                    }
                }
            }
        }

        
        public bool UpdateSingleMeaning(WordMeaning updated, SqlConnection conn, SqlTransaction trans)
        {
            return _meaningRepo.UpdateWordMeaning(updated, conn, trans);
        }

        // ✅ 刪除單字
        public OperationResult<bool> DeleteWord(string word, int userId)
        {
            return _noteRepo.DeleteWordNoteByWord(word, userId);
        }
    }
}
