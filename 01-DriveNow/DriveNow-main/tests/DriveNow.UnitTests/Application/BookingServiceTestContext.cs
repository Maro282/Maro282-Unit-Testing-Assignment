using DriveNow.Application.Abstractions;
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;

namespace DriveNow.UnitTests.Application;

public class BookingServiceTestContext
{
    public Customer Customer { get; } = new()
    {
        Id = 1,
        FullName = "Test Customer",
        Email = "customer@test.com",
        Phone = "01000000000",
        DateOfBirth = new DateTime(1990, 1, 1),
        LicenseIssueDate = new DateTime(2020, 1, 1),
        LoyaltyTier = LoyaltyTier.None
    };

    public Vehicle Vehicle { get; } = new()
    {
        Id = 10,
        PlateNumber = "ABC123",
        Model = "Test Car",
        Category = VehicleCategory.Economy,
        DailyRate = 600,
        Mileage = 25000,
        IsAvailable = true
    };

    public FakeBookingRepository Bookings { get; } = new();

    public SpySmsSender Sms { get; } = new();

    public SpyEmailSender Email { get; } = new();

    public SpyAuditLogger Audit { get; } = new();

    public StubPaymentGateway Payments { get; } = new();

    public FakeCustomerRepository Customers { get; private set; } = null!;

    public FakeVehicleRepository Vehicles { get; private set; } = null!;

    public IPromoCodeRepository Promos { get; set; } = null!;

    public FixedTimeProvider Clock { get; } = new();

    public FixedReferenceGenerator References { get; } = new();

    public BookingService CreateBookingService(
        IPaymentGateway? paymentGateway = null)
    {
        Customers = new FakeCustomerRepository(Customer);
        Vehicles = new FakeVehicleRepository(Vehicle);
        Promos = new FakePromoCodeRepository();

        var pricing = new PricingService(Promos, Clock);

        return new BookingService(
            Customers,
            Vehicles,
            Bookings,
            Promos,
            pricing,
            paymentGateway ?? Payments,
            Sms,
            Email,
            Audit,
            Clock,
            References);
    }

    public BookingRequest CreateBookingRequest() =>
        new()
        {
            CustomerId = Customer.Id,
            VehicleId = Vehicle.Id,
            PickupAt = Clock.GetLocalNow().DateTime.AddHours(3),
            ReturnAt = Clock.GetLocalNow().DateTime.AddDays(1).AddHours(3),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0
        };
}

public class FakeCustomerRepository : ICustomerRepository
{
    private readonly Customer? _customer;

    public FakeCustomerRepository(Customer? customer)
    {
        _customer = customer;
    }

    public Customer? GetById(int id) =>
        _customer?.Id == id ? _customer : null;

    public List<Customer> GetAll() =>
        _customer is null ? [] : [_customer];

    public void Update(Customer customer) { }
}

public class FakeVehicleRepository : IVehicleRepository
{
    private readonly Vehicle? _vehicle;

    public FakeVehicleRepository(Vehicle? vehicle)
    {
        _vehicle = vehicle;
    }

    public Vehicle? GetById(int id) =>
        _vehicle?.Id == id ? _vehicle : null;

    public List<Vehicle> GetAll() =>
        _vehicle is null ? [] : [_vehicle];

    public void Update(Vehicle vehicle) { }
}

public class FakePromoCodeRepository : IPromoCodeRepository
{
    public DriveNow.Domain.Entities.PromoCode? GetByCode(string code) => null;

    public void Update(DriveNow.Domain.Entities.PromoCode promo) { }
}

public class SpyEmailSender : IEmailSender
{
    public List<(string To, string Subject, string Body)> Messages { get; } = [];

    public void Send(string to, string subject, string body)
    {
        Messages.Add((to, subject, body));
    }
}

public class SpyAuditLogger : IAuditLogger
{
    public List<string> Messages { get; } = [];

    public void Write(string message)
    {
        Messages.Add(message);
    }
}

public class FixedTimeProvider : TimeProvider
{
    public override DateTimeOffset GetUtcNow() =>
        new(2026, 11, 13, 10, 0, 0, TimeSpan.Zero);

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}

public class FixedReferenceGenerator : IReferenceGenerator
{
    public string Generate(int length) => "AB12";
}