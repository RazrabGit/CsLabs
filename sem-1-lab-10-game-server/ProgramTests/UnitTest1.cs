using NUnit.Framework;
using lab10;

namespace sem_1_lab_10_tests;

[TestFixture]
public class PlayerTests
{
    [Test]
    public void Constructor_ClampsHealthAndLevel()
    {
        var player = new Player("test", 150f, 100, true);

        Assert.That(player.health, Is.EqualTo(100f));
        Assert.That(player.level, Is.EqualTo(50));
    }

    [Test]
    public void Constructor_ClampsNegativeValuesToZero()
    {
        var player = new Player("test", -10f, -5, false);

        Assert.That(player.health, Is.EqualTo(0f));
        Assert.That(player.level, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_KeepsLoginAndOnlineFlag()
    {
        var player = new Player("hero", 42f, 7, true);

        Assert.That(player.login, Is.EqualTo("hero"));
        Assert.That(player.bIsOnline, Is.True);
        Assert.That(player.currentLobby, Is.Null);
    }

    [Test]
    public void AddHealth_StaysWithinBounds()
    {
        var player = new Player("test", 50f, 10, true);

        player.AddHealth(60f);
        Assert.That(player.health, Is.EqualTo(100f));

        player.AddHealth(-150f);
        Assert.That(player.health, Is.EqualTo(0f));
    }

    [Test]
    public void AddLevel_StaysWithinBounds()
    {
        var player = new Player("test", 50f, 10, true);

        player.AddLevel(50);
        Assert.That(player.level, Is.EqualTo(50));

        player.AddLevel(-100);
        Assert.That(player.level, Is.EqualTo(0));
    }
}

[TestFixture]
public class ServerTests
{
    private Server _server;
    private Player _player1;

    [SetUp]
    public void SetUp()
    {
        _server = new Server();
        _player1 = new Player("player1", 80f, 20, true);
    }

    [Test]
    public void Constructor_CreatesDefaultLobby()
    {
        Assert.That(_server.GetLobbiesCount(), Is.EqualTo(1));
        Assert.That(_server.GetLobby(0), Is.Not.Null);
    }

    [Test]
    public void ConnectPlayer_AddsPlayerToServerAndDefaultLobby()
    {
        _server.ConnectPlayer(_player1);

        Assert.That(_server.GetPlayersList(), Has.Count.EqualTo(1));
        Assert.That(_server.SearchPlayerFromDictByLogin("player1"), Is.SameAs(_player1));
        Assert.That(_server.SearchPlayerFromListByLogin("player1"), Is.SameAs(_player1));
        Assert.That(_player1.currentLobby, Is.SameAs(_server.GetLobby(0)));
    }

    [Test]
    public void ConnectPlayer_SamePlayerTwice_DoesNotDuplicate()
    {
        _server.ConnectPlayer(_player1);
        _server.ConnectPlayer(_player1);

        Assert.That(_server.GetPlayersList(), Has.Count.EqualTo(1));
        Assert.That(_server.GetLobby(0)!.GetPlayersList(), Has.Count.EqualTo(1));
        Assert.That(_player1.currentLobby, Is.SameAs(_server.GetLobby(0)));
    }

    [Test]
    public void ConnectPlayer_AfterTransfer_KeepsPlayerInTransferredLobby()
    {
        _server.ConnectPlayer(_player1);
        _server.CreateLobby();
        var lobby1 = _server.GetLobby(1)!;
        _server.TransferPlayerToLobby(_player1, lobby1);

        _server.ConnectPlayer(_player1);

        Assert.That(_player1.currentLobby, Is.SameAs(lobby1));
        Assert.That(lobby1.GetPlayersList(), Does.Contain(_player1));
        Assert.That(_server.GetLobby(0)!.GetPlayersList(), Does.Not.Contain(_player1));
        Assert.That(_server.GetPlayersList(), Has.Count.EqualTo(1));
    }

    [Test]
    public void ConnectPlayer_AfterDisconnect_ConnectsAgainToDefaultLobby()
    {
        _server.ConnectPlayer(_player1);
        _server.DisconnectPlayer(_player1);

        _server.ConnectPlayer(_player1);

        Assert.That(_server.GetPlayersList(), Has.Count.EqualTo(1));
        Assert.That(_player1.currentLobby, Is.SameAs(_server.GetLobby(0)));
    }

    [Test]
    public void ConnectPlayer_Null_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => _server.ConnectPlayer(null!));
        Assert.That(ex!.ParamName, Is.EqualTo("player"));
    }

    [Test]
    public void DisconnectPlayer_RemovesPlayerFromServerAndLobby()
    {
        _server.ConnectPlayer(_player1);
        _server.DisconnectPlayer(_player1);

        Assert.That(_server.GetPlayersList(), Is.Empty);
        Assert.That(_server.SearchPlayerFromDictByLogin("player1"), Is.Null);
        Assert.That(_player1.currentLobby, Is.Null);
    }

    [Test]
    public void DisconnectPlayer_NotConnectedPlayer_DoesNotThrow()
    {
        var stranger = new Player("stranger", 50f, 5, false);

        Assert.DoesNotThrow(() => _server.DisconnectPlayer(stranger));
        Assert.That(_server.GetPlayersList(), Is.Empty);
    }

    [Test]
    public void DisconnectPlayer_Null_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => _server.DisconnectPlayer(null!));
        Assert.That(ex!.ParamName, Is.EqualTo("player"));
    }

    [Test]
    public void SearchPlayerFromListByLogin_ReturnsPlayer()
    {
        _server.ConnectPlayer(_player1);

        Assert.That(_server.SearchPlayerFromListByLogin("player1"), Is.SameAs(_player1));
    }

    [Test]
    public void SearchPlayerFromListByLogin_ReturnsNullForMissing()
    {
        _server.ConnectPlayer(_player1);

        Assert.That(_server.SearchPlayerFromListByLogin("missing"), Is.Null);
    }

    [Test]
    public void SearchPlayerFromListByLogin_NullLogin_ReturnsNull()
    {
        _server.ConnectPlayer(_player1);

        Assert.That(_server.SearchPlayerFromListByLogin(null!), Is.Null);
    }

    [Test]
    public void SearchPlayerFromDictByLogin_ReturnsPlayer()
    {
        _server.ConnectPlayer(_player1);

        Assert.That(_server.SearchPlayerFromDictByLogin("player1"), Is.SameAs(_player1));
    }

    [Test]
    public void SearchPlayerFromDictByLogin_ReturnsNullForMissing()
    {
        _server.ConnectPlayer(_player1);

        Assert.That(_server.SearchPlayerFromDictByLogin("missing"), Is.Null);
    }

    [Test]
    public void CreateLobby_IncreasesLobbyCount()
    {
        var before = _server.GetLobbiesCount();
        _server.CreateLobby();

        Assert.That(_server.GetLobbiesCount(), Is.EqualTo(before + 1));
    }

    [Test]
    public void TransferPlayerToLobby_MovesPlayerBetweenLobbies()
    {
        _server.ConnectPlayer(_player1);
        _server.CreateLobby();
        var lobby1 = _server.GetLobby(1)!;

        _server.TransferPlayerToLobby(_player1, lobby1);

        Assert.That(_player1.currentLobby, Is.SameAs(lobby1));
        Assert.That(_server.GetLobby(0)!.GetPlayersList(), Does.Not.Contain(_player1));
        Assert.That(lobby1.GetPlayersList(), Does.Contain(_player1));
    }

    [Test]
    public void TransferPlayerToLobby_NotConnectedPlayer_ConnectsToTargetLobby()
    {
        _server.CreateLobby();
        var lobby1 = _server.GetLobby(1)!;

        Assert.DoesNotThrow(() => _server.TransferPlayerToLobby(_player1, lobby1));
        Assert.That(_player1.currentLobby, Is.SameAs(lobby1));
        Assert.That(lobby1.GetPlayersList(), Does.Contain(_player1));
    }

    [Test]
    public void TransferPlayerToLobby_NullPlayer_ThrowsArgumentNullException()
    {
        var lobby = _server.GetLobby(0)!;

        var ex = Assert.Throws<ArgumentNullException>(() => _server.TransferPlayerToLobby(null!, lobby));
        Assert.That(ex!.ParamName, Is.EqualTo("player"));
    }

    [Test]
    public void TransferPlayerToLobby_NullLobby_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => _server.TransferPlayerToLobby(_player1, null!));
        Assert.That(ex!.ParamName, Is.EqualTo("lobby"));
    }

    [Test]
    public void GetLobby_ReturnsNullForInvalidIndex()
    {
        Assert.That(_server.GetLobby(-1), Is.Null);
        Assert.That(_server.GetLobby(999), Is.Null);
    }

    [Test]
    public void GetLobbyIndex_ReturnsCorrectIndexOrMinusOne()
    {
        var lobby = _server.GetLobby(0)!;
        Assert.That(_server.GetLobbyIndex(lobby), Is.EqualTo(0));

        var stranger = new Lobby();
        Assert.That(_server.GetLobbyIndex(stranger), Is.EqualTo(-1));
    }
}

[TestFixture]
public class LobbyTests
{
    private Lobby _lobby;
    private Player _player;

    [SetUp]
    public void SetUp()
    {
        _lobby = new Lobby();
        _player = new Player("lobbyPlayer", 100f, 10, true);
    }

    [Test]
    public void ConnectPlayer_AddsToLobbyAndSetsCurrentLobby()
    {
        _lobby.ConnectPlayer(_player);

        Assert.That(_lobby.GetPlayersList(), Has.Count.EqualTo(1));
        Assert.That(_lobby.SearchPlayerFromDictByLogin("lobbyPlayer"), Is.SameAs(_player));
        Assert.That(_lobby.SearchPlayerFromListByLogin("lobbyPlayer"), Is.SameAs(_player));
        Assert.That(_player.currentLobby, Is.SameAs(_lobby));
    }

    [Test]
    public void ConnectPlayer_Null_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => _lobby.ConnectPlayer(null!));
        Assert.That(ex!.ParamName, Is.EqualTo("player"));
    }

    [Test]
    public void ConnectPlayer_DoesNotDuplicateSamePlayer()
    {
        _lobby.ConnectPlayer(_player);
        _lobby.ConnectPlayer(_player);

        Assert.That(_lobby.GetPlayersList(), Has.Count.EqualTo(1));
    }

    [Test]
    public void DisconnectPlayer_RemovesAndClearsCurrentLobby()
    {
        _lobby.ConnectPlayer(_player);
        _lobby.DisconnectPlayer(_player);

        Assert.That(_lobby.GetPlayersList(), Is.Empty);
        Assert.That(_lobby.SearchPlayerFromDictByLogin("lobbyPlayer"), Is.Null);
        Assert.That(_player.currentLobby, Is.Null);
    }

    [Test]
    public void DisconnectPlayer_Null_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => _lobby.DisconnectPlayer(null!));
        Assert.That(ex!.ParamName, Is.EqualTo("player"));
    }

    [Test]
    public void SearchPlayerFromListByLogin_ReturnsNullForMissing()
    {
        Assert.That(_lobby.SearchPlayerFromListByLogin("missing"), Is.Null);
    }

    [Test]
    public void SearchPlayerFromDictByLogin_ReturnsNullForMissing()
    {
        Assert.That(_lobby.SearchPlayerFromDictByLogin("missing"), Is.Null);
    }
}