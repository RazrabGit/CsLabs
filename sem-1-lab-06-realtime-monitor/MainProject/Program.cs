
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

    static List<double> lastServerCPUDataTenSec = new List<double>();
    static List<double> lastServerCPUDataOneMin = new List<double>();
    static List<double> lastServerCPUDataFiveMin = new List<double>();

    static List<double> lastServerRAMDataTenSec = new List<double>();
    static List<double> lastServerRAMDataOneMin = new List<double>();
    static List<double> lastServerRAMDataFiveMin = new List<double>();

    static List<double> lastServerTemperatureDataTenSec = new List<double>();
    static List<double> lastServerTemperatureDataOneMin = new List<double>();
    static List<double> lastServerTemperatureDataFiveMin = new List<double>();

    static SGraphParams CPUParams = new SGraphParams(0, 100, 10);
    static SGraphParams RAMParams = new SGraphParams(0, 100, 10);
    static SGraphParams TemperatureParams = new SGraphParams(0, 100, 10);

    static EScaleMode scaleMode;

    static double sumOfCPUTenSec;
    static double sumOfCPUOneMin;
    static double sumOfCPUFiveMin;

    static double sumOfRAMTenSec;
    static double sumOfRAMOneMin;
    static double sumOfRAMFiveMin;

    static double sumOfTemperatureTenSec;
    static double sumOfTemperatureOneMin;
    static double sumOfTemperatureFiveMin;

    static int secondsForCount;

    public static void Main()
    {
        monitor.DataReceived += OnDataReceived;

        monitor.Start();

        Console.CursorVisible = false;

        while (true)
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
        }
 
    }

    private static void OnDataReceived(double cpu, double ram, double temperature)
    {
        secondsForCount += 1;

        CPUSum(cpu);
        RAMSum(ram);
        TemperatureSum(temperature);

        switch (scaleMode)
        {
            case EScaleMode.tenSec:
                RenderGraph(lastServerCPUDataTenSec, CPUParams, 200, 20, 8, 0);
                RenderGraph(lastServerRAMDataTenSec, RAMParams, 200, 20, 8, 30);
                RenderGraph(lastServerTemperatureDataTenSec, TemperatureParams, 200, 20, 8, 60);
                break;
            case EScaleMode.oneMin:
                RenderGraph(lastServerCPUDataOneMin, CPUParams, 1200, 20, 8, 0); 
                RenderGraph(lastServerRAMDataOneMin, RAMParams, 1200, 20, 8, 30); 
                RenderGraph(lastServerTemperatureDataOneMin, TemperatureParams, 1200, 20, 8, 60); 
                break;
            case EScaleMode.fiveMin:
                RenderGraph(lastServerCPUDataFiveMin, CPUParams, 6000, 20, 8, 0);
                RenderGraph(lastServerRAMDataFiveMin, RAMParams, 6000, 20, 8, 30);
                RenderGraph(lastServerTemperatureDataFiveMin, TemperatureParams, 6000, 20, 8, 60);
                break;
        }
        
        UpdateLine(19, 0, $"CPU: {cpu}%");
        //UpdateLine(20, "");
        //UpdateLine(21, "");
        UpdateLine(19, 30, $"RAM: {ram}%");
        UpdateLine(19, 60, $"Temperature: {temperature} °C");  
    }

    private static void UpdateLine(int lineY, int lineX, string text)
    {
        Console.SetCursorPosition(lineX, lineY);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(lineX, lineY);
        Console.Write(text);
    }

    private static void RenderGraph(List<double> parameter, SGraphParams graphParams, int maxColumnInterval, int maxColumnSize, int lineY, int lineX)
    {
        UpdateLine(lineY, lineX, $"{graphParams.maxValue}");

        int sectionSize = (graphParams.maxValue - graphParams.minValue) / graphParams.rowSections;

        for (int i = lineX; i < parameter.Count + lineX + 1; i++)
        {
            if (i == lineX)
            {
                for (int j = lineY + graphParams.rowSections - 1; j >= lineY + 1; j--)
                {
                    Console.SetCursorPosition(i, j);
                    Console.Write(" ");
                }
                continue;
            }
            for (int j = lineY + graphParams.rowSections - 1; j >= lineY + 1; j--)
            {
                Console.SetCursorPosition(i, j);
                if (Math.Abs(j - lineY - graphParams.rowSections) * sectionSize < parameter[i - lineX - 1]) Console.Write("█");
                else Console.Write(" ");
            }
        }

        UpdateLine(lineY + graphParams.rowSections, lineX, $"{graphParams.minValue}" + new string(' ', maxColumnSize) + $"{maxColumnInterval}");
    }

    private static void CPUSum(double cpu)
    {
        sumOfCPUTenSec += cpu;
        if (secondsForCount % 10 == 0)
        {
            sumOfCPUTenSec /= 10;
            lastServerCPUDataTenSec.Add(sumOfCPUTenSec);
            if (lastServerCPUDataTenSec.Count > 20) lastServerCPUDataTenSec.RemoveAt(0);
            sumOfCPUTenSec = 0;
        }

        sumOfCPUOneMin += cpu;
        if (secondsForCount % 60 == 0)
        {
            sumOfCPUOneMin /= 60;
            lastServerCPUDataOneMin.Add(sumOfCPUOneMin);
            if (lastServerCPUDataOneMin.Count > 20) lastServerCPUDataOneMin.RemoveAt(0);
            sumOfCPUOneMin = 0;
        }

        sumOfCPUFiveMin += cpu;
        if (secondsForCount % 300 == 0)
        {
            sumOfCPUFiveMin /= 300;
            lastServerCPUDataFiveMin.Add(sumOfCPUFiveMin);
            if (lastServerCPUDataFiveMin.Count > 20) lastServerCPUDataFiveMin.RemoveAt(0);
            sumOfCPUFiveMin = 0;
        }
    }

    private static void RAMSum(double ram)
    {
        sumOfRAMTenSec += ram;
        if (secondsForCount % 10 == 0)
        {
            sumOfRAMTenSec /= 10;
            lastServerRAMDataTenSec.Add(sumOfRAMTenSec);
            if (lastServerRAMDataTenSec.Count > 20) lastServerRAMDataTenSec.RemoveAt(0);
            sumOfRAMTenSec = 0;
        }

        sumOfRAMOneMin += ram;
        if (secondsForCount % 60 == 0)
        {
            sumOfRAMOneMin /= 60;
            lastServerRAMDataOneMin.Add(sumOfRAMOneMin);
            if (lastServerRAMDataOneMin.Count > 20) lastServerRAMDataOneMin.RemoveAt(0);
            sumOfRAMOneMin = 0;
        }

        sumOfRAMFiveMin += ram;
        if (secondsForCount % 300 == 0)
        {
            sumOfRAMFiveMin /= 300;
            lastServerRAMDataFiveMin.Add(sumOfRAMFiveMin);
            if (lastServerRAMDataFiveMin.Count > 20) lastServerRAMDataFiveMin.RemoveAt(0);
            sumOfRAMFiveMin = 0;
        }
    }

    private static void TemperatureSum(double temperature)
    {
        sumOfTemperatureTenSec += temperature;
        if (secondsForCount % 10 == 0)
        {
            sumOfTemperatureTenSec /= 10;
            lastServerTemperatureDataTenSec.Add(sumOfTemperatureTenSec);
            if (lastServerTemperatureDataTenSec.Count > 20) lastServerTemperatureDataTenSec.RemoveAt(0);
            sumOfTemperatureTenSec = 0;
        }

        sumOfTemperatureOneMin += temperature;
        if (secondsForCount % 60 == 0)
        {
            sumOfTemperatureOneMin /= 60;
            lastServerTemperatureDataOneMin.Add(sumOfTemperatureOneMin);
            if (lastServerTemperatureDataOneMin.Count > 20) lastServerTemperatureDataOneMin.RemoveAt(0);
            sumOfTemperatureOneMin = 0;
        }

        sumOfTemperatureFiveMin += temperature;
        if (secondsForCount % 300 == 0)
        {
            sumOfTemperatureFiveMin /= 300;
            lastServerTemperatureDataFiveMin.Add(sumOfTemperatureFiveMin);
            if (lastServerTemperatureDataFiveMin.Count > 20) lastServerTemperatureDataFiveMin.RemoveAt(0);
            sumOfTemperatureFiveMin = 0;
        }
    }
}
