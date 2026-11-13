using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using DriveNow.Application.Abstractions;
using DriveNow.UnitTests.Application;

namespace DriveNow.UnitTests.AutoFixture;

public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute()
        : base(() =>
        {
            var fixture = new Fixture();

            fixture.Customize(new AutoMoqCustomization
            {
                ConfigureMembers = true
            });

            fixture.Register<TimeProvider>(
                () => new FixedTimeProvider());

            fixture.Register<IReferenceGenerator>(
                () => new FixedReferenceGenerator());

            return fixture;
        })
    {
    }
}