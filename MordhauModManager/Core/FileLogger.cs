using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace MordhauModManager.Core
{
    public class FileLogger : IDisposable
    {
        private readonly FileStream fileStream;
        private readonly StreamWriter fileWriter;

        private static FileLogger instance;
        public static FileLogger Instance
        {
            get
            {
                if (instance == null)
                    instance = new FileLogger();

                return instance;
            }
        }

        private object LockObj = new object();

        public FileLogger()
        {
            fileStream = new FileStream("session.log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            fileWriter = new StreamWriter(fileStream);
        }

        public void WriteLine(string message, [CallerMemberName] string caller = "")
        {
            lock(LockObj)
            {
                var timeString = DateTime.Now.ToString("dd.MM.yyyy-HH:mm:ss");
                fileWriter.WriteLine($"[{timeString}][{caller}] {message}");
                fileWriter.Flush();
            }
        }

        public void Dispose()
        {
            fileWriter?.Dispose();
            fileStream?.Dispose();
        }
    }
}
