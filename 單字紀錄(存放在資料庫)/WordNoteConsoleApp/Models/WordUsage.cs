using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordNoteConsoleApp.Models
{
    public class WordUsage
    {
        public int Id { get; set; }
        public int WordNoteId { get; set; }           // 對應 WordNotes.Id
        public string PartOfSpeech { get; set; }      // 詞性（名詞、動詞等）
        public string MeaningCn { get; set; }         // 中文意思
        public DateTime CreatedAt { get; set; }
    }
}
