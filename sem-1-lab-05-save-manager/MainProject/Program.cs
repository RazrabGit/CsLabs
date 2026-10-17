using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.Arm;

namespace lab05;

public enum EOperationType
{
    CheckSave,
    EditSave,
    CreateBackup,
    RestoreBackup,
    DeleteSave
}

public class SaveEntry
{
    public string Name { get; set; }
    public string File { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public struct Position
{
    public int X { get; set; }
    public int Y { get; set; }
    public Position(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}

public class SaveData
{
    public string Character { get; set; }
    public string Map { get; set; }
    public Position Position { get; set; }
    public int Health { get; set; }
    public List<string> Inventory { get; set; }
}

public class Program
{
    public static List<SaveEntry> entries = null;
    public static string _indexPath = "index.json";
    public static string _savesDirectory = "saves/";
    public static string _backupsDirectory = "backups/";
    public static string _logsDirectory = "logs/";
    public static List<string> log = new List<string>();

    public static void Main()
    {
        entries = LoadIndex(_indexPath);
        if (entries == null) return;

        GoToMainMenu();
        
    }

    public static List<SaveEntry> LoadIndex(string indexPath)
    {
        if (!File.Exists(indexPath))
        {
            Console.WriteLine("Cannot read this save file! It does not exist!");
            return null;
        }
        try
        {
            string json = File.ReadAllText(indexPath);
            return JsonConvert.DeserializeObject<List<SaveEntry>>(json);
        }
        catch (JsonReaderException ex)
        {
            Console.WriteLine("Cannot read file about save files info! It has incorrect format!");
            return null;
        }
        catch (JsonSerializationException ex)
        {
            Console.WriteLine("Cannot read file about save files info! It has incorrect format!");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public static SaveData LoadSave(string savesDirectory, SaveEntry entry)
    {
        if (!Directory.Exists(savesDirectory)) Directory.CreateDirectory(savesDirectory);

        if (!File.Exists($"{savesDirectory}/{entry.File}"))
        {
            Console.Clear();
            Console.WriteLine("Cannot read this save file! It does not exist!");

            return null;
        }
        try
        {
            string json = File.ReadAllText($"{savesDirectory}/{entry.File}");
            return JsonConvert.DeserializeObject<SaveData>(json);
        }
        catch (JsonReaderException ex)
        {
            Console.Clear();
            Console.WriteLine("Cannot read this save file! It has incorrect format!");

            return null;
        }
        catch (JsonSerializationException ex)
        {
            Console.Clear();
            Console.WriteLine("Cannot read this save file! It has incorrect format!");

            return null;
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine(ex.Message);

            return null;
        }
    }

    public static void PrintSave(SaveEntry entry, SaveData save)
    {
        Console.Clear();

        Console.WriteLine("");
        Console.WriteLine($"Save name: {entry.Name}");
        Console.WriteLine($"Character: {save.Character}");
        Console.WriteLine($"Map: {save.Map}");
        Console.WriteLine($"Location: X: {save.Position.X}, Y: {save.Position.Y}");
        Console.WriteLine($"Health: {save.Health}");
        Console.WriteLine("Inventory: ");
        for (int i = 0; i < save.Inventory.Count; i++)
        {
            Console.WriteLine($" - {save.Inventory[i]}");
        }
        Console.WriteLine("");

        Console.WriteLine("Enter any key or leave empty to go back");

        if (Console.ReadLine() != null) GoToMainMenu();
    }

    public static SaveEntry FindByName(List<SaveEntry> entries, string name)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (string.Equals(entries[i].Name, name, StringComparison.OrdinalIgnoreCase)) return entries[i];
        }
        return null;
    }

    public static void SaveGame(string savesDirectory, SaveEntry entry, SaveData save)
    {
        if (!Directory.Exists(savesDirectory)) Directory.CreateDirectory(savesDirectory);

        string json = JsonConvert.SerializeObject(save, Formatting.Indented);
        File.WriteAllText($"{savesDirectory}/{entry.File}", json);
        entry.UpdatedAt = DateTime.Now;
    }

    public static void SaveIndex(string indexPath, List<SaveEntry> entries)
    {
        var settings = new JsonSerializerSettings
        {
            DateFormatString = "yyyy-MM-ddTHH:mm:ss"
        };
        string json = JsonConvert.SerializeObject(entries, Formatting.Indented, settings);
        File.WriteAllText($"{indexPath}", json);
    }

    public static void CreateBackup(string savesDirectory, string backupsDirectory, SaveEntry entry)
    {
        if (!Directory.Exists(savesDirectory)) Directory.CreateDirectory(savesDirectory);
        if (!Directory.Exists(backupsDirectory)) Directory.CreateDirectory(backupsDirectory);
        if (!File.Exists($"{savesDirectory}/{entry.File}"))
        {
            Console.WriteLine("Cannot backup this save file! It does not exist!");
            return;
        }
        try
        {
            File.Copy($"{savesDirectory}/{entry.File}", $"{backupsDirectory}/{entry.File}", overwrite: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }
        Console.WriteLine("Backup created.");
    }

    public static void RestoreBackup(string savesDirectory, string backupsDirectory, SaveEntry entry)
    {
        if (!Directory.Exists(savesDirectory)) Directory.CreateDirectory(savesDirectory);
        if (!Directory.Exists(backupsDirectory)) Directory.CreateDirectory(backupsDirectory);

        if (!File.Exists($"{backupsDirectory}/{entry.File}"))
        {
            Console.WriteLine("Cannot restore this backup! It does not exist!");
            return;
        }
        try
        {
            File.Copy($"{backupsDirectory}/{entry.File}", $"{savesDirectory}/{entry.File}", overwrite: true);
            entry.UpdatedAt = DateTime.Now;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }
        Console.WriteLine("Backup restored.");
    }

    public static void DeleteSave(string savesDirectory, string indexPath, List<SaveEntry> entries, SaveEntry entry)
    {
        if (!Directory.Exists(savesDirectory)) Directory.CreateDirectory(savesDirectory);

        if (!File.Exists($"{savesDirectory}/{entry.File}"))
        {
            Console.WriteLine("Save file does not exist!");
            entries.Remove(entry);
            SaveIndex(indexPath, entries);
            return;
        }
        try
        {
            File.Delete($"{savesDirectory}/{entry.File}");
            entries.Remove(entry);
            SaveIndex(indexPath, entries);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            entries.Remove(entry);
            SaveIndex(indexPath, entries);
            return;
        }
    }

    public static void GoToMainMenu()
    {
        Console.Clear();

        Console.WriteLine("");
        Console.WriteLine("1. Show save list.");
        Console.WriteLine("2. Show save data.");
        Console.WriteLine("3. Edit save data.");
        Console.WriteLine("4. Create backup.");
        Console.WriteLine("5. Restore backup.");
        Console.WriteLine("6. Delete save.");
        Console.WriteLine("7. Exit.");
        Console.WriteLine("");

        bool bRightChoise = int.TryParse(Console.ReadLine(), out int choise);

        if (!bRightChoise) GoToMainMenu();

        if (choise <= 0 || choise > 7) GoToMainMenu();

        switch (choise)
        {
            case 1:
                ShowSaveList();
                break;
            case 2:
                ShowSaveData();
                break;
            case 3:
                EditSaveData();
                break;
            case 4:
                CreateBackupFromMenu();
                break;
            case 5:
                RestoreBackupFromMenu();
                break;
            case 6:
                DeleteSaveFromMenu();
                break;
            case 7:
                if (!Directory.Exists(_logsDirectory)) Directory.CreateDirectory(_logsDirectory);

                File.WriteAllLines($"{_logsDirectory}/latest.log", log);
                Environment.Exit(0);
                break;
        }
    }

    public static void ShowSaveList()
    {
        Console.Clear();

        Console.WriteLine("");

        for (int i = 0; i < entries.Count; i++)
        {
            Console.WriteLine($"{i}. {entries[i].Name}, File: {entries[i].File}, Updated at: {entries[i].UpdatedAt}");
        }

        Console.WriteLine("");
        Console.WriteLine("Enter any key or leave empty to go back");
        if (Console.ReadLine() != null) GoToMainMenu();
        return;
    }

    public static void GetSaveAndEntryByNameFromMenu(out SaveData save, out SaveEntry entry)
    {
        GetEntryByNameFromMenu(out SaveEntry foundEntry);
        entry = foundEntry;
        if (entry == null)
        {
            save = null;
            return;
        }

        save = LoadSave(_savesDirectory, foundEntry);
        return;
    }

    public static void GetEntryByNameFromMenu(out SaveEntry entry)
    {
        Console.Clear();
        Console.WriteLine("Enter the save name:");

        string saveName = Console.ReadLine();

        if (saveName == null)
        {
            Console.Clear();
            Console.WriteLine("Some error occured with the save name you typed.");
            entry = null;
            return;
        }

        entry = FindByName(entries, saveName);
        if (entry == null)
        {
            Console.Clear();
            Console.WriteLine("Save entry not found.");
            return;
        }
    }

    public static void ShowSaveData()
    {
        GetSaveAndEntryByNameFromMenu(out SaveData save, out SaveEntry entry);

        if (save == null || entry == null)
        {
            Console.WriteLine("");
            Console.WriteLine("Enter any key or leave empty to go back");
            if (Console.ReadLine() != null) GoToMainMenu();
            return;
        }

        WriteLogLine(entry, EOperationType.CheckSave);
        PrintSave(entry, save);

        Console.WriteLine("");
        Console.WriteLine("Enter any key or leave empty to go back");
        if (Console.ReadLine() != null) GoToMainMenu();
        return;
    }

    public static void EditSaveData()
    {
        GetSaveAndEntryByNameFromMenu(out SaveData save, out SaveEntry entry);

        if (save == null || entry == null)
        {
            Console.WriteLine("");
            Console.WriteLine("Enter any key or leave empty to go back");
            if (Console.ReadLine() != null) GoToMainMenu();
            return;
        }

        Console.Clear();
        Console.WriteLine("Enter new Map (leave empty to leave without edit)");
        string mapStr = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(mapStr)) mapStr = save.Map;

        Console.Clear();
        Console.WriteLine("Enter new PosX (leave empty or not an integer to leave without edit)");
        string PosXStr = Console.ReadLine();
        bool bCorrectPosX = int.TryParse(PosXStr, out int posX);
        if (!bCorrectPosX) posX = save.Position.X;

        Console.Clear();
        Console.WriteLine("Enter new PosY (leave empty or not an integer to leave without edit)");
        string PosYStr = Console.ReadLine();
        bool bCorrectPosY = int.TryParse(PosYStr, out int posY);
        if (!bCorrectPosY) posY = save.Position.Y;

        Console.Clear();
        Console.WriteLine("Enter new Health (leave empty or not an integer to leave without edit)");
        string healthStr = Console.ReadLine();
        bool bCorrectHealth = int.TryParse(healthStr, out int health);
        if (!bCorrectHealth) health = save.Health;

        List<string> list = save.Inventory;

        Console.Clear();
        Console.WriteLine("Do you want to edit inventory?");
        Console.WriteLine("1. Yes");
        Console.WriteLine("2. No");
        bool bRightChoise = int.TryParse(Console.ReadLine(), out int choise);
        if (bRightChoise && choise == 1)
        {
            list = ChangeInventory(new List<string>());
        }

        save.Map = mapStr;
        save.Position = new Position(posX, posY);
        save.Health = health;
        save.Inventory = list;

        SaveGame(_savesDirectory, entry, save);
        SaveIndex(_indexPath, entries);
        WriteLogLine(entry, EOperationType.EditSave);

        Console.Clear();
        Console.WriteLine("Your game is saved.");
        Console.WriteLine("");
        Console.WriteLine("Enter any key or leave empty to go back");
        if (Console.ReadLine() != null) GoToMainMenu();
        return;
    }

    public static List<string> ChangeInventory(List<string> inventory)
    {
        Console.Clear();
        Console.WriteLine("Do you want to add new item?");
        Console.WriteLine("1. Yes");
        Console.WriteLine("2. No (end of editing inventory)");
        bool bRightChoise = int.TryParse(Console.ReadLine(), out int choise);
        if (!bRightChoise) return inventory;
        if (choise < 1 || choise >= 2) return inventory;

        Console.Clear();
        Console.WriteLine("Enter your item:");
        string item = Console.ReadLine();
        if (item != null) inventory.Add(item);

        return ChangeInventory(inventory);
    }

    public static void CreateBackupFromMenu()
    {
        GetEntryByNameFromMenu(out SaveEntry entry);

        if (entry == null)
        {
            Console.WriteLine("");
            Console.WriteLine("Enter any key or leave empty to go back");
            if (Console.ReadLine() != null) GoToMainMenu();
            return;
        }

        Console.Clear();
        CreateBackup(_savesDirectory, _backupsDirectory, entry);
        WriteLogLine(entry, EOperationType.CreateBackup);
        Console.WriteLine("");
        Console.WriteLine("Enter any key or leave empty to go back");

        if (Console.ReadLine() != null) GoToMainMenu();
        return;
    }

    public static void RestoreBackupFromMenu()
    {
        GetEntryByNameFromMenu(out SaveEntry entry);

        if (entry == null)
        {
            Console.WriteLine("");
            Console.WriteLine("Enter any key or leave empty to go back");
            if (Console.ReadLine() != null) GoToMainMenu();
            return;
        }

        Console.Clear();
        RestoreBackup(_savesDirectory, _backupsDirectory, entry);
        SaveIndex(_indexPath, entries);
        WriteLogLine(entry, EOperationType.RestoreBackup);
        Console.WriteLine("");
        Console.WriteLine("Enter any key or leave empty to go back");

        if (Console.ReadLine() != null) GoToMainMenu();
        return;
    }

    public static void DeleteSaveFromMenu()
    {
        GetEntryByNameFromMenu(out SaveEntry entry);
        if (entry == null)
        {
            Console.WriteLine("");
            Console.WriteLine("Enter any key or leave empty to go back");
            if (Console.ReadLine() != null) GoToMainMenu();
            return;
        }

        Console.Clear();
        DeleteSave(_savesDirectory, _indexPath, entries, entry);
        WriteLogLine(entry, EOperationType.DeleteSave);
        Console.WriteLine("Save entry deleted.");
        Console.WriteLine("");
        Console.WriteLine("Enter any key or leave empty to go back");
        if (Console.ReadLine() != null) GoToMainMenu();
        return;
    }

    public static void WriteLogLine(SaveEntry entry, EOperationType type)
    {
        log.Add($"[{DateTime.Now}] {entry.Name}: {type}");
    }
}
