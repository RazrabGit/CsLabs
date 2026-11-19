using Microsoft.VisualBasic;
using System;
using System.Numerics;
using System.Xml.Linq;

namespace lab08;

public enum CellType
{
    Ground,
    Wall,
    Water,
    Lava,
    Null
}

public struct Position
{
    public int x;
    public int y;
    public Position(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public struct Player
{
    public Position pos;
    public Player(Position position)
    {
        this.pos = position;
    }
}

public struct Cell
{
    public Position pos;
    public CellType type;
    public Cell(Position pos, CellType type)
    {
        this.pos = pos;
        this.type = type;
    }
}

public class State
{
    public Cell[][] cells;
    public Player player;
}

public class Map
{
    public string name;
    public int width;
    public int height;
    public List<State> states = new List<State>();
    public int activeState;

    private const string _savesPath = "saves/";
    private const string _backupsPath = "backups/";

    public void CreateNewMap(int newWidth, int newHeight, string name)
    {
        activeState = 0;
        states.Clear();
        this.width = newWidth;
        this.height = newHeight;
        this.name = name;

        State state = new State();

        state.player = new(new Position(0, 0));

        state.cells = new Cell[height][];

        for (int i = 0; i < height; i++)
        {
            state.cells[i] = new Cell[width];
            for (int j = 0; j < width; j++)
            {
                state.cells[i][j] = new Cell(new Position(j, i), CellType.Ground);
            }
        }
        states.Add(state);

        Console.WriteLine("Map created.");
    }

    public void SelectState(int stateIndex)
    {
        if (stateIndex >= states.Count || states[stateIndex] == null)
        {
            Console.WriteLine($"State {stateIndex} does not exist!");
            return;
        }
        activeState = stateIndex;
        Console.WriteLine("State selected.");
    }

    public void CopyState(int stateIndex)
    {
        if (states[stateIndex] == null)
        {
            Console.WriteLine($"State {stateIndex} does not exist!");
            return;
        }
        State state = new State();
        state.player = states[stateIndex].player;
        Cell[][] cellsCopy = new Cell[height][];
        for (int i = 0; i < height; i++)
        {
            cellsCopy[i] = new Cell[width];
            for (int j = 0; j < width; j++)
            {
                Cell cellRef = states[stateIndex].cells[i][j];
                cellsCopy[i][j] = new Cell(cellRef.pos, cellRef.type);
            }
        }
        state.cells = cellsCopy;
        states.Add(state);

        Console.WriteLine("State copied.");
    }

    public Cell GetCell(int x, int y)
    {
        if (x < 0 || y < 0 || y >= states[activeState].cells.Length || x >= states[activeState].cells[y].Length) return new Cell(new Position(-1, -1), CellType.Null);
        return states[activeState].cells[y][x];
    }

    public void MovePlayer(int x, int y)
    {
        if (GetCell(x, y).type == CellType.Null)
        {
            Console.WriteLine("Incorrect cell coords!");
            return;
        }
        if (GetCell(x, y).type == CellType.Ground)
        {
            states[activeState].player.pos = new Position(x, y);
            Console.WriteLine("Player moved.");
        }
        else Console.WriteLine("Player cannot move to a cell that is not a ground!");
    }

    public void ChangeCell(int x, int y, CellType type)
    {
        if (GetCell(x, y).type == CellType.Null)
        {
            Console.WriteLine("Incorrect cell coords!");
            return;
        }
        if (!states[activeState].player.pos.Equals(new Position(x, y)))
        {
            states[activeState].cells[y][x].type = type;
            Console.WriteLine("Cell changed.");
        }
        else Console.WriteLine("You cannot change the cell while the player is here!");
    }

    public string[] GetStateLines(int state)
    {
        string[] lines = new string[height];
        for (int i = 0; i < height; i++)
        {
            string line = "";
            for (int j = 0; j < width; j++)
            {
                switch (states[state].cells[i][j].type)
                {
                    case CellType.Ground:
                        if (states[state].player.pos.Equals(new Position(j, i))) line += "P";
                        else line += ".";
                        break;
                    case CellType.Wall:
                        line += "#";
                        break;
                    case CellType.Water:
                        line += "~";
                        break;
                    case CellType.Lava:
                        line += "^";
                        break;
                }
            }
            lines[i] = line;
        }
        return lines;
    }

    public void PrintMap()
    {
        foreach (var line in GetStateLines(activeState))
        {
            Console.WriteLine(line);
        }
    }

    public void SaveMap()
    {
        if (!Directory.Exists(Path.Combine(_savesPath, name))) Directory.CreateDirectory(Path.Combine(_savesPath, name));
        string[] statesInfo = { states.Count.ToString(), $"{width}|{height}" };
        File.WriteAllLines(Path.Combine(_savesPath, name + "/", name + "Info.txt"), statesInfo);
        for (int i = 0;i < states.Count;i++)
        {
            SaveState(i);
        }

        Console.WriteLine("Map saved.");
    }

    private void SaveState(int stateIndex)
    {
        if (!Directory.Exists(Path.Combine(_savesPath, name))) Directory.CreateDirectory(Path.Combine(_savesPath, name));
        File.WriteAllLines(Path.Combine(_savesPath, name + "/", $"mapstate{stateIndex}.txt"), GetStateLines(stateIndex));

    }

    private State LoadState(int stateIndex)
    {
        if (!Directory.Exists(Path.Combine(_savesPath, name)))
        {
            Console.WriteLine("Map with this name does not exist!");
            return null;
        }
        if (!File.Exists(Path.Combine(_savesPath, name + "/", $"mapstate{stateIndex}.txt")))
        {
            Console.WriteLine($"Map state {stateIndex} does not exist!");
            return null;
        }
        string[] lines = File.ReadAllLines((Path.Combine(_savesPath, name + "/", $"mapstate{stateIndex}.txt")));
        State state = new State();
        state.cells = new Cell[height][];
        for (int i = 0; i < height; i++)
        {
            state.cells[i] = new Cell[lines[i].Length];
            for (int j = 0; j < width; j++)
            {
                switch (lines[i][j])
                {
                    case 'P':
                        state.cells[i][j] = new Cell(new Position(j, i), CellType.Ground);
                        state.player = new Player(new Position(j, i));
                        break;
                    case '.':
                        state.cells[i][j] = new Cell(new Position(j, i), CellType.Ground);
                        break;
                    case '#':
                        state.cells[i][j] = new Cell(new Position(j, i), CellType.Wall);
                        break;
                    case '~':
                        state.cells[i][j] = new Cell(new Position(j, i), CellType.Water);
                        break;
                    case '^':
                        state.cells[i][j] = new Cell(new Position(j, i), CellType.Lava);
                        break;
                }
            }
        }
        return state;
    }

    public void LoadMap(string name)
    {
        activeState = 0;
        states.Clear();
        if (!Directory.Exists(Path.Combine(_savesPath, name)))
        {
            Console.WriteLine("Map with this name does not exist!");
            return;
        }
        if (!File.Exists(Path.Combine(_savesPath, name + "/", name + "Info.txt")))
        {
            Console.WriteLine("Map with this name does not exist!");
            return;
        }
        string[] stateLines = File.ReadAllLines(Path.Combine(_savesPath, name + "/", name + "Info.txt"));
        string[] sizeInfoLines = stateLines[1].Split("|", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string statesCountLine = stateLines[0];
        bool bValidStatesCount = int.TryParse(statesCountLine, out int statesCount);
        bool bValidWidth = int.TryParse(sizeInfoLines[0], out int width);
        bool bValidHeight = int.TryParse(sizeInfoLines[1], out int height);
        if (!bValidStatesCount || !bValidWidth || !bValidHeight || width <= 0 || height <= 0)
        {
            Console.WriteLine("Map info corrupted!");
            return;
        }
        this.name = name;
        this.width = width;
        this.height = height;
        for (int i = 0; i < statesCount; i++)
        {
            states.Add(LoadState(i));
        }

        Console.WriteLine("Map loaded.");
    }

    public void CreateBackup()
    {
        if (!Directory.Exists(Path.Combine(_savesPath, name)))
        {
            Console.WriteLine("Map with this name does not exist!");
            return;
        }
        if (!Directory.Exists(Path.Combine(_backupsPath, name))) Directory.CreateDirectory(Path.Combine(_backupsPath, name));
        var dir = new DirectoryInfo(Path.Combine(_savesPath, name));
        foreach (var file in dir.GetFiles())
        {
            file.CopyTo(Path.Combine(_backupsPath, name + "/", file.Name), true);
        }

        Console.WriteLine("Backup created.");
    }

    public void RestoreBackup(string filename)
    {
        if (!Directory.Exists(Path.Combine(_backupsPath, filename)))
        {
            Console.WriteLine("Map backup with this name does not exist!");
            return;
        }
        if (!Directory.Exists(Path.Combine(_savesPath, filename))) Directory.CreateDirectory(Path.Combine(_savesPath, filename));
        var dir = new DirectoryInfo(Path.Combine(_backupsPath, filename));
        foreach (var file in dir.GetFiles())
        {
            file.CopyTo(Path.Combine(_savesPath, name + "/", file.Name), true);
        }

        Console.WriteLine("Backup restored.");
    }
}

class Program
{
    static Map map = new Map();

    public static void Main()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("1. Create new map");
            Console.WriteLine("2. Select state");
            Console.WriteLine("3. Copy state");
            Console.WriteLine("4. Move player");
            Console.WriteLine("5. Change cell");
            Console.WriteLine("6. Print map");
            Console.WriteLine("7. Save map");
            Console.WriteLine("8. Load map");
            Console.WriteLine("9. Create backup");
            Console.WriteLine("10. Restore backup");
            Console.WriteLine("11. Exit");

            string input = Console.ReadLine();
            bool bValidChoice = int.TryParse(input, out int choice);
            if (!bValidChoice || choice < 1 || choice > 11) continue;

            switch (choice)
            {
                case 1:
                    CreateNewMap();
                    break;
                case 2:
                    SelectState();
                    break;
                case 3:
                    CopyState();
                    break;
                case 4:
                    MovePlayer();
                    break;
                case 5:
                    ChangeCell();
                    break;
                case 6:
                    PrintMap();
                    break;
                case 7:
                    SaveMap();
                    break;
                case 8:
                    LoadMap();
                    break;
                case 9:
                    CreateBackup();
                    break;
                case 10:
                    RestoreBackup();
                    break;
                case 11:
                    Environment.Exit(0);
                    break;
            }
        }
    }

    private static void CreateNewMap()
    {
        int width = 0;
        int height = 0;
        string mapName = "";

        while (true)
        {
            if (width <= 0)
            {
                Console.Clear();
                Console.WriteLine("Enter map width (max 20):");
                string input = Console.ReadLine();
                bool bValidInput = int.TryParse(input, out width);
                if (!bValidInput || width <= 0 || width > 20) continue;
            }

            if (height <= 0)
            {
                Console.Clear();
                Console.WriteLine("Enter map height (max 20):");
                string input = Console.ReadLine();
                bool bValidInput = int.TryParse(input, out height);
                if (!bValidInput || height <= 0 || height > 20) continue;
            }

            if (string.IsNullOrWhiteSpace(mapName))
            {
                Console.Clear();
                Console.WriteLine("Enter map name:");
                mapName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(mapName)) continue;
            }

            Console.Clear();
            map.CreateNewMap(width, height, mapName);
            EnterAnyKey();
            return;
        }
    }

    private static void SelectState()
    {
        if (!IsMapLoaded()) return;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Enter map state index:");
            string input = Console.ReadLine();
            bool bValidInput = int.TryParse(input, out int stateIndex);
            if (!bValidInput || stateIndex < 0) continue;

            Console.Clear();
            map.SelectState(stateIndex);
            EnterAnyKey();
            return;
        }  
    }

    private static void CopyState()
    {
        if (!IsMapLoaded()) return;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Enter map state index:");
            string input = Console.ReadLine();
            bool bValidInput = int.TryParse(input, out int stateIndex);
            if (!bValidInput || stateIndex < 0) continue;

            Console.Clear();
            map.CopyState(stateIndex);
            EnterAnyKey();
            return;
        }
    }

    private static void MovePlayer()
    {
        if (!IsMapLoaded()) return;

        int xPos = -1;
        int yPos = -1;

        while (true)
        {
            if (xPos < 0)
            {
                Console.Clear();
                Console.WriteLine("Enter x pos:");
                string input = Console.ReadLine();
                bool bValidInput = int.TryParse(input, out xPos);
                if (!bValidInput || xPos < 0) continue;
            }

            if (yPos < 0)
            {
                Console.Clear();
                Console.WriteLine("Enter y pos:");
                string input = Console.ReadLine();
                bool bValidInput = int.TryParse(input, out yPos);
                if (!bValidInput || yPos < 0) continue;
            }

            Console.Clear();
            map.MovePlayer(xPos, yPos);
            EnterAnyKey();
            return;
        }
    }

    private static void ChangeCell()
    {
        if (!IsMapLoaded()) return;

        int xPos = -1;
        int yPos = -1;
        char cell = ' ';
        string cellsAlp = ".#~^";
        CellType cellType = CellType.Null;

        while (true)
        {
            if (xPos < 0)
            {
                Console.Clear();
                Console.WriteLine("Enter x pos:");
                string input = Console.ReadLine();
                bool bValidInput = int.TryParse(input, out xPos);
                if (!bValidInput || xPos < 0) continue;
            }

            if (yPos < 0)
            {
                Console.Clear();
                Console.WriteLine("Enter y pos:");
                string input = Console.ReadLine();
                bool bValidInput = int.TryParse(input, out yPos);
                if (!bValidInput || yPos < 0) continue;
            }

            if (!cellsAlp.Contains(cell))
            {
                Console.Clear();
                Console.WriteLine("Enter cell type");
                Console.WriteLine(". - ground");
                Console.WriteLine("# - wall");
                Console.WriteLine("~ - water");
                Console.WriteLine("^ - lava");
                string input = Console.ReadLine();
                bool bValidInput = char.TryParse(input, out cell);
                if (!bValidInput || !cellsAlp.Contains(cell)) continue;
                switch (cell)
                {
                    case '#':
                        cellType = CellType.Wall; 
                        break;
                    case '.':
                        cellType = CellType.Ground;
                        break;
                    case '~':
                        cellType = CellType.Water;
                        break;
                    case '^':
                        cellType = CellType.Lava;
                        break;
                }
            }

            Console.Clear();
            map.ChangeCell(xPos, yPos, cellType);
            EnterAnyKey();
            return;
        }
    }

    private static void PrintMap()
    {
        if (!IsMapLoaded()) return;
        Console.Clear();
        map.PrintMap();
        EnterAnyKey();
        return;
    }

    private static void SaveMap()
    {
        if (!IsMapLoaded()) return;
        Console.Clear();
        map.SaveMap();
        EnterAnyKey();
        return;
    }

    private static void LoadMap()
    {
        string mapName = "";

        while (true)
        {
            if (string.IsNullOrWhiteSpace(mapName))
            {
                Console.Clear();
                Console.WriteLine("Enter map name:");
                mapName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(mapName)) continue;
            }

            Console.Clear();
            map.LoadMap(mapName);
            EnterAnyKey();
            return;
        }
    }

    private static void CreateBackup()
    {
        if (!IsMapLoaded()) return;
        Console.Clear();
        map.CreateBackup();
        EnterAnyKey();
        return;
    }

    private static void RestoreBackup()
    {
        string mapName = "";

        while (true)
        {
            if (string.IsNullOrWhiteSpace(mapName))
            {
                Console.Clear();
                Console.WriteLine("Enter map name:");
                mapName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(mapName)) continue;
            }

            Console.Clear();
            map.RestoreBackup(mapName);
            EnterAnyKey();
            return;
        }
    }

    private static bool IsMapLoaded()
    {
        if (map.states.Count == 0)
        {
            Console.Clear();
            Console.WriteLine("Map is not loaded.");
            EnterAnyKey();
            return false;
        }
        return true;
    }

    private static void EnterAnyKey()
    {
        Console.WriteLine("");
        Console.WriteLine("Enter any key or leave empty to go back");
        Console.ReadLine();
    }
}