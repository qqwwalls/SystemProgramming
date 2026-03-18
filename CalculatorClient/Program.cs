using System.Runtime.InteropServices;

namespace CalculatorClient;

internal static partial class NativeMethods
{
    [LibraryImport("libcalculator.so")]
    internal static partial nint CreateCalculatorObject();

    [LibraryImport("libcalculator.so")]
    internal static partial void DeleteCalculatorObject(nint obj);

    [LibraryImport("libcalculator.so", EntryPoint = "Add")]
    internal static partial int AddNumbers(nint obj, int a, int b);
}

internal static class Program
{
    private static void Main()
    {
        nint calculator = NativeMethods.CreateCalculatorObject();

        try
        {
            int result = NativeMethods.AddNumbers(calculator, 5, 7);
            Console.WriteLine($"5 + 7 = {result}");
        }
        finally
        {
            if (calculator != 0)
            {
                NativeMethods.DeleteCalculatorObject(calculator);
            }
        }
    }
}
