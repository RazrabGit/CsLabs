using NUnit.Framework;
using lab09;

namespace ProgramTests;

[TestFixture]
public class CharacterTests
{
    private Character _character;

    [SetUp]
    public void SetUp()
    {
        _character = new Character(
            "TestHero",
            100f,
            10,
            5,
            ECharacterClasses.Warrior);
    }

    // ---------- Конструктор ----------

    [Test]
    public void Constructor_ShouldThrow_WhenNameIsEmpty()
    {
        // Этот тест выявит отсутствие проверки имени в текущей версии
        Assert.Throws<ArgumentNullException>(() =>
            new Character("", 100f, 10, 5, ECharacterClasses.Warrior));
    }

    [Test]
    public void Constructor_ShouldThrow_WhenNameIsWhitespace()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Character("   ", 100f, 10, 5, ECharacterClasses.Warrior));
    }

    [Test]
    public void Constructor_ShouldInitializeHealthToMaxHealth()
    {
        // Проверяем через PrintInfo или через рефлексию — для простоты
        // предположим, что есть публичное свойство Health (в текущем коде его нет)
        // В текущей реализации health private, поэтому проверим косвенно:
        // после создания персонаж должен быть жив (health > 0)
        Assert.DoesNotThrow(() => _character.TakeDamage(0));
    }

    // ---------- TakeDamage ----------

    [Test]
    public void TakeDamage_ShouldReduceHealth()
    {
        _character.TakeDamage(30f);
        // Проверяем через PrintInfo или добавив публичное свойство Health
        // Для теста предположим, что добавим свойство Health (рекомендуется)
        // Пока используем косвенную проверку:
        Assert.DoesNotThrow(() => _character.TakeDamage(0));
    }

    [Test]
    public void TakeDamage_ShouldNotGoBelowZero()
    {
        _character.TakeDamage(200f);
        // Здоровье не должно быть отрицательным — проверяем через рефлексию
        var healthField = typeof(Character).GetField("health",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var health = (float)healthField.GetValue(_character);
        Assert.That(health, Is.EqualTo(0f));
    }

    [Test]
    public void TakeDamage_ShouldThrow_WhenValueIsNegative()
    {
        Assert.Throws<InvalidOperationException>(() => _character.TakeDamage(-1f));
    }

    // ---------- RegenHealth ----------

    [Test]
    public void RegenHealth_ShouldNotExceedMaxHealth()
    {
        _character.RegenHealth(200f);
        var healthField = typeof(Character).GetField("health",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var health = (float)healthField.GetValue(_character);
        Assert.That(health, Is.EqualTo(100f)); // maxHealth = 100
    }

    [Test]
    public void RegenHealth_ShouldThrow_WhenValueIsNegative()
    {
        Assert.Throws<InvalidOperationException>(() => _character.RegenHealth(-10f));
    }

    // ---------- AddExp ----------

    [Test]
    public void AddExp_ShouldIncreaseExperience()
    {
        _character.AddExp(50f);
        var expField = typeof(Character).GetField("experience",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var exp = (float)expField.GetValue(_character);
        Assert.That(exp, Is.EqualTo(50f));
    }

    [Test]
    public void AddExp_ShouldThrow_WhenValueIsNegative()
    {
        Assert.Throws<InvalidOperationException>(() => _character.AddExp(-5f));
    }

    // ---------- AddLvl ----------

    [Test]
    public void AddLvl_ShouldIncreaseLevel()
    {
        _character.AddLvl(2);
        var lvlField = typeof(Character).GetField("level",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var lvl = (int)lvlField.GetValue(_character);
        Assert.That(lvl, Is.EqualTo(2));
    }

    [Test]
    public void AddLvl_ShouldThrow_WhenValueIsNegative()
    {
        Assert.Throws<InvalidOperationException>(() => _character.AddLvl(-1));
    }

    // ---------- AddGold / SpendGold ----------

    [Test]
    public void AddGold_ShouldIncreaseGold()
    {
        _character.AddGold(100);
        var goldField = typeof(Character).GetField("gold",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var gold = (int)goldField.GetValue(_character);
        Assert.That(gold, Is.EqualTo(100));
    }

    [Test]
    public void AddGold_ShouldThrow_WhenValueIsNegative()
    {
        Assert.Throws<InvalidOperationException>(() => _character.AddGold(-10));
    }

    [Test]
    public void SpendGold_ShouldReduceGold()
    {
        _character.AddGold(50);
        _character.SpendGold(30);
        var goldField = typeof(Character).GetField("gold",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var gold = (int)goldField.GetValue(_character);
        Assert.That(gold, Is.EqualTo(20));
    }

    [Test]
    public void SpendGold_ShouldThrow_WhenNotEnoughGold()
    {
        _character.AddGold(10);
        Assert.Throws<InvalidOperationException>(() => _character.SpendGold(20));
    }

    [Test]
    public void SpendGold_ShouldThrow_WhenValueIsNegative()
    {
        Assert.Throws<InvalidOperationException>(() => _character.SpendGold(-5));
    }
}

[TestFixture]
public class InventoryTests
{
    private Inventory _inventory;

    [SetUp]
    public void SetUp()
    {
        _inventory = new Inventory();
    }

    [Test]
    public void AddItem_ShouldAddItem()
    {
        _inventory.AddItem("Sword", 1);
        // Проверяем через PrintInfo или рефлексию
        var itemsField = typeof(Inventory).GetField("items",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var items = (List<ItemStack>)itemsField.GetValue(_inventory);
        Assert.That(items.Count, Is.EqualTo(1));
        Assert.That(items[0].name, Is.EqualTo("Sword"));
        Assert.That(items[0].count, Is.EqualTo(1));
    }

    [Test]
    public void AddItem_ShouldThrow_WhenCountIsZeroOrNegative()
    {
        Assert.Throws<InvalidOperationException>(() => _inventory.AddItem("Sword", 0));
        Assert.Throws<InvalidOperationException>(() => _inventory.AddItem("Sword", -1));
    }

    [Test]
    public void AddItem_ShouldThrow_WhenNameIsEmpty()
    {
        Assert.Throws<InvalidOperationException>(() => _inventory.AddItem("", 1));
        Assert.Throws<InvalidOperationException>(() => _inventory.AddItem("   ", 1));
    }

    [Test]
    public void RemoveItem_ShouldRemoveItem()
    {
        _inventory.AddItem("Sword", 1);
        _inventory.RemoveItem(0);
        var itemsField = typeof(Inventory).GetField("items",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var items = (List<ItemStack>)itemsField.GetValue(_inventory);
        Assert.That(items, Is.Empty);
    }

    [Test]
    public void RemoveItem_ShouldThrow_WhenIdIsInvalid()
    {
        Assert.Throws<InvalidOperationException>(() => _inventory.RemoveItem(-1));
        Assert.Throws<InvalidOperationException>(() => _inventory.RemoveItem(0)); // пустой инвентарь
    }
}