using System;
using WordNoteConsoleApp.Models;
using WordNoteConsoleApp.Services;

namespace WordNoteConsoleApp.Menus
{
    public class LoginMenu
    {
        private readonly UserService _userService;

        public LoginMenu(UserService userService)
        {
            _userService = userService;
        }

        public User? Show()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== 單字筆記系統 ===");
                Console.WriteLine("1. 登入");
                Console.WriteLine("2. 註冊");
                Console.WriteLine("3. 離開");
                Console.Write("請選擇：");
                string option = Console.ReadLine()?.Trim();

                if (option == "3") return null;

                // ✅ 建立 UserMenu 並傳入 _userService
                var userMenu = new UserMenu(_userService);
                var currentUser = userMenu.ShowLoginOrRegister(option);

                if (currentUser != null)
                    return currentUser;

                Console.WriteLine("登入或註冊失敗，請重新嘗試...");
                Console.WriteLine("請按任意鍵繼續...");
                Console.ReadKey();
            }
        }
    }
}
