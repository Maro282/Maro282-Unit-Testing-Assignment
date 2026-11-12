
using Bogus;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;

namespace DriveNow.UnitTests.Application.Pricing;

public class VehicleFaker : Faker<Vehicle>
{
    public VehicleFaker()
    {
        // Fixed seed makes generated vehicles repeatable.
        UseSeed(54321);

        RuleFor(v => v.PlateNumber, f => f.Vehicle.Vin());
        RuleFor(v => v.Model, f => f.Vehicle.Model());
        RuleFor(v => v.Category,
            f => f.PickRandom<VehicleCategory>());

        RuleFor(v => v.DailyRate,
            f => f.Random.Decimal(500, 5000));

        RuleFor(v => v.Mileage,
            f => f.Random.Int(1000, 100000));

        RuleFor(v => v.IsAvailable, f => true);
    }
}
