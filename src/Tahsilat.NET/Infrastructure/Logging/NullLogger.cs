using System;
using Tahsilat.NET.Models.Enums;

namespace Tahsilat.NET.Infrastructure.Logging
{
    public class NullLogger : ITahsilatLogger
    {
        public void Log(TahsilatLogLevel level, string message, Exception ex = null) { }
    }
}
