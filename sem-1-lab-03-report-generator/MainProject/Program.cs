namespace lab03;

public enum EEvent
{
    GayParade,
    Match,
    GiftSending,
}

public struct SDiscordMessage
{
    public EEvent gameEvent;
    public byte maxPlayers;
    public byte curPlayers;
    public byte playersToStart;
    public string winner;
    public byte sessionTime;
    public DateTime sessionEndTime;
    public string prize;
}

public class Program
{
    
    static void Input(ref SDiscordMessage message)
    {
        Console.Clear();
        Random rand = new Random();

        Console.WriteLine("Choose event");
        Console.WriteLine("0. Gay parade");
        Console.WriteLine("1. Match");
        Console.WriteLine("2. GiftSending");
        if (byte.TryParse(Console.ReadLine(), out byte gameEvent)) message.gameEvent = (EEvent)gameEvent;
        else message.gameEvent = (EEvent)rand.Next(0, 3);
        Console.Clear();

        Console.WriteLine("Do we have a winner?");
        Console.WriteLine("0. No");
        Console.WriteLine("1. Yes");
        if (bool.TryParse(Console.ReadLine() == "1" ? "True" : "False", out bool bHave))
        {
            if (bHave)
            {
                Console.Clear();
                Console.WriteLine("Write winner nickname: ");
                message.winner = Console.ReadLine();
            }
            else message.winner = null;
        }
        else message.winner = null;
        Console.Clear();

        if (message.winner != null)
        {
            Console.WriteLine("Choose Prize: ");
            Console.WriteLine("0. Bag of chips");
            Console.WriteLine("1. Salmon");
            Console.WriteLine("2. 1 рубль");
            Console.WriteLine("3. Smell rtx 5090");
            Console.WriteLine("4. Nothing");
            if (byte.TryParse(Console.ReadLine(), out byte choice)) message.prize = ChoosePrize(choice);
            else message.prize = ChoosePrize((byte)rand.Next(0, 5));
        }
        else message.prize = "Nothing";
        Console.Clear();

        switch (message.gameEvent)
        {
            case EEvent.GayParade:
                message.maxPlayers = 100;
                message.playersToStart = 20;
                break;
            case EEvent.Match:
                message.maxPlayers = 120;
                message.playersToStart = 10;
                break;
            case EEvent.GiftSending:
                message.maxPlayers = 30;
                message.playersToStart = 5;
                break;
        }

        message.curPlayers = (byte)rand.Next(0, message.maxPlayers + 1);
        message.sessionTime = (byte)rand.Next(0, 3);
        message.sessionEndTime = DateTime.Now;
    }

    public static void SendMessage(ref SDiscordMessage message, bool bbriefly)
    {
        Console.Clear();

        string eventName = "event";
        switch (message.gameEvent)
        {
            case EEvent.GayParade:
                eventName = "Gay parade";
                break;
            case EEvent.Match:
                eventName = "Match";
                break;
            case EEvent.GiftSending:
                eventName = "GiftSending";
                break;

        }

        bool checkedPlayers = CheckSuccessByPlayers(message.curPlayers, message.playersToStart, out string playersMessage);
        bool checkedTime = CheckSuccessByTime(message.sessionTime, out string timeMessage);


        if (bbriefly)
        {
            Console.WriteLine($"Brief results about the {eventName}:\n");
            Console.WriteLine($"\u2022 Event success: {(checkedPlayers && checkedTime)}");
            Console.WriteLine($"\u2022 Event end time: {message.sessionEndTime}");
            if (message.winner != null && (checkedPlayers && checkedTime)) Console.WriteLine($"\u2022 Winner: @{message.winner}");
            else Console.WriteLine($"\u2022 Winner: None (draw)");
        }
        else 
        {
            Console.WriteLine($"{eventName} is over! Results:\n");
            Console.WriteLine($"\u2022 Event success: {(checkedPlayers && checkedTime)}");
            Console.WriteLine($"\u2022 Players: {message.curPlayers}/{message.maxPlayers}");
            Console.WriteLine($"\u2022 Event time: {message.sessionTime} hour{(message.sessionTime == 1 ? "" : "s" )}");
            Console.WriteLine($"\u2022 Event end time: {message.sessionEndTime}");
            if (message.winner != null && (checkedPlayers && checkedTime)) Console.WriteLine($"\u2022 Winner: @{message.winner}");
            else Console.WriteLine($"\u2022 Winner: None (draw)");
            Console.WriteLine($"\u2022 Prize: {message.prize}\n");
            if (!(checkedPlayers && checkedTime))
            {
                Console.WriteLine("Reasons of failure:\n");
                if (playersMessage != "") Console.WriteLine($"\u2022 {playersMessage}");
                if (timeMessage != "") Console.WriteLine($"\u2022 {timeMessage}");
            }
        }
    }

    public static string ChoosePrize(byte choice)
    {
        switch (choice)
        {
            case 0:
                return "Bag of chips";
            case 1:
                return "Salmon";
            case 2:
                return "1 рубль";
            case 3:
                return "Smell rtx 5090";
            case 4:
                return "Nothing";

        }
        return "Nothing";
    }

    public static bool CheckSuccessByPlayers(byte players, byte playersToStart, out string message)
    {
        if (players < playersToStart)
        {
            message = "Not enough players to start the event";
            return false;
        }
        message = null;
        return true;
    }

    public static bool CheckSuccessByTime(byte time, out string message)
    {
        if (time == 0)
        {
            message = "The event lasted less than an hour";
            return false;
        }
        message = null;
        return true;
    }

    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        SDiscordMessage message = new SDiscordMessage();

        Input(ref message);

        Console.WriteLine("How to format the report?");
        Console.WriteLine("0. In detail");
        Console.WriteLine("1. Briefly");
        if (bool.TryParse(Console.ReadLine() == "1" ? "True" : "False", out bool bBriefly)) SendMessage(ref message, bBriefly);
        else SendMessage(ref message, false);
    }
}