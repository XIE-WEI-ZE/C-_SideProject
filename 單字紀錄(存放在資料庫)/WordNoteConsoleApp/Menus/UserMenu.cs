using WordNoteConsoleApp.Models;
using WordNoteConsoleApp.Services;
using System;

namespace WordNoteConsoleApp.Menus
{
    public class UserMenu
    {
        private readonly UserService _userService;

        public UserMenu(UserService userService)
        {
            _userService = userService;
        }

        public User ShowLoginOrRegister(string action)
        {
            while (true)
            {
                if (action == "1")
                {
                    Console.Write("帳號：");
                    string account = Console.ReadLine()?.Trim() ?? "";

                    Console.Write("密碼：");
                    string password = ReadPassword();

                    var user = _userService.Login(account, password);
                    if (user != null)
                    {
                        Console.WriteLine($"登入成功，歡迎 {user.Name}！");
                        return user;
                    }
                    else
                    {
                        Console.WriteLine("登入失敗，帳號或密碼錯誤！");
                        return null;
                    }
                }
                else if (action == "2")
                {
                    Console.WriteLine("若要取消註冊，請在帳號輸入 -1。");
                    Console.Write("姓名：");
                    string name = Console.ReadLine()?.Trim() ?? "";

                    string account;
                    while (true)
                    {
                        Console.Write("帳號：");
                        account = Console.ReadLine()?.Trim() ?? "";

                        if (account == "-1")
                        {
                            Console.WriteLine("已取消註冊，返回主選單。");
                            return null;
                        }

                        // 利用 Login 驗證是否已存在帳號
                        if (_userService.Login(account, "wrong") == null)
                        {
                            break;
                        }

                        Console.WriteLine("此帳號已存在，請重新輸入！");
                    }

                    Console.Write("密碼：");
                    string password = ReadPassword();

                    bool success = _userService.Register(name, account, password);
                    Console.WriteLine(success ? "註冊成功！請重新登入。" : "註冊失敗！");
                    return null;
                }
                else
                {
                    Console.WriteLine("無效的選擇，返回主選單。");
                    return null;
                }
            }
        }

        private string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[..^1];
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }
    }
}
