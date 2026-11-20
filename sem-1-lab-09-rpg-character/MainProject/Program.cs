namespace lab09;

public enum ECharacterClasses
{
    Warrior,
    Gunslinger,
    Mage,
    Summoner,
    Rogue
}

public class Character
{
    public readonly string name;
    public readonly ECharacterClasses characterClass;
    private float maxHealth;
    private float health;
    private float experience;
    private int level;
    private int gold;
    private int power;
    private int defense;
    public Inventory inventory = new Inventory();

    public Character(string name, float maxHealth, int power, int defense, ECharacterClasses characterClass)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("Character name cannot be null or empty");
        if (maxHealth <= 0) throw new ArgumentOutOfRangeException("Character max health cannot be zero and less");
        if (power < 0 || defense < 0) throw new ArgumentOutOfRangeException("Character properties cannot be less than zero");

        this.name = name;
        this.maxHealth = maxHealth;
        health = maxHealth; // ну а хули оно на нуле при создании
        this.power = power;
        this.defense = defense;
        this.characterClass = characterClass;
    }

    public void TakeDamage(float value)
    {
        if (value < 0) throw new InvalidOperationException("Character cannot take less than zero damage");
        health = Math.Clamp(health - value, 0, maxHealth);
    }

    public void RegenHealth(float value)
    {
        if (value < 0) throw new InvalidOperationException("Character cannot regen less than zero health");
        health = Math.Clamp(health + value, 0, maxHealth);
    }

    public void AddExp(float value)
    {
        if (value < 0) throw new InvalidOperationException("Invalid amount of exp cannot be added");
        experience += value;
    }

    public void AddLvl(int value)
    {
        if (value < 0) throw new InvalidOperationException("Invalid amount of lvls cannot be added");
        level += value;
    }

    public void AddGold(int value)
    {
        if (value < 0) throw new InvalidOperationException("Invalid amount of gold cannot be added");
        gold += value;
    }

    public void SpendGold(int value)
    {
        if (value < 0) throw new InvalidOperationException("Invalid amount of gold cannot be spent");

        if (gold < value) throw new InvalidOperationException("Not enough gold to spent");

        gold -= value;
    }

    public void PrintInfo()
    {
        Console.WriteLine($"Name: {name}");
        Console.WriteLine($"Class: {characterClass}");
        Console.WriteLine($"Health: {health}/{maxHealth}");
        Console.WriteLine($"Experience: {experience}");
        Console.WriteLine($"Level: {level}");
        Console.WriteLine($"Gold: {gold}G");
        Console.WriteLine($"Power: {power}");
        Console.WriteLine($"Defense: {defense}");
        Console.WriteLine("Inventory: ");
        inventory.PrintInfo();
    }
}

public class Inventory
{
    private List<ItemStack> items = new List<ItemStack>();

    public void AddItem(string name, int count)
    {
        if (count <= 0) throw new InvalidOperationException("Item cannot be added with invalid count");

        if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Item cannot be added with invalid name");

        items.Add(new(name, count));
    }

    public void RemoveItem(int id)
    {
        if (id < 0 || id >= items.Count) throw new InvalidOperationException("Item cannot be removed with invalid id");

        items.RemoveAt(id);
    }

    public void PrintInfo()
    {
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            Console.WriteLine($" {i}. {item.name} x{item.count}"); 
        }
    }
}

public struct ItemStack
{
    public string name;
    public int count;
    public ItemStack(string name, int count)
    {
        this.name = name;
        this.count = count;
    }
}

class Program
{
    static Character character;
    public static void Main()
    {
        character = new("Mr. Unrealovsky", 1488, 1337, 67, ECharacterClasses.Rogue);
        while (true)
        {
            Console.Clear();

            Console.WriteLine("1. Show character info");
            Console.WriteLine("2. Take damage");
            Console.WriteLine("3. Regenerate health");
            Console.WriteLine("4. Take experience");
            Console.WriteLine("5. Increase level");
            Console.WriteLine("6. Take gold");
            Console.WriteLine("7. Spend gold");
            Console.WriteLine("8. Add item");
            Console.WriteLine("9. Remove item");
            Console.WriteLine("0. Exit");

            string input = Console.ReadLine();
            bool bValidInput = int.TryParse(input, out int choice);
            if (!bValidInput || choice < 0 || choice > 9) continue;

            try
            {
                if (choice >= 2 && choice <= 4) PerformOperationsWithFloats(choice);
                if ((choice >= 5 &&  choice <= 7) || choice == 9) PerformOperationsWithNumbers(choice);
                if (choice == 8) PerformAddItem();
            }
            catch (Exception e)
            {
                Console.Clear();
                Console.WriteLine(e.Message);
                EnterAnyKey();
            }
           
            if (choice == 1)
            {
                Console.Clear();
                character.PrintInfo();
                EnterAnyKey();
            }
            if (choice == 0) return;
        }
    }

    public static void PerformOperationsWithFloats(int choice)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Enter a number:");
            string input = Console.ReadLine();
            bool bValidInput = float.TryParse(input, out float number);
            if (!bValidInput) continue;
            switch (choice)
            {
                case 2:
                    character.TakeDamage(number); 
                    break;
                case 3:
                    character.RegenHealth(number);
                    break;
                case 4:
                    character.AddExp(number);
                    break;
            }
            Console.Clear();
            Console.WriteLine("Operation successful");
            EnterAnyKey();
            return;
        }
    }

    public static void PerformOperationsWithNumbers(int choice)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Enter a number:");
            string input = Console.ReadLine();
            bool bValidInput = int.TryParse(input, out int number);
            if (!bValidInput) continue;
            switch (choice)
            {
                case 5:
                    character.AddLvl(number);
                    break;
                case 6:
                    character.AddGold(number);
                    break;
                case 7:
                    character.SpendGold(number);
                    break;
                case 9:
                    character.inventory.RemoveItem(number);
                    break;
            }
            Console.Clear();
            Console.WriteLine("Operation successful");
            EnterAnyKey();
            return;
        }
    }

    public static void PerformAddItem()
    {
        string input = "";
        bool bValidInput = false;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Enter item name:");
            input = Console.ReadLine();
            string name = input;

            Console.Clear();
            Console.WriteLine("Enter item count:");
            input = Console.ReadLine();
            bValidInput = int.TryParse(input, out int count);
            if (!bValidInput) continue;

            character.inventory.AddItem(name, count);
            Console.Clear();
            Console.WriteLine("Operation successful");
            EnterAnyKey();
            return;
        }
    }

    public static void EnterAnyKey()
    {
        Console.WriteLine("");
        Console.WriteLine("Enter any key or leave empty to go back");
        Console.ReadLine();
    }
}
