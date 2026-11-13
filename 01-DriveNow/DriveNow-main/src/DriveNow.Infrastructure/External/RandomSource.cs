using DriveNow.Application.Abstractions;

namespace DriveNow.Infrastructure.External;

public class RandomSource : IRandomSource
{
    private readonly Random _random;

    public RandomSource(Random random)
    {
        _random = random;
    }

    public int Next(int maxValue) => _random.Next(maxValue);

    public string GenerateCode(int length)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        return new string(
            Enumerable.Range(0, length)
                .Select(_ => alphabet[_random.Next(alphabet.Length)])
                .ToArray());
    }
}