using System;

class Program
{
    static void Main()
    {
        // 輸入矩陣的行數與列數，限制在 1~10 之間
        int rows = GetValidInput("請輸入矩陣的行數 (1~10): ", 1, 10);
        int cols = GetValidInput("請輸入矩陣的列數 (1~10): ", 1, 10);

        // 建立原始矩陣
        int[,] matrix = new int[rows, cols];

        // 提示使用者輸入每一列的數字
        Console.WriteLine($"\n請輸入矩陣內容（逐行輸入，每行 {cols} 個整數，以空格分隔）：");
        for (int i = 0; i < rows; i++)
        {
            // 呼叫方法取得每一列輸入，並存入 matrix 中
            matrix = GetRowInput(matrix, i, cols);
        }

        // 輸出原始矩陣
        Console.WriteLine("\n原始矩陣：");
        PrintMatrix(matrix);

        // 轉置後輸出新矩陣
        Console.WriteLine("\n轉置後的矩陣：");
        PrintMatrix(TransposeMatrix(matrix));

        // 程式結束提示
        Console.Write("\n按 Enter 鍵退出...");
        Console.ReadLine();
    }

    // 驗證輸入是否為範圍內的整數
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

    // 取得指定列的輸入並轉成整數陣列
    static int[,] GetRowInput(int[,] matrix, int rowIndex, int cols)
    {
        while (true)
        {
            Console.Write($"第 {rowIndex + 1} 行: ");
            string[] input = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // 檢查輸入是否剛好為指定欄數
            if (input.Length != cols)
            {
                Console.WriteLine($"請輸入剛好 {cols} 個數字！");
                continue;
            }

            try
            {
                // 將每個數字轉成整數，存入 matrix 中
                for (int j = 0; j < cols; j++)
                    matrix[rowIndex, j] = int.Parse(input[j]);

                return matrix;
            }
            catch
            {
                // 若輸入無法轉成整數則提示錯誤
                Console.WriteLine("請確保輸入的是有效的整數！");
            }
        }
    }

    // 將原始矩陣轉置（行列互換）
    static int[,] TransposeMatrix(int[,] matrix)
    {
        int rows = matrix.GetLength(0); // 原始行數
        int cols = matrix.GetLength(1); // 原始列數

        int[,] transposed = new int[cols, rows]; // 建立轉置後的新矩陣

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                transposed[j, i] = matrix[i, j]; // 將 [i, j] 轉為 [j, i]

        return transposed;
    }

    // 輸出矩陣內容，格式整齊對齊
    static void PrintMatrix(int[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        Console.WriteLine(new string('-', cols * 5)); // 輸出分隔線

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                // 每個數值輸出寬度固定 4 格，避免排版混亂
                Console.Write($"{matrix[i, j],4} ");
            }
            Console.WriteLine();
        }

        Console.WriteLine(new string('-', cols * 5)); // 再輸出一次分隔線
    }
}
