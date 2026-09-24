
namespace lab02;

public enum EGameMaps
{
    Savanna, // Defuse, Deadmatch, Default
    DristII, // Defuse, Robbery, Deadmatch, Default
    MeatFactory, // Defuse, Robbery, Hostage, Deadmatch, Default
    Backrooms, // BattleRoyal, Deadmatch, Default
    Test01, // Default
    Sewerage_Beta // Deadmatch, Default
}
public enum EServerAccessibility
{
    Public,
    ForFriends,
    Private
}
public enum EGamemode
{
    Defuse,
    Hostage,
    Robbery,
    Deadmatch,
    BattleRoyal,
    Default
}

public struct SServerProperties
{
    public byte maxPlayers;
    public EGameMaps map;
    public bool bHavePassword;
    public string password;
    public EServerAccessibility accessibility;
    public EGamemode gamemode;
}

public class Program
{
    static void InputByYourself(ref SServerProperties serverConfig)
    {
        Random rand = new Random();

        Console.WriteLine("Set max players:");
        if (byte.TryParse(Console.ReadLine(), out byte maxPlayers)) serverConfig.maxPlayers = maxPlayers; 
        else serverConfig.maxPlayers = (byte)rand.Next(1, 129);

        Console.WriteLine("Choose map");
        Console.WriteLine("0. Savanna");
        Console.WriteLine("1. Drist II");
        Console.WriteLine("2. Meat Factory");
        Console.WriteLine("3. Backrooms");
        Console.WriteLine("4. Test01");
        Console.WriteLine("5. Sewerage Beta");
        if (byte.TryParse(Console.ReadLine(), out byte map)) serverConfig.map = (EGameMaps)map;
        else serverConfig.map = (EGameMaps)rand.Next(0, 6);

        Console.WriteLine("Choose server accessibility");
        Console.WriteLine("0. Public");
        Console.WriteLine("1. For friedns");
        Console.WriteLine("2. Private");
        if (byte.TryParse(Console.ReadLine(), out byte accessibility)) serverConfig.accessibility = (EServerAccessibility)accessibility;
        else serverConfig.accessibility = (EServerAccessibility)rand.Next(0, 3);

        Console.WriteLine("Does server have password?");
        Console.WriteLine("0. No");
        Console.WriteLine("1. Yes");
        if (bool.TryParse(Console.ReadLine() == "1" ? "True" : "False", out bool bHave)) serverConfig.bHavePassword = bHave;

        if (serverConfig.bHavePassword)
        {
            Console.WriteLine("Set password:");
            serverConfig.password = Console.ReadLine();
        }
        else serverConfig.password = "";

        Console.WriteLine("Choose gamemode");
        Console.WriteLine("0. Defuse");
        Console.WriteLine("1. Hostage");
        Console.WriteLine("2. Robbery");
        Console.WriteLine("3. Deadmatch");
        Console.WriteLine("4. Battle royal");
        Console.WriteLine("5. Default");
        if (byte.TryParse(Console.ReadLine(), out byte gamemode)) serverConfig.gamemode = (EGamemode)gamemode;
        else serverConfig.gamemode = (EGamemode)rand.Next(0, 6);
    }

   static void Randomize(ref SServerProperties serverConfig)
   {
       Random rand = new Random();
       serverConfig.maxPlayers = (byte)rand.Next(1, 129);
       serverConfig.map = (EGameMaps)rand.Next(0, 6);
       serverConfig.bHavePassword = rand.Next(0, 2) == 1 ? true : false;
       byte randomPassword = (byte)rand.Next(0, 4);
       switch (randomPassword)
       {
           case 0:
               serverConfig.password = "1111";
               break;
           case 1:
               serverConfig.password = "FUCKTHISSHIT9872";
               break;
           case 2:
               serverConfig.password = "HFP(*& *&!)_";
               break;
            case 3:
                serverConfig.password = "";
                break;
        }
       serverConfig.accessibility = (EServerAccessibility)rand.Next(0, 3);
       serverConfig.gamemode = (EGamemode)rand.Next(0, 6);
    }

    public static bool CheckMaxPlayers(byte maxPlayers, EGamemode gamemode, out string message)
    {
        byte playerNeededToStart = 0;
        byte maxPlayersForGamemode = 0;
        switch (gamemode)
        {
            case EGamemode.Hostage:
                playerNeededToStart = 2;
                maxPlayersForGamemode = 10;
                break;
            case EGamemode.Defuse:
                playerNeededToStart = 2;
                maxPlayersForGamemode = 10;
                break;
            case EGamemode.Robbery:
                playerNeededToStart = 2;
                maxPlayersForGamemode = 5;
                break;
            case EGamemode.Deadmatch:
                playerNeededToStart = 2;
                maxPlayersForGamemode = 128;
                break;
            case EGamemode.BattleRoyal:
                playerNeededToStart = 16;
                maxPlayersForGamemode = 128;
                break;
            case EGamemode.Default:
                playerNeededToStart = 2;
                maxPlayersForGamemode = 128;
                break;
        }

        if (maxPlayers > maxPlayersForGamemode)
        {
            message = $"Too much players to play {gamemode}";
            return false;
        }
        else if (maxPlayers < playerNeededToStart)
        {
            message = $"Too low players to play {gamemode}";
            return false;
        }
        else
        {
            message = "";
            return true;
        }
    }

    public static bool CheckGameMaps(EGameMaps map, EGamemode gamemode, out string message)
    {
        switch (map)
        {
            case EGameMaps.Savanna:
                if (gamemode == EGamemode.Defuse || gamemode == EGamemode.Deadmatch || gamemode == EGamemode.Default)
                {
                    message = "";
                    return true;
                }
                break;
            case EGameMaps.DristII:
                if (gamemode == EGamemode.Defuse || gamemode == EGamemode.Deadmatch || gamemode == EGamemode.Default || gamemode == EGamemode.Robbery)
                {
                    message = "";
                    return true;
                }
                break;
            case EGameMaps.MeatFactory:
                if (gamemode == EGamemode.Defuse || gamemode == EGamemode.Deadmatch || gamemode == EGamemode.Default || gamemode == EGamemode.Robbery 
                    || gamemode == EGamemode.Hostage)
                {
                    message = "";
                    return true;
                }
                break;
            case EGameMaps.Backrooms:
                if (gamemode == EGamemode.BattleRoyal || gamemode == EGamemode.Deadmatch || gamemode == EGamemode.Default)
                {
                    message = "";
                    return true;
                }
                break;
            case EGameMaps.Sewerage_Beta:
                if (gamemode == EGamemode.BattleRoyal || gamemode == EGamemode.Deadmatch || gamemode == EGamemode.Default)
                {
                    message = "Sewerage_Beta not fully tested, may have some bugs";
                    return true;
                }
                break;
            case EGameMaps.Test01:
                message = "Test01 is a wrong map for playing!";
                return false;
        }
        message = $"{gamemode} is a wrong gamemode for map {map}";
        return false;
    }

    public static bool CheckAccessibility(bool bHavePassword, string password, EServerAccessibility accessibility, out string message)
    {
        switch (accessibility)
        {
            case EServerAccessibility.Public:
                if (bHavePassword && password != "")
                {
                    message = "What kind of idiot put a password on a public server?";
                    return false;
                }
                else
                {
                    message = "";
                    return true;
                }
            case EServerAccessibility.ForFriends:
                if (bHavePassword)
                {
                    if (password == "")
                    {
                        message = "Password is empty, when it is excepted.";
                        return false;
                    }
                    if (password.All(c => c == '1'))
                    {
                        message = "Password is too easy, not so private, huh?";
                        return true;
                    }
                    message = "";
                    return true;
                }
                else
                {
                    message = "";
                    return true;
                }
                break;
            case EServerAccessibility.Private:
                if (bHavePassword)
                {
                    if (password == "")
                    {
                        message = "Password is empty, when it is excepted.";
                        return false;
                    }
                    if (password.All(c => c == '1'))
                    {
                        message = "Password is too easy, not so private, huh?";
                        return true;
                    }
                    message = "";
                    return true;
                }
                else
                {
                    message = "Private server has no password, unable to join the server";
                    return false;
                }
        }
        message = "Failed to check accessibility";
        return false;
    }

   public static void Main()
    {
        SServerProperties serverConfig = new SServerProperties();

        Console.WriteLine("Do you want to get the random config values, or you want to set it by yourself");
        Console.WriteLine("0. Random");
        Console.WriteLine("1. Yourself");
        if (int.TryParse(Console.ReadLine(), out int result))
        {
            switch (result)
            {
                case 0:
                    Randomize(ref serverConfig);
                    Console.WriteLine("");
                    Console.WriteLine("Server config:");
                    Console.WriteLine("");
                    Console.WriteLine($"Max players: {serverConfig.maxPlayers}");
                    Console.WriteLine($"Map: {serverConfig.map}");
                    Console.WriteLine($"Accessibility: {serverConfig.accessibility}");
                    Console.WriteLine($"Have password: {serverConfig.bHavePassword}");
                    Console.WriteLine($"Password: {serverConfig.password}");
                    Console.WriteLine($"Gamemode: {serverConfig.gamemode}");
                    Console.WriteLine("");
                    break;
                case 1:
                    InputByYourself(ref serverConfig);
                    Console.WriteLine("");
                    Console.WriteLine("Server config:");
                    Console.WriteLine("");
                    Console.WriteLine($"Max players: {serverConfig.maxPlayers}");
                    Console.WriteLine($"Map: {serverConfig.map}");
                    Console.WriteLine($"Accessibility: {serverConfig.accessibility}");
                    Console.WriteLine($"Have password: {serverConfig.bHavePassword}");
                    Console.WriteLine($"Password: {serverConfig.password}");
                    Console.WriteLine($"Gamemode: {serverConfig.gamemode}");
                    Console.WriteLine("");
                    break;
            }
        }

        bool bMaxPlayersChecked = CheckMaxPlayers(serverConfig.maxPlayers, serverConfig.gamemode, out string maxPlayersMessage);
        bool bGameMapsChecked = CheckGameMaps(serverConfig.map, serverConfig.gamemode, out string gameMapsMessage);
        bool bAccessibilityChecked = CheckAccessibility(serverConfig.bHavePassword, serverConfig.password, serverConfig.accessibility, out string accessibiltyMessage);

        if (!bMaxPlayersChecked || !bGameMapsChecked || !bAccessibilityChecked)
        {
            Console.WriteLine("Server cannot be started. Logs: ");
            Console.WriteLine("");
            Console.WriteLine($"{maxPlayersMessage}");
            Console.WriteLine($"{gameMapsMessage}");
            Console.WriteLine($"{accessibiltyMessage}");
        }
        else if (maxPlayersMessage != "" || gameMapsMessage != "" || accessibiltyMessage != "")
        {
            Console.WriteLine("Server can be started, but you need to warn the administration. Logs: ");
            Console.WriteLine("");
            Console.WriteLine($"{maxPlayersMessage}");
            Console.WriteLine($"{gameMapsMessage}");
            Console.WriteLine($"{accessibiltyMessage}");
        }
        else Console.WriteLine("Everything is fine! Server can be started without any problems!");
    }
}


