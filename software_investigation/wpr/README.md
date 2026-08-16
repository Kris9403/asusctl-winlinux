# Broad Windows Performance Recorder (WPR) kernel trace

Requested implicitly by the Linux-side collaborator's feedback ("suggested
for kernel level trace") after the first, narrower ETW attempt
(`../etw/README.md`, `Microsoft-Windows-Input-HIDCLASS` +
`Microsoft-Windows-Devices-AccessBroker`) came back negative -- that
attempt guessed specific provider names up front and could easily have
missed the real one. This capture instead uses WPR's built-in
comprehensive kernel profiles, so nothing depends on guessing a provider
name: everything the kernel logs during the window gets captured, then
searched afterwards for our device.

Raw capture files (`wpr_broad_trace.etl` 978MB, `wpr_broad_trace.csv`
3.76GB) are **not** included here -- both far exceed GitHub's 100MB
per-file limit. This folder contains the two things extracted from them:

- `wpr_broad_summary.txt` (86KB) -- `tracerpt`'s own summary of every
  provider active during the capture and how many events each produced.
- `wpr_device_matches.csv` (123KB, 267 rows) -- every row from the full
  3.76GB CSV whose text matches our device (`VID_0B05`/`PID_19B6`, either
  byte order), extracted with `grep -iE "VID_0B05.*19B6|19B6.*0B05"`.

## Method

```
wpr -start GeneralProfile -start CPU -start FileIO -start Registry -start Handle -filemode
# ... trigger a real lighting mode change here ...
wpr -stop wpr_broad_trace.etl
tracerpt wpr_broad_trace.etl -o wpr_broad_trace.csv -of CSV -summary wpr_broad_summary.txt
```

`GeneralProfile` + `CPU` + `FileIO` + `Registry` + `Handle` together pull
in the classic NT Kernel Logger providers: CPU sampling/stackwalks,
thread scheduling, PnP/power events, handle create/close, file I/O,
registry access -- a much wider net than any single named provider,
specifically to avoid repeating the guess-a-provider-name mistake from
the first ETW attempt. Capture ran across a real mode change (confirmed
by conversation timestamps and the user's "done" after triggering it).
Result: 6.9 million total events.

## Search process (and one false-positive lesson)

First pass, `grep -i "19B6"` against the 3.76GB CSV, returned 1689
matches. Spot-checking a sample showed almost all of them were a
coincidental hex substring (`0xFFFFB18719B6F920`) inside unrelated
`DiskIo` kernel pointer values -- nothing to do with our device at all.
Refined to a combined pattern requiring both halves of the VID/PID pair
to appear together, `VID_0B05.*19B6|19B6.*0B05`, which correctly narrowed
to 267 genuine device-path matches (`wpr_device_matches.csv`). Lesson,
consistent with earlier mistakes in this investigation: never trust a
raw substring match count against binary/pointer-heavy trace data without
sampling a few actual matches first.

## What the 267 matches actually are

Broken down by event provider/category:

- **`Object` (CloseHandle / HandleDCEnd), ~240 rows** -- handle-close and
  handle-table-rundown events referencing our device's `MI_00` HID
  collections, from several distinct, already-exited PIDs (including one,
  29568, that could no longer be identified via `tasklist` by the time it
  was checked). These are handle lifecycle bookkeeping, not I/O content --
  no read/write payload, no IOCTL code, nothing tying any of them to the
  specific moment of the mode change.
- **`Microsoft-Windows-Kernel-Power`, 13 rows** -- a PnP device-tree power
  state snapshot, listing every HID/USB sub-collection under
  `VID_0B05&PID_19B6` (`MI_00&Col01` through `Col09`, `MI_01`, the raw USB
  device itself) alongside its bound driver (`kbdhid`, `mouhid`, `WUDFRd`,
  `HidUsb`, `usbccgp`) and a power-state code. This is a device-tree
  inventory dump, structurally the same kind of thing as an ETW provider
  rundown -- not an event fired because of the mode change.
- **`SystemConfig`, 13 rows** -- the classic NT Kernel Logger's own PnP
  device inventory, one row per collection, giving each collection's
  friendly name (`"HID-compliant vendor-defined device"`,
  `"HID-compliant consumer control device"`) and driver stack object path.
  Same story: a device inventory snapshot taken once, not a live I/O
  event.
- **`FileIo`, 1 row** -- checked individually; not a `Write`/`WriteInit`
  operation, no HID device path in its payload.

Also checked, specifically because they could plausibly carry per-request
I/O detail that the categories above don't: **zero** `FileIo` events
anywhere in the whole 6.9M-event capture are both a `Write`-type
operation *and* reference our device (`grep` for `Write.*hid`-style
patterns, case-insensitive, zero hits) and **zero** of the 675
`WdfTraceLoggingProvider` events (the framework-level provider that
`hidclass.sys`/miniport drivers built on WDF would use) mention our
VID/PID at all.

## Conclusion

Sixth independent method (after two corrected WinDbg breakpoints, a full
Process Monitor capture, handle enumeration, and the first named-provider
ETW trace) confirming the same result: nothing in a genuinely
comprehensive kernel-level trace -- CPU, file I/O, registry, handles, PnP
power state, WDF framework events -- captures a discrete event tied to
the actual `SET_REPORT`/`IOCTL_HID_SET_FEATURE` write at the moment of a
real lighting mode change. Every device-specific event found is some
flavor of inventory/rundown/lifecycle bookkeeping (handle close, PnP
power-state snapshot, device-tree enumeration), never per-request I/O
content.

This does not mean the write is literally untraceable in principle --
`GeneralProfile`+`CPU`+`FileIO`+`Registry`+`Handle` is broad but still not
*every* kernel provider (a targeted `xperf` trace with explicit IRP/driver
tracing flags, or a kernel debugger with a real breakpoint on
`hidclass!HidClass_IOCTL_Handler` or similar, might still see it). But as
a practical kernel-level ETW/WPR trace of the kind reasonably runnable
without a kernel debugger and driver symbols, this is the same negative
result as every other method tried in this investigation, and is treated
as the final data point: `LightingService.exe`, and no traceable Windows
kernel logging path used in this investigation, exposes the raw
`SET_REPORT` call -- it happens inside a boundary (most likely the
`Windows.Devices.Lights.LampArray` broker's own compiled code, or a
driver-level path not covered by these specific providers) that this
Windows-side investigation cannot see into further without kernel
debugging tools and symbols beyond what's been used so far.
