# Process Monitor capture summary

Raw capture files (`procmon_capture.pml` 422MB, `procmon_capture.csv` 175MB)
are **not** included here -- both exceed GitHub's 100MB per-file limit.
`lightingservice_all_io.csv` in this folder is the useful subset: every
single I/O operation `LightingService.exe` (PID 6180) performed during the
capture, extracted from the full dump.

## Capture details

- Tool: Sysinternals Process Monitor 64-bit (`Procmon64.exe`), installed via
  `winget install Microsoft.Sysinternals.ProcessMonitor`
- Launch: `/AcceptEula /Quiet /Minimized /BackingFile <path>` (unfiltered,
  system-wide, all default event categories -- Registry, File System,
  Process/Thread, Profiling)
- Time range covered: `01:51:42.8002168` to `01:52:31.8449245` IST,
  2026-08-16 (~49 seconds)
- Marker: capture started at 01:51:53.341 IST, user changed lighting mode
  to "starry" shortly after; `LightingService.exe`'s `AacDeviceImp::apply`
  cycle (`LightCount Success` -> `SetLightColor Success` -> `SetMode
  Success` -> `Apply Success`) is confirmed to have run at 01:52:07 within
  this window (cross-referenced against `LightingService.log`)
- File hit its default 512MB-ish cap before being stopped (real capture
  duration was longer than intended due to conversation latency between
  starting the capture and stopping it -- not a deliberately long window)

## Key negative result

Searched the **entire** capture, all processes, for any `DeviceIoControl`
operation targeting a HID device path:

```
grep -iE "deviceiocontrol" procmon_capture.csv | grep -i "hid"
```

**Zero matches.** The only `DeviceIoControl` activity in the whole
49-second window (which verifiably spans the actual mode-change event) is
unrelated background noise: `SamsungMagicianSVC.exe` polling SSD health
(`IOCTL_SCSI_PASS_THROUGH_DIRECT`) and `Explorer.exe` doing routine shell
I/O (`IOCTL_MOUNTDEV_QUERY_DEVICE_NAME`).

Searching specifically within `LightingService.exe`'s own I/O
(`lightingservice_all_io.csv`, 2459 rows) confirms all of it is: writes to
its own `LightingService.log`, one write to `AuraProcess.ini`, and
NTFS journal/MFT metadata writes (`$LogFile`, `$Mft`) that are automatic
side effects of the above -- **no `DeviceIoControl`, no `WriteFile` to any
device path, anywhere in its own I/O either.**

## Conclusion

Process Monitor operates via a kernel-level minifilter, so this result is
independent of (and consistent with) the WinDbg breakpoint findings in
`../windbg/`: `LightingService.exe` does not perform the raw HID write
itself, at any level Windows exposes to conventional tracing. The actual
`IOCTL_HID_SET_FEATURE` call happens inside a system component this
investigation did not identify (most likely part of the
`Windows.Devices.Lights.LampArray` WinRT broker infrastructure).
