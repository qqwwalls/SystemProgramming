namespace TestProgramm
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0].ToLower().CompareTo("hi") == 0)
                {
                    Console.WriteLine("Hello");
                }
                else if (args[0].ToLower().CompareTo("bye") == 0)
                {
                    Console.WriteLine("Good Bye");
                }
                else
                {
                    Console.WriteLine("Unknown instruction");
                }
            }
        }
    }
}
