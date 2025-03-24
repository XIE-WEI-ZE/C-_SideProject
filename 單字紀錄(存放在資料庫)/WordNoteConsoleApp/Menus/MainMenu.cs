using System;
using WordNoteConsoleApp.Models;
using WordNoteConsoleApp.Services;

namespace WordNoteConsoleApp.Menus
{
    public class MainMenu
    {
        private readonly WordMenu _wordMenu;

        public MainMenu(WordMenu wordMenu)
        {
            _wordMenu = wordMenu;
        }

        public void Show(User currentUser)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== 單字筆記系統 ===");
                Console.WriteLine("1. 新增單字");
                Console.WriteLine("2. 查詢單字");
                Console.WriteLine("3. 修改單字");
                Console.WriteLine("4. 刪除單字");
                Console.WriteLine("5. 登出");
                Console.Write("請選擇：");

                string input = Console.ReadLine()?.Trim();
                switch (input)
                {
                    case "1":
                        _wordMenu.AddWordFlow(currentUser.UserId);
                        break;
                    case "2":
                        _wordMenu.ShowSearchMenu(currentUser.UserId);
                        break;
                    case "3":
                        _wordMenu.EditMenu(currentUser.UserId);
                        break;
                    case "4":
                        _wordMenu.DeleteWord(currentUser.UserId);
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("請輸入有效選項");
                        break;
                }

                Console.WriteLine("\n請按任意鍵繼續...");
                Console.ReadKey();
            }
        }
    }
}
