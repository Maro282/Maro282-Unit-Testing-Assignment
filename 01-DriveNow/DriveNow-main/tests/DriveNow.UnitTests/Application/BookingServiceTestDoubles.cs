

using DriveNow.Application.Abstractions;
using DriveNow.Domain.Entities;

namespace DriveNow.UnitTests.Application;


// Dummy
public class DummyAuditLogger : IAuditLogger
{
    public void Write(string message) { }
}

// STUB
public class StubPaymentGateway : IPaymentGateway
{
    public string Charge(int customerId, decimal amount, string description)
        => "PAY-TEST-001";

    public void Refund(int customerId, decimal amount, string description) { }
}

// SPY: WE WANT TO KNOW WHAT HAPPENED AND SEE SMS SENT TO WHO AND WHAT IS IT'S CONTENT
public class SpySmsSender : ISmsSender
{
    public List<(string Phone, string Text)> Messages { get; } = [];

    public void Send(string phone, string text)
    {
        Messages.Add((phone, text));
    }

}

// MOCK: we use this to test that the call happened with expected times
public class ManualPaymentMock : IPaymentGateway
{
    private readonly int _expectedCustomerId;
    private readonly decimal _expectedAmount;
    private readonly string _expectedDescription;

    public int ChargeCalls { get; private set; }

    public ManualPaymentMock(
        int expectedCustomerId,
        decimal expectedAmount,
        string expectedDescription)
    {
        _expectedCustomerId = expectedCustomerId;
        _expectedAmount = expectedAmount;
        _expectedDescription = expectedDescription;
    }

    public string Charge(int customerId, decimal amount, string description)
    {
        ChargeCalls++;

        if (customerId != _expectedCustomerId ||
            amount != _expectedAmount ||
            description != _expectedDescription)
        {
            throw new InvalidOperationException("Unexpected payment call");
        }

        return "PAY-MOCK-001";
    }

    public void Refund(int customerId, decimal amount, string description) { }

    public void Verify()
    {
        if (ChargeCalls != 1)
            throw new InvalidOperationException("Expected exactly one Charge call");
    }
}

// FAKE: we have used it to do some simple actions simulating the actual DB
public class FakeBookingRepository : IBookingRepository
{

    private readonly List<Booking> _bookings = [];

    private int _nextId = 1;

    public Booking? GetById(int id) =>
        _bookings.FirstOrDefault(x => x.Id == id);

    public List<Booking> GetAll() => _bookings.ToList();

    public void Add(Booking booking)
    {
        booking.Id = _nextId++;
        _bookings.Add(booking);
    }

    public void Update(Booking booking)
    {
        var index = _bookings.FindIndex(x => x.Id == booking.Id);

        if (index >= 0)
            _bookings[index] = booking;
    }

    public List<Booking> GetOpenBookingsForVehicle(int vehicleId) =>
        _bookings
            .Where(x => x.VehicleId == vehicleId &&
                (x.Status == DriveNow.Domain.Enums.BookingStatus.Reserved ||
                 x.Status == DriveNow.Domain.Enums.BookingStatus.Active))
            .ToList();

    public int CountOpenBookingsForCustomer(int customerId) =>
        _bookings.Count(x =>
            x.CustomerId == customerId &&
            (x.Status == DriveNow.Domain.Enums.BookingStatus.Reserved ||
             x.Status == DriveNow.Domain.Enums.BookingStatus.Active));

    public void AddExisting(Booking booking) => Add(booking);


}


public class ThrowingPaymentGateway : IPaymentGateway
{
    public string Charge(int customerId, decimal amount, string description)
    {
        throw new InvalidOperationException("Payment failed");
    }

    public void Refund(int customerId, decimal amount, string description) { }
}

public class FakePromoCodeRepositoryWithPromo : IPromoCodeRepository
{
    private readonly DriveNow.Domain.Entities.PromoCode _promo;

    public FakePromoCodeRepositoryWithPromo(
        DriveNow.Domain.Entities.PromoCode promo)
    {
        _promo = promo;
    }

    public DriveNow.Domain.Entities.PromoCode? GetByCode(string code) =>
        string.Equals(_promo.Code, code, StringComparison.OrdinalIgnoreCase)
            ? _promo
            : null;

    public void Update(DriveNow.Domain.Entities.PromoCode promo) { }
}