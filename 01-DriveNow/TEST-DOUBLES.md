# Task 6 — BookingService Test Doubles

## R1 — Hand-written Test Doubles

| Double                | Kind  | Test                                   | Why this kind                                      |
| --------------------- | ----- | -------------------------------------- | -------------------------------------------------- |
| DummyAuditLogger      | Dummy | Tests that do not inspect audit output | It only satisfies a constructor dependency         |
| StubPaymentGateway    | Stub  | Successful booking test                | It returns a predictable result                    |
| SpySmsSender          | Spy   | BR-27 notification test                | It records calls for later inspection              |
| ManualPaymentMock     | Mock  | BR-26 deposit test                     | It checks expected calls and arguments             |
| FakeBookingRepository | Fake  | BR-21 and BR-22                        | It implements simple in-memory repository behavior |

## R2 — Moq

Three tests were rewritten using Moq:

- Missing customer: Setup, Returns and Verify.
- Platinum deposit: Verify with Times.Never.
- SMS notification: Verify with Times.Once.

### Comparison

Hand-written doubles are useful when the behavior is simple and reused by many tests. Moq is concise when verifying interactions and call counts. Moq-based tests can be more coupled to implementation details.

## R3 — AutoFixture and AutoMoq

AutoMoqDataAttribute combines AutoFixture with AutoMoqCustomization. The test uses a frozen Mock<IPaymentGateway> so the same mock instance is injected into BookingService and can be verified.

## R4 — Customer Customization

Anonymous Customer objects may contain birth dates or license issue dates that violate booking rules. A test using such data can fail before reaching the behavior it intended to test.

ValidAdultCustomerCustomization provides a deterministic adult customer with a license held for at least one year.

## R5 — InlineAutoData

InlineAutoData supplies the specific ages 20, 21 and 25 while AutoFixture generates the remaining anonymous arguments.

## R6 — AutoFixture or Bogus?

Use AutoFixture when the test needs anonymous objects and the exact values are not important. It reduces setup noise.

Use Bogus when realistic and controllable data is useful, such as generating customer names, email addresses and vehicle details.

For example, CustomerFaker is useful when realistic customer data improves readability. AutoFixture is useful when a test needs a Customer object but only cares about one or two properties.

A fixed seed is important for repeatability. Random data that changes between runs can make a test flaky and violate the Repeatable part of FIRST.

## R7 — Verify Snapshot

The created Booking is snapshotted using Verify. The test uses a fixed clock and a fixed reference generator so CreatedAt and Reference remain stable.

Snapshots are useful for checking a complete structured output. Explicit assertions are better when only a few business-critical properties matter, because they explain the expected behavior more directly.

## R8 — Self-Review

Complete the five-step self-review loop in TESTS.md:

1. Write tests independently and commit them.
2. Review coverage of BR-10 to BR-27.
3. Ask AI to review the rules and test names only.
4. Record accepted and rejected suggestions.
5. Record at least one missed issue or incorrect suggestion from the AI review.
