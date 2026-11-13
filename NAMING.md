### Naming Convention

#### task 0

R1 =>

- Test project naming

  - any unit testing project will ends with (.UnitTests) => DriveNow.UnitTests

- Test Class naming

  - if we testing (RentalMath) => RentalMathTests

- Test file naming
  - file name will be like class name => RentalMathTests.cs

R2 => for test methods naming, we have three different styles as shown :-

1- Method_Scenario_ExpectedResult => CalculateAge_CustomerUnder18_ThrowsException()
2- Should_X_When_Y => Should_ThrowException_when_CustomerUnder18()
1- Given_When_Then => Given_CustomerUnder18_When_CalculateAge_then_ThrowsException()

I will go with the first style is't popular, easy, clear and readable for everyone.

R3 =>

- for double tests the classes names will be clear like => MockEmailService / SpySmsSender and so on
- for bogus classes => CustomerFaker / VehicleFaker

R4 => Production code naming

- interfaces will starts with I => IBookingRepository lives in namespace called interfaces
- repositories ends with Repository => BookingRepository lives in namespace called repositories
- services => BookingService lives in namespace called services (DriveNow.Domain.Services;)
- BookingException => BookingException lives in namespace called (DriveNow.Domain.Exceptions;)
