namespace DriveNow.Application.Abstractions;

public interface IRandomSource
{
    int Next(int maxValue);
    string GenerateCode(int length);
}