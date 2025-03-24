using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using WordNoteConsoleApp.Models;
using WordNoteConsoleApp.Services;

namespace WordNoteConsoleApp.Menus
{
    public class WordMenu
    {
        private readonly WordService _wordService;

        public WordMenu(WordService wordService)
        {
            _wordService = wordService;
        }

        public void ShowSearchMenu(int userId)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== 查詢單字選單 ===");
                Console.WriteLine("1. 查看所有單字");
                Console.WriteLine("2. 模糊搜尋單字");
                Console.WriteLine("3. 詞性分類顯示");
                Console.WriteLine("4. 返回主選單");
                Console.Write("請選擇操作（1-4）：");
                string input = Console.ReadLine()?.Trim();

                switch (input)
                {
                    case "1":
                        ShowWordList(userId);
                        break;
                    case "2":
                        SearchWordDetail(userId);
                        break;
                    case "3":
                        GroupWordsByPartOfSpeech(userId);
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("請輸入有效選項！");
                        break;
                }

                Console.WriteLine("\n請按任意鍵繼續...");
                Console.ReadKey();
            }
        }

        public void AddWordFlow(int userId)
        {
            Console.WriteLine("=== 新增英文單字 ===");
            Console.Write("請輸入單字：");
            string word = Console.ReadLine()?.Trim() ?? "";

            Console.Write("請輸入音標（KK）：");
            string kk = Console.ReadLine()?.Trim() ?? "";

            int levelCode = 0;
            while (true)
            {
                Console.Write("請輸入等級數字（1~5）：");
                string input = Console.ReadLine();
                if (int.TryParse(input, out levelCode) && levelCode >= 1 && levelCode <= 5) break;
                Console.WriteLine("請輸入 1~5 數字");
            }

            Console.Write("請輸入主註解（可空）：");
            string note = Console.ReadLine();

            var wordNote = new WordNote
            {
                UserId = userId,
                Word = word,
                KK = kk,
                LevelCode = levelCode,
                Note = note,
                AudioUrl = null
            };

            var usageList = GetUsageList();

            var result = _wordService.AddWord(wordNote, usageList);
            Console.WriteLine(result.Success ? "單字新增成功" : $"新增失敗：{result.Message}");
        }

        public void EditWord(int userId)
        {
            Console.WriteLine("=== 修改單字 ===");
            Console.Write("請輸入要修改的單字：");
            string word = Console.ReadLine()?.Trim() ?? "";

            var existing = _wordService.FindWord(word, userId);
            if (existing == null)
            {
                Console.WriteLine("找不到此單字");
                return;
            }

            Console.WriteLine($"目前單字：{existing.Word} / KK: {existing.KK} / 等級: {existing.LevelCode} / 註解: {existing.Note}");

            Console.Write("新單字（Enter 跳過）：");
            string newWord = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newWord)) existing.Word = newWord;

            Console.Write("新音標（Enter 跳過）：");
            string newKK = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newKK)) existing.KK = newKK;

            Console.Write("新等級（1~5，Enter 跳過）：");
            string level = Console.ReadLine();
            if (int.TryParse(level, out int newLevel) && newLevel >= 1 && newLevel <= 5)
                existing.LevelCode = newLevel;

            Console.Write("新註解（Enter 跳過）：");
            string newNote = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newNote)) existing.Note = newNote;

            Console.Write("是否要編輯詞性與意思？（Y/N）：");
            string editChoice = Console.ReadLine()?.Trim().ToUpper();
            List<(string, List<WordMeaning>)> newUsages = new();

            if (editChoice == "Y")
                newUsages = GetUsageList();

            var result = _wordService.UpdateWord(existing, newUsages);
            Console.WriteLine(result.Success ? "修改成功！" : $"修改失敗：{result.Message}");
        }

        public void ShowWordList(int userId)
        {
            const int PageSize = 5; // 每頁顯示的單字數量，可調整
            int currentPage = 1;
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("=== 所有單字列表 ===");

                // 獲取當前頁資料與總筆數
                var (results, totalCount) = _wordService.GetWordListByPage(userId, currentPage, PageSize);
                int totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                if (results.Count == 0)
                {
                    Console.WriteLine("目前無單字記錄。");
                }
                else
                {
                    int currentWordId = -1;
                    foreach (var (note, partOfSpeech, meaningCn, exampleSentence, meaningNote) in results)
                    {
                        if (note.Id != currentWordId)
                        {
                            Console.WriteLine("\n" + new string('-', 40));
                            Console.WriteLine($"單字：{note.Word} | KK: {note.KK} | 難易度：{note.LevelCode}");
                            Console.WriteLine($"新增時間：{note.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                            currentWordId = note.Id;
                        }

                        Console.WriteLine($"詞性：{partOfSpeech}");
                        Console.WriteLine($"  中文：{meaningCn}");
                        if (!string.IsNullOrEmpty(exampleSentence))
                            Console.WriteLine($"  例句：{exampleSentence}");
                        if (!string.IsNullOrEmpty(meaningNote))
                            Console.WriteLine($"  解釋：{meaningNote}");
                    }
                }

                // 顯示分頁資訊與導航選項
                Console.WriteLine($"\n頁數：{currentPage}/{totalPages} | 總筆數：{totalCount}");
                Console.WriteLine("\n導航：1（上一頁） | 2（下一頁） | 3（跳至頁數） | 4（離開）");
                Console.Write("請輸入選擇：");

                string input = Console.ReadLine()?.Trim().ToUpper();
                switch (input)
                {
                    case "1":
                        if (currentPage > 1) currentPage--;
                        break;
                    case "2":
                        if (currentPage < totalPages) currentPage++;
                        break;
                    case "3":
                        Console.Write($"請輸入頁數 (1-{totalPages})：");
                        if (int.TryParse(Console.ReadLine(), out int page) && page >= 1 && page <= totalPages)
                            currentPage = page;
                        else
                            Console.WriteLine("無效的頁數！");
                        break;
                    case "4":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("請輸入有效選項！");
                        break;
                }

                if (!exit)
                {
                    Console.WriteLine("\n請按任意鍵繼續...");
                    Console.ReadKey();
                }
            }
        }

        public void SearchWordDetail(int userId)
        {
            Console.Write("請輸入搜尋關鍵字：");
            string keyword = Console.ReadLine()?.Trim() ?? "";

            var connStr = _wordService.GetRawConnectionString();
            string sql = @"
        SELECT W.Id AS WordId, W.Word, W.KK, W.LevelCode, W.CreatedAt,
               U.PartOfSpeech, M.MeaningCn, M.ExampleSentence, M.Note
        FROM WordNotes W
        LEFT JOIN WordUsages U ON W.Id = U.WordNoteId
        LEFT JOIN WordMeanings M ON U.Id = M.UsageId
        WHERE W.UserId = @UserId AND W.Word LIKE @Keyword
        ORDER BY W.CreatedAt DESC, U.PartOfSpeech";

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
            conn.Open();

            using var reader = cmd.ExecuteReader();
            int currentWordId = -1;
            bool hasResult = false;

            while (reader.Read())
            {
                hasResult = true;
                int wordId = (int)reader["WordId"];
                if (wordId != currentWordId)
                {
                    Console.WriteLine("\n" + new string('-', 40));
                    Console.WriteLine($"單字：{reader["Word"]} | KK: {reader["KK"]} | 難易度：{reader["LevelCode"]}");
                    Console.WriteLine($"新增時間：{Convert.ToDateTime(reader["CreatedAt"]):yyyy-MM-dd HH:mm:ss}");
                    currentWordId = wordId;
                }

                Console.WriteLine($"詞性：{reader["PartOfSpeech"]}");
                Console.WriteLine($"  中文：{reader["MeaningCn"]}");
                if (reader["ExampleSentence"] != DBNull.Value)
                    Console.WriteLine($"  例句：{reader["ExampleSentence"]}");
                if (reader["Note"] != DBNull.Value)
                    Console.WriteLine($"  解釋：{reader["Note"]}");
            }

            if (!hasResult)
                Console.WriteLine("查無符合的單字！");
        }

        public void GroupWordsByPartOfSpeech(int userId)
        {
            var connStr = _wordService.GetRawConnectionString();
            string sql = @"
        SELECT U.PartOfSpeech, W.Word, M.MeaningCn
        FROM WordNotes W
        JOIN WordUsages U ON W.Id = U.WordNoteId
        JOIN WordMeanings M ON U.Id = M.UsageId
        WHERE W.UserId = @UserId
        ORDER BY U.PartOfSpeech, W.Word";

            var grouped = new Dictionary<string, List<string>>();

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string pos = reader["PartOfSpeech"].ToString();
                string word = reader["Word"].ToString();
                string meaning = reader["MeaningCn"].ToString();

                if (!grouped.ContainsKey(pos))
                    grouped[pos] = new List<string>();

                grouped[pos].Add($"{word}：{meaning}");
            }

            Console.WriteLine("\n=== 詞性分類顯示 ===");
            foreach (var kv in grouped)
            {
                Console.WriteLine($"\n【{kv.Key}】");
                foreach (var entry in kv.Value)
                    Console.WriteLine($"- {entry}");
            }
        }

        public void EditMenu(int userId)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== 修改單字功能 ===");
                Console.WriteLine("1. 修改單字基本資料");
                Console.WriteLine("2. 修改某一筆意思 / 例句 / 解釋");
                Console.WriteLine("3. 返回上一頁");
                Console.Write("請選擇操作（1-3）：");

                string input = Console.ReadLine()?.Trim();
                switch (input)
                {
                    case "1": EditWord(userId); break;
                    case "2": EditSingleMeaningFlow(userId); break;
                    case "3": return;
                    default: Console.WriteLine("請輸入有效選項！"); break;
                }

                Console.WriteLine("\n請按任意鍵繼續...");
                Console.ReadKey();
            }
        }

        public void EditSingleMeaningFlow(int userId)
        {
            Console.Write("請輸入要修改的單字：");
            string word = Console.ReadLine()?.Trim() ?? "";

            var wordNote = _wordService.FindWord(word, userId);
            if (wordNote == null)
            {
                Console.WriteLine("找不到此單字");
                return;
            }

            var connStr = _wordService.GetRawConnectionString();
            string sql = @"
                SELECT M.Id, U.PartOfSpeech, M.MeaningCn, M.ExampleSentence, M.Note
                FROM WordNotes W
                JOIN WordUsages U ON W.Id = U.WordNoteId
                JOIN WordMeanings M ON U.Id = M.UsageId
                WHERE W.Id = @WordNoteId";

            var meanings = new List<(int Id, string PartOfSpeech, string MeaningCn, string Example, string Note)>();

            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@WordNoteId", wordNote.Id);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        meanings.Add((
                            (int)reader["Id"],
                            reader["PartOfSpeech"].ToString(),
                            reader["MeaningCn"].ToString(),
                            reader["ExampleSentence"]?.ToString() ?? "",
                            reader["Note"]?.ToString() ?? ""));
                    }
                }
            }

            if (meanings.Count == 0)
            {
                Console.WriteLine("此單字尚無意思紀錄");
                return;
            }

            for (int i = 0; i < meanings.Count; i++)
            {
                var m = meanings[i];
                Console.WriteLine($"[{i + 1}] 詞性：{m.PartOfSpeech} - {m.MeaningCn}");
            }

            Console.Write("請輸入要修改的編號：");
            if (!int.TryParse(Console.ReadLine(), out int index) || index < 1 || index > meanings.Count)
            {
                Console.WriteLine("輸入錯誤");
                return;
            }

            var selected = meanings[index - 1];
            var updated = new WordMeaning
            {
                Id = selected.Id,
                MeaningCn = selected.MeaningCn,
                ExampleSentence = selected.Example,
                Note = selected.Note
            };

            Console.Write($"新的中文意思（原：{selected.MeaningCn}）：");
            string newCn = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newCn)) updated.MeaningCn = newCn;

            Console.Write($"新的例句（原：{selected.Example}）：");
            string newEx = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newEx)) updated.ExampleSentence = newEx;

            Console.Write($"新的解釋（原：{selected.Note}）：");
            string newNote = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newNote)) updated.Note = newNote;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    bool success = _wordService.UpdateSingleMeaning(updated, conn, trans);
                    if (success)
                    {
                        trans.Commit();
                        Console.WriteLine("更新成功！");
                    }
                    else
                    {
                        trans.Rollback();
                        Console.WriteLine("更新失敗！");
                    }
                }
            }
        }

        public void DeleteWord(int userId)
        {
            Console.Write("請輸入要刪除的單字：");
            string word = Console.ReadLine()?.Trim() ?? "";

            Console.Write($"確認刪除 '{word}'？（Y/N）：");
            if (Console.ReadLine()?.Trim().ToUpper() != "Y")
            {
                Console.WriteLine("已取消");
                return;
            }

            var result = _wordService.DeleteWord(word, userId);
            Console.WriteLine(result.Success ? "刪除成功" : $"刪除失敗：{result.Message}");
        }

        private List<(string, List<WordMeaning>)> GetUsageList()
        {
            var usageList = new List<(string, List<WordMeaning>)>();

            while (true)
            {
                Console.Write("是否新增詞性？（Y/N）：");
                if (Console.ReadLine()?.Trim().ToUpper() != "Y") break;

                Console.Write("請輸入詞性（如：名詞）：");
                string partOfSpeech = Console.ReadLine()?.Trim() ?? "";
                var meanings = new List<WordMeaning>();

                Console.WriteLine("請輸入該詞性的中文意思與例句（在中文意思輸入 N 可結束）：");
                while (true)
                {
                    Console.Write("中文意思（輸入 N 結束）：");
                    string meaning = Console.ReadLine();
                    if (meaning?.Trim().ToUpper() == "N") break;
                    if (string.IsNullOrWhiteSpace(meaning))
                    {
                        Console.WriteLine("中文意思不可為空");
                        continue;
                    }

                    Console.Write("例句（可空）：");
                    string example = Console.ReadLine();

                    Console.Write("例句中文解釋（可空）：");
                    string note = Console.ReadLine();

                    meanings.Add(new WordMeaning
                    {
                        MeaningCn = meaning,
                        ExampleSentence = example,
                        Note = note
                    });
                }

                usageList.Add((partOfSpeech, meanings));
            }

            return usageList;
        }
    }
}