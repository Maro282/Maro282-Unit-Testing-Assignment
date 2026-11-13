

using DriveNow.Application.Abstractions;
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Domain.ValueObjects;
using DriveNow.UnitTests.Application.Pricing;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Time.Testing;

namespace DriveNow.UnitTests.Application;

public class PricingServiceTests
{
    // Fake IPromoCodeRepository
    public class FakePromoCodeRepository : IPromoCodeRepository
    {
        private readonly PromoCode? _promo;

        public FakePromoCodeRepository(PromoCode? promo)
        {
            _promo = promo;
        }

        public PromoCode? GetByCode(string code)
        {
            return _promo != null &&
       string.Equals(
           _promo.Code,
           code,
           StringComparison.OrdinalIgnoreCase)
    ? _promo
    : null;
        }
        public void Update(PromoCode promo)
        {

        }
    }



    // BR-31 

    [Theory]
    [InlineData(20, 120)] // friday
    [InlineData(21, 120)] // saturday
    [InlineData(22, 0)] // sunday
    public void Calculate_weekendDays_ReturnCorrectSurcharge(int day, decimal expected)
    {
        // Arrange
        var customer = new Customer
        {
            DateOfBirth = new DateTime(2000, 9, 15),
            LoyaltyTier = LoyaltyTier.None
        };

        var vehicle = new Vehicle
        {
            DailyRate = 600,
            IsAvailable = true,
            Category = VehicleCategory.Economy
        };


        var request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = new DateTime(2026, 11, day),
            ReturnAt = new DateTime(2026, 11, day + 1),
            InsurancePlan = InsurancePlan.Basic

        };

        var service = new PricingService(new FakePromoCodeRepository(null), new FakeTimeProvider());

        // Act 
        var result = service.Calculate(customer, vehicle, request);

        // Assert 
        result.WeekendSurcharge.Should().Be(expected);
    }


    // BR-32 

    [Theory]
    [InlineData(20, 0)]
    [InlineData(21, 150)] // first valid
    [InlineData(24, 150)] // last valid
    [InlineData(25, 0)]
    public void Calculate_YoungDriverAge_ReturnsCorrectSurcharge(int age, decimal expected)
    {
        // Arrange: use a fixed pickup date to calculate the customer's age.
        var pickupDate = new DateTime(2026, 11, 12, 10, 0, 0);

        var customer = new Customer
        {
            DateOfBirth = pickupDate.AddYears(-age),
            LoyaltyTier = LoyaltyTier.None
        };

        var vehicle = new Vehicle
        {
            DailyRate = 1000,
            Category = VehicleCategory.Economy
        };

        var request = new BookingRequest
        {
            PickupAt = pickupDate,
            ReturnAt = pickupDate.AddDays(1),
            InsurancePlan = InsurancePlan.Basic
        };

        var service = new PricingService(
            new FakePromoCodeRepository(null),
            new FakeTimeProvider());

        // Act
        var result = service.Calculate(customer, vehicle, request);

        // Assert
        result.YoungDriverSurcharge.Should().Be(expected);
    }


    // BR-33

    [Theory]
    [InlineData(6, 0)]
    [InlineData(7, 700)] // will not take the discount because code say that which is not same as BR
    [InlineData(13, 1300)]
    [InlineData(14, 2100)]
    public void Calculate_DurationDiscount_ReturnsCorrectDiscount(int days, decimal expected)
    {

        var pickupDate = new DateTime(2026, 11, 12, 10, 0, 0);

        var customer = new Customer
        {
            DateOfBirth = new DateTime(2000, 9, 15),
            LoyaltyTier = LoyaltyTier.None
        };

        var vehicle = new Vehicle
        {
            DailyRate = 1000,
            Category = VehicleCategory.Economy
        };

        var request = new BookingRequest
        {
            PickupAt = pickupDate,
            ReturnAt = pickupDate.AddDays(days),
            InsurancePlan = InsurancePlan.Basic
        };

        var service = new PricingService(
            new FakePromoCodeRepository(null),
            new FakeTimeProvider());

        // Act
        var result = service.Calculate(customer, vehicle, request);

        // Assert
        result.DurationDiscount.Should().Be(expected);
    }


    // BR-34

    [Theory]
    [InlineData(LoyaltyTier.None, 0)]
    [InlineData(LoyaltyTier.Silver, 50)]
    [InlineData(LoyaltyTier.Gold, 100)]
    [InlineData(LoyaltyTier.Platinum, 150)]
    public void Calculate_LoyaltyTier_ReturnsCorrectDiscount(LoyaltyTier tier, decimal expected)
    {
        // Arrange
        var pickupDate = new DateTime(2026, 11, 12, 10, 0, 0);

        var customer = new Customer
        {
            DateOfBirth = new DateTime(1990, 1, 1),
            LoyaltyTier = tier
        };

        var vehicle = new Vehicle
        {
            DailyRate = 1000,
            Category = VehicleCategory.Economy
        };

        var request = new BookingRequest
        {
            PickupAt = pickupDate,
            ReturnAt = pickupDate.AddDays(1),
            InsurancePlan = InsurancePlan.Basic
        };

        var service = new PricingService(
            new FakePromoCodeRepository(null),
            new FakeTimeProvider());

        // Act
        var result = service.Calculate(customer, vehicle, request);

        // Assert
        result.LoyaltyDiscount.Should().Be(expected);
    }


    // BR-38:
    // Insurance has a daily cost, and Luxury vehicles pay double.
    [Theory]
    [InlineData(InsurancePlan.Basic, VehicleCategory.Economy, 0)]
    [InlineData(InsurancePlan.Standard, VehicleCategory.Economy, 150)]
    [InlineData(InsurancePlan.Premium, VehicleCategory.Economy, 300)]
    [InlineData(InsurancePlan.Basic, VehicleCategory.Luxury, 0)]
    [InlineData(InsurancePlan.Standard, VehicleCategory.Luxury, 300)]
    [InlineData(InsurancePlan.Premium, VehicleCategory.Luxury, 600)]
    public void Calculate_Insurance_ReturnsCorrectCost(InsurancePlan plan, VehicleCategory category, decimal expected)
    {
        // Arrange
        var pickupDate = new DateTime(2026, 11, 12, 10, 0, 0);

        var customer = new Customer
        {
            DateOfBirth = new DateTime(1990, 1, 1),
            LoyaltyTier = LoyaltyTier.None
        };

        var vehicle = new Vehicle
        {
            DailyRate = 1000,
            Category = category
        };

        var request = new BookingRequest
        {
            PickupAt = pickupDate,
            ReturnAt = pickupDate.AddDays(1),
            InsurancePlan = plan
        };

        var service = new PricingService(
            new FakePromoCodeRepository(null),
            new FakeTimeProvider());

        // Act
        var result = service.Calculate(customer, vehicle, request);

        // Assert
        result.InsuranceCost.Should().Be(expected);
    }


    // BR-39:
    // Gold and Platinum customers get the first additional driver free.
    [Theory]
    [InlineData(LoyaltyTier.None, 400)]
    [InlineData(LoyaltyTier.Silver, 400)]
    [InlineData(LoyaltyTier.Gold, 200)]
    [InlineData(LoyaltyTier.Platinum, 200)]
    public void Calculate_AdditionalDrivers_ReturnsCorrectCost(LoyaltyTier tier, decimal expected)
    {
        // Arrange
        var pickupDate = new DateTime(2026, 11, 12, 10, 0, 0);

        var customer = new Customer
        {
            DateOfBirth = new DateTime(1990, 1, 1),
            LoyaltyTier = tier
        };

        var vehicle = new Vehicle
        {
            DailyRate = 1000,
            Category = VehicleCategory.Economy
        };

        var request = new BookingRequest
        {
            PickupAt = pickupDate,
            ReturnAt = pickupDate.AddDays(2),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 2
        };

        var service = new PricingService(
            new FakePromoCodeRepository(null),
            new FakeTimeProvider());

        // Act
        var result = service.Calculate(customer, vehicle, request);

        // Assert
        result.AdditionalDriversCost.Should().Be(expected);
    }


    // BR-30 to BR-40:
    // MemberData supplies complete scenarios from PricingTestData.
    // We compare the complete PriceBreakdown, not just one property.


    public static IEnumerable<object[]> Examples
    {
        get
        {
            yield return new object[]
            {
                // Customer
                new Customer
                {
                    DateOfBirth = new DateTime(1992, 11, 12),
                    LoyaltyTier = LoyaltyTier.Gold
                },

                // Vehicle
                new Vehicle
                {
                    DailyRate = 600,
                    Category = VehicleCategory.Economy
                },

                // Booking request
                new BookingRequest
                {
                    PickupAt = new DateTime(2026, 11, 22, 10, 0, 0),
                    ReturnAt = new DateTime(2026, 11, 25, 10, 0, 0),
                    InsurancePlan = InsurancePlan.Standard,
                    AdditionalDrivers = 0
                },

                // No promo code in Example A.
                null,

                // Expected complete price breakdown.
                new PriceBreakdown
                {
                    RentalDays = 3,
                    BaseAmount = 1800,
                    WeekendSurcharge = 0,
                    YoungDriverSurcharge = 0,
                    DurationDiscount = 0,
                    LoyaltyDiscount = 180,
                    PromoDiscount = 0,
                    InsuranceCost = 450,
                    AdditionalDriversCost = 0,
                    Total = 2070
                }
            };

            // Example B from README:
            // Silver customer, 7 days, SAVE15, Premium insurance, one additional driver.
            yield return new object[]
            {
                // Customer
                new Customer
                {
                    DateOfBirth = new DateTime(2003, 11, 19),
                    LoyaltyTier = LoyaltyTier.Silver
                },

                // Vehicle
                new Vehicle
                {
                    DailyRate = 1500,
                    Category = VehicleCategory.SUV
                },

                // Booking request
                new BookingRequest
                {
                    PickupAt = new DateTime(2026, 11, 19, 9, 0, 0),
                    ReturnAt = new DateTime(2026, 11, 26, 9, 30, 0),
                    InsurancePlan = InsurancePlan.Premium,
                    AdditionalDrivers = 1,
                    PromoCode = "SAVE15"
                },

                // Promo code
                new PromoCode
                {
                    Code = "SAVE15",
                    DiscountPercent = 15,
                    ValidFrom = new DateTime(2026, 11, 2),
                    ValidUntil = new DateTime(2027, 1, 11),
                    MinDays = 1,
                    AllowedCategories = null,
                    MaxUses = 100,
                    UsedCount = 3
                },

                // Expected complete price breakdown.
                new PriceBreakdown
                {
                    RentalDays = 7,
                    BaseAmount = 10500,
                    WeekendSurcharge = 600,
                    YoungDriverSurcharge = 1575,
                    DurationDiscount = 1050,
                    LoyaltyDiscount = 525,
                    PromoDiscount = 1050,
                    InsuranceCost = 2100,
                    AdditionalDriversCost = 700,
                    Total = 12850
                }
            };
        }
    }



    [Theory]
    [MemberData(nameof(Examples))]
    public async Task Calculate_WorkedExamples_ReturnsCorrectPrice(
        Customer customer,
        Vehicle vehicle,
        BookingRequest request,
        PromoCode? promo,
        PriceBreakdown expected)
    {
        // Arrange
        var service = new PricingService(
            new FakePromoCodeRepository(promo),
            new FakeTimeProvider());

        // Act
        var actual = service.Calculate(customer, vehicle, request);



        // Assert: compare all properties.
        using (new AssertionScope())
        {
            actual.Should().BeEquivalentTo(expected);
        }

        // Snapshot: save and compare the complete result.
        await Verify(actual);
    }




    // ClassData provides all promo-code scenarios.
    // ExpectedMessage is null when the promo is valid.
    [Theory]
    [ClassData(typeof(PromoCodeTestData))]
    public void Calculate_PromoCode_ReturnsExpectedResult(
        string code,
        PromoCode? promo,
        int days,
        VehicleCategory category,
        string? expectedMessage)
    {
        // Arrange
        var customer = new Customer
        {
            DateOfBirth = new DateTime(1990, 1, 1),
            LoyaltyTier = LoyaltyTier.None
        };

        var vehicle = new Vehicle
        {
            DailyRate = 500,
            Category = category
        };

        var request = new BookingRequest
        {
            PickupAt = new DateTime(2026, 11, 12, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0)
                .AddDays(days),
            InsurancePlan = InsurancePlan.Basic,
            PromoCode = code
        };

        var service = new PricingService(
            new FakePromoCodeRepository(promo),
            new FakeTimeProvider());

        // Act and Assert:
        // If an error message is expected, verify the exception.
        if (expectedMessage != null)
        {
            var action = () => service.Calculate(customer, vehicle, request);

            action.Should()
                .Throw<InvalidPromoCodeException>()
                .WithMessage(expectedMessage);
        }
        else
        {
            // A valid promo should apply 15% of the base amount.
            var result = service.Calculate(customer, vehicle, request);

            result.PromoDiscount.Should().Be(150);
        }
    }


    // ====================== R4 ==========================


    // R4:
    // Random data is acceptable when we only check that objects are generated correctly.
    [Fact]
    public void CustomerFaker_GeneratesCustomer()
    {
        var customer = new CustomerFaker().Generate();
        var vehicle = new VehicleFaker().Generate();

        customer.FullName.Should().NotBeNullOrEmpty();
        customer.Email.Should().NotBeNullOrEmpty();
        vehicle.Model.Should().NotBeNullOrEmpty();
        vehicle.DailyRate.Should().BeGreaterThan(0);
    }

    // R4:
    // Random age is NOT acceptable when testing a specific age boundary.
    // We use WithAge(23) to control the exact value.
    [Fact]
    public void CustomerFaker_WithAgeAndTier_ReturnsSelectedValues()
    {
        var customer = new CustomerFaker()
            .WithAge(23)
            .WithTier(LoyaltyTier.Silver)
            .Generate();

        customer.DateOfBirth.Should().Be(
            new DateTime(2026, 11, 12).AddYears(-23));

        customer.LoyaltyTier.Should().Be(LoyaltyTier.Silver);
    }







}
