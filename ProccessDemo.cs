using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SystemProgramming;

internal static class NativeMethods
{
    [DllImport("libcalculator.so", CallingConvention = CallingConvention.Cdecl)]
    internal static extern nint CreateCalculatorObject();

    [DllImport("libcalculator.so", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void DeleteCalculatorObject(nint obj);

    [DllImport("libcalculator.so", EntryPoint = "Add", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int AddNumbers(nint obj, int a, int b);
}

internal class ProcessDemo
{
    public void Run()
    {
        char key;

        do
        {
            Console.WriteLine("Process Demo");
            Console.WriteLine("1 - ShowAllProcessesFilter");
            Console.WriteLine("2 - ShowAllProcesses");
            Console.WriteLine("3 - GetProcessByPid");
            Console.WriteLine("4 - CreateProcess");
            Console.WriteLine("5 - KillProcess");
            Console.WriteLine("6 - CallTestProgram");
            Console.WriteLine("7 - WriteStringToTextFile");
            Console.WriteLine("8 - CallCalculatorLibrary");
            Console.WriteLine("0 - Exit");
            Console.Write("Choose: ");

            var input = Console.ReadLine();
            key = string.IsNullOrWhiteSpace(input) ? '\0' : input[0];

            switch (key)
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
                    CallTestProgram();
                    break;
                case '7':
                    WriteStringToTextFile();
                    break;
                case '8':
                    CallCalculatorLibrary();
                    break;
                case '0':
                    Console.WriteLine("Exit");
                    break;
                default:
                    Console.WriteLine("unknown operation");
                    break;
            }

            Console.WriteLine();
        } while (key != '0');
    }

    private void ShowAllProcessesFilter()
    {
        Process[] processes = Process.GetProcesses();
        var processNames = new List<string>();

        foreach (var process in processes)
        {
            try
            {
                processNames.Add(GetGroupName(process.ProcessName));
            }
            catch
            {
                processNames.Add("Unknown");
            }
        }

        var groupedProcesses = processNames
            .GroupBy(processName => processName)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key);

        foreach (var group in groupedProcesses)
        {
            Console.WriteLine($"{group.Key} - {group.Count()}");
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
        Console.WriteLine("Enter program name: ");
        string? program = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(program))
        {
            Console.WriteLine("Program name is empty");
            return;
        }

        var processName = NormalizeProcessName(program);
        var alreadyRunning = Process.GetProcessesByName(processName).Length > 0;

        if (alreadyRunning)
        {
            Console.WriteLine("Process is already running");
            return;
        }

        try
        {
            var startedProcess = Process.Start(new ProcessStartInfo(program) { UseShellExecute = true });

            if (startedProcess != null)
            {
                Console.WriteLine(startedProcess.Id);
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
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Value is empty");
            return;
        }

        if (int.TryParse(input, out var pid))
        {
            try
            {
                var process = Process.GetProcessById(pid);
                process.Kill();
                Console.WriteLine("Process killed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return;
        }

        var processName = NormalizeProcessName(input);
        var processes = Process.GetProcessesByName(processName);

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

    private void CallTestProgram()
    {
        string exePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "TestProgram",
            "bin",
            "Debug",
            "net10.0",
            "TestProgram.dll");
        Console.WriteLine("Enter arg (hi, bye, etc..):");
        string arg = Console.ReadLine() ?? "hi";
        ProcessStartInfo processInfo = new ProcessStartInfo()
        {
            FileName = "dotnet",
            Arguments = $"\"{exePath}\" {arg}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (Process process = new Process())
        {
            process.StartInfo = processInfo;

            if (!File.Exists(exePath))
            {
                Console.WriteLine($"File not found: {exePath}");
                return;
            }

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();
            process.WaitForExit();
            Console.WriteLine($"Result: {output.Trim()}");

            if (!string.IsNullOrWhiteSpace(errors))
            {
                Console.WriteLine($"Errors: {errors.Trim()}");
            }
        }
    }

    private void WriteStringToTextFile()
    {
        Console.Write("Enter text: ");
        string text = Console.ReadLine() ?? string.Empty;

        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "note.txt");
        File.WriteAllText(filePath, text);
        Console.WriteLine($"Saved to: {filePath}");

        try
        {
            ProcessStartInfo startInfo = OperatingSystem.IsWindows()
                ? new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = true
                }
                : new ProcessStartInfo
                {
                    FileName = "xdg-open",
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = false
                };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void CallCalculatorLibrary()
    {
        Console.Write("Enter first number: ");
        if (!int.TryParse(Console.ReadLine(), out int left))
        {
            Console.WriteLine("Invalid first number");
            return;
        }

        Console.Write("Enter second number: ");
        if (!int.TryParse(Console.ReadLine(), out int right))
        {
            Console.WriteLine("Invalid second number");
            return;
        }

        nint calculator = 0;

        try
        {
            calculator = NativeMethods.CreateCalculatorObject();

            if (calculator == 0)
            {
                Console.WriteLine("Calculator object was not created");
                return;
            }

            int result = NativeMethods.AddNumbers(calculator, left, right);
            Console.WriteLine($"Result: {left} + {right} = {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            if (calculator != 0)
            {
                NativeMethods.DeleteCalculatorObject(calculator);
            }
        }
    }

    private string GetGroupName(string processName)
    {
        var separators = new[] { '/', ':', '-', '[', '(', ' ', '_', '.' };
        var cutIndex = processName.Length;

        foreach (var separator in separators)
        {
            var index = processName.IndexOf(separator);

            if (index > 0 && index < cutIndex)
            {
                cutIndex = index;
            }
        }

        return cutIndex < processName.Length ? processName[..cutIndex] : processName;
    }

    private string NormalizeProcessName(string program)
    {
        var fileName = Path.GetFileName(program.Trim());
        return Path.GetFileNameWithoutExtension(fileName);
    }
}
