// НАПИСАН С ПОМОЩЬЮ НЕЙРОСЕТИ!

using NUnit.Framework;
using lab02;

namespace Lab02_NUnit_Tests
{
    [TestFixture]
    public class ServerValidationTests
    {
        #region CheckMaxPlayers Tests

        [Test]
        public void CheckMaxPlayers_ValidPlayers_Defuse_ReturnsTrue()
        {
            bool result = Program.CheckMaxPlayers(5, EGamemode.Defuse, out string message);

            Assert.That(result, Is.True);
            Assert.That(message, Is.EqualTo(""));
        }

        [Test]
        public void CheckMaxPlayers_TooManyPlayers_ReturnsFalse()
        {
            // В Defuse максимум 10, передаем 15
            bool result = Program.CheckMaxPlayers(15, EGamemode.Defuse, out string message);

            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("Too much players"));
        }

        [Test]
        public void CheckMaxPlayers_TooFewPlayers_BattleRoyal_ReturnsFalse()
        {
            // Для BattleRoyal нужно минимум 16, передаем 10
            bool result = Program.CheckMaxPlayers(10, EGamemode.BattleRoyal, out string message);

            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("Too low players"));
        }

        #endregion

        #region CheckGameMaps Tests

        [Test]
        public void CheckGameMaps_SavannaWithDefuse_ReturnsTrue()
        {
            bool result = Program.CheckGameMaps(EGameMaps.Savanna, EGamemode.Defuse, out string message);

            Assert.That(result, Is.True);
            Assert.That(message, Is.EqualTo(""));
        }

        [Test]
        public void CheckGameMaps_SavannaWithHostage_ReturnsFalse()
        {
            bool result = Program.CheckGameMaps(EGameMaps.Savanna, EGamemode.Hostage, out string message);

            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("wrong gamemode"));
        }

        [Test]
        public void CheckGameMaps_Test01_AlwaysReturnsFalse()
        {
            bool result = Program.CheckGameMaps(EGameMaps.Test01, EGamemode.Default, out string message);

            Assert.That(result, Is.False);
            Assert.That(message, Is.EqualTo("Test01 is a wrong map for playing!"));
        }

        [Test]
        public void CheckGameMaps_SewerageBeta_ReturnsTrueWithWarning()
        {
            bool result = Program.CheckGameMaps(EGameMaps.Sewerage_Beta, EGamemode.Deadmatch, out string message);

            Assert.That(result, Is.True);
            Assert.That(message, Does.Contain("not fully tested"));
        }

        #endregion

        #region CheckAccessibility Tests

        [Test]
        public void CheckAccessibility_PublicWithoutPassword_ReturnsTrue()
        {
            bool result = Program.CheckAccessibility(false, "", EServerAccessibility.Public, out string message);

            Assert.That(result, Is.True);
            Assert.That(message, Is.EqualTo(""));
        }

        [Test]
        public void CheckAccessibility_PublicWithPassword_ReturnsFalse()
        {
            bool result = Program.CheckAccessibility(true, "12345", EServerAccessibility.Public, out string message);

            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("idiot"));
        }

        [Test]
        public void CheckAccessibility_ForFriendsWithEasyPassword_ReturnsTrueWithWarning()
        {
            bool result = Program.CheckAccessibility(true, "1111", EServerAccessibility.ForFriends, out string message);

            Assert.That(result, Is.True);
            Assert.That(message, Does.Contain("too easy"));
        }

        [Test]
        public void CheckAccessibility_PrivateWithoutPassword_ReturnsFalse()
        {
            bool result = Program.CheckAccessibility(false, "", EServerAccessibility.Private, out string message);

            Assert.That(result, Is.False);
            Assert.That(message, Does.Contain("unable to join"));
        }

        [Test]
        public void CheckAccessibility_PrivateWithGoodPassword_ReturnsTrue()
        {
            bool result = Program.CheckAccessibility(true, "SuperSecurePass!", EServerAccessibility.Private, out string message);

            Assert.That(result, Is.True);
            Assert.That(message, Is.EqualTo(""));
        }

        #endregion
    }
}