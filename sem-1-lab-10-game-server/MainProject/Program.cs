using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;

namespace lab10;

public class Server
{
    private List<Lobby> lobbies = new List<Lobby>();
    private List<Player> playersByList = new List<Player>();
    private Dictionary<string, Player> playersByDict = new Dictionary<string, Player>();

    public Server()
    {
        lobbies.Add(new Lobby());
    }

    public void ConnectPlayer(Player player)
    {
        if (player == null) return;

        if (!playersByDict.ContainsKey(player.login)) playersByDict.Add(player.login, player);
        if (!playersByList.Contains(player)) playersByList.Add(player);

        Lobby lobby = lobbies[0];
        lobby.ConnectPlayer(player);
    }

    public void DisconnectPlayer(Player player)
    {
        if (player == null) return;

        if (player.currentLobby != null) player.currentLobby.DisconnectPlayer(player);

        playersByList.Remove(player);
        playersByDict.Remove(player.login);
    }

    public void TransferPlayerToLobby(Player player, Lobby lobby)
    {
        if (player == null || lobby == null) return;

        if (player.currentLobby != null) player.currentLobby.DisconnectPlayer(player);
        lobby.ConnectPlayer(player);
    }

    public Player SearchPlayerFromListByLogin(string login)
    {
        for (int i = 0; i < playersByList.Count; i++)
        {
            if (playersByList[i].login == login) return playersByList[i];
        }
        return null;
    }

    public Player SearchPlayerFromDictByLogin(string login)
    {
        bool bFound = playersByDict.TryGetValue(login, out Player p);
        if (bFound && p != null) return p;
        return null;
    }

    public List<Player> GetPlayersList()
    {
        return playersByList;
    }

    public int GetLobbyIndex(Lobby lobby)
    {
        if (!lobbies.Contains(lobby)) return -1;
        return lobbies.IndexOf(lobby);
    }

    public Lobby GetLobby(int index)
    {
        if ((index < 0) || (index >= lobbies.Count)) return null;
        return lobbies[index];
    }

    public void CreateLobby()
    {
        lobbies.Add(new Lobby());
    }

    public int GetLobbiesCount()
    {
        return lobbies.Count;
    }
}

public class Lobby
{
    private List<Player> playersByList = new List<Player>();
    private Dictionary<string, Player> playersByDict = new Dictionary<string, Player>();

    public void ConnectPlayer(Player player)
    {
        if (player == null) return;

        if (!playersByList.Contains(player)) playersByList.Add(player);
        if (!playersByDict.ContainsKey(player.login)) playersByDict.Add(player.login, player);
        player.currentLobby = this;
    }

    public void DisconnectPlayer(Player player)
    {
        if (player == null) return;

        playersByList.Remove(player);
        playersByDict.Remove(player.login);
        player.currentLobby = null;
    }

    public Player SearchPlayerFromListByLogin(string login)
    {
        for (int i = 0; i < playersByList.Count; i++)
        {
            if (playersByList[i].login == login) return playersByList[i];
        }
        return null;
    }

    public Player SearchPlayerFromDictByLogin(string login)
    {
        bool bFound = playersByDict.TryGetValue(login, out Player p);
        if (bFound && p != null) return p;
        return null;
    }

    public List<Player> GetPlayersList()
    {
        return playersByList;
    }

}

public class Player
{
    public readonly string login;
    public int level { private set; get; }
    public float health { private set; get; }
    public bool bIsOnline;
    public Lobby currentLobby;

    public Player(string login, float health, int level, bool bIsOnline)
    {
        this.login = login;
        this.health = Math.Clamp(health, 0, 100); ;
        this.level = Math.Clamp(level, 0, 50);
        this.bIsOnline = bIsOnline;
    }

    public void AddHealth(float value)
    {
        health = Math.Clamp(health + value, 0, 100);
    }

    public void AddLevel(int value)
    {
        level = Math.Clamp(level + value, 0, 50);
    }
}

class Program
{
    private static bool _bLoadFromFile = false;

    private static Dictionary<string, Player> players = new Dictionary<string, Player>();
    private static Server server = new Server(); 

    private static void Main()
    {
        GetPlayers();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("1. Connect player");
            Console.WriteLine("2. Transfer player to other lobby");
            Console.WriteLine("3. Disconnect player");
            Console.WriteLine("4. Show all players");
            Console.WriteLine("5. Find player");
            Console.WriteLine("6. Change player stats");
            Console.WriteLine("7. Show player info");
            Console.WriteLine("8. Create lobby");
            Console.WriteLine("9. Show lobbies count");
            Console.WriteLine("10. Connect all players");
            Console.WriteLine("0. Exit");

            string input = Console.ReadLine();
            bool bValidInput = int.TryParse(input, out int choice);
            if (!bValidInput || choice < 0 || choice > 10) continue;

            switch (choice)
            {
                case 0:
                    return;
                case 1:
                    PerformConnectPlayer();
                    break;
                case 2:
                    PerformTransferPlayer();
                    break;
                case 3:
                    PerformDisconnectPlayer();
                    break;
                case 4:
                    PerformShowAllPlayers();
                    break;
                case 5:
                    PerformFindPlayer();
                    break;
                case 6:
                    PerformChangeStats();
                    break;
                case 7:
                    PerformPrintInfo();
                    break;
                case 8:
                    PerformCreateLobby();
                    break;
                case 9:
                    PerformShowLobbiesCount();
                    break;
                case 10:
                    PerformConnectAllPlayers();
                    break;
            }


        }
    }

    private static void PerformConnectAllPlayers()
    {
        Console.Clear();
        foreach (var(login, player) in players)
        {            
            server.ConnectPlayer(player);
        }
        Console.WriteLine("Players connected");
        EnterAnyKey();
    }

    private static void PerformCreateLobby()
    {
        Console.Clear();
        server.CreateLobby();
        Console.WriteLine("Lobby created");
        EnterAnyKey();
    }

    private static void PerformShowLobbiesCount()
    {
        Console.Clear();
        Console.WriteLine($"Lobbies count: {server.GetLobbiesCount()}");
        EnterAnyKey();
    }

    private static void PerformConnectPlayer()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Enter player login:");
            string input = Console.ReadLine();
            if (input == null || string.IsNullOrWhiteSpace(input)) continue;

            bool bValidPlayer = players.TryGetValue(input, out Player player);

            Console.Clear();
            if (!bValidPlayer || player == null)
            {
                Console.WriteLine("Invalid login");
                EnterAnyKey();
                return;
            }

            server.ConnectPlayer(player);
            Console.WriteLine("Player connected");
            EnterAnyKey();
            return;
        }
    }

    private static void PerformShowAllPlayers()
    {
        Console.Clear();
        foreach (var player in server.GetPlayersList())
        {
            Console.WriteLine(player.login);
        }

        EnterAnyKey();
        return;
    }

    private static void PerformDisconnectPlayer()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Enter player login:");
            string input = Console.ReadLine();
            if (input == null || string.IsNullOrWhiteSpace(input)) continue;

            Player player = server.SearchPlayerFromDictByLogin(input);
            Console.Clear();

            if (player == null)
            {
                Console.WriteLine("Invalid login");
                EnterAnyKey();
                return;
            }

            server.DisconnectPlayer(player);
            Console.WriteLine("Player disconnected");
            EnterAnyKey();
            return;
        }
    }

    private static void PerformTransferPlayer()
    {
        Player player = null;
        string input = "";
        while (true)
        {
            Console.Clear();
            if (player == null)
            {
                Console.WriteLine("Enter player login:");
                input = Console.ReadLine();
                if (input == null || string.IsNullOrWhiteSpace(input)) continue;

                player = server.SearchPlayerFromDictByLogin(input);
                Console.Clear();

                if (player == null)
                {

                    Console.WriteLine("Invalid login");
                    EnterAnyKey();
                    return;
                }
            }

            Console.WriteLine("Enter lobby index:");
            input = Console.ReadLine();
            bool bValidInput = int.TryParse(input, out int index);
            if (!bValidInput || index < 0) continue;

            Lobby lobby = server.GetLobby(index);
            
            Console.Clear();
            if (lobby == null)
            {

                Console.WriteLine("Invalid lobby");
                EnterAnyKey();
                return;
            }

            server.TransferPlayerToLobby(player, lobby);

            Console.WriteLine("Player transfered");
            EnterAnyKey();
            return;
        }
    }

    private static void PerformFindPlayer()
    {
        string login = "";
        while (true)
        {
            Console.Clear();

            if (string.IsNullOrWhiteSpace(login))
            { 
                Console.WriteLine("Enter player login:");
                login = Console.ReadLine();
                if (login == null || string.IsNullOrWhiteSpace(login)) continue;
                if (server.SearchPlayerFromDictByLogin(login) == null)
                {
                    Console.WriteLine("Invalid login");
                    EnterAnyKey();
                    return;
                }
            }

            Console.Clear();
            Console.WriteLine("1. Find by list");
            Console.WriteLine("2. Find by dict");
            string input = Console.ReadLine();
            bool bValidInput = int.TryParse(input, out int choice);
            if (!bValidInput || choice < 0 || choice > 2) continue;

            Console.Clear();
            if (choice == 1)
            {
                var stopwatch = Stopwatch.StartNew();

                for (var i = 0; i < 10000; i++)
                {
                    server.SearchPlayerFromListByLogin(login); 
                }

                stopwatch.Stop();

                Console.WriteLine("Player found");
                Console.WriteLine($"Find time: {stopwatch.Elapsed.TotalMilliseconds} ms");
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                for (var i = 0; i < 10000; i++)
                {
                    server.SearchPlayerFromDictByLogin(login);
                }

                stopwatch.Stop();

                Console.WriteLine("Player found");
                Console.WriteLine($"Find time: {stopwatch.Elapsed.TotalMilliseconds} ms");
            }

            EnterAnyKey();
            return;
        }
    }

    private static void PerformChangeStats()
    {
        Player player = null;
        string input = "";
        int choice = 0;
        bool bValidInput = false;
        while (true)
        {
            Console.Clear();
            if (player == null)
            {
                Console.WriteLine("Enter player login:");
                input = Console.ReadLine();
                if (input == null || string.IsNullOrWhiteSpace(input)) continue;

                player = server.SearchPlayerFromDictByLogin(input);
                Console.Clear();

                if (player == null)
                {

                    Console.WriteLine("Invalid login");
                    EnterAnyKey();
                    return;
                }
            }

            if (choice == 0)
            {
                Console.WriteLine("1. Add health");
                Console.WriteLine("2. Add level");
                input = Console.ReadLine();
                bValidInput = int.TryParse(input, out choice);
                if (!bValidInput || choice < 1 || choice > 2) continue;
            }

            Console.Clear();
            Console.WriteLine("Enter the value:");
            input = Console.ReadLine();
            if (input == null || string.IsNullOrWhiteSpace(input)) continue;
            Console.Clear();
            

            if (choice == 1)
            {
                bValidInput = float.TryParse(input, out float value);
                if (!bValidInput)
                {
                    Console.WriteLine("Invalid value");
                    EnterAnyKey();
                    return;
                }
                player.AddHealth(value);
            }
            else
            {
                bValidInput = int.TryParse(input, out int value);
                if (!bValidInput)
                {
                    Console.WriteLine("Invalid value");
                    EnterAnyKey();
                    return;
                }
                player.AddLevel(value);
            }

            Console.WriteLine("Stat changed");
            EnterAnyKey();
            return;
        }
    }

    private static void GetPlayers()
    {
        Random random = new Random();

        if (_bLoadFromFile && File.Exists("players.txt"))
        {
            string[] lines = File.ReadAllLines("players.txt");
            foreach (string line in lines)
            {
                string[] stats = line.Split('|');
                if (stats.Length < 4) continue;

                string login = stats[0];
                if (login == null || string.IsNullOrWhiteSpace(login)) continue;

                bool bValidValue = int.TryParse(stats[1], out int level);
                if (!bValidValue) continue;

                bValidValue = float.TryParse(stats[2], out float health);
                if (!bValidValue) continue;

                bValidValue = bool.TryParse(stats[3], out bool bIsOnline);
                if (!bValidValue) continue;

                Player player = new Player(login, health, level, bIsOnline);
                players.Add(login, player);
            }
        }
        else if (players.Count == 0)
        {
            for (int i = 0; i < 100; i++)
            { 
                string login = $"Player{i}";
                float health = (float)random.Next(0, 100);
                int level = random.Next(0, 50);
                bool bIsOnline = random.Next(0, 1) == 1 ? true : false;

                Player player = new Player(login, health, level, bIsOnline);
                players.Add(login, player);
            }
        }
    }

    private static void PerformPrintInfo()
    {
        Player player = null;
        while (true)
        {
            Console.Clear();
            if (player == null)
            {
                Console.WriteLine("Enter player login:");
                string input = Console.ReadLine();
                if (input == null || string.IsNullOrWhiteSpace(input)) continue;

                player = server.SearchPlayerFromDictByLogin(input);
                Console.Clear();

                if (player == null)
                {
                    
                    Console.WriteLine("Invalid login");
                    EnterAnyKey();
                    return;
                }
            }

            Console.WriteLine($"--- Player {player.login} ---");

            Console.WriteLine($"Health: {player.health}");
            Console.WriteLine($"Level: {player.level}");
            Console.WriteLine($"Online: {player.bIsOnline}");
            int lobbyIndex = server.GetLobbyIndex(player.currentLobby);
            string lobbyIndexStr = "";
            if (lobbyIndex == -1) lobbyIndexStr = "none";
            else lobbyIndexStr = lobbyIndex.ToString();
            Console.WriteLine($"Lobby: {lobbyIndexStr}");

            EnterAnyKey();
            return;
        }
    }

    private static void EnterAnyKey()
    {
        Console.WriteLine("");
        Console.WriteLine("Enter any key or leave empty to go back");
        Console.ReadLine();
    }
}