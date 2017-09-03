# Shomrei Torah Schedulizer

This system generates schedules for every day of the year, factoring in זמנים, ימים טובים, and everything else that could possibly affect a schedule.

The core calculation & database logic lives in Schedulizer.Core, which is not coupled to UI in any way.

As a regression test, the build will emit ten years of schedule data to [Generated-Values.txt](Generated-Values.txt).  To easily diff this, add the following settings to `.git/config` in your clone:

```
[diff "dump"]
	xfuncname = "^---- (.*)$"
```