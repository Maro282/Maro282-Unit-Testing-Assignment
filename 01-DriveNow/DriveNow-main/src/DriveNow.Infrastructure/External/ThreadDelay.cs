using DriveNow.Application.Abstractions;

namespace DriveNow.Infrastructure.External;

public class ThreadDelay : IDelay
{
    public void Wait(TimeSpan duration) =>
        Thread.Sleep(duration);
}