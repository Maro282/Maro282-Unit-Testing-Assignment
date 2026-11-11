namespace DriveNow.Application.Abstractions;

public interface IEmailSender
{
    void Send(string to, string subject, string body);
}