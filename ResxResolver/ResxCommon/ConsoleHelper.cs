namespace ResxCommon
{
    using System;
    using System.IO;

    public static class ConsoleHelper
    {
        private static FileStream fileStream;

        private static StreamWriter writer;

        private static TextWriter oldOut;

        public static void StartFileLogging()
        {
            StartFileLogging($"ResxLog_{DateTime.UtcNow.Ticks}");
        }

        public static void StartFileLogging(string fileName)
        {
            oldOut = Console.Out;

            try
            {
                fileStream = new FileStream($"./{fileName}.txt", FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(fileStream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }

            Console.SetOut(writer);
        }

        public static void StopFileLogging()
        {
            try
            {
                Console.SetOut(oldOut);
                writer.Close();
                fileStream.Close();
            }
            catch (Exception)
            {
                // Ignored
            }
        }
    }
}