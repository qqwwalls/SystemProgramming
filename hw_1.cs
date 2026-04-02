using System.Diagnostics;

namespace SystemProgramming;

internal class Hw1ProcessDemo
{
    public void Run()
    {
        SaveProcessesToNewFile();
        ConsoleKeyInfo key;

        do
        {
            Console.WriteLine();
            Console.WriteLine("Process Demo");
            Console.WriteLine("1 - ShowAllProcessesFilter");
            Console.WriteLine("2 - ShowAllProcesses");
            Console.WriteLine("3 - GetProcessByPid");
            Console.WriteLine("4 - CreateProcess");
            Console.WriteLine("5 - KillProcess");
            Console.WriteLine("6 - Open dou.ua");
            Console.WriteLine("0 - Exit");
            key = Console.ReadKey();
            Console.WriteLine();

            switch (key.KeyChar)
            {
                case '1':
                    ShowAllProcessesFilter();
                    break;
                case '2':
                    ShowAllProcesses();
                    break;
                case '3':
                    GetProcessByPid();
                    break;
                case '4':
                    CreateProcess();
                    break;
                case '5':
                    KillProcess();
                    break;
                case '6':
                    OpenDouUa();
                    break;
                case '0':
                    Console.WriteLine("Exit");
                    break;
                default:
                    Console.WriteLine("unknown operation");
                    break;
            }
        } while (key.KeyChar != '0');
    }

    private void ShowAllProcessesFilter()
    {
        Process[] processes = Process.GetProcesses();
        var taskManager = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var process in processes)
        {
            var processName = "unknown";
            try
            {
                processName = process.ProcessName;
            }
            catch
            {
                // Keep "unknown" if name is inaccessible.
            }

            if (taskManager.ContainsKey(processName))
            {
                taskManager[processName] += 1;
            }
            else
            {
                taskManager[processName] = 1;
            }
        }

        foreach (var process in taskManager.OrderByDescending(x => x.Value).ThenBy(x => x.Key))
        {
            Console.WriteLine($"{process.Key} {process.Value}");
        }
    }

    private void ShowAllProcesses()
    {
        Process[] processes = Process.GetProcesses();

        foreach (var process in processes.OrderBy(process => process.ProcessName))
        {
            try
            {
                Console.WriteLine($"{process.ProcessName} PID: {process.Id}");
            }
            catch
            {
                Console.WriteLine("Unknown process");
            }
        }
    }

    private void GetProcessByPid()
    {
        try
        {
            Console.WriteLine("Enter pid:");
            int pid = Convert.ToInt32(Console.ReadLine());
            var process = Process.GetProcessById(pid);
            Console.WriteLine($"{process.ProcessName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void CreateProcess()
    {
        Console.WriteLine("Enter program name:");
        string? program = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(program))
        {
            Console.WriteLine("Program name is empty");
            return;
        }

        string processName = NormalizeProcessName(program);
        bool isAlreadyRunning = Process.GetProcessesByName(processName).Length > 0;

        if (isAlreadyRunning)
        {
            Console.WriteLine("This process is already running.");
            return;
        }

        try
        {
            var started = Process.Start(new ProcessStartInfo(program) { UseShellExecute = true });
            if (started != null)
            {
                Console.WriteLine($"Started PID: {started.Id}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void KillProcess()
    {
        Console.Write("Enter PID or process name: ");
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Value is empty");
            return;
        }

        if (int.TryParse(input, out int pid))
        {
            try
            {
                Process.GetProcessById(pid).Kill();
                Console.WriteLine("Process killed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return;
        }

        string processName = NormalizeProcessName(input);
        Process[] processes = Process.GetProcessesByName(processName);

        if (processes.Length == 0)
        {
            Console.WriteLine("Process not found");
            return;
        }

        foreach (var process in processes)
        {
            try
            {
                process.Kill();
                Console.WriteLine($"{process.ProcessName} killed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private void OpenDouUa()
    {
        const string url = "https://dou.ua/";
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            Console.WriteLine("dou.ua opened");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void SaveProcessesToNewFile()
    {
        string fileName = $"processes_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

        var lines = Process.GetProcesses()
            .OrderBy(p => p.ProcessName)
            .Select(p =>
            {
                try
                {
                    return $"{p.ProcessName} PID: {p.Id}";
                }
                catch
                {
                    return "Unknown process";
                }
            });

        File.WriteAllLines(filePath, lines);
        Console.WriteLine($"Process list saved to: {filePath}");
    }

    private string NormalizeProcessName(string program)
    {
        string fileName = Path.GetFileName(program.Trim());
        return Path.GetFileNameWithoutExtension(fileName);
    }
}
