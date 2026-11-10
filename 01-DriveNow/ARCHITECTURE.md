# Architecture Test Results

## R2 – Application Depends Only on Domain

**Status:** Failed

**Offending types:**

- DriveNow.Application.Services.BookingService
- DriveNow.Application.Services.PricingService
- DriveNow.Application.Services.ReturnService
- DriveNow.Application.Bookings.CancellationService

**Issue:**
DriveNow.Application project depends on DriveNow.Infrastructure, although it should depend only on DriveNow.Domain

**Why it matters for testing:**

This creates tight coupling between the application business logic and infrastructure implementation. It makes unit testing harder because application services may depend directly on infrastructure components instead of being isolated and tested using abstractions or mocks.

## R4 – Production Naming Conventions

**Status:** Failed

The following naming convention violations were identified:

## Offending Type Violation

DriveNow.Infrastructure.Repositories.CustomerRepo Repository class name does not end with Repository.
DriveNow.Domain.Services.RentalMath Class is located in a Services namespace but does not end with Service.
DriveNow.Application.Receipts.ReceiptService Service class is not located in a Services namespace.
DriveNow.Application.Bookings.CancellationService Service class is not located in a Services namespace.
DriveNow.Infrastructure.External.PaymentFailedException Exception is not located in DriveNow.Domain.Exceptions.

**Why it matters for testing:**

Consistent naming conventions make the codebase easier to understand, navigate, maintain, and test. They also help developers identify the responsibility and role of each type quickly.

---

## Passing Rules

The following architecture rules passed successfully:

- **R1:** DriveNow.Domain has no dependencies on other DriveNow projects.
- **R3:** DriveNow.Infrastructure does not depend on DriveNow.ConsoleApp.
- **R5:** Unit test classes and test methods follow the required naming conventions.
