# Kernel-level ETW trace: Microsoft-Windows-Input-HIDCLASS + Devices-AccessBroker

Requested by the Linux-side investigation (`LINUX_INVESTIGATION.md`) as
"the one technique that could plausibly see the actual mechanism
directly" -- tracing the HID class driver's own ETW provider to see what
the untraceable broker actually does, since WinDbg breakpoints, a
full-system Process Monitor capture, and handle enumeration had all
already independently shown `LightingService.exe` never touches the
hardware directly (see `../windbg/`, `../procmon/`,
`../handle_lightingservice_pid6180.txt`).

## Providers traced

Found via `logman query providers`, filtered for hid/usb/lamp/light/device:

- `Microsoft-Windows-Input-HIDCLASS` `{6465DA78-E7A0-4F39-B084-8F53C7C30DC6}`
  -- the HID class driver's own provider, exactly what was requested.
- `Microsoft-Windows-Devices-AccessBroker`
  `{64FB8D23-F0B6-5D2D-B1F6-488303C1761F}` -- name suggested it could be
  directly relevant to the unidentified WinRT device-access broker
  component found in the earlier live-debugging investigation.

## Method

```
logman create trace HidLampTrace -ow -o hid_etw_trace.etl -p "Microsoft-Windows-Input-HIDCLASS" 0xffffffffffffffff 0xff -ets
logman update trace HidLampTrace -p "Microsoft-Windows-Devices-AccessBroker" 0xffffffffffffffff 0xff -ets
# ... trigger a real lighting mode change here ...
logman stop HidLampTrace -ets
tracerpt hid_etw_trace.etl -o hid_etw_trace.csv -of CSV -summary hid_etw_summary.txt
```

Both providers confirmed running (`logman query ... -ets` showed both
listed, Level 255, KeywordsAny 0xffffffffffffffff -- maximum verbosity)
before the mode change was triggered. Trace ran 85 seconds, mode change
happened well inside that window (marker 13:51:25.691 IST, trace
started slightly before, stopped after user confirmation).

## Result: 12 total events, all a session-start rundown, zero tied to the actual write

`hid_etw_summary.txt` shows only 12 events total across the whole
85-second trace -- all `Microsoft-Windows-Input-HIDCLASS`
(2x Start, 2x Stop, 6x Information), **zero from
`Microsoft-Windows-Devices-AccessBroker`** (not even a rundown).

Decoding the raw `Clock-Time` values in `hid_etw_trace.csv`: every one
of the 10 real content events clusters within **~1.7ms of the trace
session starting**, not anywhere near the mode-change event ~85 seconds
later. This is a standard ETW "rundown" pattern -- providers dump their
current state once when a new trace session attaches. It fired twice,
from two different short-lived processes (PIDs 20500 and 3736, both
already exited by the time this was checked -- likely brief
device-enumeration helper processes coincidentally running at session
start).

**Content of the rundown** (informative on its own, if incidental): each
"Information" event carries a device's VID/PID and its full raw HID
Report Descriptor bytes. Confirmed our two known collections (`MI_00`,
`MI_01`, VID `0x0B05` PID `0x19B6`), plus a **previously uncatalogued
device**: `ACPI\ASUF1209`, an I2C HID device (`hidi2c`/`mshidkmdf`
driver stack) -- not part of this investigation, almost certainly the
laptop's touchpad, surfaced only because it also happens to be a HID
device.

**The actual mode change produced no additional events on either
provider.** Fifth independent method (after two corrected WinDbg
breakpoints, a full Process Monitor capture, and handle enumeration),
and specifically the kernel-level one suggested as the most promising
remaining option -- still a clean negative.

## What this means

`Microsoft-Windows-Input-HIDCLASS`'s ETW instrumentation only covers
device lifecycle events (start/stop/enumerate), not individual I/O
requests (`SET_REPORT`/`GET_REPORT`/`IOCTL_HID_SET_FEATURE`) flowing
through the class driver -- so this provider was never going to catch a
per-write event even if the write does pass through `hidclass.sys`
normally. `Devices-AccessBroker` producing literally nothing suggests
either it's genuinely inactive for this device class, or it's the wrong
component name entirely -- the actual WinRT `LampArray` broker's real
provider name/GUID is still unidentified.

Not pursued further past this point in this session. The natural next
step, if anyone wants to keep going, is a broader unfiltered kernel
trace (`xperf`/full WPR "CPU + FileIO + Registry" profile capturing
*every* provider) rather than guessing individual provider names, since
guessing has now failed twice (`AccessBroker` here, plus the earlier
WinDbg symbol-resolution misses).
