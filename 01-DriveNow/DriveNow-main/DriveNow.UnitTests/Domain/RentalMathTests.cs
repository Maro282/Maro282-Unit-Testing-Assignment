using DriveNow.Domain.Services;
using FluentAssertions;

namespace DriveNow.UnitTests.Domain
{
    public class RentalMathTests
    {

        [Theory]
        [InlineData(2000, 9, 15, 2026, 9, 14, 25)]
        [InlineData(2000, 9, 15, 2026, 9, 15, 26)]
        [InlineData(2000, 9, 15, 2026, 9, 16, 26)]

        // Testing 29 february in a non-leap year
        [InlineData(2000, 2, 29, 2025, 2, 28, 25)]
        [InlineData(2000, 2, 29, 2025, 2, 27, 24)]

        // Testing 29 february in a leap year
        [InlineData(2000, 2, 29, 2024, 2, 29, 24)]
        [InlineData(2000, 2, 29, 2024, 2, 28, 23)]
        public void CalculateAge_CustomerBirthDay_ReturnsFullYears(
            int birthYear,
            int birthMonth,
            int birthDay,
            int onDateYear,
            int onDateMonth,
            int onDateDay,
            int expectedAge)
        {
            // Arrange
            var birthDate = new DateTime(birthYear, birthMonth, birthDay);
            var onDate = new DateTime(onDateYear, onDateMonth, onDateDay);

            // ACT
            int result = RentalMath.CalculateAge(birthDate, onDate);

            // Assert
            result.Should().Be(expectedAge);

        }




        [Theory]
        [InlineData(2000, 9, 15, 2026, 9, 14, 25)]
        [InlineData(2000, 9, 15, 2026, 9, 15, 26)]
        [InlineData(2000, 9, 15, 2026, 9, 16, 26)]

        // Testing 29 february in a non-leap year
        [InlineData(2000, 2, 29, 2025, 2, 28, 25)]
        [InlineData(2000, 2, 29, 2025, 2, 27, 24)]

        // Testing 29 february in a leap year
        [InlineData(2000, 2, 29, 2024, 2, 29, 24)]
        [InlineData(2000, 2, 29, 2024, 2, 28, 23)]
        public void CalculateLicenseYears_LicenseIssueDate_ReturnsFullYears(
            int issueYear,
            int issuehMonth,
            int issueDay,
            int onDateYear,
            int onDateMonth,
            int onDateDay,
            int expectedAge)
        {
            // Arrange
            var issueDate = new DateTime(issueYear, issuehMonth, issueDay);
            var onDate = new DateTime(onDateYear, onDateMonth, onDateDay);

            // ACT
            int result = RentalMath.CalculateAge(issueDate, onDate);

            // Assert
            result.Should().Be(expectedAge);

        }


        [Theory]
        [InlineData("2026-11-28 10:00", "2026-11-29 10:30", 1)]
        [InlineData("2026-11-28 10:00", "2026-11-29 10:59", 1)]

        [InlineData("2026-11-28 10:00", "2026-11-29 11:00", 1)] // will fail because of the logic  >= 60 
        [InlineData("2026-11-28 10:00", "2026-11-29 11:01", 2)]

        [InlineData("2026-11-28 10:00", "2026-11-30 10:00", 2)]
        [InlineData("2026-11-28 10:00", "2026-11-30 11:01", 3)]

        public void CalculateRentalDays_PickupAndReturnDates_ReturnsTotalDays(
            DateTime pickupDate,
            DateTime returnDate,
            int expectedDays)
        {
            //Arrange with Act
            var result = RentalMath.CalculateRentalDays(pickupDate, returnDate);

            // Assert 
            result.Should().Be(expectedDays);

        }



        [Theory]
        [InlineData("2026-11-28 10:00", "2026-11-28 10:00")]
        [InlineData("2026-11-28 10:00", "2026-11-27 10:00")]

        public void CalculateRentalDays_ReturnLessThanOrEqualPickup_ThrowsException(DateTime pickupDate,
            DateTime returnDate)
        {

            //Arrange -  Act
            var result = () => RentalMath.CalculateRentalDays(pickupDate, returnDate);

            // Assert
            result.Should().Throw<ArgumentException>().WithMessage("Return must be after pickup");

        }


    }
}
