using System.ComponentModel;
using System.Threading;

namespace lab06;

public struct SGraphParams
{
    public int minValue;
    public int maxValue;
    public int rowSections;
    
    public SGraphParams(int minValue, int maxValue, int rowSections)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.rowSections = rowSections;
    }
}

public enum EScaleMode
{
    tenSec,
    oneMin,
    fiveMin
}

public class Program
{
    static ServerMonitor monitor = new ServerMonitor();

    static List<double> lastServerCPUData = new List<double>();
    static List<double> lastServerCPUDataOneMin = new List<double>();
    static List<double> lastServerCPUDataFiveMin = new List<double>();

    static SGraphParams CPUParams = new SGraphParams(0, 100, 10);

    static EScaleMode scaleMode;

    static double sumOfCPUOneMin;
    static double sumOfCPUFiveMin;

    static int secondsForCount;

    public static void Main()
    {
        monitor.DataReceived += OnDataReceived;

        monitor.Start();

        Console.CursorVisible = false;
        MenuInput();
 
    }

    private static void OnDataReceived(double cpu, double ram, double temperature)
    {
        secondsForCount += 1;

        CPUSum(cpu);

        switch (scaleMode)
        {
            case EScaleMode.tenSec:
                RenderGraph(lastServerCPUData, CPUParams, 10, 10, 8);
                break;
            case EScaleMode.oneMin:
                RenderGraph(lastServerCPUDataOneMin, CPUParams, 60, 20, 8); 
                break;
            case EScaleMode.fiveMin:
                RenderGraph(lastServerCPUDataFiveMin, CPUParams, 300, 20, 8);
                break;
        }
        
        UpdateLine(19, $"CPU: {cpu}%");
        UpdateLine(20, "");
        UpdateLine(21, "");
        UpdateLine(22, $"RAM: {ram}%");
        UpdateLine(23, $"Temperature: {temperature} °C");  
    }

    private static void UpdateLine(int line, string text)
    {
        Console.SetCursorPosition(0, line);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, line);
        Console.Write(text);
    }

    private static void RenderGraph(List<double> parameter, SGraphParams graphParams, int maxColumnInterval, int maxColumnSize, int line)
    {
        UpdateLine(line, $"{graphParams.maxValue}");

        int sectionSize = (graphParams.maxValue - graphParams.minValue) / graphParams.rowSections;

        for (int i = 0; i < parameter.Count + 1; i++)
        {
            if (i == 0)
            {
                for (int j = line + graphParams.rowSections - 1; j >= line + 1; j--)
                {
                    Console.SetCursorPosition(i, j);
                    Console.Write(" ");
                }
                continue;
            }
            for (int j = line + graphParams.rowSections - 1; j >= line + 1; j--)
            {
                Console.SetCursorPosition(i, j);
                if (Math.Abs(j - line - graphParams.rowSections) * sectionSize < parameter[i - 1]) Console.Write("█");
                else Console.Write(" ");
            }
        }

        UpdateLine(line + graphParams.rowSections, $"{graphParams.minValue}" + new string(' ', maxColumnSize) + $"{maxColumnInterval}");
    }

    private static void MenuInput()
    {
        Console.WriteLine("Мониторинг запущен.");
        Console.WriteLine("1. Поменять масштаб на 10 секунд");
        Console.WriteLine("2. Поменять масштаб на 1 минуту");
        Console.WriteLine("3. Поменять масштаб на 5 минут");
        Console.WriteLine("4. Выйти");
        Console.WriteLine("");

        ConsoleKeyInfo key = Console.ReadKey();
        if (key.Key == ConsoleKey.D1) scaleMode = EScaleMode.tenSec;
        if (key.Key == ConsoleKey.D2) scaleMode = EScaleMode.oneMin;
        if (key.Key == ConsoleKey.D3) scaleMode = EScaleMode.fiveMin;
        if (key.Key == ConsoleKey.D4)
        {
            monitor.Stop();
            Environment.Exit(0);
        }
   
        Console.Clear();
        MenuInput();
    }

    private static void CPUSum(double cpu)
    {
        lastServerCPUData.Add(cpu);
        if (lastServerCPUData.Count > 10) lastServerCPUData.RemoveAt(0);

        sumOfCPUOneMin += cpu;
        if (secondsForCount % 3 == 0)
        {
            sumOfCPUOneMin /= 3;
            lastServerCPUDataOneMin.Add(sumOfCPUOneMin);
            if (lastServerCPUDataOneMin.Count > 20) lastServerCPUDataOneMin.RemoveAt(0);
            sumOfCPUOneMin = 0;
        }

        sumOfCPUFiveMin += cpu;
        if (secondsForCount % 15 == 0)
        {
            sumOfCPUFiveMin /= 15;
            lastServerCPUDataFiveMin.Add(sumOfCPUFiveMin);
            if (lastServerCPUDataFiveMin.Count > 20) lastServerCPUDataFiveMin.RemoveAt(0);
            sumOfCPUFiveMin = 0;
        }
    }
}
