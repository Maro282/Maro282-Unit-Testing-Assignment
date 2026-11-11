using DriveNow.Application.Abstractions;

namespace DriveNow.Infrastructure.External;

public class RandomReferenceGenerator : IReferenceGenerator
{
    private readonly IRandomSource _random;

    public RandomReferenceGenerator(IRandomSource random)
    {
        _random = random;
    }

    public string Generate(int length) =>
        _random.GenerateCode(length);
}