# Assumptions

Only what the brief does not say.

**Classes**
- The admin form builds the name from grade and shift choices (for example "Class 6 - Morning Shift"). The API takes any name, as in the brief.
- Class names are unique (a duplicate returns 409). `totalSeats` must be greater than zero.

**Students and access**
- There is no login, so anyone can open the admin screen and use any 17-digit ID. The API allows any origin (CORS).
- The student ID is kept in the browser, so applications are still visible after a refresh or reopening the browser.

**Errors**
- Invalid input (bad student ID, class name or seats) returns 400. Someone else's application returns 403. An unknown application or class returns 404.
- Confirming an expired or cancelled application returns 409.
- A confirmed seat cannot be cancelled (409). Cancelling an application that is already cancelled or expired returns 204.

**Other**
- `GET /holds/{holdId}` needs no `X-Student-Id`, because the brief shows none. Ids are random GUIDs.
- All times are UTC. The countdown uses the browser's clock, but the server decides, so a late payment is rejected with a message.
- Seat safety: the class row is locked inside a database transaction, and a unique index allows only one active application per student. This should also hold with several backend processes on one database, but I did not test that.