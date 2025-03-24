using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordNoteConsoleApp.Models
{
    public class User
    {
        public int UserId { get; set; }                 // 主鍵
        public string Name { get; set; }                // 使用者姓名
        public string Account { get; set; }             // 登入帳號
        public string PasswordHash { get; set; }        // 密碼雜湊（加鹽後雜湊）
        public string Salt { get; set; }                // 加鹽字串
    }
}
