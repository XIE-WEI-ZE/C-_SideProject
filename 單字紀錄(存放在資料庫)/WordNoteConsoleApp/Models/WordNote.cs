using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordNoteConsoleApp.Models
{
    public class WordNote
    {
        public int Id { get; set; }             // 資料庫自動產生
        public int UserId { get; set; }         // 使用者ID（目前固定 1）
        public string Word { get; set; }        // 單字
        public string KK { get; set; }          // 音標
        public int LevelCode { get; set; }      // 等級（1~5）
        public string? Note { get; set; }       // 主註解（可空）
        public string? AudioUrl { get; set; }   // 音檔網址（目前可空）
        public DateTime CreatedAt { get; set; } // 建立時間（資料庫自動帶入）
    }
}
