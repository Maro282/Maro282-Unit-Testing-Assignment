namespace DriveNow.Application.Abstractions;

public interface IAuditLogger
{
    void Write(string message);
}