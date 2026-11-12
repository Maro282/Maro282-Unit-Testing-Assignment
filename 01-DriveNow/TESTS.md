# RentalMath Feature

##### BR-01 : Age is the number of full years on the given date. A birthday that has not happened yet this year does not count. A person born on 29 February has their birthday on 28 February in a non-leap year.

My tests =>
1- CalculateAge_CustomerBirthDay_ReturnsFullYears

Boundries :
[InlineData(2000, 9, 15, 2026, 9, 14, 25)]
[InlineData(2000, 9, 15, 2026, 9, 15, 26)]
[InlineData(2000, 9, 15, 2026, 9, 16, 26)]

// Testing 29 february in a non-leap year
[InlineData(2000, 2, 29, 2025, 2, 28, 25)]
[InlineData(2000, 2, 29, 2025, 2, 27, 24)]

// Testing 29 february in a leap year
[InlineData(2000, 2, 29, 2024, 2, 29, 24)]
[InlineData(2000, 2, 29, 2024, 2, 28, 23)]

---

##### BR-02 License years use the same rule as BR-01, measured from the license issue date.

My tests =>
1- CalculateLicenseYears_LicenseIssueDate_ReturnsFullYears

i used the same test data of BR-01

---

##### BR-03 Rental days: the return must be after pickup. Count full 24-hour blocks. If the leftover time is more than 60 minutes, add one day. Exactly 60 minutes does not add a day. The result is at least 1.

My tests =>
1- CalculateRentalDays_PickupAndReturnDates_ReturnsTotalDays
Bounderies that i have tested : -

[InlineData("2026-11-28 10:00", "2026-11-29 10:30", 1)]
[InlineData("2026-11-28 10:00", "2026-11-29 10:59", 1)]
[InlineData("2026-11-28 10:00", "2026-11-29 11:00", 1)]
[InlineData("2026-11-28 10:00", "2026-11-29 11:01", 2)]
[InlineData("2026-11-28 10:00", "2026-11-30 10:00", 2)]
[InlineData("2026-11-28 10:00", "2026-11-31 11:01", 4)]

2- CalculateRentalDays_ReturnLessThanPickup_ThrowsException

## Ai Suggestions

## Final coverage assessment

| Business rule                | Suggestion                                                                        | my decision                                     |
| ---------------------------- | --------------------------------------------------------------------------------- | ----------------------------------------------- |
| BR-01: CalculateAge          | Add February 29 birthday on March 1 in a non-leap year.                           | it's not important i have covered the same idea |
| BR-02: CalculateLicenseYears | Add February 29 issue date on February 28 in a non-leap year (exact anniversary). | I have already covered it                       |
| BR-03: CalculateRentalDays   | Add 48h + 60m (2 days), 48h + 61m (3 days), and exactly 60 minutes total (1 day). | i have covered all                              |



 --- 



## R4 — Bogus

I created CustomerFaker and VehicleFaker using Bogus.

Both use a fixed seed with UseSeed().

A fixed seed makes generated data repeatable. This helps FIRST because the same test data is generated every time the tests run.

Random data is fine when I only test that a generated customer or vehicle has valid basic properties.

Random data is not fine when I test a specific business rule such as the young-driver age boundary. For that test, I use WithAge(23) to control the exact age.
