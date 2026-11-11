using DriveNow.Application.Abstractions;
using DriveNow.Domain.Exceptions;
namespace DriveNow.Infrastructure.External;

public class PaymentGateway : IPaymentGateway
{

    private readonly IRandomSource _random;
    private readonly IDelay _delay;

    public PaymentGateway(IRandomSource random, IDelay delay)
    {
        _random = random;
        _delay = delay;
    }

    public string Charge(int customerId, decimal amount, string description)
    {
        PauseAndMaybeFail();
        Console.WriteLine($"[Payment] Charged {amount:N2} EGP to customer #{customerId}");
        return "PAY-" + _random.GenerateCode(8);
    }

    public void Refund(int customerId, decimal amount, string description)
    {
        PauseAndMaybeFail();
        Console.WriteLine($"[Payment] Refunded {amount:N2} EGP to customer #{customerId}");
    }

    void PauseAndMaybeFail()
    {
        _delay.Wait(TimeSpan.FromMilliseconds(1500));
        if (_random.Next(10) == 0)
            throw new PaymentFailedException("Gateway timeout");
    }


}
