using System.Diagnostics;
using System.Threading;

namespace SystemProgramming;

internal record Hero(string Name, int Health, int Power);
internal record Quest(string Title, int DifficultyLevel, int Bonus, TimeSpan Duration);

internal enum QuestStatus
{
    Victory,
    Defeat,
    Retreated
}

internal record QuestRunResult(Hero Hero, Quest Quest, QuestStatus Status, int GoldReward);

internal class Hw6DungeonSimulator
{
    private static readonly Random Random = new();
    private readonly Stopwatch _clock = Stopwatch.StartNew();

    public async Task RunAsync()
    {
        Hero bob = new("Bob", 120, 65);
        Hero alice = new("Alice", 95, 78);

        Quest dragonCave = new("Печеру дракона", 80, 200, TimeSpan.FromSeconds(5));
        Quest mageTower = new("Вежу мага", 70, 150, TimeSpan.FromSeconds(2));

        using var bobCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        using var aliceCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        Task<QuestRunResult> bobTask = StartQuestWithLoggingAsync(bob, dragonCave, bobCts.Token);
        Task<QuestRunResult> aliceTask = StartQuestWithLoggingAsync(alice, mageTower, aliceCts.Token);

        var pending = new List<Task<QuestRunResult>> { bobTask, aliceTask };

        while (pending.Count > 0)
        {
            Task<QuestRunResult> finished = await Task.WhenAny(pending);
            QuestRunResult result = await finished;
            LogResult(result);
            pending.Remove(finished);
        }

        QuestRunResult[] allResults = await Task.WhenAll(bobTask, aliceTask);
        int totalGold = allResults.Where(r => r.Status == QuestStatus.Victory).Sum(r => r.GoldReward);

        Log($"Забіги завершено. Загальна здобич: {totalGold} золота.");
    }

    public async Task<QuestRunResult> RunQuestAsync(Hero hero, Quest quest, CancellationToken ct)
    {
        await Task.Delay(quest.Duration, ct);

        int roll;
        lock (Random)
        {
            roll = Random.Next(-20, 21);
        }

        int heroScore = hero.Power + roll;
        bool isVictory = heroScore >= quest.DifficultyLevel;

        return isVictory
            ? new QuestRunResult(hero, quest, QuestStatus.Victory, quest.Bonus)
            : new QuestRunResult(hero, quest, QuestStatus.Defeat, 0);
    }

    private async Task<QuestRunResult> StartQuestWithLoggingAsync(Hero hero, Quest quest, CancellationToken ct)
    {
        Log($"{hero.Name} вирушив у \"{quest.Title}\"");

        try
        {
            return await RunQuestAsync(hero, quest, ct);
        }
        catch (OperationCanceledException)
        {
            return new QuestRunResult(hero, quest, QuestStatus.Retreated, 0);
        }
    }

    private void LogResult(QuestRunResult result)
    {
        if (result.Status == QuestStatus.Victory)
        {
            Log($"{result.Hero.Name} завершила \"{result.Quest.Title}\" — ПЕРЕМОГА! (+{result.GoldReward} золота)");
            return;
        }

        if (result.Status == QuestStatus.Defeat)
        {
            Log($"{result.Hero.Name} завершив \"{result.Quest.Title}\" — ПОРАЗКА.");
            return;
        }

        Log($"{result.Hero.Name} відступив з \"{result.Quest.Title}\" (тайм-аут)");
    }

    private void Log(string message)
    {
        TimeSpan t = _clock.Elapsed;
        Console.WriteLine($"[{t:mm\\:ss\\.fff}] {message}");
    }
}
