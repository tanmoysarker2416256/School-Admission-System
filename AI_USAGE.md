# AI usage

**Tool:** Claude (Anthropic), in the chat app, for the whole project.

**Used for:** planning, explaining the design (layers, row locks, migrations, Docker), and drafting code , ran and tested.

**Where it was wrong, and how I caught it**
1. The unique index in my database configuration covered only (student, class), so one student could hold several classes. A review caught it, and I changed it to one active application per student.
2. Expired holds were only cleaned when something read them. I asked for it to happen automatically, so a background worker was added.
4. Only 6 of 21 tests showed in Test Explorer. Counting them showed that a test file was missing.
5. In Docker the backend kept logging `relation "holds" does not exist`, because nothing created the tables. I found it in the logs and added migrations at startup.

**How I checked:** the 21 backend tests (including 40 simultaneous applicants), manual checks of every endpoint and screen, expiry with `HOLD_DURATION_SECONDS=10`, and `docker compose down` / `up` keeping the data.