namespace DriveNow.Application.Abstractions;

public interface IDelay
{
    void Wait(TimeSpan duration);
}