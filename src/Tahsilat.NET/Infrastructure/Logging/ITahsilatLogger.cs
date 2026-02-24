using System;
using Tahsilat.NET.Models.Enums;

namespace Tahsilat.NET.Infrastructure.Logging
{
    public interface ITahsilatLogger
    {
        void Log(TahsilatLogLevel level, string message, Exception ex = null);
    }
}
