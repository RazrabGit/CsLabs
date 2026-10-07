namespace lab04;

public struct LogEntry
{
    public DateTime Timestamp;
    public string Level;
    public string Category;
    public string Message;
}

public class Program
{
    public static void Main()
    {
        string[] lines = File.ReadAllLines("event_server.log");
        LogEntry[] logEntries = ParseLog(lines);

        // Отладка статуса
        Console.WriteLine(GetServerStatus(logEntries));

        // Отладка функции экспорта
        logEntries = FilterByCategory(logEntries, "Server");
        ExportFiltered("filtered_event_server.log", logEntries);
    }
    public static LogEntry GetLogParts(string line)
    {
        LogEntry logEntry = new LogEntry();

        int index1 = line.IndexOf(' ');
        int index2 = line.IndexOf(' ', index1 + 1);
        logEntry.Timestamp = DateTime.Parse(line.Substring(0, index2));

        index1 = line.IndexOf('[');
        index2 = line.IndexOf(']', index1);
        logEntry.Level = line.Substring(index1 + 1, index2 - (index1 + 1));

        index1 = line.IndexOf('[', index2);
        index2 = line.IndexOf(']', index1);
        logEntry.Category = line.Substring(index1 + 1, index2 - (index1 + 1));

        logEntry.Message = line.Substring(index2 + 2);

        return logEntry;
    }

    public static LogEntry[] ParseLog(string[] lines)
    {
        List<LogEntry> logEntries = new List<LogEntry>();
        for (int i = 0; i < lines.Length; i++)
        {
            logEntries.Add(GetLogParts(lines[i]));
        }
        return logEntries.ToArray();
    }

    public static LogEntry[] FilterByDate(LogEntry[] entries, DateTime date)
    {
        List<LogEntry> logEntries = new List<LogEntry>();
        for (int i = 0; i < entries.Length; i++)
        {
            LogEntry entry = entries[i];
            if (entry.Timestamp.Date == date.Date) logEntries.Add(entry);
        }
        return logEntries.ToArray();
    }

    public static LogEntry[] FilterByLevel(LogEntry[] entries, string level)
    {
        List<LogEntry> logEntries = new List<LogEntry>();
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Level ==level) logEntries.Add(entries[i]);
        }
        return logEntries.ToArray();
    }

    public static LogEntry[] FilterByCategory(LogEntry[] entries, string category)
    {
        List<LogEntry> logEntries = new List<LogEntry>();
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Category == category) logEntries.Add(entries[i]);
        }
        return logEntries.ToArray();
    }

    public static LogEntry[] Search(LogEntry[] entries, string text)
    {
        List<LogEntry> logEntries = new List<LogEntry>();
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Message.Contains(text, StringComparison.OrdinalIgnoreCase)) logEntries.Add(entries[i]);
        }
        return logEntries.ToArray();
    }

    public static int CountByLevel(LogEntry[] entries, string level)
    {
        int count = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Level == level) count++;
        }
        return count;
    }

    public static string GetServerStatus(LogEntry[] entries)
    {
        // Можно было в цикле отдельную проверку сделать и брейкать, но я решил выпендриться
        if (entries.Any(t => t.Level == "Fatal" && t.Category == "Server")) return "КРИТИЧЕСКАЯ ОШИБКА: сервер остановлен";
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Level == "Warning" || entries[i].Level == "Fatal") return "Есть ошибки: требуется проверка";
        }
        return "Сервер работает штатно";
    }

    public static void ExportFiltered(string path, LogEntry[] entries)
    {
        List<string> lines = new List<string>();
        foreach (LogEntry entry in entries)
        {
            lines.Add($"{entry.Timestamp} [{entry.Level}][{entry.Category}] {entry.Message}");
        }
        // Там было кучу разных методов создания файлов, этот показался мне наиболее удобным. По крайней мере, оно работает
        File.WriteAllLines(path, lines.ToArray());
    }
}
