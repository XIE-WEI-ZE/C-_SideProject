using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordNoteConsoleApp.Models
{
    public class WordMeaning
    {
        public int Id { get; set; }
        public int UsageId { get; set; }              // 對應 WordUsages.Id
        public string? Phrase { get; set; }           // 片語
        public string MeaningCn { get; set; }         // 中文意思
        public string? ExampleSentence { get; set; }  // 例句
        public string? Note { get; set; }             // 補充說明
        public DateTime CreatedAt { get; set; }
    }
}

