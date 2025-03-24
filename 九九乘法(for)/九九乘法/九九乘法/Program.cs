using System;

class Program
{
    static void Main(string[] args)
    {
        int rowStart = GetValidInput("輸入直列的起始值(1~9): ", 1, 9);
        int colStart = GetValidInput("輸入橫行的起始值(1~9): ", 1, 9);
        int rowSize = GetValidInput("輸入直列的長度(1~9): ", 1, 9);
        int colSize = GetValidInput("輸入橫行的長度(1~9): ", 1, 9);

        int rowEnd = Math.Min(rowStart + rowSize - 1, 9); //Math.Min() 一行解決 if 判斷後調整
        int colEnd = Math.Min(colStart + colSize - 1, 9);

        Console.WriteLine($"\n橫行 col 的起始值: {colStart}");
        Console.WriteLine($"直列 row 的起始值: {rowStart}\n");

        for (int col = colStart; col <= colEnd; col++)
        {
            for (int row = rowStart; row <= rowEnd; row++)
            {
                string expression = $"{row}*{col}={row * col}";
                Console.Write(expression.PadRight(10)); // 統一寬度排版
            }
            Console.WriteLine();
        }

        Console.Write("\n按下 Enter 鍵退出...");
        Console.ReadLine();
    }

    static int GetValidInput(string message, int min, int max)
    {
        while (true)
        {
            Console.Write(message);
            if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
                return value;
            Console.WriteLine($"請輸入 {min}~{max} 之間的整數！");
        }
    }
}
