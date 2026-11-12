

using DriveNow.Application.Abstractions;
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using FluentAssertions;
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
            if (_promo?.Code == code)
                return _promo;

            return null;
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
    public void Calculate_AdditionalDrivers_ReturnsCorrectCost(
        LoyaltyTier tier,
        decimal expected)
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

}
