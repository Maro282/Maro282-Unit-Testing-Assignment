1- Class => CancellationService
Line => 33 (DateTime.Now) => data changers depending on day => inject TimeProvider

2- Class => BookingService
Line => 56 (DateTime.Now) => data changers depending on day => inject TimeProvider
Line => 156/153 (Ramdom) => unpredictable data => inject reference generator

3- Class => ReturnService
Line => 37 (DateTime.Now) => data changers depending on day => inject TimeProvider

4- Class => EmailSender
Line => 7 (File system) => write files depends on system speed => depend on interface and use doubles
Line => 9 (DateTime.Now) => data changers depending on day => inject TimeProvider

5- Class => SmsSender
Line => 7 (Threed.sleep) =>tests becomes unnecessary slow => depend on interfaces and use doubles

6- Class => PaymentGateway
Line => 20 (Threed.sleep) =>tests becomes unnecessary slow => depend on interfaces and use doubles
Line => 21 / 28 (Ramdom) => unpredictable data => inject reference generator

7- Class => AuditLog
Line => 7 (File system) => write files depends on system speed => depend on interface and use doubles

8- Class => InMemoryDatabase
Line => 9 to 13 (used static lists and id counter) => tests shared data and affect each other => convert DB to instance
Line => 49/50/60/61 (DateTime.Now) => data changers depending on day => inject TimeProvider
