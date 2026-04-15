using System.Collections.Concurrent;
using System.Threading;

namespace SystemProgramming;

internal class Client
{
    public Client(string name, string visitPurpose)
    {
        Name = name;
        VisitPurpose = visitPurpose;
    }

    public string Name { get; }
    public string VisitPurpose { get; }
}

internal class Hw7ConcurrentCollectionsDemo
{
    public void Run()
    {
        Console.WriteLine("Task 1: Bank queue simulation");
        RunBankQueueSimulation();

        Console.WriteLine();
        Console.WriteLine("Task 2: User actions stack");
        RunUserActionsStackSimulation();
    }

    private void RunBankQueueSimulation()
    {
        var queue = new ConcurrentQueue<Client>();
        int producerCount = 3;
        int clientsPerProducer = 4;

        int activeProducers = producerCount;
        Thread[] producers = new Thread[producerCount];

        for (int i = 0; i < producerCount; i++)
        {
            int producerId = i + 1;
            producers[i] = new Thread(() =>
            {
                for (int clientIndex = 1; clientIndex <= clientsPerProducer; clientIndex++)
                {
                    var client = new Client(
                        $"Client-{producerId}-{clientIndex}",
                        clientIndex % 2 == 0 ? "Open deposit" : "Pay utilities");

                    queue.Enqueue(client);
                    Console.WriteLine($"Added: {client.Name}, purpose: {client.VisitPurpose}");
                    Thread.Sleep(Random.Shared.Next(80, 180));
                }

                Interlocked.Decrement(ref activeProducers);
            });

            producers[i].Start();
        }

        Thread cashier = new Thread(() =>
        {
            while (Volatile.Read(ref activeProducers) > 0 || !queue.IsEmpty)
            {
                if (queue.TryDequeue(out Client? client))
                {
                    Console.WriteLine($"Served: {client.Name}, purpose: {client.VisitPurpose}");
                    Thread.Sleep(150);
                }
                else
                {
                    Thread.Sleep(40);
                }
            }
        });

        cashier.Start();

        for (int i = 0; i < producers.Length; i++)
        {
            producers[i].Join();
        }

        cashier.Join();
        Console.WriteLine("All clients served.");
    }

    private void RunUserActionsStackSimulation()
    {
        var stack = new ConcurrentStack<string>();
        int producerCount = 3;

        string[][] actionsByThread =
        {
            new[] { "opened document", "edited text", "saved document" },
            new[] { "opened settings", "changed theme", "closed settings" },
            new[] { "opened file", "renamed file", "closed file" }
        };

        Thread[] actionThreads = new Thread[producerCount];

        for (int i = 0; i < producerCount; i++)
        {
            int threadId = i;
            actionThreads[i] = new Thread(() =>
            {
                foreach (string action in actionsByThread[threadId])
                {
                    stack.Push(action);
                    Console.WriteLine($"Action added: {action}");
                    Thread.Sleep(Random.Shared.Next(60, 140));
                }
            });

            actionThreads[i].Start();
        }

        for (int i = 0; i < actionThreads.Length; i++)
        {
            actionThreads[i].Join();
        }

        Console.WriteLine();
        Console.WriteLine("Undo last actions:");

        while (!stack.IsEmpty)
        {
            if (stack.TryPop(out string? task))
            {
                Console.WriteLine($"Processing: {task}");
                Thread.Sleep(100);
            }
        }

        Console.WriteLine("No actions left in stack.");
    }
}
