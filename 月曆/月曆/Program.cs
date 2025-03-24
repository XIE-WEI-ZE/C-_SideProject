using System;

class Program
{
    static void Main()
    {
        try
        {
            //使用者輸入年份與月份，並轉換成整數。
            Console.WriteLine("請輸入年份");
            int year = int.Parse(Console.ReadLine());
            Console.WriteLine("請輸入月份");
            int month = int.Parse(Console.ReadLine());

            if (month < 1 || month > 12)
            {
                Console.WriteLine("月份輸入錯誤(1~12)");
                return;
            }

            //可被 4 整除且不能被 100 整除，或可被 400 整除 ➝ 是閏年
            bool isLeapYear = (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;
            //根據月份與是否閏年，得到「這個月有幾天」。
            int daysInMonth = GetDaysInMonth(month, isLeapYear);
            Console.WriteLine($"{year}年{month}月有{daysInMonth}天");
            //利用蔡勒公式算出「這個月的 1 號是星期幾」。
            int firstDayOfWeek = GetFirstDayOfMonth(year, month);
            PrintCalender(year, month, daysInMonth, firstDayOfWeek);
        }
        catch (FormatException)
        {
            Console.WriteLine("請輸入有效的數字！");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"發生錯誤：{ex.Message}");
        }
    }

    static int GetDaysInMonth(int month, bool isLeapYear)
    {
        int[] dayInMonth = new int[] { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        if (month == 2 && isLeapYear)
        {
            return 29;
        }
        return dayInMonth[month]; // 修正：直接返回陣列中的值
    }

    static int GetFirstDayOfMonth(int year, int month)
    {
        if (month == 1 || month == 2)
        {
            month += 12;
            year--;
        }
        int k = 1;
        int m = month;
        int D = year % 100; // 修正：取年份後兩位
        int C = year / 100;
        // 蔡勒公式：W = (k + (2 * m) + (3 * (m + 1) / 5) + D + (D / 4) + (C / 4) - 2 * C) % 7
        int W = (k + (2 * m) + (3 * (m + 1) / 5) + D + (D / 4) + (C / 4) - 2 * C) % 7;
        if (W < 0)
        {
            W += 7;
        }
        // 調整：0 表示星期日，1 表示星期一，...，6 表示星期六
        W = (W + 1) % 7;
        return W;
    }

    static void PrintCalender(int year, int month, int daysInMonth, int firstDayOfWeek)
    {
        Console.WriteLine($"\n-----{year} 年 {month} 月的月曆-------");
        Console.WriteLine("日 一 二 三 四 五 六");
        for (int i = 0; i < firstDayOfWeek; i++)
        {
            Console.Write("   ");
        }
        int currentDayOfWeek = firstDayOfWeek;
        for (int day = 1; day <= daysInMonth; day++)
        {
            Console.Write($"{day,2}");
            currentDayOfWeek++;
            if (currentDayOfWeek == 7)
            {
                Console.WriteLine();
                currentDayOfWeek = 0;
            }
            else
            {
                Console.Write(" ");
            }
        }
        Console.WriteLine(); // 最後換行
    }
}