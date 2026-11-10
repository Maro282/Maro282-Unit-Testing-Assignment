using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Infrastructure.Persistence;
using FluentAssertions;
namespace DriveNow.UnitTests.Naive;

public class CreateBookingTests

{


    // happy path
    [Fact]

    public void CreateBooking_ValidRequest_ReturnBooking()

    {

        //Arrange

        var customer = InMemoryDatabase.Customers.First(c => !c.IsBlacklisted); // Take first customer isn't blocked

        var vehicle = InMemoryDatabase.Vehicles.First(v => v.IsAvailable); // Take first Vecihle isn't blocked

        var date = DateTime.Now;

        var bookingRequest = new BookingRequest()

        {

            CustomerId = customer.Id,

            VehicleId = vehicle.Id,

            AdditionalDrivers = 0,

            PromoCode = null,

            PickupAt = date.AddHours(3),

            ReturnAt = date.AddDays(2)

        };



        // Act

        var result = new BookingService().CreateBooking(bookingRequest);



        // Assert

        result.Should().NotBeNull();

        result.CustomerId.Should().Be(bookingRequest.CustomerId);

        result.VehicleId.Should().Be(bookingRequest.VehicleId);

        InMemoryDatabase.Bookings.Should().Contain(b => b.Id == result.Id);

        //Assert.Contains(InMemoryDatabase.Bookings, b => b.Id == result.Id);



    }



    // Platinum Test

    [Fact]
    public void CreateBooking_PlatinumCustomer_ReturnBooking()

    {

        //Arrange
        var customer = InMemoryDatabase.Customers.First(c => c.LoyaltyTier is LoyaltyTier.Platinum); // Take first customer isn't blocked

        var vehicle = InMemoryDatabase.Vehicles.First(v => v.IsAvailable); // Take first Vecihle isn't blocked

        var date = DateTime.Now;

        var bookingRequest = new BookingRequest()

        {

            CustomerId = customer.Id,

            VehicleId = vehicle.Id,

            AdditionalDrivers = 0,

            PromoCode = null,

            PickupAt = date.AddHours(3),

            ReturnAt = date.AddDays(2)

        };



        //// Act

        var result = new BookingService().CreateBooking(bookingRequest);

        //// Assert

        result.Should().NotBeNull();

        result.Deposit.Should().Be(0m);

    }









    [Fact]

    public void CreateBooking_OverlapDate_Throws()

    {

        //Arrange

        var existedBooking = InMemoryDatabase.Bookings.First();

        var date = DateTime.Now;

        var bookingRequest = new BookingRequest()

        {

            CustomerId = existedBooking.CustomerId,

            VehicleId = existedBooking.VehicleId,

            AdditionalDrivers = existedBooking.AdditionalDrivers,

            PromoCode = existedBooking.PromoCode,

            PickupAt = existedBooking.PickupAt.AddHours(1), //i have changed time to overlap

            ReturnAt = existedBooking.ReturnAt.AddHours(1)

        };


        // Act
        var act = () => new BookingService().CreateBooking(bookingRequest);

        act.Should().Throw<BookingRejectedException>().WithMessage("Vehicle is already booked for these dates");



    }



}