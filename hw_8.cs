using System;
using System.Threading;

internal static class Hw8
{
    private static readonly Semaphore semaphore = new Semaphore(1, 1);

    private static void Work(object? id)
    {
        Console.WriteLine($"Thread {id} чекає...");

        bool entered = false;

        try
        {
            entered = semaphore.WaitOne(10000);

            if (!entered)
            {
                Console.WriteLine($"Thread {id} не дочекався семафора за 10 секунд");
                return;
            }

            Console.WriteLine($"Thread {id} зайшов");
            Thread.Sleep(2000);
        }
        finally
        {
            if (entered)
            {
                Console.WriteLine($"Thread {id} виходить");
                semaphore.Release();
            }
        }
    }

    public static void Main()
    {
        Thread[] threads = new Thread[5];

        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(Work);
            threads[i].Start(i + 1);
        }

        for (int i = 0; i < threads.Length; i++)
        {
            threads[i].Join();
        }
    }
}
