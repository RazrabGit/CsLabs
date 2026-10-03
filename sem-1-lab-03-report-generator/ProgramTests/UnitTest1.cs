using NUnit.Framework;
using lab03; // Убедитесь, что namespace вашего основного проекта совпадает

namespace lab03.Tests
{
    [TestFixture]
    public class ProgramTests
    {
        #region ChoosePrize Tests

        [TestCase(0, "Bag of chips")]
        [TestCase(1, "Salmon")]
        [TestCase(2, "1 рубль")]
        [TestCase(3, "Smell rtx 5090")]
        [TestCase(4, "Nothing")]
        [TestCase(5, "Nothing")] // Проверка некорректного значения (выход за пределы switch)
        public void ChoosePrize_ValidAndInvalidChoices_ReturnsExpectedPrize(byte choice, string expectedPrize)
        {
            // Act
            string result = Program.ChoosePrize(choice);

            // Assert (Современный синтаксис NUnit)
            Assert.That(result, Is.EqualTo(expectedPrize));
        }

        #endregion

        #region CheckSuccessByPlayers Tests

        [Test]
        public void CheckSuccessByPlayers_NotEnoughPlayers_ReturnsFalseAndCorrectMessage()
        {
            // Arrange
            byte players = 5;
            byte playersToStart = 10;

            // Act
            bool result = Program.CheckSuccessByPlayers(players, playersToStart, out string message);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(message, Is.EqualTo("Not enough players to start the event"));
        }

        [TestCase(10, 10)] // Ровно столько, сколько нужно
        [TestCase(15, 10)] // Больше, чем нужно
        public void CheckSuccessByPlayers_EnoughPlayers_ReturnsTrueAndNullMessage(byte players, byte playersToStart)
        {
            // Act
            bool result = Program.CheckSuccessByPlayers(players, playersToStart, out string message);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(message, Is.Null);
        }

        #endregion

        #region CheckSuccessByTime Tests

        [Test]
        public void CheckSuccessByTime_ZeroTime_ReturnsFalseAndCorrectMessage()
        {
            // Arrange
            byte time = 0;

            // Act
            bool result = Program.CheckSuccessByTime(time, out string message);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(message, Is.EqualTo("The event lasted less than an hour"));
        }

        [TestCase(1)]
        [TestCase(3)]
        public void CheckSuccessByTime_PositiveTime_ReturnsTrueAndNullMessage(byte time)
        {
            // Act
            bool result = Program.CheckSuccessByTime(time, out string message);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(message, Is.Null);
        }

        #endregion
    }
}