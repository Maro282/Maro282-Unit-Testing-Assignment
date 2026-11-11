using DriveNow.Application.Abstractions;

namespace DriveNow.Infrastructure.External;

public class EmailSender : IEmailSender
{


    private readonly TimeProvider _timeProvider;

    public EmailSender(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public void Send(string to, string subject, string body)
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "outbox");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, $"{_timeProvider.GetLocalNow:yyyyMMdd_HHmmssfff}_{to}.txt");
        var content = $"To: {to}{Environment.NewLine}Subject: {subject}{Environment.NewLine}{Environment.NewLine}{body}";
        File.WriteAllText(path, content);
    }
}
