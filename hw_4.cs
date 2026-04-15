using System.Threading;

namespace SystemProgramming;

internal class Hw4ThreadingDemo
{
    public void Run()
    {
        Console.WriteLine("Task 1: Race condition");
        RunRaceConditionTask();

        Console.WriteLine();
        Console.WriteLine("Task 2: ThreadPool");
        RunThreadPoolTask();
    }

    private void RunRaceConditionTask()
    {
        var account = new BankAccount(initialBalance: 1000);

        int totalDeposited = 0;
        int totalWithdrawn = 0;

        Thread[] workers = new Thread[3];

        for (int i = 0; i < workers.Length; i++)
        {
            int workerId = i + 1;
            workers[i] = new Thread(() =>
            {
                while (true)
                {
                    if (Random.Shared.Next(0, 2) == 0)
                    {
                        int amount = Random.Shared.Next(20, 101);
                        bool success = account.Deposit(amount);

                        if (!success)
                        {
                            Console.WriteLine($"Thread {workerId}: account is locked, stopping.");
                            break;
                        }

                        Interlocked.Add(ref totalDeposited, amount);
                    }
                    else
                    {
                        int amount = Random.Shared.Next(20, 101);
                        bool success = account.Withdraw(amount);

                        if (!success)
                        {
                            if (account.IsLocked)
                            {
                                Console.WriteLine($"Thread {workerId}: account is locked, stopping.");
                                break;
                            }

                            continue;
                        }

                        Interlocked.Add(ref totalWithdrawn, amount);
                    }

                    Thread.Sleep(Random.Shared.Next(20, 80));
                }
            });

            workers[i].Start();
        }

        Thread.Sleep(1200);
        account.LockAccount();

        for (int i = 0; i < workers.Length; i++)
        {
            workers[i].Join();
        }

        int expectedBalance = 1000 + totalDeposited - totalWithdrawn;

        Console.WriteLine($"Deposited: {totalDeposited}");
        Console.WriteLine($"Withdrawn: {totalWithdrawn}");
        Console.WriteLine($"Expected balance: {expectedBalance}");
        Console.WriteLine($"Actual balance: {account.Balance}");
        Console.WriteLine(expectedBalance == account.Balance
            ? "Balance is correct (race condition fixed)."
            : "ERROR: balance is incorrect.");
    }

    private void RunThreadPoolTask()
    {
        const int ordersCount = 10;

        using var countdown = new CountdownEvent(ordersCount);

        for (int orderNumber = 1; orderNumber <= ordersCount; orderNumber++)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    int currentOrder = (int)state!;

                    Thread.Sleep(Random.Shared.Next(250, 900));

                    Console.WriteLine($"Order {currentOrder} processed on thread {Thread.CurrentThread.ManagedThreadId}");
                }
                finally
                {
                    countdown.Signal();
                }
            }, orderNumber);
        }

        countdown.Wait();
        Console.WriteLine("All orders processed");
    }
}

internal class BankAccount
{
    private readonly object _sync = new();
    private int _balance;
    private bool _isLocked;

    public BankAccount(int initialBalance)
    {
        _balance = initialBalance;
    }

    public int Balance
    {
        get
        {
            lock (_sync)
            {
                return _balance;
            }
        }
    }

    public bool IsLocked
    {
        get
        {
            lock (_sync)
            {
                return _isLocked;
            }
        }
    }

    public bool Deposit(int amount)
    {
        lock (_sync)
        {
            if (_isLocked)
            {
                return false;
            }

            _balance += amount;
            return true;
        }
    }

    public bool Withdraw(int amount)
    {
        lock (_sync)
        {
            if (_isLocked)
            {
                return false;
            }

            if (_balance < amount)
            {
                return false;
            }

            _balance -= amount;
            return true;
        }
    }

    public void LockAccount()
    {
        lock (_sync)
        {
            _isLocked = true;
        }
    }
}
