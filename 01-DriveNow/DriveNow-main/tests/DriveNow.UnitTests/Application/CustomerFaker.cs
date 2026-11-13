
using Bogus;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;

namespace DriveNow.UnitTests.Application.Pricing;

public class CustomerFaker : Faker<Customer>
{
    private int _age = 30;
    private LoyaltyTier _tier = LoyaltyTier.None;

    public CustomerFaker()
    {
        // Fixed seed makes generated customers repeatable between test runs.
        UseSeed(12345);

        RuleFor(c => c.FullName, f => f.Name.FullName());
        RuleFor(c => c.Email, f => f.Internet.Email());
        RuleFor(c => c.Phone, f => f.Phone.PhoneNumber());

        // Generate a birth date based on the age selected by the test.
        RuleFor(c => c.DateOfBirth,
            f => new DateTime(2026, 11, 12).AddYears(-_age));

        RuleFor(c => c.LicenseIssueDate,
            f => new DateTime(2021, 11, 12));

        RuleFor(c => c.LoyaltyTier, f => _tier);
        RuleFor(c => c.IsBlacklisted, f => false);
        RuleFor(c => c.CompletedRentals, f => f.Random.Int(0, 30)); // is fine to be random 
    }

    // Allows a test to choose a specific customer age.
    public CustomerFaker WithAge(int age)
    {
        _age = age;
        return this;
    }

    // Allows a test to choose a specific loyalty tier.
    public CustomerFaker WithTier(LoyaltyTier tier)
    {
        _tier = tier;
        return this;
    }
}
