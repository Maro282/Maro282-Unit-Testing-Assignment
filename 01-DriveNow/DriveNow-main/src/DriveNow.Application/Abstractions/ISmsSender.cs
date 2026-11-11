namespace DriveNow.Application.Abstractions;

public interface ISmsSender
{
    void Send(string phone, string text);
}