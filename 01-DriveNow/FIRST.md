================== R1 =====================
Run => 1  
passed => 2  
failed => 1
duration => 2.1 S
failed test summary => Platinum – Vehicle already booked

---

Run => 2  
passed => 2  
failed => 1
duration => 2.1 S
Failed test summary =>  Valid Request – Gateway timeout

---

Run => 3
passed => 2  
failed => 1
duration => 2.1 S
failed test summary =>  Valid Request – Gateway timeout

---

Run => 4
passed => 2  
failed => 1
duration => 2.1 S
failed test summary => Platinum – Vehicle already booked

---

Run => 5
passed => 2  
failed => 1
duration => 2.1 S
failed test summary =>  Platinum – Vehicle already booked

================== R2 =====================

FAST ANALYSIS
1- F = Fast
this principle needs improvement because for this test case (CreateBooking_ValidRequest_ReturnBooking) takes around 1.5 s to reach failure . falis because PaymentGateway which meaning that the current test depends on an external dependency

2- I = Isolated
this principle violated as all tests use the same static shared collections, so if any test modifies the booking list it may affect the other tests results

3- R = repeatable
this principle is violated also as all tests depends on random paymentGateway timeout and DateTime.Now and in each run of this five consecutive tests we got different failed test

4- S = Self-validating
i think it Satisfied as we have used xUnit and Fluent assertion

5- T = Timely
violated
The tests are written to verify the booking functionality after the production implementation already exists

================== R3 =====================

1- For [Platinum – Vehicle already booked] this block of code which case the failure :-

if (\_bookings.GetOpenBookingsForVehicle(vehicle.Id).Any(existing =>
request.PickupAt <= existing.ReturnAt && request.ReturnAt >= existing.PickupAt))
throw new BookingRejectedException("Vehicle is already booked for these dates");

2- For [Valid Request – Gateway timeout] this block of code which case the failure inside the charge methond in paymentGateWay :-

    static void PauseAndMaybeFail()
    {
        Thread.Sleep(1500);
        if (new Random().Next(10) == 0)
            throw new PaymentFailedException("Gateway timeout");
    }
