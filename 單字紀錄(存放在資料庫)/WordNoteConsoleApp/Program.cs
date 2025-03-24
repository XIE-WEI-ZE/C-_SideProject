using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using WordNoteConsoleApp.DataAccess;
using WordNoteConsoleApp.Models;
using WordNoteConsoleApp.Services;
using WordNoteConsoleApp.Menus;

class Program
{
    static void Main(string[] args)
    {
        // 讀取 appsettings.json
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string connectionString = config.GetConnectionString("WordNoteDB")
            ?? throw new Exception("找不到資料庫連線字串，請確認 appsettings.json 設定正確");

        // 建立 Repository 與 Service
        var userRepo = new UserRepository(connectionString);
        var noteRepo = new WordNoteRepository(connectionString);
        var usageRepo = new WordUsageRepository(connectionString);
        var meaningRepo = new WordMeaningRepository(connectionString);

        var userService = new UserService(userRepo);
        var wordService = new WordService(noteRepo, usageRepo, meaningRepo, connectionString);

        // 建立 Menu（畫面層）
        var wordMenu = new WordMenu(wordService);                    // 
        var loginMenu = new LoginMenu(userService);                  //UserService
        var mainMenu = new MainMenu(wordMenu);                       //WordMenu

        // 登入流程
        while (true)
        {
            var currentUser = loginMenu.Show();// 顯示登入 / 註冊流程
            if (currentUser == null) break;// 使用者選擇離開

            mainMenu.Show(currentUser);// 登入成功後進入主選單
        }
    }
}
