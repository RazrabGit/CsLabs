namespace practice2;

class Program
{
    public static (DateTime, string, string, string) GetLogParts(string line)
    {
        int index1 = line.IndexOf(' ');
        int index2 = line.IndexOf(' ', index1 + 1);
        string datetimeStr = line.Substring(0, index2);
        DateTime dateTime = DateTime.Parse(datetimeStr);

        int index3 = line.IndexOf('[');
        int index4 = line.IndexOf(']');
        string lineType = line.Substring(index3 + 1, index4 - (index3 + 1));

        int index5 = line.IndexOf('[', index3 + 1);
        int index6 = line.IndexOf(']', index4 + 1);
        string tag1 = line.Substring(index5 + 1, index6 - (index5 + 1));

        string otherText = line.Substring(index6 + 1, line.Length - (1 + index6));

        return (dateTime, lineType, tag1, otherText);
    }

    public static void Main()
    {
        string[] lines = File.ReadAllLines("event_server.log");
        List<(DateTime, string, string, string)> sosalovo = [];

        for (int i = 0; i < lines.Length; i++) 
        {
            sosalovo.Add(GetLogParts(lines[i]));
            
        }

        // Он правда и так сортирован по дате, но ладно
        var sortedsosalovo = sosalovo.OrderBy(t => t.Item1).ToList();

        foreach (var s in sortedsosalovo)
        {
            Console.WriteLine(s);
        }
    }
}
