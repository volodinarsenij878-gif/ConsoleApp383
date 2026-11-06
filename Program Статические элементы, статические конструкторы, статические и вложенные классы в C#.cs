using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Program13
{
    namespace BankingHub
    {
        // =========================================================
        // Шаг 1. Статический класс CurrencyConverter
        // =========================================================
        public static class CurrencyConverter
        {
            // Статические свойства (private set — менять можно только внутри класса)
            public static double UsdRate { get; private set; }
            public static double EurRate { get; private set; }

            // Статический конструктор: без модификаторов доступа, без параметров,
            // вызывается CLR один раз при первом обращении к классу
            static CurrencyConverter()
            {
                Console.WriteLine("[CurrencyConverter]: Загрузка курсов валют...");
                UsdRate = 92.5;
                EurRate = 100.2;
            }

            // Обновление курсов с проверкой (> 0)
            public static void UpdateRates(double newUsd, double newEur)
            {
                if (newUsd <= 0 || newEur <= 0)
                {
                    Console.WriteLine("[CurrencyConverter] Ошибка: курсы должны быть строго больше нуля.");
                    return;
                }
                UsdRate = newUsd;
                EurRate = newEur;
                Console.WriteLine($"[CurrencyConverter] Курсы обновлены: USD={UsdRate}, EUR={EurRate}.");
            }

            // Перевод в рубли; при некорректном коде — исходная сумма
            public static decimal ConvertToRub(decimal foreignAmount, string currencyCode)
            {
                switch (currencyCode)
                {
                    case "USD":
                        return foreignAmount * (decimal)UsdRate;
                    case "EUR":
                        return foreignAmount * (decimal)EurRate;
                    default:
                        Console.WriteLine($"[CurrencyConverter] Неизвестный код валюты: {currencyCode}. Возврат исходной суммы.");
                        return foreignAmount;
                }
            }
        }

        // =========================================================
        // Шаги 2–4. Класс BankAccount со статическими и вложенными элементами
        // =========================================================
        public class BankAccount
        {
            // ---------- Статические элементы ----------
            private static int _nextAccountNumber;      // глобальный автоинкремент
            public static int TotalAccountsCreated { get; private set; }
            public static decimal GlobalBankReserves { get; private set; }

            // Статический конструктор ядра банка
            static BankAccount()
            {
                _nextAccountNumber = 10001;
                TotalAccountsCreated = 0;
                GlobalBankReserves = 0;
                Console.WriteLine("[BankAccount]: Статический конструктор — ядро банка инициализировано. Первый номер счета: 10001.");
            }

            // ---------- Экземплярные элементы ----------
            public int AccountNumber { get; }
            public string OwnerName { get; }
            public decimal Balance { get; private set; }

            // История операций: массив закрытого вложенного типа на 10 записей
            private StatementRecord[] _records = new StatementRecord[10];
            private int _recordCount = 0;

            // ЗАКРЫТЫЙ конструктор: создавать счета можно только через фабричный метод
            private BankAccount(string owner, decimal initialBalance)
            {
                AccountNumber = _nextAccountNumber++;
                OwnerName = owner;
                Balance = initialBalance;
            }

            // ---------- Статический фабричный метод ----------
            public static BankAccount OpenAccount(string owner, decimal initialDeposit)
            {
                if (initialDeposit < 1000m)
                {
                    Console.WriteLine($"[Банк] Отказ в открытии счета для {owner}: минимальный депозит 1000 руб. (запрошено {initialDeposit} руб.).");
                    return null;
                }

                var account = new BankAccount(owner, initialDeposit);
                TotalAccountsCreated++;
                GlobalBankReserves += initialDeposit;
                account.AddRecord("Открытие счета (депозит)", initialDeposit);

                Console.WriteLine($"[Банк] Открыт счет №{account.AccountNumber} для {owner}. Баланс: {account.Balance} руб.");
                return account;
            }

            // ---------- Шаг 3. Закрытый вложенный класс StatementRecord ----------
            private class StatementRecord
            {
                public DateTime Timestamp { get; }
                public string OperationType { get; }
                public decimal Amount { get; }

                public StatementRecord(string opType, decimal amount)
                {
                    Timestamp = DateTime.Now;
                    OperationType = opType;
                    Amount = amount;
                }
            }

            // Закрытый метод добавления записи в журнал
            private void AddRecord(string op, decimal amount)
            {
                if (_recordCount >= _records.Length)
                {
                    Console.WriteLine($"[Счет №{AccountNumber}] Журнал операций заполнен — запись \"{op}\" не сохранена.");
                    return;
                }

                _records[_recordCount] = new StatementRecord(op, amount);
                _recordCount++;
            }

            // ---------- Шаг 3. Публичный вложенный класс SecurityAudit ----------
            public class SecurityAudit
            {
                // Имеет прямой доступ к закрытым членам BankAccount
                public static void Inspect(BankAccount account)
                {
                    Console.WriteLine($"--- АУДИТ СЧЕТА №{account.AccountNumber} ({account.OwnerName}) ---");
                    Console.WriteLine($"Всего операций в журнале: {account._recordCount}");

                    bool suspiciousFound = false;
                    for (int i = 0; i < account._recordCount; i++)
                    {
                        var rec = account._records[i];
                        Console.WriteLine($"  [{rec.Timestamp:dd.MM.yyyy HH:mm:ss}] {rec.OperationType}: {rec.Amount} руб.");

                        if (rec.Amount > 50000m)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"  [ФИНАНСОВЫЙ МОНИТОРИНГ] Операция на {rec.Amount} руб. превышает порог 50 000 руб.!");
                            Console.ResetColor();
                            suspiciousFound = true;
                        }
                    }

                    if (!suspiciousFound)
                    {
                        Console.WriteLine("  Подозрительных операций не обнаружено.");
                    }
                    Console.WriteLine("--- КОНЕЦ АУДИТА ---\n");
                }
            }

            // ---------- Шаг 4. Бизнес-методы ----------
            public void Deposit(decimal amount)
            {
                if (amount <= 0)
                {
                    Console.WriteLine($"[Счет №{AccountNumber}] Сумма пополнения должна быть положительной.");
                    return;
                }

                Balance += amount;
                GlobalBankReserves += amount;
                AddRecord("Пополнение", amount);
                Console.WriteLine($"[Счет №{AccountNumber}] Пополнение на {amount} руб. Баланс: {Balance} руб.");
            }

            public bool Withdraw(decimal amount)
            {
                if (amount <= 0)
                {
                    Console.WriteLine($"[Счет №{AccountNumber}] Сумма снятия должна быть положительной.");
                    return false;
                }

                if (amount > Balance)
                {
                    Console.WriteLine($"[Счет №{AccountNumber}] Отказ: недостаточно средств (запрошено {amount}, доступно {Balance}).");
                    return false;
                }

                Balance -= amount;
                GlobalBankReserves -= amount;
                AddRecord("Снятие", amount);
                Console.WriteLine($"[Счет №{AccountNumber}] Снятие {amount} руб. Баланс: {Balance} руб.");
                return true;
            }
        }

        /*
         =========================================================
         Шаг 5.7 (в комментарии): попытка создать закрытый вложенный класс снаружи

         var rec = new BankAccount.StatementRecord(); // ОШИБКА КОМПИЛЯЦИИ:
         CS0122: 'BankAccount.StatementRecord' is inaccessible due to its protection level.

         StatementRecord объявлен как private внутри BankAccount, поэтому
         извне его тип недоступен — с ним работает только сам класс BankAccount
         (и вложенный SecurityAudit, который находится внутри BankAccount).
         =========================================================
         */

        // =========================================================
        // Шаг 5. Program.cs — моделирование
        // =========================================================
        class Program
        {
            static void Main()
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.WriteLine("=== ЭЛЕКТРОННЫЙ РАСЧЕТНЫЙ ЦЕНТР ===\n");

                // 1. Конвертация ДО создания счетов — здесь сработает
                //    статический конструктор CurrencyConverter
                decimal converted = CurrencyConverter.ConvertToRub(100, "USD");
                Console.WriteLine($"Конвертация: 100 USD = {converted} руб.\n");

                // 2. Попытка открыть счет с депозитом 500 руб. — обработка отказа
                BankAccount rejected = BankAccount.OpenAccount("Пётр Петров", 500m);
                if (rejected == null)
                {
                    Console.WriteLine("Счет не открыт: депозит меньше минимального.\n");
                }

                // 3. Открытие двух счетов (первый вызов OpenAccount запустит
                //    статический конструктор BankAccount, если он еще не выполнен)
                BankAccount account1 = BankAccount.OpenAccount("Иван Иванов", 50000m);
                BankAccount account2 = BankAccount.OpenAccount("Анна Смирнова", 20000m);

                // 4. Транзакции
                Console.WriteLine("\n--- ТРАНЗАКЦИИ ---\n");
                account1.Deposit(15000m);          // пополнение
                account1.Withdraw(60000m);         // крупное снятие (для аудита)
                account2.Deposit(30000m);          // пополнение
                account2.Withdraw(5000m);          // снятие
                account2.Withdraw(999999m);        // попытка снять слишком много

                // Демонстрация конвертера в действии
                decimal eurInRub = CurrencyConverter.ConvertToRub(200, "EUR");
                Console.WriteLine($"Конвертация: 200 EUR = {eurInRub} руб.");
                decimal wrongCode = CurrencyConverter.ConvertToRub(100, "GBP");
                Console.WriteLine($"Конвертация: 100 GBP (некорректный код) = {wrongCode} руб.\n");

                // 5. Сводка по банку через статические свойства
                Console.WriteLine("--- СВОДКА ПО БАНКУ ---");
                Console.WriteLine($"Всего счетов создано:        {BankAccount.TotalAccountsCreated}");
                Console.WriteLine($"Совокупные резервы банка:    {BankAccount.GlobalBankReserves} руб.\n");

                // 6. Аудит счета через публичный вложенный класс
                BankAccount.SecurityAudit.Inspect(account1);
                BankAccount.SecurityAudit.Inspect(account2);

                Console.WriteLine("Нажмите любую клавишу для завершения...");
                Console.ReadKey();
            }
        }
    }
}