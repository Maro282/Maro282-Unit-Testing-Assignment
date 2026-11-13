using AutoFixture;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;

namespace DriveNow.UnitTests.AutoFixture;

public class ValidAdultCustomerCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Customer>(composer =>
            composer
                .With(x => x.DateOfBirth, new DateTime(1990, 1, 1))
                .With(x => x.LicenseIssueDate, new DateTime(2020, 1, 1))
                .With(x => x.IsBlacklisted, false)
                .With(x => x.LoyaltyTier, LoyaltyTier.None));
    }
}