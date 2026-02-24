using System;
using Tahsilat.NET.Models.Enums;

namespace Tahsilat.NET.Infrastructure.Logging
{
    public class ConsoleLogger : ITahsilatLogger
    {
        public void Log(TahsilatLogLevel level, string message, Exception ex = null)
        {
            Console.WriteLine($"[{level}] {message}");
            if (ex != null)
                Console.WriteLine(ex);
        }
    }
}
