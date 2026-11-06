// О великий Qwen сильно постарался над тестами

using System.Reflection;

namespace lab06.Tests
{
    [TestFixture]
    public class ProgramTests
    {
        // Флаги для доступа к приватным статическим членам через Reflection
        private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Static;

        [SetUp]
        public void Setup()
        {
            ResetStaticState();
        }

        /// <summary>
        /// Сбрасывает всё статическое состояние Program перед каждым тестом,
        /// чтобы тесты были независимыми друг от друга.
        /// </summary>
        private void ResetStaticState()
        {
            var type = typeof(Program);

            // Все 9 списков данных
            ClearList(type, "lastServerCPUDataTenSec");
            ClearList(type, "lastServerCPUDataOneMin");
            ClearList(type, "lastServerCPUDataFiveMin");
            ClearList(type, "lastServerRAMDataTenSec");
            ClearList(type, "lastServerRAMDataOneMin");
            ClearList(type, "lastServerRAMDataFiveMin");
            ClearList(type, "lastServerTemperatureDataTenSec");
            ClearList(type, "lastServerTemperatureDataOneMin");
            ClearList(type, "lastServerTemperatureDataFiveMin");

            // Все 9 полей сумм
            type.GetField("sumOfCPUTenSec", Flags)?.SetValue(null, 0.0);
            type.GetField("sumOfCPUOneMin", Flags)?.SetValue(null, 0.0);
            type.GetField("sumOfCPUFiveMin", Flags)?.SetValue(null, 0.0);
            type.GetField("sumOfRAMTenSec", Flags)?.SetValue(null, 0.0);
            type.GetField("sumOfRAMOneMin", Flags)?.SetValue(null, 0.0);
            type.GetField("sumOfRAMFiveMin", Flags)?.SetValue(null, 0.0);
            type.GetField("sumOfTemperatureTenSec", Flags)?.SetValue(null, 0.0);
            type.GetField("sumOfTemperatureOneMin", Flags)?.SetValue(null, 0.0);
            type.GetField("sumOfTemperatureFiveMin", Flags)?.SetValue(null, 0.0);

            // Счётчик секунд
            type.GetField("secondsForCount", Flags)?.SetValue(null, 0);
        }

        private void ClearList(Type type, string fieldName)
        {
            var field = type.GetField(fieldName, Flags);
            if (field?.GetValue(null) is List<double> list)
            {
                list.Clear();
            }
        }

        #region SGraphParams Tests

        [Test]
        public void SGraphParams_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var param = new SGraphParams(0, 100, 10);

            // Assert
            Assert.That(param.minValue, Is.EqualTo(0));
            Assert.That(param.maxValue, Is.EqualTo(100));
            Assert.That(param.rowSections, Is.EqualTo(10));
        }

        [Test]
        public void SGraphParams_Constructor_CustomValues()
        {
            var param = new SGraphParams(10, 200, 5);

            Assert.That(param.minValue, Is.EqualTo(10));
            Assert.That(param.maxValue, Is.EqualTo(200));
            Assert.That(param.rowSections, Is.EqualTo(5));
        }

        #endregion

        #region CPUSum Tests

        [Test]
        public void CPUSum_10SecBuffer_AveragesEvery10Calls()
        {
            // Arrange
            var method = typeof(Program).GetMethod("CPUSum", Flags);
            var secField = typeof(Program).GetField("secondsForCount", Flags);
            var listField = typeof(Program).GetField("lastServerCPUDataTenSec", Flags);

            // Act - симулируем 10 вызовов со значением 10.0
            for (int i = 1; i <= 10; i++)
            {
                secField.SetValue(null, i);
                method.Invoke(null, new object[] { 10.0 });
            }

            // Assert - должно появиться одно усреднённое значение
            var list = (List<double>)listField.GetValue(null);
            Assert.That(list.Count, Is.EqualTo(1), "После 10 вызовов должно быть 1 усреднённое значение");
            Assert.That(list[0], Is.EqualTo(10.0).Within(0.01), "Среднее 10 одинаковых значений = 10.0");
        }

        [Test]
        public void CPUSum_10SecBuffer_CalculatesCorrectAverage()
        {
            // Arrange
            var method = typeof(Program).GetMethod("CPUSum", Flags);
            var secField = typeof(Program).GetField("secondsForCount", Flags);
            var listField = typeof(Program).GetField("lastServerCPUDataTenSec", Flags);

            // Act - 10 разных значений: 1, 2, ..., 10. Среднее = 5.5
            for (int i = 1; i <= 10; i++)
            {
                secField.SetValue(null, i);
                method.Invoke(null, new object[] { (double)i });
            }

            // Assert
            var list = (List<double>)listField.GetValue(null);
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(5.5).Within(0.01), "Среднее от 1 до 10 = 5.5");
        }

        [Test]
        public void CPUSum_1MinBuffer_AveragesEvery60Calls()
        {
            // Arrange
            var method = typeof(Program).GetMethod("CPUSum", Flags);
            var secField = typeof(Program).GetField("secondsForCount", Flags);
            var listField = typeof(Program).GetField("lastServerCPUDataOneMin", Flags);

            // Act - 59 вызовов не должны давать результата
            for (int i = 1; i <= 59; i++)
            {
                secField.SetValue(null, i);
                method.Invoke(null, new object[] { 20.0 });
            }

            var list = (List<double>)listField.GetValue(null);
            Assert.That(list.Count, Is.EqualTo(0), "До 60 вызовов список должен быть пуст");

            // 60-й вызов должен дать усреднённое значение
            secField.SetValue(null, 60);
            method.Invoke(null, new object[] { 20.0 });

            list = (List<double>)listField.GetValue(null);
            Assert.That(list.Count, Is.EqualTo(1), "На 60-м вызове должно появиться значение");
            Assert.That(list[0], Is.EqualTo(20.0).Within(0.01));
        }

        [Test]
        public void CPUSum_5MinBuffer_AveragesEvery300Calls()
        {
            // Arrange
            var method = typeof(Program).GetMethod("CPUSum", Flags);
            var secField = typeof(Program).GetField("secondsForCount", Flags);
            var listField = typeof(Program).GetField("lastServerCPUDataFiveMin", Flags);

            // Act - 299 вызовов
            for (int i = 1; i <= 299; i++)
            {
                secField.SetValue(null, i);
                method.Invoke(null, new object[] { 15.0 });
            }

            var list = (List<double>)listField.GetValue(null);
            Assert.That(list.Count, Is.EqualTo(0), "До 300 вызовов список должен быть пуст");

            // 300-й вызов
            secField.SetValue(null, 300);
            method.Invoke(null, new object[] { 15.0 });

            list = (List<double>)listField.GetValue(null);
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(15.0).Within(0.01));
        }

        [Test]
        public void CPUSum_AllBuffers_LimitTo20Items()
        {
            // Arrange
            var method = typeof(Program).GetMethod("CPUSum", Flags);
            var secField = typeof(Program).GetField("secondsForCount", Flags);
            var list10 = (List<double>)typeof(Program).GetField("lastServerCPUDataTenSec", Flags).GetValue(null);
            var list1 = (List<double>)typeof(Program).GetField("lastServerCPUDataOneMin", Flags).GetValue(null);
            var list5 = (List<double>)typeof(Program).GetField("lastServerCPUDataFiveMin", Flags).GetValue(null);

            // Act - симулируем 600 вызовов (21 усреднение для 10-сек, 10 для 1-мин, 2 для 5-мин)
            for (int i = 1; i <= 600; i++)
            {
                secField.SetValue(null, i);
                method.Invoke(null, new object[] { 5.0 });
            }

            // Assert - все списки не должны превышать 20 элементов
            Assert.That(list10.Count, Is.LessThanOrEqualTo(20), "10-сек буфер ограничен 20 элементами");
            Assert.That(list1.Count, Is.LessThanOrEqualTo(20), "1-мин буфер ограничен 20 элементами");
            Assert.That(list5.Count, Is.LessThanOrEqualTo(20), "5-мин буфер ограничен 20 элементами");
        }

        #endregion

        #region RAMSum Tests

        [Test]
        public void RAMSum_10SecBuffer_AveragesEvery10Calls()
        {
            // Arrange
            var method = typeof(Program).GetMethod("RAMSum", Flags);
            var secField = typeof(Program).GetField("secondsForCount", Flags);
            var listField = typeof(Program).GetField("lastServerRAMDataTenSec", Flags);

            // Act
            for (int i = 1; i <= 10; i++)
            {
                secField.SetValue(null, i);
                method.Invoke(null, new object[] { 50.0 });
            }

            // Assert
            var list = (List<double>)listField.GetValue(null);
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(50.0).Within(0.01));
        }

        [Test]
        public void RAMSum_1MinBuffer_AveragesEvery60Calls()
        {
            var method = typeof(Program).GetMethod("RAMSum", Flags);
            var secField = typeof(Program).GetField("secondsForCount", Flags);
            var listField = typeof(Program).GetField("lastServerRAMDataOneMin", Flags);

            for (int i = 1; i <= 60; i++)
            {
                secField.SetValue(null, i);
                method.Invoke(null, new object[] { 30.0 });
            }

            var list = (List<double>)listField.GetValue(null);
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(30.0).Within(0.01));
        }

        [Test]
        public void RAMSum_5MinBuffer_AveragesEvery300Calls()
        {
            var method = typeof(Program).GetMethod("RAMSum", Flags);
            var secField = typeof(Program).GetField("secondsForCount", Flags);
            var listField = typeof(Program).GetField("lastServerRAMDataFiveMin", Flags);

            for (int i = 1; i <= 300; i++)
            {
                secField.SetValue(null, i);
                method.Invoke(null, new object[] { 40.0 });
            }

            var list = (List<double>)listField.GetValue(null);
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(40.0).Within(0.01));
        }

        #endregion

        #region TemperatureSum Tests

        [Test]
        public void TemperatureSum_10SecBuffer_AveragesEvery10Calls()
        {
            var method = typeof(Program).GetMethod("TemperatureSum", Flags);
            var secField = typeof(Program).GetField("secondsForCount", Flags);
            var listField = typeof(Program).GetField("lastServerTemperatureDataTenSec", Flags);

            for (int i = 1; i <= 10; i++)
            {
                secField.SetValue(null, i);
                method.Invoke(null, new object[] { 65.5 });
            }

            var list = (List<double>)listField.GetValue(null);
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(65.5).Within(0.01));
        }

        [Test]
        public void TemperatureSum_1MinBuffer_AveragesEvery60Calls()
        {
            var method = typeof(Program).GetMethod("TemperatureSum", Flags);
            var secField = typeof(Program).GetField("secondsForCount", Flags);
            var listField = typeof(Program).GetField("lastServerTemperatureDataOneMin", Flags);

            for (int i = 1; i <= 60; i++)
            {
                secField.SetValue(null, i);
                method.Invoke(null, new object[] { 70.0 });
            }

            var list = (List<double>)listField.GetValue(null);
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(70.0).Within(0.01));
        }

        [Test]
        public void TemperatureSum_5MinBuffer_AveragesEvery300Calls()
        {
            var method = typeof(Program).GetMethod("TemperatureSum", Flags);
            var secField = typeof(Program).GetField("secondsForCount", Flags);
            var listField = typeof(Program).GetField("lastServerTemperatureDataFiveMin", Flags);

            for (int i = 1; i <= 300; i++)
            {
                secField.SetValue(null, i);
                method.Invoke(null, new object[] { 55.0 });
            }

            var list = (List<double>)listField.GetValue(null);
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(55.0).Within(0.01));
        }

        #endregion
    }
}