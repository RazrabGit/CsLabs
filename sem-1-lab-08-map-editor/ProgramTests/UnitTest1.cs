using lab08;
using Microsoft.VisualBasic;
using NUnit.Framework;
using System.Reflection;

namespace ProgramTests;

[TestFixture]
public class MapTests
{
    private Map _map = null!;
    private string _tempRoot = null!;

    [SetUp]
    public void SetUp()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "MapEditorTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempRoot);

        _map = new Map();

        // Изолируем файловые пути во временной директории через рефлексию
        SetPrivateField(_map, "_savesPath", Path.Combine(_tempRoot, "saves") + Path.DirectorySeparatorChar);
        SetPrivateField(_map, "_backupsPath", Path.Combine(_tempRoot, "backups") + Path.DirectorySeparatorChar);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private static void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(obj, value);
    }

    // ---------- CreateNewMap ----------

    [Test]
    public void CreateNewMap_ShouldInitializeGroundMapAndPlayerAtOrigin()
    {
        _map.CreateNewMap(5, 4, "test");

        Assert.That(_map.width, Is.EqualTo(5));
        Assert.That(_map.height, Is.EqualTo(4));
        Assert.That(_map.states, Has.Count.EqualTo(1));

        var state = _map.states[0];
        Assert.That(state.player.pos.x, Is.EqualTo(0));
        Assert.That(state.player.pos.y, Is.EqualTo(0));

        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 5; x++)
                Assert.That(state.cells[y][x].type, Is.EqualTo(CellType.Ground));
    }

    // ---------- MovePlayer ----------

    [Test]
    public void MovePlayer_ToGroundCell_ShouldUpdatePlayerPosition()
    {
        _map.CreateNewMap(3, 3, "test");
        _map.MovePlayer(2, 1);

        Assert.That(_map.states[_map.activeState].player.pos.x, Is.EqualTo(2));
        Assert.That(_map.states[_map.activeState].player.pos.y, Is.EqualTo(1));
    }

    [Test]
    public void MovePlayer_ToWallCell_ShouldNotMovePlayer()
    {
        _map.CreateNewMap(3, 3, "test");
        _map.ChangeCell(1, 1, CellType.Wall);
        _map.MovePlayer(1, 1);

        Assert.That(_map.states[_map.activeState].player.pos.x, Is.EqualTo(0));
        Assert.That(_map.states[_map.activeState].player.pos.y, Is.EqualTo(0));
    }

    [Test]
    public void MovePlayer_OutOfBounds_ShouldNotMovePlayer()
    {
        _map.CreateNewMap(3, 3, "test");
        _map.MovePlayer(10, 10);

        Assert.That(_map.states[_map.activeState].player.pos.x, Is.EqualTo(0));
        Assert.That(_map.states[_map.activeState].player.pos.y, Is.EqualTo(0));
    }

    // ---------- ChangeCell ----------

    [Test]
    public void ChangeCell_OnEmptyCell_ShouldChangeType()
    {
        _map.CreateNewMap(3, 3, "test");
        _map.ChangeCell(2, 2, CellType.Water);

        Assert.That(_map.states[_map.activeState].cells[2][2].type, Is.EqualTo(CellType.Water));
    }

    [Test]
    public void ChangeCell_OnPlayerCell_ShouldNotChangeType()
    {
        _map.CreateNewMap(3, 3, "test");
        _map.ChangeCell(0, 0, CellType.Lava);

        Assert.That(_map.states[_map.activeState].cells[0][0].type, Is.EqualTo(CellType.Ground));
    }

    // ---------- SelectState / CopyState ----------

    [Test]
    public void SelectState_ValidIndex_ShouldChangeActiveState()
    {
        _map.CreateNewMap(2, 2, "test");
        _map.CopyState(0); // теперь два состояния

        _map.SelectState(1);

        Assert.That(_map.activeState, Is.EqualTo(1));
    }

    [Test]
    public void CopyState_ShouldCreateIndependentCopy()
    {
        _map.CreateNewMap(2, 2, "test");
        _map.ChangeCell(1, 1, CellType.Wall);
        _map.CopyState(0);

        // Изменяем оригинал
        _map.ChangeCell(1, 1, CellType.Lava);

        Assert.That(_map.states[0].cells[1][1].type, Is.EqualTo(CellType.Lava));
        Assert.That(_map.states[1].cells[1][1].type, Is.EqualTo(CellType.Wall));
    }

    // ---------- SaveMap / LoadMap ----------

    [Test]
    public void SaveAndLoadMap_ShouldRestoreMapAndStates()
    {
        _map.CreateNewMap(3, 2, "persist");
        _map.ChangeCell(1, 1, CellType.Water);
        _map.MovePlayer(2, 0);
        _map.CopyState(0);
        _map.SelectState(1);
        _map.ChangeCell(0, 1, CellType.Wall);

        _map.SaveMap();

        var loadedMap = new Map();
        SetPrivateField(loadedMap, "_savesPath", Path.Combine(_tempRoot, "saves") + Path.DirectorySeparatorChar);
        loadedMap.LoadMap("persist");

        Assert.That(loadedMap.width, Is.EqualTo(3));
        Assert.That(loadedMap.height, Is.EqualTo(2));
        Assert.That(loadedMap.states, Has.Count.EqualTo(2));

        // Состояние 0 (после MovePlayer)
        Assert.That(loadedMap.states[0].cells[1][1].type, Is.EqualTo(CellType.Water));
        Assert.That(loadedMap.states[0].player.pos.x, Is.EqualTo(2));
        Assert.That(loadedMap.states[0].player.pos.y, Is.EqualTo(0));

        // Состояние 1 (после ChangeCell)
        Assert.That(loadedMap.states[1].cells[1][0].type, Is.EqualTo(CellType.Wall));
    }

    // ---------- CreateBackup / RestoreBackup ----------

    [Test]
    public void CreateBackup_ThenModifyAndRestore_ShouldReturnToBackupState()
    {
        _map.CreateNewMap(2, 2, "backupTest");
        _map.ChangeCell(0, 1, CellType.Water);
        _map.SaveMap();               // сохраняем исходное состояние на диск
        _map.CreateBackup();          // создаём резервную копию

        // Меняем карту после бэкапа
        _map.ChangeCell(0, 1, CellType.Lava);
        _map.SaveMap();               // перезаписываем сохранение изменённой картой

        // Восстанавливаем
        _map.RestoreBackup("backupTest");

        // Загружаем восстановленную карту
        var restored = new Map();
        SetPrivateField(restored, "_savesPath", Path.Combine(_tempRoot, "saves") + Path.DirectorySeparatorChar);
        restored.LoadMap("backupTest");

        Assert.That(restored.states[0].cells[1][0].type, Is.EqualTo(CellType.Water),
            "После восстановления бэкапа клетка должна иметь тип Water, а не Lava.");
    }

    [Test]
    public void RestoreBackup_WithoutBackup_ShouldNotThrow()
    {
        _map.CreateNewMap(2, 2, "noBackup");
        _map.SaveMap();

        Assert.DoesNotThrow(() => _map.RestoreBackup("noBackup"));
    }

    // ---------- GetStateLines / PrintMap ----------

    [Test]
    public void GetStateLines_ShouldReturnCorrectSymbols()
    {
        _map.CreateNewMap(3, 2, "symbols");
        _map.ChangeCell(0, 1, CellType.Wall);
        _map.ChangeCell(1, 1, CellType.Water);
        _map.ChangeCell(2, 1, CellType.Lava);
        _map.MovePlayer(1, 0);

        var lines = _map.GetStateLines(0);

        Assert.That(lines[0], Is.EqualTo(".P."));
        Assert.That(lines[1], Is.EqualTo("#~^"));
    }
}
