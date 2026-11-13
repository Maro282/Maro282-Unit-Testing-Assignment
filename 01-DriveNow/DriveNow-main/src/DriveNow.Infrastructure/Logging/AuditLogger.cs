using DriveNow.Application.Abstractions;

namespace DriveNow.Infrastructure.Logging;

public class AuditLogger : IAuditLogger
{

    private readonly TimeProvider _timeProvider;

    public AuditLogger(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }


    public void Write(string message)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "audit.log");
        File.AppendAllText(path, $"[{_timeProvider.GetLocalNow:O}] {message}{Environment.NewLine}");
    }
}
