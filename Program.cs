using System;
using System.Collections;
using System.IO;

namespace Medvedev
{
    enum AccountType
    {
        Deposit, //депозитныйй
        Credit, //кредитный
        Settlement, //расчетный
        Savings //сберегательный
    }

    class BankTransaction
    {
        private readonly DateTime date;
        private readonly decimal amount;

        public BankTransaction(decimal amount)
        {
            this.amount = amount;
            this.date = DateTime.Now;
        }

        public DateTime Date
        {
            get { return date; }
        }

        public decimal Amount 
        {  
            get { return amount; }
        }

        public override string ToString()
        {
            return $"{date:dd.MM.yyy HH:mm:ss} | {amount:+0.00; -0.00; 0.00} ₽";
        }

        class BankAccount
        {
            private static int nextAccountNumber = 1;

            private int accountNumber; //номер счета
            private decimal balance; //баланс
            private AccountType type; //тип счета - ENUM
            private Queue transactions = new Queue();

            private static int GenerateAccountNumber()
            {
                int number = nextAccountNumber; //взяли текущий
                nextAccountNumber++; //увеличили для следующего
                return number; // вернули уникальный
            }

            public BankAccount()
            {
                accountNumber = GenerateAccountNumber();
            }

            public BankAccount(decimal balance) : this()
            {
                this.balance = balance;
            }

            public BankAccount(AccountType type) : this()
            {
                this.type = type;
            }

            public BankAccount(decimal balance, AccountType type) : this()
            {
                this.balance = balance;
                this.type = type;
            }

            public int AccountNumber
            {
                get { return accountNumber; }
            }

            public decimal Balance
            {
                get { return balance; }
            }

            public AccountType Type
            {
                get { return type; }
            }

            public void Access()
            {
                Console.Write("Введите баланс: ");
                decimal b;
                while (!decimal.TryParse(Console.ReadLine(), out b) || b < 0)
                    Console.Write("Некорректно, повторите: ");

                balance = b;

                Console.WriteLine("Выберите тип счёта:");
                Console.WriteLine("1 — депозитный");
                Console.WriteLine("2 — кредитный");
                Console.WriteLine("3 — расчётный");
                Console.WriteLine("4 — сберегательный");
                Console.Write("Ваш выбор: ");

                int choice = int.Parse(Console.ReadLine());

                if (choice == 1) type = AccountType.Deposit;
                else if (choice == 2) type = AccountType.Credit;
                else if (choice == 3) type = AccountType.Settlement;
                else type = AccountType.Savings;

                Console.WriteLine("Выберите действие со счётом:");
                Console.WriteLine("1 - Снять деньги");
                Console.WriteLine("2 - Пополнить счёт");
                Console.Write("Ваш выбор: ");
                int action;
                while (!int.TryParse(Console.ReadLine(), out action) || action < 1 || action > 2)
                    Console.Write("Некорректно (1–2), повторите: ");

                if (action == 1)
                {
                    Console.Write("Какую сумму хотите снять: ");
                    decimal db = decimal.Parse(Console.ReadLine());
                    TakeOff(db);
                }
                else if (action == 2)
                {
                    Console.Write("Какую сумму хотите положить: ");
                    decimal bd = decimal.Parse(Console.ReadLine());
                    Put(bd);
                }
            }

            private void TakeOff(decimal db)
            {
                if (db > balance)
                {
                    Console.WriteLine($"Недостаточно средств на счёте {Type}");
                }
                else
                {
                    balance -= db;
                    transactions.Enqueue(new BankTransaction(-db));
                }
            }

            private void Put(decimal bd)
            {
                balance += bd;
                transactions.Enqueue(new BankTransaction(+bd));
            }

            public void ShowTransactions()
            {
                Console.WriteLine($"История операций по счету №{accountNumber}");
                if(transactions.Count == 0)
                {
                    Console.WriteLine("Операций небыло");
                    return;
                }

                foreach(BankTransaction item in transactions)
                {
                    Console.WriteLine(item);
                }
            }

            public void Conclusion()
            {
                Console.WriteLine($"Номер счёта: {accountNumber}");
                Console.WriteLine($"Баланс: {balance:F2} ₽");
                Console.WriteLine($"Тип: {TypeToRussian(type)}");
            }

            private static string TypeToRussian(AccountType t)
            {
                if (t == AccountType.Deposit) return "депозитный";
                if (t == AccountType.Credit) return "кредитный";
                if (t == AccountType.Settlement) return "расчётный";
                if (t == AccountType.Savings) return "сберегательный";
                return "неизвестный";
            }

            public override string ToString()
            {
                return $"Счёт № {accountNumber} | тип: {TypeToRussian(type)} | баланс: {balance:F2} ₽";
            }

            public void Transfer(BankAccount fromAccount, decimal amount)
            {
                fromAccount.balance -= amount;
                this.balance += amount;
            }
        }

        class Program
        {
            public static void Main(string[] args)
            {
                BankAccount bank1 = new BankAccount();
                Console.WriteLine("--- Счёт № 1 ---");
                bank1.Access();
                bank1.Conclusion();

                Console.WriteLine();
                BankAccount bank2 = new BankAccount();
                Console.WriteLine("--- Счёт № 2 ---");
                bank2.Access();
                bank2.Conclusion();

                Console.WriteLine("\nПеревод 100 ₽ со счёта №1 на счёт №2");
                try
                {
                    bank2.Transfer(bank1, 100);
                    bank1.Conclusion();
                    bank2.Conclusion();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка перевода: {ex.Message}");
                }

                Console.WriteLine();
                bank1.ShowTransactions();
                bank2.ShowTransactions();
            }
        }
    }
}