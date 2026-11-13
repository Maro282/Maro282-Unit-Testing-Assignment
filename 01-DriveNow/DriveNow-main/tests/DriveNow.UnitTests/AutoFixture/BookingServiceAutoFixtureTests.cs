using AutoFixture;
using AutoFixture.Xunit2;
using DriveNow.Application.Abstractions;
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using FluentAssertions;
using Moq;

namespace DriveNow.UnitTests.AutoFixture;

public class BookingServiceAutoFixtureTests
{
    // R3
    [Theory]
    [AutoMoqData]
    public void CreateBooking_WithAutoFixture_UsesFrozenPaymentMock(
        [Frozen] Mock<IPaymentGateway> payments,
        [Frozen] Mock<ICustomerRepository> customers,
        [Frozen] Mock<IVehicleRepository> vehicles,
        BookingService service,
        Customer customer,
        Vehicle vehicle,
        BookingRequest request)
    {
        customer.Id = 1;
        customer.DateOfBirth = new DateTime(1990, 1, 1);
        customer.LicenseIssueDate = new DateTime(2020, 1, 1);
        customer.LoyaltyTier = LoyaltyTier.None;

        vehicle.Id = 10;
        vehicle.Category = VehicleCategory.Economy;
        vehicle.IsAvailable = true;
        vehicle.Mileage = 1000;
        vehicle.DailyRate = 600;

        request.CustomerId = customer.Id;
        request.VehicleId = vehicle.Id;
        request.PickupAt = new DateTime(2026, 11, 13, 13, 0, 0);
        request.ReturnAt = request.PickupAt.AddDays(1);
        request.InsurancePlan = InsurancePlan.Basic;

        customers
            .Setup(x => x.GetById(customer.Id))
            .Returns(customer);

        vehicles
            .Setup(x => x.GetById(vehicle.Id))
            .Returns(vehicle);

        var booking = service.CreateBooking(request);

        booking.Should().NotBeNull();

        payments.Verify(
            x => x.Charge(
                customer.Id,
                1000m,
                $"Deposit {booking.Reference}"),
            Times.Once);
    }


    // R4
    [Fact]
    public void AnonymousCustomer_CanHaveInvalidBookingData()
    {
        var fixture = new Fixture();

        var customer = fixture.Create<Customer>();

        var age = DriveNow.Domain.Services.RentalMathService.CalculateAge(
            customer.DateOfBirth,
            new DateTime(2026, 11, 13));

        var licenseYears = DriveNow.Domain.Services.RentalMathService.CalculateLicenseYears(
            customer.LicenseIssueDate,
            new DateTime(2026, 11, 13));


        (age >= 21 && licenseYears >= 1).Should().BeFalse();
    }

    // R4
    [Fact]
    public void ValidCustomerCustomization_CreatesValidAdultCustomer()
    {
        var fixture = new Fixture();

        fixture.Customize(new ValidAdultCustomerCustomization());

        var customer = fixture.Create<Customer>();

        var age = DriveNow.Domain.Services.RentalMathService.CalculateAge(
            customer.DateOfBirth,
            new DateTime(2026, 11, 13));

        var licenseYears = DriveNow.Domain.Services.RentalMathService.CalculateLicenseYears(
            customer.LicenseIssueDate,
            new DateTime(2026, 11, 13));

        age.Should().BeGreaterThanOrEqualTo(21);
        licenseYears.Should().BeGreaterThanOrEqualTo(1);
    }


}