using DriveNow.Application.Abstractions;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace DriveNow.UnitTests.Application;

public class BookingServiceTests
{
    // BR-10 
    [Fact]
    public void CreateBooking_CustomerDoesNotExist_ThrowsException()
    {
        var context = new BookingServiceTestContext();
        context.Customer.Id = 99;

        var service = context.CreateBookingService();
        var request = context.CreateBookingRequest();
        request.CustomerId = 1;

        var act = () => service.CreateBooking(request);

        act.Should()
            .Throw<CustomerNotFoundException>()
            .WithMessage($"Customer {context.Customer.Id} not found");
    }

    // BR-11
    [Fact]
    public void CreateBooking_BlacklistedCustomer_ThrowsException()
    {
        var context = new BookingServiceTestContext();
        context.Customer.IsBlacklisted = true;

        var service = context.CreateBookingService();

        var act = () => service.CreateBooking(context.CreateBookingRequest());

        act.Should()
            .Throw<BookingRejectedException>()
            .WithMessage("Customer is blacklisted");
    }

    // BR-12
    [Fact]
    public void CreateBooking_CustomerUnder21_ThrowsException()
    {
        var context = new BookingServiceTestContext();
        context.Customer.DateOfBirth = new DateTime(2006, 11, 13);

        var service = context.CreateBookingService();

        var act = () => service.CreateBooking(context.CreateBookingRequest());

        act.Should()
            .Throw<BookingRejectedException>()
            .WithMessage("Driver must be at least 21");
    }

    // BR-13
    [Fact]
    public void CreateBooking_LicenseLessThanOneYear_ThrowsException()
    {
        var context = new BookingServiceTestContext();
        context.Customer.LicenseIssueDate = new DateTime(2026, 1, 1);

        var service = context.CreateBookingService();

        var act = () => service.CreateBooking(context.CreateBookingRequest());

        act.Should()
            .Throw<BookingRejectedException>()
            .WithMessage("License must be held for at least 1 year");
    }

    // BR-14
    [Fact]
    public void CreateBooking_VehicleDoesNotExist_ThrowsException()
    {
        var context = new BookingServiceTestContext();
        var service = context.CreateBookingService();

        var request = context.CreateBookingRequest();
        request.VehicleId = 99;

        var act = () => service.CreateBooking(request);

        act.Should()
            .Throw<VehicleNotFoundException>()
            .WithMessage("Vehicle #99 not found");
    }

    // BR-15
    [Fact]
    public void CreateBooking_VehicleUnavailable_ThrowsException()
    {
        var context = new BookingServiceTestContext();
        context.Vehicle.IsAvailable = false;

        var service = context.CreateBookingService();

        var act = () => service.CreateBooking(context.CreateBookingRequest());

        act.Should()
            .Throw<BookingRejectedException>()
            .WithMessage("Vehicle is not available");
    }

    // BR-16
    [Theory]
    [InlineData(24, 4)]
    [InlineData(25, 2)]
    public void CreateBooking_LuxuryRequirementsNotMet_ThrowsException(int age, int licenseYears)
    {
        var context = new BookingServiceTestContext();

        context.Vehicle.Category = VehicleCategory.Luxury;
        context.Customer.DateOfBirth = new DateTime(2026 - age, 1, 1);
        context.Customer.LicenseIssueDate =
            new DateTime(2026 - licenseYears, 1, 1);

        var service = context.CreateBookingService();

        var act = () => service.CreateBooking(context.CreateBookingRequest());

        act.Should()
            .Throw<BookingRejectedException>()
            .WithMessage("Luxury vehicles require age 25+ and 3 years of license");
    }

    // BR-17
    [Fact]
    public void CreateBooking_VanLicenseLessThanTwoYears_ThrowsException()
    {
        var context = new BookingServiceTestContext();

        context.Vehicle.Category = VehicleCategory.Van;
        context.Customer.LicenseIssueDate = new DateTime(2025, 1, 1);

        var service = context.CreateBookingService();

        var act = () => service.CreateBooking(context.CreateBookingRequest());

        act.Should()
            .Throw<BookingRejectedException>()
            .WithMessage("Vans require 2 years of license");
    }

    // BR-18
    [Fact]
    public void CreateBooking_PickupLessThanTwoHoursAway_ThrowsException()
    {
        var context = new BookingServiceTestContext();

        var request = context.CreateBookingRequest();
        request.PickupAt = context.Clock.GetLocalNow().DateTime.AddMinutes(119); // less than 1 minute

        var service = context.CreateBookingService();

        var act = () => service.CreateBooking(request);

        act.Should()
            .Throw<BookingRejectedException>()
            .WithMessage("Pickup must be at least 2 hours from now");
    }

    // BR-19.
    [Fact]
    public void CreateBooking_RentalMoreThan30Days_ThrowsException()
    {
        var context = new BookingServiceTestContext();

        var request = context.CreateBookingRequest();
        request.ReturnAt = request.PickupAt.AddDays(31);

        var service = context.CreateBookingService();

        var act = () => service.CreateBooking(request);

        act.Should()
            .Throw<BookingRejectedException>()
            .WithMessage("Rental period is invalid");
    }

    // BR-20
    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void CreateBooking_InvalidAdditionalDrivers_ThrowsException(int drivers)
    {
        var context = new BookingServiceTestContext();

        var request = context.CreateBookingRequest();
        request.AdditionalDrivers = drivers;

        var service = context.CreateBookingService();

        var act = () => service.CreateBooking(request);

        act.Should()
            .Throw<BookingRejectedException>()
            .WithMessage("Up to 3 additional drivers allowed");
    }

    // BR-21
    [Fact]
    public void CreateBooking_OverlappingBooking_ThrowsException()
    {
        var context = new BookingServiceTestContext();
        var request = context.CreateBookingRequest();

        context.Bookings.AddExisting(new Booking
        {
            CustomerId = 2,
            VehicleId = context.Vehicle.Id,
            PickupAt = request.PickupAt.AddHours(-1),
            ReturnAt = request.ReturnAt.AddHours(1),
            Status = BookingStatus.Reserved
        });

        var service = context.CreateBookingService();

        var act = () => service.CreateBooking(request);

        act.Should()
            .Throw<BookingRejectedException>()
            .WithMessage("Vehicle is already booked for these dates");
    }

    // BR-22
    [Fact]
    public void CreateBooking_CustomerAlreadyHasTwoOpenBookings_ThrowsException()
    {
        var context = new BookingServiceTestContext();
        var request = context.CreateBookingRequest();

        context.Bookings.AddExisting(new Booking
        {
            CustomerId = context.Customer.Id,
            VehicleId = 20,
            PickupAt = request.PickupAt.AddDays(1),
            ReturnAt = request.ReturnAt.AddDays(1),
            Status = BookingStatus.Reserved
        });

        context.Bookings.AddExisting(new Booking
        {
            CustomerId = context.Customer.Id,
            VehicleId = 30,
            PickupAt = request.PickupAt.AddDays(2),
            ReturnAt = request.ReturnAt.AddDays(2),
            Status = BookingStatus.Active
        });

        var service = context.CreateBookingService();

        var act = () => service.CreateBooking(request);

        act.Should()
            .Throw<BookingRejectedException>()
            .WithMessage("Customer already has 2 active bookings");
    }


    // BR-23: BookingService use PricingService to calculate price.
    [Fact]
    public void CreateBooking_ValidRequest_UsesPricingService()
    {
        var context = new BookingServiceTestContext();

        var service = context.CreateBookingService();

        var booking = service.CreateBooking(context.CreateBookingRequest());

        booking.Price.RentalDays.Should().Be(1);
        booking.Price.BaseAmount.Should().Be(600);
    }

    // BR-24
    [Fact]
    public void CreateBooking_ValidRequest_SetsBookingProperties()
    {
        var context = new BookingServiceTestContext();

        var service = context.CreateBookingService();

        var booking = service.CreateBooking(context.CreateBookingRequest());

        booking.Status.Should().Be(BookingStatus.Reserved);
        booking.CreatedAt.Should().Be(context.Clock.GetLocalNow().DateTime);
        booking.StartMileage.Should().Be(context.Vehicle.Mileage);
    }

    // BR-25
    [Fact]
    public void CreateBooking_ValidRequest_GeneratesCorrectReference()
    {
        var context = new BookingServiceTestContext();

        var service = context.CreateBookingService();

        var booking = service.CreateBooking(context.CreateBookingRequest());

        booking.Reference.Should().Be("DN-20261113-AB12");
    }

    // BR-26
    [Fact]
    public void CreateBooking_EconomyVehicle_Charges1000Deposit()
    {
        var context = new BookingServiceTestContext();

        var paymentMock = new ManualPaymentMock(
            context.Customer.Id,
            1000m,
            "Deposit DN-20261113-AB12");

        var service = context.CreateBookingService(paymentMock);

        var booking = service.CreateBooking(context.CreateBookingRequest());

        booking.Deposit.Should().Be(1000m);
        paymentMock.Verify();
    }

    // BR-26
    [Fact]
    public void CreateBooking_PlatinumCustomer_DoesNotChargeDeposit()
    {
        var context = new BookingServiceTestContext();
        context.Customer.LoyaltyTier = LoyaltyTier.Platinum;

        var paymentMock = new ManualPaymentMock(
            context.Customer.Id,
            1000m,
            "This call should not happen");

        var service = context.CreateBookingService(paymentMock);

        var booking = service.CreateBooking(context.CreateBookingRequest());

        booking.Deposit.Should().Be(0m);
        paymentMock.ChargeCalls.Should().Be(0);
    }

    // BR-26
    [Fact]
    public void CreateBooking_PremiumInsurance_HalvesDeposit()
    {
        var context = new BookingServiceTestContext();

        var request = context.CreateBookingRequest();
        request.InsurancePlan = InsurancePlan.Premium;

        var service = context.CreateBookingService();

        var booking = service.CreateBooking(request);

        booking.Deposit.Should().Be(500m);
    }

    // BR-26
    [Fact]
    [Trait("Rule", "BR-26")]
    public void CreateBooking_PaymentFails_DoesNotSaveBooking()
    {
        var context = new BookingServiceTestContext();

        var payment = new ThrowingPaymentGateway();

        var service = context.CreateBookingService(payment);

        var act = () => service.CreateBooking(context.CreateBookingRequest());

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Payment failed");

        context.Bookings.GetAll().Should().BeEmpty();
    }

    // BR-27: 
    [Fact]
    public void CreateBooking_UsesPromoCode_IncrementsPromoUsage()
    {
        var context = new BookingServiceTestContext();

        var promo = new PromoCode
        {
            Code = "SAVE15",
            DiscountPercent = 15,
            ValidFrom = new DateTime(2026, 11, 1),
            ValidUntil = new DateTime(2026, 12, 31),
            MinDays = 1,
            MaxUses = 100,
            UsedCount = 5
        };

        context.Promos = new FakePromoCodeRepositoryWithPromo(promo);

        var service = context.CreateBookingService();

        var request = context.CreateBookingRequest();
        request.PromoCode = "SAVE15";

        var booking = service.CreateBooking(request);

        context.Promos.GetByCode("SAVE15")!.UsedCount.Should().Be(6);
        context.Bookings.GetAll().Should().ContainSingle();
    }

    // BR-27: يجب إرسال SMS وEmail وكتابة Audit Log بعد حفظ الحجز.
    [Fact]
    [Trait("Rule", "BR-27")]
    public void CreateBooking_ValidRequest_SendsNotificationsAndAudit()
    {
        var context = new BookingServiceTestContext();

        var service = context.CreateBookingService();

        var booking = service.CreateBooking(context.CreateBookingRequest());

        var expectedMessage = $"Booking {booking.Reference} confirmed";

        context.Sms.Messages.Should().ContainSingle()
            .Which.Text.Should().Be(expectedMessage);

        context.Email.Messages.Should().ContainSingle()
            .Which.Subject.Should().Be(expectedMessage);

        context.Audit.Messages.Should()
            .Contain($"Booking {booking.Reference} created");
    }

    // =============================== R2 USING MOQ =======================================


    [Fact]
    public void CreateBooking_WithMoq_CustomerDoesNotExist_ThrowsException()
    {
        var context = new BookingServiceTestContext();

        var customers = new Mock<ICustomerRepository>();

        customers
            .Setup(x => x.GetById(1))
            .Returns((Customer?)null);

        var pricing = new PricingService(
            new FakePromoCodeRepository(),
            context.Clock);

        var service = new BookingService(
            customers.Object,
            new FakeVehicleRepository(context.Vehicle),
            context.Bookings,
            new FakePromoCodeRepository(),
            pricing,
            context.Payments,
            context.Sms,
            context.Email,
            context.Audit,
            context.Clock,
            context.References);

        var act = () => service.CreateBooking(context.CreateBookingRequest());

        act.Should()
            .Throw<CustomerNotFoundException>()
            .WithMessage("Customer #1 not found");

        customers.Verify(x => x.GetById(1), Times.Once);
    }


    [Fact]
    public void CreateBooking_WithMoq_PlatinumDoesNotCallPaymentGateway()
    {
        var context = new BookingServiceTestContext();
        context.Customer.LoyaltyTier = LoyaltyTier.Platinum;

        var payments = new Mock<IPaymentGateway>();

        var pricing = new PricingService(
            new FakePromoCodeRepository(),
            context.Clock);

        var service = new BookingService(
            new FakeCustomerRepository(context.Customer),
            new FakeVehicleRepository(context.Vehicle),
            context.Bookings,
            new FakePromoCodeRepository(),
            pricing,
            payments.Object,
            context.Sms,
            context.Email,
            context.Audit,
            context.Clock,
            context.References);

        var booking = service.CreateBooking(context.CreateBookingRequest());

        booking.Deposit.Should().Be(0m);

        payments.Verify(
            x => x.Charge(
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<string>()),
            Times.Never);
    }


    [Fact]
    public void CreateBooking_WithMoq_SendsSmsOnce()
    {
        var context = new BookingServiceTestContext();

        var sms = new Mock<ISmsSender>();

        var pricing = new PricingService(
            new FakePromoCodeRepository(),
            context.Clock);

        var service = new BookingService(
            new FakeCustomerRepository(context.Customer),
            new FakeVehicleRepository(context.Vehicle),
            context.Bookings,
            new FakePromoCodeRepository(),
            pricing,
            context.Payments,
            sms.Object,
            context.Email,
            context.Audit,
            context.Clock,
            context.References);

        var booking = service.CreateBooking(context.CreateBookingRequest());

        sms.Verify(
            x => x.Send(
                context.Customer.Phone,
                $"Booking {booking.Reference} confirmed"),
            Times.Once);
    }

}