using DriveNow.Application.Abstractions;

namespace DriveNow.Infrastructure.External;

public class SmsSender : ISmsSender
{

    private readonly IDelay _delay;

    public SmsSender(IDelay delay)
    {
        _delay = delay;
    }


    public void Send(string phone, string text)
    {
        _delay.Wait(TimeSpan.FromMilliseconds(500));
        Console.WriteLine($"[SMS to {phone}] {text}");
    }
}
