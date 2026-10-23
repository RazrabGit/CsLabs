// О великий Qwen сильно постарался над тестами

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using lab06; // Убедитесь, что пространство имен совпадает с вашим проектом

namespace lab06.Tests
{
    [TestFixture]
    public class ProgramTests
    {
        private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Static;

        [SetUp]
        public void Setup()
        {
            // Сбрасываем статическое состояние перед каждым тестом
            ResetStaticState();
        }

        private void ResetStaticState()
        {
            var type = typeof(Program);

            // Очищаем списки
            var list10 = (List<double>)type.GetField("lastServerCPUData", Flags).GetValue(null);
            list10?.Clear();

            var list1 = (List<double>)type.GetField("lastServerCPUDataOneMin", Flags).GetValue(null);
            list1?.Clear();

            var list5 = (List<double>)type.GetField("lastServerCPUDataFiveMin", Flags).GetValue(null);
            list5?.Clear();

            // Сбрасываем счетчики и суммы
            type.GetField("sumOfCPUOneMin", Flags)?.SetValue(null, 0.0);
            type.GetField("sumOfCPUFiveMin", Flags)?.SetValue(null, 0.0);
            type.GetField("secondsForCount", Flags)?.SetValue(null, 0);
        }

        #region SGraphParams Tests

        [Test]
        public void SGraphParams_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var param = new SGraphParams(10, 100, 5);

            // Assert (Используем Assert.That и Is.EqualTo)
            Assert.That(param.minValue, Is.EqualTo(10));
            Assert.That(param.maxValue, Is.EqualTo(100));
            Assert.That(param.rowSections, Is.EqualTo(5));
        }

        #endregion

        #region CPUSum Tests

        [Test]
        public void CPUSum_10SecBuffer_KeepsOnlyLast10Values()
        {
            // Arrange
            var method = typeof(Program).GetMethod("CPUSum", Flags);
            var listField = typeof(Program).GetField("lastServerCPUData", Flags);

            // Act - добавляем 12 значений
            for (int i = 1; i <= 12; i++)
            {
                method.Invoke(null, new object[] { (double)i });
            }

            // Assert
            var list = (List<double>)listField.GetValue(null);

            Assert.That(list.Count, Is.EqualTo(10), "Список 10-секундного интервала должен содержать максимум 10 элементов");
            Assert.That(list[0], Is.EqualTo(3.0), "Первые два элемента (1 и 2) должны быть удалены");
            Assert.That(list[9], Is.EqualTo(12.0), "Последний элемент должен быть 12");
        }

        [Test]
        public void CPUSum_1MinAggregation_CalculatesAverageEvery3Seconds()
        {
            // Arrange
            var method = typeof(Program).GetMethod("CPUSum", Flags);
            var secField = typeof(Program).GetField("secondsForCount", Flags);
            var listField = typeof(Program).GetField("lastServerCPUDataOneMin", Flags);

            // Act - симулируем 3 секунды
            secField.SetValue(null, 1); method.Invoke(null, new object[] { 10.0 });
            secField.SetValue(null, 2); method.Invoke(null, new object[] { 20.0 });
            secField.SetValue(null, 3); method.Invoke(null, new object[] { 30.0 }); // Должно сработать усреднение

            // Assert
            var list = (List<double>)listField.GetValue(null);

            Assert.That(list.Count, Is.EqualTo(1), "Должно быть добавлено одно усредненное значение");

            // Для дробных чисел используем .Within() для указания погрешности
            Assert.That(list[0], Is.EqualTo(20.0).Within(0.01), "Среднее значение за 3 секунды должно быть 20.0");
        }

        [Test]
        public void CPUSum_5MinAggregation_CalculatesAverageEvery15Seconds()
        {
            // Arrange
            var method = typeof(Program).GetMethod("CPUSum", Flags);
            var secField = typeof(Program).GetField("secondsForCount", Flags);
            var listField = typeof(Program).GetField("lastServerCPUDataFiveMin", Flags);

            // Act - симулируем 15 секунд
            for (int i = 1; i <= 14; i++)
            {
                secField.SetValue(null, i);
                method.Invoke(null, new object[] { 10.0 });
            }

            secField.SetValue(null, 15);
            method.Invoke(null, new object[] { 10.0 }); // Должно сработать усреднение

            // Assert
            var list = (List<double>)listField.GetValue(null);

            Assert.That(list.Count, Is.EqualTo(1), "Должно быть добавлено одно усредненное значение");
            Assert.That(list[0], Is.EqualTo(10.0).Within(0.01), "Среднее значение за 15 секунд должно быть 10.0");
        }

        [Test]
        public void CPUSum_1MinAggregation_LimitsListTo20Items()
        {
            // Arrange
            var method = typeof(Program).GetMethod("CPUSum", Flags);
            var secField = typeof(Program).GetField("secondsForCount", Flags);
            var listField = typeof(Program).GetField("lastServerCPUDataOneMin", Flags);

            // Act - симулируем 63 секунды (21 срабатывание усреднения)
            for (int i = 1; i <= 63; i++)
            {
                secField.SetValue(null, i);
                method.Invoke(null, new object[] { 5.0 });
            }

            // Assert
            var list = (List<double>)listField.GetValue(null);

            Assert.That(list.Count, Is.EqualTo(20), "Список 1-минутного интервала не должен превышать 20 элементов");
        }

        #endregion
    }
}