# Live capture timeline — clean-strip experiment (2026-08-15)

Real-time log of everything done during the 8-minute capture window, for
correlating against the pcapng files. All timestamps IST, logged live via
`date` at the moment each action was confirmed by the user.

Capture: 3x parallel `tshark` on `\\.\USBPcap1`, `\\.\USBPcap2`,
`\\.\USBPcap3`, `-a duration:480` (8 min each), staggered start ~4s apart.
Output: `usbpcap1_8min.pcapng`, `usbpcap2_8min.pcapng`,
`usbpcap3_8min.pcapng`, all in this folder.

## Timeline

| Time (IST) | Event |
|---|---|
| 20:27:18.787 | Capture start (USBPcap1 launched; USBPcap2 ~4s later, USBPcap3 ~8s later) |
| ~20:27:19-20:28:00 | User rapidly cycled through 6 lighting modes in Armoury Crate |
| 20:28:03.038 | Device priority switched: Armoury Crate -> Windows Dynamic Lighting |
| 20:28:43.968 | Done changing modes (this segment) |
| 20:29:11.733 | Device priority switched back: Windows Dynamic Lighting -> Armoury Crate |
| 20:29:23.077 | Armoury Crate uninstaller launched (`ArmouryCrate.Uninstaller.exe`) -- required UAC, user approved |
| 20:30:44.535 | Armoury Crate uninstall confirmed complete by user |
| ~20:30:44 | Verified: Win32 "Armoury Crate Service" uninstall registry entry gone; folder left only 2 near-empty plugin subfolders (`ACStorePlugin`, `FeaturedPlugin`) -- real removal, not a failure |
| shortly after | User also ran the full elevated PowerShell batch (13 MSI-based ASUS/Aura/HAL components) separately -- confirmed after the fact, all 13 registry entries gone |
| ~20:31-20:33 | Removed `B9ECED6F.AURACreator` (Aura Creator, Store/UWP) |
| ~20:33 | Removed `ASUSAmbientHAL64` (ambient light sensor HAL, Store/UWP) and `B9ECED6F.ArmouryCrate` (the separate Store/UWP Armoury Crate frontend, distinct from the Win32 service already removed) |
| ~20:33 | **User's real-time observation**: keyboard lighting had stayed under a "stale Armoury" influence even after the Win32 service uninstall -- only fully stopped once the UWP frontend package (`B9ECED6F.ArmouryCrate`) was removed. Real signal that the MSIX/Store app has its own independent background presence, separate from the traditional Win32 service. |
| ~20:35:19 | Capture window ends (8 min from start) |
| 20:37:38.514 | Checked Settings > Personalization > Dynamic Lighting post-cleanup |

## Settings > Dynamic Lighting state check (20:37:38, pre-reboot)

Reviewed via screenshot before rebooting:

- **"Use Dynamic Lighting on my devices"**: On
- **"Compatible apps in the foreground always control lighting"**: On
- **Background light control priority list**: `Dynamic Lighting Background
  Controller`, then **`Armoury Crate` -- still listed**, despite Armoury
  Crate (both the Win32 service and the UWP frontend) being confirmed
  uninstalled at this point. Real, notable finding: this looks like a
  stale UI entry that Windows Settings hasn't refreshed, not evidence
  anything is still actually running (no process left to run) -- but
  worth confirming after a reboot rather than assuming.
- Per-device (KRISHNA keyboard) page shows the same "Use Dynamic Lighting
  on this device" / "Compatible apps..." toggles, both On, plus
  Brightness and Effects (currently "Solid Color").

**Decision**: keep "Use Dynamic Lighting on my devices" On (that's the
point of the experiment -- want Windows' own controller actually active).
Recommended turning "Compatible apps in the foreground always control
lighting" Off for a cleaner signal (removes any chance of a foreground
app hijacking control away from Windows' own Dynamic Lighting during the
test) -- not yet confirmed whether this was actually toggled before the
reboot below.

**Plan**: reboot now specifically to check whether the stale "Armoury
Crate" entry in the Background light control list clears on its own
once Windows re-enumerates installed apps, or whether it persists
(which would suggest a leftover registration somewhere not yet found).

## What's confirmed removed by end of capture

- Armoury Crate Service (Win32) + its Store/UWP frontend (`B9ECED6F.ArmouryCrate`)
- Aura Creator (`B9ECED6F.AURACreator`)
- ASUS Ambient HAL (both Win32 MSI and `ASUSAmbientHAL64` UWP)
- 13 MSI-based components: Aac_NBDT HAL, Aac_GmAcc HAL, AniMeVisionFont_STRIX_SCAR,
  ROG Live Service, Aura Wallpaper Service, AURA lighting effect add-on (x86+x64),
  ASUS Aura SDK, AURA Service (= standalone LightingService), GameSDK Service,
  ASUS Smart Display Control, ASUS Update Helper

## Explicitly kept (MyASUS family, per instruction)

`B9ECED6F.ASUSPCAssistant` (MyASUS), `AsusTranslateAgent`, `AsusLLMAgentPkg-NVdGPU`,
`AsusEmbModelLcppGpuPkg`, `AsusVLModelLcppGpuPkg`, `AsusWhisperRecorder-NVDA`,
`llm-server`, `Virtual Assistant`, `AIFrameworkService`.

## Not yet done

- `ROG CustomHotkey`, `Aura Wallpaper HTML`, `ASUS Framework Service` -- InstallShield-based,
  not covered by the MSI batch, still need removal
- Reboot (planned next step per the original experiment design)
- Reinstalling Armoury Crate with Wireshark running throughout, to capture the real
  first-touch handshake

## Reinstall-capture planning (2026-08-15, ~20:40-20:52 IST)

- Verified download source: `asus.com/support/download-center` (official) --
  ruled out `aurasync.net`, an unaffiliated fan-made site that surfaced separately
  and was **not** used. Real ASUS page confirmed by SHA-256 checksums listed per
  installer, standard ASUS download-center layout.
- Utilities: Armoury Crate page listed 5 options; considered two:
  - **Armoury Crate & Aura Creator Installer** -- v3.3.6.0, 2.2 MB, 2025/11/04,
    SHA-256 `A0C3181B135C0439F12338D6D6B77181B54A62BC44C1FE95C57945F1735D54E1`.
    Small bootstrapper; per its own description, drives live device detection +
    on-line component download at install time -- this is what would generate
    real first-touch handshake traffic on the wire.
  - **Armoury Crate Full Installation Package** -- v1.5.0.7, 4.99 GB, 2025/08/18,
    SHA-256 `FCF9D0ECF3350837F68F1223F452CB0E1CD7B8F28753AC6ECC4376C792ACBD87`.
    Own description states it installs "without instant device detection and
    on-line component downloading" -- flagged that this likely means it will
    **not** produce the same live handshake traffic as the bootstrapper. User
    chose to proceed with this one anyway.
  - Installs (per bootstrapper's changelog, for reference either way):
    Armoury Crate v6.4.7, Aura Creator v4.4.3, Armoury Crate Service /
    Armoury Crate Lite Service v6.4.7.0, Aura Service (Lighting Service)
    v3.10.04, ROG Live Service v3.4.11.0, ASUS Framework Service v4.2.4.8,
    ASUS Core SDK v2.01.52.
- Connection speed clarified as **5 MB/s** (not 5 Mbps) -- 4.99 GB download
  estimated at ~17 minutes (4990 MB / 5 MB/s ~ 998s), not the ~2-3h first
  estimated under the Mbps assumption.
- Capture window decided: **35 minutes** (2100s), same 3x parallel `tshark`
  staggered ~4s apart, next output files: `usbpcap1_35min.pcapng`,
  `usbpcap2_35min.pcapng`, `usbpcap3_35min.pcapng`, same folder. Prepared
  commands (not yet launched as of this entry):
  ```
  tshark -i \\.\USBPcap1 -a duration:2100 -w ".../usbpcap1_35min.pcapng"
  tshark -i \\.\USBPcap2 -a duration:2100 -w ".../usbpcap2_35min.pcapng"
  tshark -i \\.\USBPcap3 -a duration:2100 -w ".../usbpcap3_35min.pcapng"
  ```
- 20:52:10 -- Logging this plan, then rebooting (this also fulfills the
  still-pending reboot from the clean-strip experiment above). Prior capture
  files (`usbpcap{1,2,3}_8min.pcapng`) confirmed intact on disk, untouched, at
  time of this entry.
- **Next after reboot**: launch the three staggered `tshark` captures above,
  then download+run the 4.99 GB Armoury Crate Full Installation Package during
  the capture window.

## Pre-reboot state check (found NOT actually clean)

Checked processes/services right before rebooting. Contrary to the "confirmed
removed" section above, found still **Running**:
- `LightingService` (AURA Sync, `C:\Program Files (x86)\LightingService\`)
  -- was in the confirmed-removed list, but is back.
- `ROG Live Service` (`C:\Program Files\ASUS\ROG Live Service\`) -- same,
  was in the confirmed-removed list, but is back.
- `ArmourySocketServer` process, plus a Scheduled Task of the same name
  ("Ready" state) that independently relaunches it.
- `C:\ProgramData\ASUS` last-write time was 20:41:38 -- *after* the uninstall
  finished (20:29-20:33), suggesting something wrote there post-uninstall
  rather than this being stale pre-uninstall data.

Separately, confirmed NOT a problem (never in scope of the app uninstall,
driver-level not app-level): `ArmouryCrateControlInterface`
(`System32\ASUSACCI\`), `AsusAppService` / `ASUSSoftwareManager` /
`ASUSSystemDiagnosis` (all under `DriverStore\FileRepository\asussci2.inf_...`)
-- these belong to the OEM ASUS System Control Interface driver package,
installed via INF, separate from Armoury Crate's own installer/uninstaller.

**Decision**: user chose to leave the leftover LightingService/ROG Live
Service/ArmourySocketServer state as-is rather than clean it up further --
proceeding to reboot with this as the actual baseline, not a true blank
slate. Noting this so the eventual capture is interpreted correctly: the
reinstall will be layering onto already-present AURA Sync + ROG Live Service
binaries/services, not installing them fresh.

## 35-minute capture launched (post-reboot, 2026-08-15 ~21:00 IST)

First launch attempt (via backgrounded bash script with `nohup`/`disown`,
raw device paths `\\.\USBPcap1` etc.) **failed silently** -- the parent
shell exiting killed the child `tshark` processes despite `nohup`/`disown`,
and separately the raw device-path string hit
`Error opening adapter: ... (123)` when passed through bash. 0 packets
captured on all three, files never created.

Fixed by: (1) using numeric interface indices from `tshark -D`
(USBPcap1=10, USBPcap2=11, USBPcap3=12) instead of the raw device path
string, and (2) launching via PowerShell `Start-Process` (truly detached
process, not tied to the calling shell) instead of backgrounded bash.

**Actual capture start (IST), confirmed via `Get-Process` + capture logs,
duration 2100s / 35 min each:**

| Interface | PID | Start time (IST) | Expected end |
|---|---|---|---|
| USBPcap1 | 11884 | 21:00:40.731 | ~21:35:40 |
| USBPcap2 | 23692 | 21:00:44.746 | ~21:35:44 |
| USBPcap3 | 17496 | 21:00:48.769 | ~21:35:48 |

Output: `usbpcap1_35min.pcapng`, `usbpcap2_35min.pcapng`,
`usbpcap3_35min.pcapng` in this folder. Logs: `tshark{1,2,3}.log` /
`tshark{1,2,3}_err.log`, same folder.

**Next**: download + run the 4.99 GB Armoury Crate Full Installation
Package now, within this window.

**Stopped early**: user stopped all three captures at **21:08:08 IST**
(~7.5 min into the planned 35-min window). Final file sizes:
`usbpcap1_35min.pcapng` 288 B (header only, no packets -- same as the
earlier 8-min run), `usbpcap2_35min.pcapng` 288 B (same),
`usbpcap3_35min.pcapng` 66,792 B (has real captured traffic -- USBPcap3
was again the interface that saw data, matching the prior clean-strip
capture).

**Reason (explained ~21:10 IST)**: the 4.99 GB Full Installation Package
install was proceeding, but hit a blocker mid-install -- a popup stating
install could not proceed and directing to use the Armoury Crate Uninstall
Tool. User downloaded the official Uninstall Tool (from the same
asus.com download-center page; v2.3.7.0, 1.21 MB, SHA-256
`1DA3EEDD09600D4F1FBF9D1F0705FE77F0A91B0865E7086EC27149223D32A128`, listed
earlier in this doc) and ran it. Confirmed via screenshot: "Uninstallation
of Armoury Crate is complete" -- tool now prompting to restart before
reinstalling. This is consistent with the pre-reboot finding above that
LightingService/ROG Live Service were still present and not a true clean
slate -- likely what the fresh installer's conflict check tripped on.

This means the 35-min capture (particularly the 66,792 B `usbpcap3`
segment, 21:00:40-21:08:08) likely contains: the start of the 4.99 GB
install attempt, its failure/blocker, and the beginning of the uninstall
tool download+run -- genuine signal, not wasted. All six capture files
(three from the 8-min clean-strip run, three from this 35-min run)
confirmed intact on disk as of 21:10:17 IST, none deleted or modified.

**State now**: Armoury Crate fully uninstalled again (via the official
tool this time, not the MSI batch). Uninstall tool is asking to restart
before any reinstall attempt.

**Clarification (user, ~21:12 IST)**: the clean-strip experiment earlier
never actually ran the official Armoury Crate Uninstall Tool -- it used
the app's own uninstaller (`ArmouryCrate.Uninstaller.exe`) plus a manual
MSI batch for the 13 related components. This is the **first** time the
official Uninstall Tool has been run. That explains the leftover
LightingService/ROG Live Service found in the pre-reboot check -- the
prior method genuinely missed them, and the official tool's own
uninstall-then-restart flow is what should actually produce a correct,
clean baseline. Expectation: after this reboot, the next Armoury Crate
install will be onto a truly clean system for the first time in this
experiment.

## Second 35-minute capture launched (post-reboot, 2026-08-15 ~21:13 IST)

Reboot confirmed successful by user. Re-ran `tshark -D` before launching --
**interface indices shifted after reboot** (USBPcap1/2/3 were 10/11/12
before, now 6/7/8). Launched with corrected indices via the same
`Start-Process` detached-process method that worked last time. New
filenames used (`_v2` suffix) to avoid overwriting the previous 35-min
run's data (which had real signal in `usbpcap3_35min.pcapng`, 66,792 B).

**Start times (IST), confirmed via `Get-Process` + capture logs, duration
2100s / 35 min each:**

| Interface | PID | Start time (IST) | Expected end |
|---|---|---|---|
| USBPcap1 | 24112 | 21:13:06.050 | ~21:48:06 |
| USBPcap2 | 23432 | 21:13:10.068 | ~21:48:10 |
| USBPcap3 | 25656 | 21:13:14.082 | ~21:48:14 |

Output: `usbpcap1_35min_v2.pcapng`, `usbpcap2_35min_v2.pcapng`,
`usbpcap3_35min_v2.pcapng` in this folder. This capture is intended to
catch the actual clean reinstall (system state should now be truly clean
per the official Uninstall Tool run above).

**Next**: download + run the Armoury Crate installer within this window.
Still undecided as of this entry which installer (small bootstrapper vs.
4.99 GB Full Package) -- prior attempt with the Full Package hit a
conflict blocker requiring the Uninstall Tool, which is now resolved, so
either should be viable this time.

| 21:14:28.508 | **Installer launched** by user, ~1m22s into the capture window. Which installer (small bootstrapper vs. Full Package) not yet confirmed. |
| 21:17:51.307 | **Captures stopped** by user ("done"), ~3m23s after installer launch, ~4m37s total capture window used. |

## Detailed analysis of usbpcap3_35min_v2.pcapng (2026-08-15 ~21:29 IST)

Installer used was confirmed as the **small 2.2 MB bootstrapper** (Armoury
Crate & Aura Creator Installer, v3.3.6.0), not the Full Package -- so this
file should contain the genuine live first-touch handshake the whole
experiment was after.

**Protocol hierarchy**: 3038 frames, 150,471 bytes total. Top-level `usb`,
with 42 frames of `bluetooth`/`hci_usb` (Intel Bluetooth controller,
device address 3.3, `0x8087:0x0036` -- unrelated noise, not further
analysed).

**Devices identified on this capture**:
- Address **3.1** = `0x0B05:0x19B6` -- the ROG N-Key device, confirmed
  match for the hardware documented in README.md.
- Address 3.2 = `0x2B7E:0xC711` -- another ASUS peripheral, not analysed.
- Address 3.3 = `0x8087:0x0036` -- Intel Bluetooth controller (source of
  the bthci_usb frames above).

**Timeline for device 3.1 (N-Key), relative to capture start 21:13:14.082:**

| t (s) | IST | Event |
|---|---|---|
| 0.000 | 21:13:14 | Standard enumeration already complete at capture start (GET_DESCRIPTOR DEVICE 18B + CONFIG 59B) -- device was already live, laptop already booted |
| 62.2 | 21:14:16 | First interrupt-IN keyboard polling begins (61B/32B alternating reports) -- ordinary input activity, not lighting |
| 74.4 | 21:14:28 | Installer launched (matches logged time above) |
| 107.5-202.5 | 21:15:01-21:16:36 | **Report ID 0x5D handshake** (see below) -- 169 occurrences total, ReportType Feature(3) x169 / Output(2) x34 |
| 202.5-202.95 | 21:16:36-21:16:37 | **300x GET_DESCRIPTOR(STRING, index 1, langid en-US, wLength 1026) in 386ms (~777 req/s)** -- every single one USBD_STATUS_SUCCESS, decoded response = "ASUSTeK Computer Inc." (44-byte string descriptor, UTF-16LE). Almost certainly Armoury Crate's live device-identification routine -- likely several of its services (Armoury Crate Service, Aura Service, ROG Live Service, Control Interface) independently re-verifying device identity in the same instant. This is the actual "first-touch handshake" signal the experiment was chasing. |
| 246.999-255.231 | 21:17:21-21:17:29 | **Report ID 0x04 Feature-report lighting traffic** (see below) -- triggered by user switching to breathing mode before stopping the capture |
| 275.082 (last frame) | 21:17:49 | Capture ends (user stopped at 21:17:51.307) |

**Report ID 0x5D handshake (NEW FINDING, not in README.md)**:
`SET_REPORT` (bmRequestType 0x21, bRequest 0x09) with `wValue=0x035D`
(ReportType=Feature(3), ReportID=93/0x5D), wIndex=0, wLength=64 --
payload = `5D` + ASCII `"ASUS Tech.Inc."` null-padded to 64 bytes,
immediately followed by a `GET_REPORT` (bmRequestType 0xA1, bRequest
0x01) on the same wValue, reading back the identical 64-byte payload to
verify it round-tripped correctly. 169 Feature-type + 34 Output-type
occurrences, first at t=107.5s (21:15:01), spanning up to t=202.5s
(21:16:36) -- i.e. this handshake activity precedes and overlaps with
the device-identification descriptor burst. Read as a device
authenticity/compatibility check: host writes a known identity string
into a vendor Feature report, then reads it back to confirm it's
talking to genuine ASUS hardware, before proceeding further. **Worth
testing whether Linux asusctl needs this same SET-then-GET 0x5D
handshake before the device will accept report 0x04 lighting commands**
-- not attempted yet per LINUX_INVESTIGATION.md.

**Report ID 0x04 Feature-report lighting traffic (matches README.md
protocol)**: 260 `SET_REPORT` calls (wValue=0x0304, ReportType=Feature(3),
ReportID=4), t=246.999s-255.231s (~8.2s), sent as **pairs** ~1.3ms apart,
each pair repeating every ~42-60ms (~16-24 fps -- real animation frame
rate). Each pair = two 51-byte packets: first covers zones 0-7 (e.g.
frame 1605), second covers zones 8-15 (e.g. frame 1607, its 1.3ms-later
partner) -- together updating all 16 documented zones every animation
frame. Payload structure decoded byte-for-byte, confirmed to exactly
match README.md's documented layout:
`[0]=ReportID(0x04) [1]=ZoneCount(8) [2]=flag [3-18]=8x zoneID(u16 LE)
[19-50]=8x 4-byte color`.

**Discrepancy vs README.md**: flag byte at offset 2 was **0x00** in this
native traffic, not 0x01 as documented. Either the flag's meaning is more
nuanced than assumed (e.g. 0x01=one-shot/static vs 0x00=streaming/
animated frame), or it doesn't matter to the firmware. **Worth testing
on Windows**: try sending 0x00 instead of 0x01 in `aura_animate.ps1` and
confirm it still works.

**Conclusion**: Armoury Crate's breathing effect is confirmed to be
**client-side software animation**, not firmware-driven -- it just
repeatedly resends computed 16-zone colours via the exact same raw
Feature-report mechanism (`0x04`) as the user's own `aura_animate.ps1`.
Strong evidence the Linux blocker is an implementation/init detail (how
the feature report is opened/written), not a missing secondary protocol
-- consistent with LINUX_INVESTIGATION.md's existing conclusion. The
0x5D identity handshake is the one genuinely new lead surfaced here,
not previously known or tested on the Linux side.

## Third capture: targeted lightbar / keyboard-zone / log-tool test (2026-08-15 ~21:30 IST)

Planned sequence per user (2026-08-15 21:29 IST): (1) already downloaded
and about to start the **Armoury Crate Lite Log Tool** first, (2) then
start **Aura Creator, lightbars only**, (3) then separately light
**keyboard zones only**. Goal: isolate which zone group (chassis lightbar
vs keyboard) maps to which Report 0x04 zone IDs, and capture the Log
Tool's own traffic for comparison.

Re-checked `tshark -D` before launching -- indices back to 10/11/12 for
USBPcap1/2/3 (no reboot since last check, list order still shifts
between runs, so re-verified rather than assumed). New filenames used
(`_5min_v3` suffix) to avoid overwriting prior data.

**Start times (IST), confirmed via `Get-Process` + capture logs, duration
300s / 5 min each:**

| Interface | PID | Start time (IST) | Expected end |
|---|---|---|---|
| USBPcap1 | 10952 | 21:30:30.413 | ~21:35:30 |
| USBPcap2 | 28472 | 21:30:34.434 | ~21:35:34 |
| USBPcap3 | 11144 | 21:30:38.454 | ~21:35:38 |

Output: `usbpcap1_5min_v3.pcapng`, `usbpcap2_5min_v3.pcapng`,
`usbpcap3_5min_v3.pcapng` in this folder. Logs: `tshark{1,2,3}_v3.log` /
`tshark{1,2,3}_v3_err.log`.

**Stopped**: user confirmed sequence done ("done") at **21:33:46.217
IST**, ~3m16s into the 5-min window (test sequence finished ahead of the
full duration). Final sizes: `usbpcap1_5min_v3.pcapng` 288 B (empty,
consistent with every prior run), `usbpcap2_5min_v3.pcapng` 288 B
(empty), `usbpcap3_5min_v3.pcapng` **541,188 B** -- largest capture of
the whole session so far (>2x the previous 249,980 B run), consistent
with three separate activities packed into ~3 min: Log Tool run, Aura
Creator lightbar-only test, keyboard-zone-only test. Not yet analysed.

**User-reported sequence detail (2026-08-15 21:36:08 IST)**: opening
Aura Creator prompted that Armoury Crate itself (the actual app, not a
checkbox/setting) needed updating -- this **actually installed something**
(confirmed by user, not just a UI prompt) before Aura Creator would
proceed. Full reported sequence, in order:
1. Aura Creator forced an Armoury Crate update/install first.
2. Once done: selected **all lightbar zones**, set **static red**, explicitly
   **no keyboard** zones included.
3. Then: **layer 2**, **static blue**, **keyboard only**, explicitly no
   lightbar.
4. Finally: **overrode with Armoury Crate's own strobing effect**.

Exact per-step timestamps not yet correlated to packet data -- next step
is analysing `usbpcap3_5min_v3.pcapng` to find: (a) the forced-update
traffic, (b) the zone-ID split between red-lightbar-only and
blue-keyboard-only Report 0x04 payloads (this should reveal exactly
which zone IDs are chassis vs keyboard), and (c) the strobing override's
payload pattern vs the earlier-captured breathing pattern.

## Analysis of usbpcap3_5min_v3.pcapng (2026-08-15 21:39:16 IST)

7218 frames total. Same three devices as before (3.1 = N-Key `0x0B05:
0x19B6`, 3.2 = other ASUS peripheral `0x2B7E:0xC711`, 3.3 = Bluetooth).
Capture start 21:30:38.454 IST.

**Timeline correlated to the user's described sequence:**

| t (s) | IST | Event |
|---|---|---|
| 0 | 21:30:38 | Device already enumerated at capture start |
| ~60.4-83.6 | 21:31:38.9-21:32:02.1 | **Report 0x5D identity handshake** recurs (191 occurrences) -- overlaps with... |
| ~62 (800x, tight burst) | ~21:31:40.5 | **GET_DESCRIPTOR(STRING) identification burst recurs, 800 occurrences** -- more than double the 300 seen in the install capture. Correlates with the forced Armoury Crate update the user reported ("it actually installed something") -- consistent with multiple services restarting and re-identifying the device after an update, more of them this time than a fresh install. |
| 130.118 | 21:32:48.572 | **Report 0x04 Feature SET** -- decoded: ZoneCount=5, **flag=0x01**, zone IDs [0,1,2,3,4], colors: zones 0-3 = near-black/off `(00,00,00,01)`, **zone 4 = pure red `(FF,00,00,FF)`**. Matches user's step: "all lightbar zones, static red, no keyboard." |
| 155.498 | 21:33:13.952 | **Report 0x04 Feature SET** -- ZoneCount=5, flag=0x01, same zone IDs [0,1,2,3,4], colors: **zones 0-3 = pure blue `(00,00,FF,FF)`**, zone 4 = near-black/off. Matches user's step: "layer 2, static blue, keyboard only, no lightbar." |
| 155.5-186 (capture end) | 21:33:14-21:33:44 | No further Report 0x04 (or any other meaningful control) traffic found on device 3.1 or 3.2 -- the reported "strobing override" step is **not present in this capture**. Either it happened in the ~2s between the last logged command and the user saying "done," or it uses a mechanism/endpoint this capture didn't reach before being stopped. |

**Zone-ID mapping now confirmed by direct evidence** (previously only
inferred from README): **zone IDs 0-3 = the 4 keyboard zones**, **zone
ID 4 = (at least a representative) chassis/lightbar zone** -- red-only
action lit zone 4 and zeroed 0-3; blue-only action lit 0-3 and zeroed
zone 4. Matches README's "4 keyboard + 12 chassis" split at the boundary.
Note this exchange only ever addressed 5 total zone slots (not the full
16 seen split across two packets in the breathing capture) -- suggests
Aura Creator sends a compact **delta/changed-zones-only** update rather
than a full 16-zone refresh per action (differs from Armoury Crate's own
breathing implementation, which resent all 16 zones every animation
frame).

**Flag-byte hypothesis now testable**: this capture's two Report 0x04
calls both used **flag=0x01** for one-shot **static** color commits,
while the earlier breathing capture used **flag=0x00** for continuous
**animated** frames. Working theory: **0x01 = static/one-shot commit,
0x00 = streaming/animation frame**. Worth testing directly on Windows
via `aura_control.ps1` (static, should logically use 0x01) vs
`aura_animate.ps1` (animated, uses 0x00) to confirm, and then testing
both flag values from the Linux side.

**Report ID 5 (Feature, 10 bytes) identified as background noise, not
lighting data**: 2281 occurrences, running continuously and evenly
throughout the *entire* ~186s capture regardless of phase (~12/sec
sustained) -- e.g. present before, during, and after both color
commands. Payload is near-constant (`05 01 00 00 0f 00 <byte> 00 00
<byte>`) with only the last one or two bytes drifting (`fe...25` early
vs `ff...fd` late) -- looks like a counter, checksum, or timestamp-like
heartbeat/status poll, not zone-color data. Safe to ignore for lighting
protocol purposes, but worth remembering it exists if the Linux
implementation needs to also emulate a "device is alive" heartbeat to
avoid being timed out by Aura Service.

**Outstanding**: strobing effect payload not yet captured. If still
wanted, a short targeted capture bracketing just the strobing trigger
(a few seconds before/after) would catch it without the noise from the
update/install phase.

## CORRECTION (2026-08-15 21:45:13 IST): strobing WAS in usbpcap3_5min_v3.pcapng -- Report ID 5 misdiagnosed

User confirmed the strobing effect was present in this capture; the
"outstanding" note above and the earlier "Report 5 = background noise"
call in this doc are **wrong**. Re-analysis below.

**Report ID 5 (Feature, 10 bytes, `wValue=0x0305`) is the strobing/ambient
lighting-effect report, not a heartbeat.** Structure:
`[05][01][00][00][0F][00][FF][00][00][<varying>]` -- every byte constant
except the last, which ramps up or down by ~15-30 per step and wraps at
0/255 (e.g. `1E,...,04` early; later `0B,05,EA(wrap),D1,B7,97,7E,64,51,
38...` decreasing) -- consistent with a brightness/phase value driving a
strobe or pulse pattern, sent continuously the same way Report 0x04 was
continuously resent for the breathing effect in the other capture (same
software-animation pattern, different report ID).

**Corrected full timeline for device 3.1:**

| t (s) | IST | Event |
|---|---|---|
| 0.035-129.990 | 21:30:38.5-21:32:48.4 | **Report 5 strobing stream already active from the very start of the capture** -- steady ~62-68ms cadence (~14-16 Hz), trailing byte continuously ramping. This means strobing was already the live effect *before* the user did anything in this test -- likely left over as Armoury Crate's active/default state from before the capture began. |
| ~60.4-83.6 | 21:31:38.9-21:32:02.1 | Report 0x5D identity handshake (191x) + GET_DESCRIPTOR(STRING) burst (800x) -- the forced Armoury Crate update, as before. Runs concurrently with the Report 5 strobe stream (interleaved, doesn't interrupt it). |
| 130.054-130.117 | 21:32:48.5-21:32:48.6 | Last 3 Report 5 frames before the pause, immediately overlapping with... |
| **130.118** | **21:32:48.572** | **Report 0x04 SET -- static RED, lightbar zone (zone 4), keyboard zones 0-3 forced off.** Report 5 stream **stops** at this exact point -- Aura Creator's static override suppresses the strobe. |
| 130.118-155.497 | 21:32:48.6-21:33:14.0 | **~25.4s gap, no Report 5 at all** -- strobing fully suppressed while Aura Creator holds static red. |
| 155.497 | 21:33:13.95 | One lone Report 5 frame right as... |
| **155.498** | **21:33:13.952** | **Report 0x04 SET -- static BLUE, keyboard zones 0-3, lightbar zone 4 forced off.** Report 5 stops again immediately after. |
| 155.498-162.156 | 21:33:14.0-21:33:20.6 | ~6.66s gap, no Report 5 -- static blue holding, strobing still suppressed. |
| **162.156** | **21:33:20.61** | **Report 5 strobing stream resumes**, steady ~62ms cadence, continues uninterrupted to the last frame of the capture (t=186.154, 21:33:44.6). **This is the moment Armoury Crate's strobing override actually took effect** -- confirmed present in this capture as the user said, just misidentified earlier as noise. |

**Revised conclusion**: Report 0x04 (51 bytes, multi-zone RGB) and
Report 0x05 (10 bytes, single ramping value) are **two different
lighting-control reports on this device** -- 0x04 for explicit per-zone
static/animated colour (Aura Creator's red/blue commands, and the
earlier-captured breathing effect), 0x05 for Armoury Crate's own
continuous ambient/strobe effect. The two are mutually exclusive at the
wire level: whichever app has control, the other's report stream goes
silent. This is a second concrete, previously-undocumented protocol
detail for the Linux side, alongside the 0x5D identity handshake found
earlier -- **strobing (and likely other native Armoury Crate ambient
effects) needs Report ID 5, not just Report ID 4.**

## Detailed analysis: usbpcap3_8min.pcapng (clean-strip experiment capture) (2026-08-15 21:47:15 IST)

5848 frames total. Same three devices (3.1 N-Key, 3.2 other ASUS
peripheral, 3.3 Bluetooth). Capture start 20:27:18.787 IST (from the
original clean-strip experiment at the top of this doc). HID report
activity broken down by the known event windows already logged above:

| Window | IST | Report activity |
|---|---|---|
| Mode-cycling (t=0.2-41.2s) | 20:27:19-20:28:00 | **20x Report 5** (ambient/strobe-style), **0x Report 4**, 3x Report 0x5D (Output). The 6 modes the user cycled through appear to have all been simple presets driven via Report 5, not custom per-zone colour via Report 4. |
| Windows Dynamic Lighting active (t=44.25-85.18s) | 20:28:03-20:28:44 | **32x Report 4**, 4x Report 5, 12x Report 0x5D (Output). Notably this is where Report 4 first appears in real volume -- consistent with Windows' own Dynamic Lighting controller (built on the HID LampArray spec) driving the device via the same generic multi-zone Feature report. |
| Priority switch back to AC, pre-uninstall (t=112.9-124.3s) | 20:29:11.7-20:29:23.1 | Silent -- no report activity, transition gap. |
| During/after uninstall launch (t=124.3-206s) | 20:29:23-20:30:44 | **74x Report 4**, 12x Report 0x5D (**Feature**-type this time, not Output) -- the uninstaller itself talks to the device during removal (likely resetting/clearing lighting state, re-verifying identity). |
| **After uninstall "confirmed complete" (t>206s)** | **after 20:30:44** | **151x Report 4** still being sent, plus a few Report 0/1 events, 1x Report 5. |

**This directly corroborates the user's real-time observation already
logged at the top of this file**: "keyboard lighting had stayed under a
stale Armoury influence even after the Win32 service uninstall -- only
fully stopped once the UWP frontend package was removed." The 151 Report
4 zone-colour commands continuing well after the uninstaller reported
success is concrete packet-level evidence of exactly that stale
influence. **Last Report 4 frame in the whole file is at t=362.948s**
(20:33:21.7 IST) -- this lines up almost exactly with the "~20:33" UWP
frontend (`B9ECED6F.ArmouryCrate`) removal already logged above, i.e.
the lighting traffic appears to stop right around when the UWP package
was actually removed, not when the Win32 uninstaller finished.

## Detailed analysis: usbpcap3_35min.pcapng (interrupted install attempt) (2026-08-15 21:47:15 IST)

Only 804 frames, 40,150 bytes total -- thin by comparison to every other
capture. Same three devices present at the USB level, but almost no HID
report activity: just **12 events total**, forming 4 identical triplets
(GET_REPORT ReportID=0 x2, then SET_REPORT ReportID=1/Output x1) spaced
~130-180s apart at t=14.1s, 145.5s, 324.5s, 423.8s -- reads as routine
background status polling, unrelated to lighting. **No Report 4, no
Report 5, no Report 0x5D identity handshake, and no GET_DESCRIPTOR
identification burst anywhere in this file.**

This confirms the earlier conclusion: the 4.99 GB Full Package install
attempt captured here never got far enough to perform any real
device-level handshake before hitting its "please use the Uninstall
Tool" blocker -- consistent with it being a largely offline/staged
install that doesn't do live device detection the way the small
bootstrapper does (as documented in the "Reinstall-capture planning"
section above).

## Summary across all four capture sets

| Capture | Real signal? | Key finding |
|---|---|---|
| `usbpcap3_8min.pcapng` | Yes (485,736 B) | Mode-cycling used Report 5; Windows Dynamic Lighting used Report 4; **151 Report-4 commands continued after uninstall "completed"**, stopping only around UWP frontend removal (~20:33) |
| `usbpcap3_35min.pcapng` | Minimal (66,792 B) | Full Package install attempt never reached device-handshake phase before hitting the uninstall-tool blocker |
| `usbpcap3_35min_v2.pcapng` | Yes (249,980 B) | Bootstrapper install: 300x device-ID descriptor burst (t=202.5s), **Report 0x5D identity handshake** (new finding), breathing effect = continuous Report 4 streaming across all 16 zones, flag byte 0x00 |
| `usbpcap3_5min_v3.pcapng` | Yes (541,188 B, largest) | Forced Armoury Crate update (800x ID burst); **zone mapping confirmed**: 0-3=keyboard, 4=lightbar; static colours use Report 4 with flag 0x01; **strobing uses Report 5** (10-byte, ramping trailing byte), paused during Aura Creator static overrides, resumed at 21:33:20.61 when override applied -- corrected after initial mis-analysis |

**Consolidated new protocol details for asusctl-winlinux (not in
README.md before this session)**:
1. **Report ID 0x5D** (Feature, 64 bytes) -- SET then GET-back identity
   handshake, payload = ASCII `"ASUS Tech.Inc."`. Purpose/necessity for
   lighting control not yet tested on Linux.
2. **Report ID 0x05** (Feature, 10 bytes) -- single ramping value,
   drives Armoury Crate's native ambient/strobe effects, separate from
   the zone-based Report 0x04 path.
3. **Report 0x04 flag byte** (offset 2) appears to distinguish
   static/one-shot (`0x01`) from streamed/animated (`0x00`) writes --
   hypothesis, not yet confirmed by direct Windows-side testing.
4. Zone IDs **0-3 = keyboard**, **4 onward = chassis/lightbar**,
   directly confirmed by isolated red-lightbar-only / blue-keyboard-only
   commands.

## Full cross-check across all four captures (2026-08-15 23:44:52 IST)

User asked to consolidate and cross-check everything -- pre-uninstall
(`usbpcap3_8min.pcapng`, old Armoury Crate version) against post-install
(`usbpcap3_35min_v2.pcapng`, `usbpcap3_5min_v3.pcapng`, new v6.4.7 with
AI Aura Lighting) -- to see what holds up and what doesn't.

### CORRECTION -- the "flag byte = static vs animated" hypothesis was WRONG

Re-checked the Report 0x04 flag byte (offset 2) across a real run of
frames in **all three** captures that contain it, not just the one
example each used before. Actual, fully consistent pattern across all
20+ 8min frames, all 6 v2 frames checked, and both v3 frames:

- **flag = 0x00** on the **first** packet of a two-packet zone update
  (the zones 0-7 half).
- **flag = 0x01** on the **second/final** packet (the zones 8-15 half,
  or the *only* packet when a single packet suffices, as in v3's 5-zone
  static commands).

This holds identically whether the effect is animated (8min's cycling
modes, v2's breathing) or static (v3's red/blue) -- **the flag byte
tracks packet position/completion in a batch, not animation state.**
The earlier "0x01=static, 0x00=streamed" theory (logged after the v2
analysis, before this cross-check) is retracted. **Correct
interpretation: 0x00 = more packets follow / buffer, 0x01 = last packet
in this update / commit now.** This is the more useful fact for the
Linux side: a single-zone-group update should always be sent with
flag=0x01; multi-group updates need 0x00 on every packet except the
last.

### Report ID 5 (ambient/strobe) structure is stable across versions

Old version (8min, pre-uninstall) sample: `05 01 00 00 0F 00 FF 00 00
FF`. New version (v3, post-update) samples: `05 01 00 00 0F 00 FF 00 00
<ramping>`. Identical fixed prefix (`05 01 00 00 0F 00 FF 00 00`) in
both -- **this report's structure did not change** between the old and
new Armoury Crate/Aura Service versions used across this session.

### Report 0x5D usage genuinely DIFFERS between old and new versions -- new finding

Old version (8min, pre-uninstall) 0x5D traffic is **Output**-type (not
Feature), with a cycling second byte and mostly-zero payload -- e.g.
`5D B3 00 02 00 00 00 EB 00...`, `5D B4 00 00...`, `5D B5 00 00...`,
repeating B3->B4->B5->B3... (179,180,181, an incrementing/cycling
index), no ASCII content anywhere. **This is structurally unrelated to**
the new version's (v2/v3) **Feature**-type SET-then-GET-back
`"ASUS Tech.Inc."` identity handshake documented earlier in this file.
Same Report ID, same physical device, but different report *type*
(Output vs Feature) and completely different payload semantics between
Armoury Crate versions. **Conclusion: Report 0x5D is version-dependent
in behaviour -- do not assume the identity-string handshake is required
or even present on older Armoury Crate/Aura Service builds.** Whether
the Linux device needs either variant, neither, or something else
entirely is still untested.

### Zone-ID mapping confirmed stable across the uninstall/reinstall boundary

Checked 8min's pre-uninstall Report 0x04 traffic (t=44-85s, Windows
Dynamic Lighting era, i.e. *before* any uninstall/reinstall happened
this session): same 0-7 / 8-15 two-packet zone split seen later in the
post-reinstall v2 capture (frame 699 -> zones 0,1,2,3...; frame 701 ->
zones 8,9,10,11...). **The 16-zone map is a hardware/firmware-level
constant, unaffected by the Armoury Crate software version or the
uninstall/reinstall cycle** -- good news for the Linux side, this part
of the protocol should be version-independent.

### Consolidated, corrected findings list (supersedes earlier per-capture notes where they conflict)

1. **Report 0x04** (Feature, 51 bytes): zone-based RGB control, zones
   0-3=keyboard/4+=chassis (stable across versions), split into 8-zone
   packets when >8 zones need updating. **Flag byte = last-packet-in-
   batch indicator (0x00=more follow, 0x01=final/commit), not an
   animation-state flag.**
2. **Report 0x05** (Feature, 10 bytes): ambient/strobe effect, single
   ramping trailing byte, structure stable across Armoury Crate
   versions, mutually exclusive with Report 0x04 at the wire level
   (whichever app/effect has control, the other report's stream stops).
3. **Report 0x5D**: version-dependent. Old build = Output-type, cycling
   index byte, no identity string. New build (v6.4.7+) = Feature-type,
   SET-then-GET identity-string round-trip (`"ASUS Tech.Inc."`). Purpose
   and necessity for lighting control still untested on Linux.
4. GET_DESCRIPTOR(STRING) identification bursts (300-800x) occur around
   Armoury Crate service startup/update events, not on every launch --
   background noise from device re-identification, not required
   protocol.

## THE ACTUAL CONNECTION HANDSHAKE (2026-08-16 00:30:26 IST)

Everything above this point focused on lighting-command payloads. User
clarified the real goal: the **connection/handshake to the hardware** --
first contact during install, and the controller handoff between
Windows Dynamic Lighting and Armoury Crate -- not the colour data
(already fully known from README.md). Re-examined both with that lens.

### First contact after a fresh install (`usbpcap3_35min_v2.pcapng`)

Traced the literal first frame touching device 3.1 after the bootstrapper
launched (t=74.4s) -- nothing happens for **33 seconds**, then at
**t=107.518s** the very first packet is:

```
frame 375/376: SET_IDLE(0x0A), ReportID=0, Duration=0   <- standard HID
                                                             device-open call
frame 380:     SET_REPORT, ReportID=1 (Output), 2 bytes = 01 01
frame 383:     SET_REPORT, ReportID=0x5D (Feature, 64B) = "ASUS Tech.Inc."
frame 385:     SET_REPORT, ReportID=0x5D (Feature, 64B) = D1 01 00 01 ...
frame 387:     GET_REPORT, ReportID=0x5D                 <- read back
frame 391:     SET_REPORT 0x5D = "ASUS Tech.Inc." (again)
frame 393:     GET_REPORT 0x5D
frame 395:     SET_REPORT 0x5D = 05 20 31 00 20 ...
frame 397:     GET_REPORT 0x5D
```

**Report 0x5D is not just an identity string -- it's a generic vendor
command channel.** Byte 0 is always the report ID (0x5D); byte 1 is a
**sub-command selector**; the rest are sub-command-specific parameters.
Nearly every SET_REPORT is immediately followed by a GET_REPORT reading
the response -- this is a real request/response negotiation, not
fire-and-forget writes. Full catalog of sub-commands observed across the
whole capture (byte 1 of the 0x5D payload), in first-seen order:

| Sub-cmd | Example payload (bytes after `5D`) | Notes |
|---|---|---|
| (ASCII) | `41 53 55 53 20 54 65 63 68 2E 49 6E 63 2E` | "ASUS Tech.Inc." identity string, sent repeatedly throughout, not just once |
| `D1` | `01 00 01`, `01 00 02` | 3rd byte increments (01, 02...) across repeats -- looks like a request counter/sequence ID |
| `05` | `20 31 00 20`, `...00 10`, `...00 1A` | last byte varies (0x20/0x10/0x1A/0x1a) between calls |
| `C0` | `00 01`, `03 01`, `01 01` | short, varying params |
| `9B` | `01 01 08` | |
| `9E` | `01 20` | |
| `BD` | `01 FF 1F DD 0D` | |
| `B3`/`B4`/`B5` | mostly zero, `B3` carries extra `00 02 00 00 00 EB` | **same triplet found in the OLD Armoury Crate version's Output-type 0x5D traffic** -- this sub-command family is the one part of 0x5D that's stable across both old and new Armoury Crate versions |
| `9F` | `01 00`, `02 00`, `03 01 01 03`, `03 01 02 01`, `03 01 03 0E`, `03 01 04 04` | **enumeration pattern** -- 3rd/4th bytes step through indices (03 01, 03 02, 03 03, 03 04...) right before the GET_DESCRIPTOR(STRING) identification burst starts (t=202.5s) -- reads as Armoury Crate iterating through device sub-components/capabilities one at a time immediately before it does its bulk identification pass |

This whole sequence (identity write, D1/05/C0/9B/9E/BD/B3-4-5/9F cycle,
each followed by GET_REPORT) repeats in full **multiple times** across
the capture (clusters at t=107.5s, t=144-148s, t=200-203s) -- not a
one-shot init, but something Armoury Crate re-runs periodically or on
specific triggers (service restarts, the forced update, etc).

### The Windows Dynamic Lighting <-> Armoury Crate handoff (`usbpcap3_8min.pcapng`)

**Switch TO Windows Dynamic Lighting (20:28:03.038, t=44.25s)**: no
distinct handshake packet at this exact moment. The old-version 0x5D
`B3/B4/B5` triplet (Output-type) is already cycling steadily every
~3.4s before, through, and after this timestamp -- uninterrupted. The
handoff appears to be software-side only (which process calls
`HidD_SetFeature`), not something the device itself is told about via a
distinct packet.

**Switch BACK to Armoury Crate (20:29:11.733, t=112.9s) -- this one IS
visible on the wire:**

| t (s) | Event |
|---|---|
| up to 110.075 | Steady 18-byte control-transfer writes (Report 5, Windows Dynamic Lighting's ambient stream) |
| **110.075 -> 113.125** | **Control-transfer writes stop completely** -- ~3s dead gap |
| 113.125-117.98+ | **Burst of interrupt-IN reads** (61B/32B alternating) on the input-report endpoint (3.1.1) -- device-presence/input polling, not lighting writes |
| up to 132.058 | ~14s more of quiet (no control-transfer lighting writes at all -- ~22s total gap since Windows DL's last write) |
| 132.058 | GET_DESCRIPTOR(CONFIG)-sized (59B) control transfer -- device re-queried |
| **133.288** | **Report 0x5D Feature SET_REPORT resumes** (`5D C0 01 01...`, sub-command 0xC0 again) -- **Armoury Crate re-running the same identity/capability handshake seen at fresh-install first-contact**, ~20.4s after the priority switch was toggled |

**This is the real handoff signature**: relinquishing control doesn't
send an explicit "goodbye" packet, but *reacquiring* control does --
Armoury Crate doesn't just resume writing colours, it **re-runs the
same 0x5D vendor handshake from scratch** (re-verify identity/
capabilities) before resuming lighting control, with an interrupt-IN
presence-poll in between. Whatever a Linux implementation needs to do to
"reconnect" to the device after another app releases it, this 0x5D
re-handshake is almost certainly required -- not just re-opening the
HID handle and writing report 0x04 directly.

## File inventory + timestamp cross-check across the three phases (2026-08-16 00:34:38 IST)

User asked to confirm all pcaps across the three named phases -- **before
uninstall**, **install attempt that hit the uninstall-tool blocker**, and
**after the clean install** -- and verify logged timestamps. Confirmed:

**9 files present, exactly as expected (3 interfaces x 3 phases):**

| Phase | USBPcap1 | USBPcap2 | USBPcap3 |
|---|---|---|---|
| Before uninstall (clean-strip) | `usbpcap1_8min.pcapng` (288B) | `usbpcap2_8min.pcapng` (288B) | `usbpcap3_8min.pcapng` (485,736B) |
| Install attempt -> uninstall-tool blocker | `usbpcap1_35min.pcapng` (288B) | `usbpcap2_35min.pcapng` (288B) | `usbpcap3_35min.pcapng` (66,792B) |
| After clean install | `usbpcap1_35min_v2.pcapng` (288B) | `usbpcap2_35min_v2.pcapng` (288B) | `usbpcap3_35min_v2.pcapng` (249,980B) |

(A 4th phase, further post-install testing red/blue/strobing, exists as
`usbpcap{1,2,3}_5min_v3.pcapng` -- not counted in the "3 phases" above
since the user's ask was specifically about before/install-attempt/after.)

**USBPcap1 and USBPcap2 genuinely contain zero packets in all three
phases** -- verified via `tshark` frame count (0 frames each, all 6
files), not just inferred from the 288-byte size. This is a real,
consistent result across the whole session: whatever root hub/host
controller USBPcap1 and USBPcap2 correspond to on this laptop, nothing
relevant (or nothing at all) is attached there. All real signal has
been on USBPcap3 throughout -- not a gap in analysis, there is simply
nothing else to analyse in files 1/2.

**Timestamp cross-check** (packet-level absolute `frame.time` vs. what
was logged during this session):

- `usbpcap3_35min.pcapng`: logged process-start 21:00:48.769 IST, actual
  first frame 21:00:50.833 (+2.06s, expected driver/tshark startup
  latency). Logged stop 21:08:08.307, actual last frame 21:08:08.332
  (+25ms). **Accurate.**
- `usbpcap3_35min_v2.pcapng`: logged process-start 21:13:14.082, actual
  first frame 21:13:16.210 (+2.13s, same expected latency). Logged stop
  21:17:51.307, actual last frame 21:17:51.294 (-13ms). **Accurate.**
- `usbpcap3_8min.pcapng`: **discrepancy found.** The pre-existing log
  (written before this session's involvement, at the top of this file)
  states capture start 20:27:18.787 IST (USBPcap1; USBPcap3 ~8s later,
  so ~20:27:26.787). The file's actual first frame is **20:26:36.104**
  -- about **42-50 seconds earlier** than logged, not later. The actual
  last frame is 20:34:30.257; adding the intended 8-minute (480s)
  duration to the true first-frame time (20:26:36.104 + 480s =
  20:34:36.1) lines up far better with the real last frame than the
  originally-logged "~20:35:19" end estimate does. **Conclusion: this
  file's true capture window is ~20:26:36-20:34:30 IST, roughly 42-49s
  earlier than what was originally logged at the top of this document.**
  All of *this session's* relative-time analysis of this file (the "t=Xs"
  values used throughout the 8min-capture sections above) remains
  internally self-consistent, but readers cross-referencing those
  relative offsets against wall-clock IST should use the corrected
  20:26:36.104 anchor, not the original 20:27:18.787 one.

## ACPI/WMI investigation begins (2026-08-16)

Per Discord discussion with NeroReflex (asusctl collaborator): concern is
not just USB -- "not entirely usb drivers, but acpi/wmi too... if the
same commands are sent and windows/linux responds differently it means
linux puts the thing in a different state at boot." Everything captured
so far this session is USBPcap (pure USB/HID) -- none of it can see
ACPI/WMI traffic, which is a separate channel.

**Dead end, ruled out**: the official Armoury Crate Lite Log Tool output
(`LogToolLiteLog_20260815-213327.logE`, 8.9MB) and the tool's own EXE
were checked. File starts with magic bytes `ACLOG` followed by
high-entropy encrypted data. Decompiling `LogToolLite.exe` confirmed a
genuine hybrid RSA+AES scheme (`AesCryptoServiceProvider` for bulk data,
`RSACryptoServiceProvider` + embedded `LogToolLitePublicKey` to wrap the
AES session key, `HMACSHA256` integrity check, method literally named
`EncryptLogEV2`) -- only ASUS's private RSA key (server-side, never
shipped in the tool) can decrypt this. Not a puzzle, genuinely
unrecoverable locally. Abandoned.

**ASUS ATK ACPI-WMI interface found on this system**: `root\wmi`
namespace has two live classes: `AsusAtkWmi_WMNB` and `AsusAtkWmiEvent`.
`AsusAtkWmi_WMNB` exposes these methods (the actual ACPI-bridge surface):
`INIT`, `BSTS`, `SFUN`, `WDOG`, `KBNI`, `SCDG`, `SPEC`, `OSVR`, `VERS`,
`SPBL`, `SDSP`, `GDSP`, `GLCD`, `ANVI`, `CBIF`, `MWGF`, `DSTS`, `DEVS`.
Matches the standard ASUS ATK ACPI-WMI method table already known to
Linux's `asus-wmi` driver. Most relevant to the current question:
- `INIT` -- initializes the ATK interface, likely called once at
  driver/boot time.
- `OSVR` -- tells the ACPI firmware which OS is running. Prime suspect
  for NeroReflex's "windows tells the EC something we are missing"
  theory -- if Linux's `asus-wmi` sends a different (or no) OS-version
  string here, the firmware could genuinely branch into a different
  mode from this single call.
- `DEVS` / `DSTS` -- generic device-set / device-status pair, used
  across nearly all ASUS ACPI-WMI features via a device-ID code + value.
  Likely mechanism for any EC-level keyboard/lighting control outside
  the USB HID path already documented above.
- `KBNI` -- name suggests keyboard notify/init, directly relevant.

**Tracing enabled**: `Microsoft-Windows-WMI-Activity/Trace` event log
channel (full per-call detail: namespace, class, method, calling
process) was disabled by default. Enabled via `wevtutil sl
"Microsoft-Windows-WMI-Activity/Trace" /e:true` (required elevation,
user ran it in an admin terminal, confirmed the clear-log prompt) at
**2026-08-16 00:45:50 IST**. This is a persistent Event Log channel
(unlike a live ETW session), so it survives a reboot and keeps logging
until read out with `Get-WinEvent`.

**Next**: reboot to capture `INIT`/`OSVR` at boot time, then redo the
Armoury Crate reinstall/handoff sequence to try to catch `DEVS`/`DSTS`
calls tied to keyboard lighting, then pull the trace and analyse.

## Boot trace results (2026-08-16 00:50:42 IST)

User rebooted with the trace active, then said go. Pulled the trace with
`Get-WinEvent -LogName "Microsoft-Windows-WMI-Activity/Trace" -Oldest`
(the `-Oldest` flag is required for analytic/debug channels or the
cmdlet errors). **3695 total WMI events since boot**, first event at
00:47:23 IST (WMI service start), ASUS-ATK-related activity clustered at
**00:47:40-00:47:42** (~17-19s after WMI itself started).

**Method calls actually observed on `AsusAtkWmi_WMNB` at boot**: only
**`DSTS`** (Device Status query) -- **11 calls, no `INIT`, no `OSVR`,
no `DEVS`** anywhere in the entire boot trace. Contrary to the
hypothesis, the OS-identification (`OSVR`) and init (`INIT`) methods
were not observed being called via WMI at all during this boot.

**Callers identified by resolving ClientProcessId to process name**:
every `DSTS` call at boot came from one of exactly four processes --
**`ArmouryCrateControlInterface`, `ArmouryCrate.Service`,
`LightingService`, `ROGLiveService`** -- the same four still-running
processes/services already flagged as "leftover" in the pre-reboot state
check earlier in this document. Each independently queries device
status via `ACPI\PNP0C14\ATK_0` (the ATK ACPI-WMI mapper device node) at
startup. `WmiPrvSE` (WMI Provider Host) appears as the executing process
but is not itself a caller -- it's the host process running the query on
the real callers' behalf. (Also noted in passing under the same broad
`PNP0C14` node: unrelated NVIDIA WMI activity --
`NvGetSetBrightnessSource` etc. from `nvcontainer`/`NVDisplay.Container`
-- coincidental, not ASUS-related, ignored.)

**Important limitation discovered**: `Microsoft-Windows-WMI-Activity/
Trace` logs *metadata only* -- namespace, class, method name, calling
process, correlation/operation IDs -- **not the actual input parameter
values or return data**. So even though 11 `DSTS` calls were confirmed,
there is no way to see *which* device-ID code was queried or what value
came back, from this trace alone. This is a real ceiling on how far WMI-
Activity tracing alone can take the investigation -- unlike the USB
captures (which had full payload bytes), this only proves *that* a
method was called and *by whom*, not *with what*. Getting actual
parameter values would need a different technique (e.g. API hooking on
the provider host, or a debugger attached to `WmiPrvSE.exe`/the calling
process at the moment of the call) -- not yet attempted.

**Next**: still want to try catching `DEVS` (the SET method) during a
live Armoury Crate handoff/lighting-change event, even without visible
parameters -- confirming DEVS gets called *at all* (and by which
process, and roughly when relative to a lighting change) is still useful
signal on its own.

## Live-action test: mode changes + priority handoff (2026-08-16, ~00:52-00:58 IST)

First attempt hit an operational snag: the `/Trace` channel has a fixed
**1MB, non-circular buffer** by default -- pure boot-time WMI noise from
unrelated Windows components filled it completely within 24 seconds of
being enabled (00:47:23-00:48:07), so the channel had been silently dead
long before any lighting test could run. Confirmed via file size
(`Microsoft-Windows-WMI-Activity%4Trace.evtx` exactly 1,048,576 bytes,
LastWriteTime frozen at 00:48:07). **Exported and preserved that boot
window before resetting**: [wmi_capture/wmi_activity_trace_boot_20260816.evtx](../wmi_capture/wmi_activity_trace_boot_20260816.evtx)
(via `wevtutil epl`, which works without elevation unlike a raw file
copy of the protected System32 log; verified 3695 events intact in the
exported copy).

Fixed by disabling, setting `/ms:134217728` (128MB), then re-enabling --
**changing max size while a channel is already enabled silently no-ops**,
has to be done in that exact disable/resize/enable order. Confirmed
working (file dropped to 4KB fresh, `maxSize: 134217728` reported).
Verified the channel was genuinely live by issuing a synthetic
`Get-CimInstance` query and seeing it appear in the trace within
seconds.

**Test sequence performed** (user, marker 00:56:53.669 IST onward):
switched lighting mode to **rainbow**, then **starry night**, then back
to **rainbow**, then switched control priority to **Windows Dynamic
Lighting**, then switched priority **back to Armoury Crate**.

**Result: zero `AsusAtkWmi_WMNB` method calls of any kind** (`DSTS`,
`DEVS`, `INIT`, `OSVR` -- none) across this entire sequence, despite the
channel being confirmed live and capturing 403 total WMI events during
the same window. The only ASUS-adjacent hit was one unrelated query
(`SELECT Manufacturer FROM Win32_ComputerSystem WHERE Manufacturer LIKE
'ASUSTeK%'`, from an unrelated background process) -- not lighting/ATK
related at all.

**Conclusion**: neither RGB mode changes (static/animated effects
alike) nor the Windows-Dynamic-Lighting <-> Armoury-Crate priority
handoff touch the ACPI-WMI `AsusAtkWmi_WMNB` interface in any observable
way. Combined with the boot-time result above (only `DSTS`, called by
background services, unrelated to any user lighting action), **this
largely rules out NeroReflex's ACPI/WMI theory for this specific
hardware/software combination** -- at least nothing WMI-visible happens
during boot-time lighting init, mode changes, or the controller handoff.
All real lighting-control signal found this entire session remains on
the **USB HID side** (Report 0x04/0x05, the `0x5D` capability handshake)
documented above. If Linux is still missing something ACPI-level, it is
not something that surfaces through WMI method calls during any of the
scenarios tested here -- the `0x5D` USB HID handshake sequence remains
the strongest concrete lead for the Linux implementation.

## Full remaining-avenues sweep (2026-08-16, ~01:00-01:10 IST)

User asked to exhaust every remaining Windows-side angle (Windows 10 ISO
test excluded, deferred). Four checks run:

### 1. Registry-cached device state -- ruled out

Checked `HKLM\SYSTEM\CurrentControlSet\Enum\HID` for every instance
under `VID_0B05&PID_19B6` (9 collections under `MI_00`, `Col01`-`Col09`,
plus a separate `MI_01`). Only two of eleven instances have a
`Device Parameters` key at all, and both contain only generic Windows
HID-class-driver bookkeeping (`Col07`: mouse scroll-wheel flags
`FlipFlopWheel`/`HScrollPageOverride`/etc.; `Col09`: keyboard-class
`Protocol`/`NumberOfInputReports`/`SupportResumeOnConnect`). No
ASUS-specific cached capability blob, no persisted first-contact state.
This theory is closed.

### 2. HID collection map via HidD_GetPreparsedData / HidP_GetCaps -- major new structural data

Wrote a small C# tool (`SetupDiGetClassDevs` + `HidD_GetPreparsedData` +
`HidP_GetCaps` via P/Invoke, compiled with `csc.exe`) to enumerate every
HID collection for this VID/PID and read back UsagePage/Usage/report
lengths. `IOCTL_HID_GET_REPORT_DESCRIPTOR` (the raw descriptor bytes)
failed on every collection with `ERROR_INVALID_FUNCTION` -- likely
blocked at this privilege level on modern Windows -- but `HidP_GetCaps`
succeeded and is enough to map the whole device:

| Collection | UsagePage | Usage | In/Out/Feature (bytes) | Identity |
|---|---|---|---|---|
| MI_00 Col01 | 0xFF89 | 0x0010 | 0/0/17 | unknown vendor collection |
| MI_00 Col02 | -- | -- | -- | access denied (locked keyboard input collection) |
| MI_00 Col03 | 0xFF31 | 0x0076 | 6/0/64 | **exact match for the `0x5D` handshake (64B)** |
| MI_00 Col04 | 0xFF31 | 0x0079 | 32/64/64 | same vendor page family as Col03, larger |
| MI_00 Col05 | 0x000C | 0x0001 | 3/0/0 | standard Consumer Control (media keys) |
| MI_00 Col06 | 0x0001 | 0x000C | 2/2/0 | Generic Desktop, non-standard usage |
| MI_00 Col07 | -- | -- | -- | access denied (locked, likely real keyboard input) |
| MI_00 Col08 | 0x0001 | 0x0080 | 2/0/0 | standard System Control (power/sleep) |
| MI_00 Col09 | 0xFF82 | 0x00CF | 17/61/61 | **unknown vendor collection, larger than anything observed on the wire (61B > Report 0x04's 51B) -- likely an undiscovered report ID, not yet triggered** |
| MI_01 | 0x0059 | 0x0001 | 0/0/51 | **exact match for Report 0x04 (zone RGB, 51B)** |

Confirms Report `0x04` lives alone on the separate `MI_01` interface,
and the `0x5D` handshake lives on `MI_00 Col03`/`Col04` (vendor page
`0xFF31`). **Col09 (vendor page 0xFF82, up to 61 bytes) is a genuinely
new, unexplored lead** -- something larger than Report 0x04 exists on
this device that hasn't been triggered/observed in any capture so far.

### 3. Full Configuration Descriptor structure -- decoded from existing capture

Re-decoded via `tshark -V` from already-captured data (no new hardware
access needed). Device address 3.1 is a **2-interface composite USB
HID device**:
- **Interface 0**: HID, Boot Protocol = Keyboard, Report Descriptor
  **419 bytes** (houses the 9 `MI_00` collections above), endpoint
  0x81 IN, interrupt, 64B max, 4ms interval.
- **Interface 1**: HID, Boot Protocol = Mouse, Report Descriptor
  **327 bytes** (houses `MI_01`'s Report-0x04 vendor collection
  alongside a standard boot-mouse collection), endpoint 0x82 IN,
  interrupt, 64B max, 2ms interval.

Total combined Report Descriptor content across both interfaces:
746 bytes, self-powered, remote-wakeup capable, 100mA.

### 4. Direct SFUN/DSTS WMI probe -- confirms ACPI/WMI bypass for lighting

Required elevation (`AsusAtkWmi_WMNB` instances are SYSTEM/Admin-only
readable). Called directly against instance `ACPI\PNP0C14\ATK_0`:
- **`SFUN`** (supported-functions bitmap) returned **33** (`0x21`,
  binary `100001`) -- confirms the interface is genuinely live and
  responds meaningfully, not just erroring out.
- **`DSTS`** with `Device_ID = 0x00050021` (standard keyboard-backlight,
  well-known constant from Linux's open-source `asus-wmi` driver)
  returned **`device_status = 4294967294` (0xFFFFFFFE)** -- ASUS's
  standard "device ID not supported" error code.
- **`DSTS`** with `Device_ID = 0x00050025` (per-key/RGB keyboard
  variant) returned the **identical** `0xFFFFFFFE` unsupported code.

Both legacy ACPI-WMI keyboard-backlight device IDs are unimplemented on
this hardware's firmware. **Conclusion, now confirmed from a third
independent angle** (boot trace, live-action trace, and now direct
protocol probing): this model's firmware never wired up ACPI-WMI for
keyboard lighting at all -- RGB control was moved entirely to the USB
HID channel. The ACPI/WMI investigation is exhausted for this hardware;
there is nothing further to find there for lighting purposes.

### Genuinely open items after this sweep

1. **MI_00 Col09** (vendor page `0xFF82`, up to 61 bytes) -- unexplored,
   worth a targeted capture to see if anything ever writes to it.
   **RESOLVED below** -- see installed-files sweep.
2. **Windows 10 ISO test** -- explicitly deferred by user, not done.
3. Raw HID Report Descriptor bytes (not just the parsed capability
   summary) -- blocked by `IOCTL_HID_GET_REPORT_DESCRIPTOR` access
   restriction at user level; would need a kernel-mode approach or a
   different extraction method to get byte-for-byte AML/HID descriptor
   content.

## Installed ASUS files sweep -- the single most valuable find of the session (2026-08-16 01:14:49 IST)

User's idea: check the actual installed Armoury Crate/Aura files and
logs on disk for anything useful, before committing. Extremely
productive.

### `Col09` mystery resolved

`ARMOURY CRATE Diagnosis\AacAmbientHal\AacAmbientHal.log` shows Col09
(`hid#vid_0b05&pid_19b6&mi_00&col09`) being **opened and immediately
closed** repeatedly via `HIDHelper::GetAllHidInfo` -- a generic
"enumerate every HID collection's basic info" pass (just
`HidD_GetAttributes`/`HidP_GetCaps`-equivalent queries), not a
functional data exchange. Matches exactly why no `SET_REPORT`/
`GET_REPORT` traffic to it ever showed up in any USB capture --
confirmed genuinely unused for lighting, not missed.

### `LastProfile.xml` (`C:\Program Files (x86)\LightingService\LastProfile.xml`)

Confirms the device's internal Aura Sync type name: **
`WDL_NB_KB_4ZONE_RGB_LIGHTING`** -- "4ZONE" directly in ASUS's own
naming, corroborating the keyboard-zone count found from packet
analysis. Device ID recorded as `0B0519B6` (this VID:PID). Reveals
Aura Sync represents colour internally as **HSL** (hue/saturation/
lightness), converting to RGB only for the wire protocol -- explains
some of the less-obviously-RGB byte patterns seen in the breathing/
strobing captures earlier (they're HSL-ramp conversions, not raw
arbitrary values). Also reveals **thermal-reactive lighting**
thresholds (40C / 60C, tied to `thermal_value_type=Thermal`) and a
**music-reactive mode** (`music_index=1`) exist as effect capabilities
in the software, beyond what was captured this session. Confirms
`exclusivemode=1` was active (Armoury Crate held exclusive control) at
last save, consistent with ending the session on "priority back to
Armoury Crate."

### `AacAmbientLighting.log` -- confirms Windows LampArray API usage

`ARMOURY CRATE Diagnosis\AacAmbientHal\AacAmbientLighting.log` shows
Aura Sync's ambient-lighting HAL hooking **`MyLampArray_AvailabilityChanged`**
-- i.e. it consumes the device through Windows' own
`Windows.Devices.Lights.LampArray` WinRT/HID LampArray API, on
`VID_0B05&PID_19B6&MI_01` (the same interface Report 0x04 lives on).
Also logs the device's LampArray geometry: **`aac h: 5`, `aac w: 8`,
`sorted: 40`** -- an 8-wide x 5-tall logical grid, 40 total addressable
positions.

### `0B0519B6.csv` -- the authoritative lamp/zone geometry map (the big one)

Found at `C:\ProgramData\ASUS\ROG Live Service\DeviceContent\0B0519B6\
0B0519B6.csv` -- ASUS's own official per-device lamp layout profile,
keyed by this exact VID:PID (`0B0519B6`). **Copied into this repo**:
[device_profile/0B0519B6_lamp_layout.csv](../device_profile/0B0519B6_lamp_layout.csv).

Header: `GridWidth=13, GridHeight=10, PhyWidth=35.4, PhyHeight=26.4`
(physical size, likely cm). Body: 40 rows (`LED 0`-`LED 39`, matching
the LampArray's 8x5=40 grid exactly), each with `grid_x, grid_y,
exist, phy_x, phy_y, phy_z, lamp_id`. Only 16 of the 40 grid cells have
`exist=1` (a real physical LED) -- **and those 16 populated cells have
`lamp_id` values 0 through 15**, an **exact match for the 16 zone IDs
(0-15) already reverse-engineered from Report 0x04 traffic this
session**. This is independent, authoritative confirmation of
everything found from raw packets, plus real physical layout data no
packet capture could ever provide:

| lamp_id | grid (x,y) | phy (x,y,z) mm | Inferred location |
|---|---|---|---|
| 0 | (2,2) | 6.5, 9.9, 0.9 | keyboard |
| 1 | (3,2) | 14.0, 9.9, 0.9 | keyboard |
| 2 | (4,2) | 21.4, 9.9, 0.9 | keyboard |
| 3 | (5,2) | 28.9, 9.9, 0.9 | keyboard |
| 4 | (6,0) | 35.4, 0, 2 | chassis, top row |
| 5 | (1,0) | 0.6, 0, 2 | chassis, top row |
| 6 | (0,0) | 0, 0, 2 | chassis, top row |
| 7 | (7,0) | 36, 0, 2 | chassis, top row |
| 8 | (7,1) | 36, 0.6, 2 | chassis, 2nd row |
| 9 | (0,1) | 0, 0.6, 2 | chassis, 2nd row |
| 10 | (7,3) | 36, 25.9, 2 | chassis, 4th row |
| 11 | (0,3) | 0, 25.9, 2 | chassis, 4th row |
| 12 | (7,4) | 36, 26.5, 2 | chassis, bottom row |
| 13 | (0,4) | 0, 26.5, 2 | chassis, bottom row |
| 14 | (6,4) | 35.4, 26.5, 2 | chassis, bottom row |
| 15 | (1,4) | 0.6, 26.5, 2 | chassis, bottom row |

Confirms cleanly: **lamp_id 0-3 (grid row y=2, the physical middle of
the layout) = the 4 keyboard zones**; **lamp_id 4-15 (grid rows y=0,1,3,4,
forming the perimeter) = the 12 chassis lightbar zones**, exactly
matching the earlier zone-0-3-is-keyboard / zone-4-plus-is-chassis
finding from the red/blue isolation test, now with an authoritative
source and real spatial layout (roughly: top edge = 4,5,6,7; upper
sides = 8,9; lower sides = 10,11; bottom edge = 12,13,14,15). This is
directly usable as a reference zone map for the Linux implementation --
no more guessing needed for which zone ID is physically where.

## ArmouryCrate.Service plaintext logs -- the 0x5D sub-commands finally explained (2026-08-16 01:18:00 IST)

The two huge service logs (`ARMOURY CRATE Diagnosis\AppLog\
ArmouryCrate.Service_2026-08-15.log` / `_2026-08-16.log`, ~5.9MB each,
not previously opened) turned out to contain full plaintext function-
level tracing -- effectively documenting, in human-readable form,
everything that was previously only visible as raw hex on the wire.

**`GetRGBKBStatus`** (the actual function behind the `0x5D` identity/
capability handshake): confirmed via log line `usage 0x79 REPORTBYTE0
0x5d` that this lives specifically in **`MI_00 Col04`** (`UsagePage
0xFF31, Usage 0x79`), refining the earlier "Col03 or Col04" guess. Logs
"SetFeature Success" then decodes the read-back Feature report as:
- `Byte 8 = 1`, `lpReport[9] = 2` -- raw offsets into the response
- `Keyboard_Years = 0x25` -- likely a manufacture-year-style code
- `Support Lightbar` -- capability flag
- `Support DefaultColor` -- capability flag
- `Support SupportBitFormatKeyPosition` -- capability flag
- `Model Type : Strix` -- the laptop's model family, read directly off
  the device itself

So the `0x5D` sub-command cycle documented earlier in this file (the
identity string, then `D1`/`05`/`C0`/`9B`/`9E`/`BD`/`B3`-`B5`/`9F`
sub-commands) is a genuine **capability negotiation** -- the software is
asking the hardware what it supports (lightbar? default-colour effects?
bit-format key positions?) and what model/generation it is, before
proceeding. This finally gives semantic meaning to bytes that were
previously just decoded structurally.

**Also confirmed directly from the logs**: exact model name `G615LR`
and firmware version `534` (`[CheckSlashFWReadyToWrite] Slash model
name = G615LR, Slash FW Version = 534`). "Slash" is ASUS's separate
lid-display product line: `Slash shares HID with AURA, using unified
lock` -- confirms Slash and Aura share the same HID device/locking, but
this particular unit has no Slash hardware (`Didn't get any slash
device`).

**`NBFWRunmodeReadback` + `ChooseHidpathForECRunmodeReadback` -- directly
answers the original NeroReflex ACPI/EC question, but via HID, not
ACPI.** Log line: `[NBFWRunmodeReadback] Add mode STROBING`, immediately
followed by `[ChooseHidpathForECRunmodeReadback] From LightingHidPath`.
This is the software reading back the **embedded controller's current
run-mode state** -- exactly the kind of "EC state" NeroReflex originally
asked about -- but it does so **through the USB HID `LightingHidPath`**,
not through ACPI-WMI. This is genuinely new confirmation of *how* EC
run-mode readback actually happens on this hardware, and it's fully
consistent with (not contradicting) the earlier WMI-Activity finding
that ACPI/WMI is uninvolved -- the EC conversation happens entirely over
the same HID channel already reverse-engineered.

**Full canonical run-mode vocabulary** (searched both days' logs for
every `Add mode X` value): **`STATIC`, `BREATH`, `RAINBOW`,
`STROBING`, `COLORCYCLE`** -- five modes total. Four match effects
already tested this session (static/breathing/rainbow/strobing);
`COLORCYCLE` was never explicitly triggered/captured.

## Second pass: full software architecture + ACPI-WMI refinement (2026-08-16 01:21:02 IST)

User asked to keep digging into the rest of the 5.9MB service log.
Enumerated all 104 distinct `[Module][Function]` log tags to find what
else was worth pulling. Two more genuinely important findings.

### ACPI-WMI DSTS *is* used by this software -- but only for a CPU-temperature sensor, not lighting

Searched the whole log for every named `DSTS X(0xHEXCODE)` call. Only
**three** appear anywhere in either day's log:

| Named call | Device ID | Result |
|---|---|---|
| `GetCPUTemperature` | `0x00120094` | **Succeeds repeatedly**, values `0x0001004A` through `0x00010061` (low byte = 74-97, i.e. degrees C -- climbing over the session, consistent with real CPU load from installs/captures running) |
| `PadModeStatus` | `0x00060077` | Fails, `LastResult=0x00000000` -- 2-in-1/tablet-mode detection, irrelevant to this clamshell |
| `PowerStatusIndicator` | `0x000600C2` | Fails, `LastResult=0x00000000` -- also unsupported on this hardware |

This **refines, not contradicts**, the earlier WMI-Activity conclusion:
ACPI-WMI DSTS genuinely is called by this software stack, but only to
read **CPU temperature** as an input signal for the thermal-reactive
lighting effect already found in `LastProfile.xml`
(`thermal_threshold_one=40`, `thermal_threshold_two=60`). It is never
used to read or write anything about the keyboard/lightbar itself --
that stays entirely on USB HID as already established. This also
explains the boot-time WMI trace finding earlier in this file (11 DSTS
calls, callers identified but device IDs unknown at the time, since
WMI-Activity doesn't log parameter values) -- those calls were almost
certainly this same `GetCPUTemperature` polling, not anything
lighting-related.

### Full software pipeline from profile to wire, and what "EC mode" actually means here

Traced the call chain from effect-apply down to the HID write:
**`AuraApply` -> `SendXmlToLightingService`** (serializes the current
profile as XML -- the same format seen in `LastProfile.xml` -- over an
inter-process socket) **-> `SendScriptToLightingService`** (a locking
step, "FunctionKey Lock"/"Unlock" around the send) **-> `LightingService.exe`**
(a separate process; this is where the actual Report 0x04/0x05 USB HID
writes happen, per everything captured earlier in this file --
`LightingService.exe` itself doesn't write to this particular log file,
so its internal HID-write logging wasn't directly inspected this pass).

**`SetECMode`** (`[AuraDlg][SetECMode] Set Zone EC mode = 3`) appears 31
times across both logs' entirety, and is **always exactly `3`**, always
paired with `"Local device is unsync, set last EC mode = 3"` and
`"Debug Mode = 3, R=0, G=0, B=0"`. This is not a dynamic per-effect mode
selector -- it is a **constant representing the "device unsynced /
lights off" idle state**, distinct from the `STATIC`/`BREATH`/
`RAINBOW`/`STROBING`/`COLORCYCLE` run-mode vocabulary found earlier
(which is what actually drives the Report 0x04/0x05 wire protocol).
Does not change the standing conclusion that lighting control itself is
100% USB HID.

## Full end-to-end software architecture, from UI action to USB wire bytes (2026-08-16 01:24:12 IST)

User asked to dig until the complete software-level picture was clear.
Found `LightingService.exe`'s own log (config in `log4cxx.properties`
pointed to `C:\ProgramData\ASUS\ARMOURY CRATE Diagnosis\LightingService\
LightingService.log`, 2.1MB, DEBUG level, not previously opened) and
traced the call chain all the way from the moment an effect changes
down to the point where it becomes an HID write. Combined with
everything already found in `ArmouryCrate.Service`'s log, this is now a
complete, evidence-backed picture:

```
User action (Aura Creator / Armoury Crate UI, or a priority switch)
        |
        v
ArmouryCrate.Service (process, "AuraPlugin" module)
  [ROGAuraInterface][AuraApply]
        |  serialises current profile as XML (same schema as LastProfile.xml)
        v
  [ROGAuraInterface][SendXmlToLightingService]   -- IPC over local socket
        |
        v
  [ROGAuraInterface][SendScriptToLightingService] -- locking wrapper
        |  (crosses process boundary here)
        v
LightingService.exe (separate process, "AsRogAuraService" / aurals_3.10.10)
  [RogAuraDeviceManager][CreateIndexConverter]
        |  loads LastProfile.xml via [AuraSettingStore][LoadLastProfile]
        |  resolves _s0ModeId / m_mode_index (e.g. 100 = Static)
        v
  [AuraController][DeviceScanner::EnumerateLightDevice]  -- finds the WDL device
        |
        v
  [AuraController][IndexConvereter::SetAuraLeds]  -- maps logical LED
        |  indices to physical zone/lamp positions (the 0-15 lamp_id
        |  scheme from 0B0519B6.csv), then [IndexConvereter::Set_XYZ]
        |  for spatial coordinate handling
        v
  [AacDeviceImp][apply]:
     "LightCount Success" -> "SetLightColor Success" -> "SetMode Success"
     -> "Apply Success", ModelName = WDL_NB_KB_4ZONE_RGB_LIGHTING
        |  ([AuEngine] logs "apply time is less than 64 ms" -- matches
        |  the ~60ms Report-0x04-pair cadence measured directly from
        |  USB captures earlier in this file)
        v
AacAmbientHal.dll / AacAmbientLighting (HAL layer, consumed via Windows'
own Windows.Devices.Lights.LampArray WinRT API on MI_01)
        |  actual HidD_SetFeature-equivalent call happens inside this
        |  compiled DLL -- not logged as raw bytes anywhere found
        v
USB HID wire (device VID_0B05:PID_19B6) -- Report 0x04 (zone RGB),
Report 0x05 (ambient/strobe), Report 0x5D (GetRGBKBStatus capability
handshake) -- everything from this point on is exactly what was
captured and decoded via USBPcap earlier in this file.
```

**What's now fully explained, with a real source for every layer**:
- Why the wire protocol looks the way it does (zone/lamp IDs 0-15,
  8-per-packet splitting) -- `IndexConvereter` is the layer that
  performs this mapping, using the same lamp geometry as
  `0B0519B6.csv`.
- Why `GetRGBKBStatus` (`0x5D`) gets called before colour writes --
  `AacDeviceImp::apply`'s `LightCount`/`SetMode` steps need the
  capability data it returns.
- Why the animation cadence is ~60ms -- `AuEngine`'s own logged timing
  constraint ("apply time is less than 64 ms"), not just something
  inferred from packet timestamps.
- Why ACPI-WMI shows up at all -- only as a CPU-temperature input,
  entirely separate from this pipeline.

**What's still a black box**: the exact moment `AacAmbientHal.dll`
converts the resolved LED colours into the literal 51/10/64-byte report
payloads and calls the OS HID API -- that happens inside compiled code
with no further plaintext logging found. Getting further than this
would require disassembling `AacAmbientHal.dll`/`aaHMLib.dll`
specifically, or attaching a debugger to `LightingService.exe` at the
moment of an effect change -- not attempted. Everything on the wire side
of that boundary, however, is already fully captured and decoded
earlier in this document, so the byte-level protocol itself is not
missing -- only the DLL-internal code path that produces it.

## Live-debugging investigation into that black box (2026-08-16, ~01:33-02:05 IST)

User asked to keep digging into exactly where the black box above
actually happens, "hook or crook." Full artifacts (scripts, WinDbg logs,
Procmon extract, handle dump) are committed in
[software_investigation/](../software_investigation/) alongside this
entry. Four independent methods were used, escalating in depth; all four
converge on the same conclusion.

### Tooling installed this session

- **WinDbg** (`winget install Microsoft.WinDbg`) -- the MSIX/Store
  package turned out to be unusable for scripting: files under
  `C:\Program Files\WindowsApps\` are ACL-locked against direct
  execution/access outside the package's own sandbox, even for an
  Administrator. Replaced with the classic **Debugging Tools for
  Windows** component (`winget install
  Microsoft.WindowsSDK.10.0.18362 --override "/features
  OptionId.WindowsDesktopDebuggers /quiet /norestart"`), which installs
  `cdb.exe` to a normal, scriptable path
  (`C:\Program Files (x86)\Windows Kits\10\Debuggers\x64\`).
- **Sysinternals Process Monitor** (`winget install
  Microsoft.Sysinternals.ProcessMonitor`) and **Sysinternals Handle**
  (`winget install Microsoft.Sysinternals.Handle`).
- Attaching `cdb.exe` to `LightingService.exe` (a Session 0 Windows
  service) requires an **elevated** terminal -- confirmed both for
  debugger attach and for `handle64.exe` enumeration; non-elevated
  attempts fail with "Access is denied" / silently return nothing,
  respectively.

### Attempt 1: `hid!HidD_SetFeature` breakpoint, 64-bit assumption -- caused a real incident

First breakpoint script assumed `LightingService.exe` was a 64-bit
process (`@rdx`/`@r8` register-based x64 calling convention). It's
actually **32-bit (WOW64)**. The malformed breakpoint action likely
caused the debugger-attach itself to destabilise the process: an actual
`Access violation - code c0000005` fired inside `clr!SafeRelease`
during a `LM_Support.exe` (screen-capture layer) launch, and the
keyboard visibly froze on a stuck rainbow effect. Recovered cleanly via
`.detach` in the debugger console -- lighting resumed normally
afterward, no lasting damage. Full transcript:
[windbg/1_windbg_hidfeature_x64_crashed.log](../software_investigation/windbg/1_windbg_hidfeature_x64_crashed.log).
**Lesson logged for future sessions: always confirm target process
bitness (`0:0xx> ` prompt shows `x86` vs no suffix) before writing a
register-based breakpoint action, and treat any live debugger attach to
a running service as carrying real destabilisation risk regardless of
script correctness.**

### Attempt 2: corrected x86 breakpoint on `hid!HidD_SetFeature` -- ran clean, zero hits

Corrected script reads the buffer pointer off the stack
(`poi(@esp+8)`, correct for x86 stdcall). Set successfully, ran through
~12 real lighting-mode changes with no crash and no freeze. **Zero
breakpoint hits.** Confirms `LightingService.exe` never calls
`HidD_SetFeature` directly, in either architecture.
[windbg/2_hidfeature_bp_x86_corrected.wds](../software_investigation/windbg/2_hidfeature_bp_x86_corrected.wds)
/
[windbg/2_windbg_hidfeature_x86_zerohits.log](../software_investigation/windbg/2_windbg_hidfeature_x86_zerohits.log).

### Attempt 3: `kernel32!DeviceIoControl` -- failed to resolve

`kernel32.dll`'s `DeviceIoControl` export is a pure forwarder stub to
`KERNELBASE.dll` on modern Windows -- no real code to breakpoint. cdb
reported "Couldn't resolve error" and silently skipped straight to
`g`, meaning an entire ~12-mode-change run happened completely
unmonitored (still useful: confirmed continued stability, and captured
more live `ACPIWMI`/`GetCPUTemperature` polling in real time).
[windbg/3_ioctl_bp_kernel32_unresolved.wds](../software_investigation/windbg/3_ioctl_bp_kernel32_unresolved.wds)
/
[windbg/3_windbg_ioctl_kernel32_unresolved.log](../software_investigation/windbg/3_windbg_ioctl_kernel32_unresolved.log).

### Attempt 4: `KERNELBASE!DeviceIoControl` -- resolved correctly, zero hits

Retargeted at the real implementation. Set successfully (no resolve
error this time), ran through ~15 more real mode changes. **Zero
hits.** Combined with Attempt 2, this rules out both of the only two
ways any Windows process can talk to a device driver --
`HidD_SetFeature` and raw `DeviceIoControl` -- from inside
`LightingService.exe`.
[windbg/4_ioctl_bp_kernelbase.wds](../software_investigation/windbg/4_ioctl_bp_kernelbase.wds)
/
[windbg/4_windbg_ioctl_kernelbase_zerohits.log](../software_investigation/windbg/4_windbg_ioctl_kernelbase_zerohits.log).

### Method 5: full-system Process Monitor capture -- zero HID DeviceIoControl, any process

Unfiltered system-wide capture (~49s, spans the actual mode-change
event, verified against `LightingService.log` timestamps). Zero
`DeviceIoControl` operations to any HID device path, from **any**
process on the system -- not just `LightingService.exe`. All of
`LightingService.exe`'s own 2459 I/O operations in the window are its
own log file, `AuraProcess.ini`, and NTFS journal metadata; no device
I/O at all. Full methodology and findings:
[procmon/README.md](../software_investigation/procmon/README.md).
Filtered extract (raw 422MB/175MB captures excluded, too large for
git):
[procmon/lightingservice_all_io.csv](../software_investigation/procmon/lightingservice_all_io.csv).

### Method 6: elevated handle enumeration -- zero device handles held

Complete dump of every handle `LightingService.exe` holds (Sysinternals
`handle64.exe`, elevated). No `\Device\` path, no HID reference
anywhere -- just its log file, loaded DLLs, and a couple of IPC/shared-
memory sections (including one literally named `AURASDK`, likely the
public Aura SDK's IPC channel for third-party peripheral vendors -- not
the hardware path itself). Full output:
[handle_lightingservice_pid6180.txt](../software_investigation/handle_lightingservice_pid6180.txt).

### Final conclusion

**Four independent methods, all in agreement: `LightingService.exe`
never touches the hardware directly, at any level Windows exposes to
conventional tracing.** It calls into the WinRT
`Windows.Devices.Lights.LampArray` API (confirmed earlier via
`AacAmbientLighting.log`'s `MyLampArray_AvailabilityChanged` hook), and
the actual `IOCTL_HID_SET_FEATURE` call happens inside a Windows system
broker component that WinRT device APIs proxy through -- a process this
investigation did not identify, and which would require targeting an
unknown system service (meaningfully higher risk than anything
attempted here, since such processes often host multiple unrelated OS
services) to trace further. Not pursued past this point -- the
byte-level wire protocol was already fully known before this
investigation began, so this thread was purely about *how Windows'
own internal plumbing* delivers those bytes, not about the ASUS
protocol itself.

## Kernel-level ETW trace, requested by the Linux side (2026-08-16 13:54:10 IST)

Pulled `LINUX_INVESTIGATION.md` (Linux collaborator pushed a major
update: replicated the full `0x5D` handshake live, closed the flag-byte
and transfer-mechanism questions, and got a real `count=5` lightbar
write working once -- genuine progress, full detail in that file). Their
doc explicitly flags this session's four-method Windows finding as
important supporting evidence, and calls out kernel-level ETW tracing
of the HID class driver's own provider as "the one technique that could
plausibly see the actual mechanism directly." Ran it.

Full method, provider selection, and detailed results:
[software_investigation/etw/README.md](../software_investigation/etw/README.md).
Short version: traced `Microsoft-Windows-Input-HIDCLASS` (the exact
provider requested) and `Microsoft-Windows-Devices-AccessBroker` (a
guess at the unidentified broker's identity) simultaneously, at maximum
verbosity, through a real mode change. **12 total events in 85 seconds,
all a one-time session-start device rundown, zero tied to the actual
write, zero from AccessBroker at all.** Fifth independent method, same
conclusion as the other four: the write doesn't surface through this
provider's instrumentation. One incidental find: revealed a previously
uncatalogued I2C HID device (`ACPI\ASUF1209`, likely the touchpad) via
the rundown's device inventory -- unrelated to the lighting
investigation, noted for completeness.

Conclusion relayed back via a note in `LINUX_INVESTIGATION.md`: this
specific provider doesn't work because it only instruments device
lifecycle (start/stop/enumerate), not per-request I/O -- so it was
never going to catch a `SET_REPORT`/`IOCTL_HID_SET_FEATURE` event
regardless of whether the write passes through `hidclass.sys` normally.
`AccessBroker` producing nothing at all suggests either genuine
inactivity or (more likely) it's simply the wrong provider name --
the real `LampArray` broker's ETW identity is still unknown. Next step
if anyone continues this: a broad unfiltered kernel trace (full
`xperf`/WPR profile capturing every provider) rather than guessing
individual provider names one at a time, since two name-guesses have
now failed (`AccessBroker` here, plus the `kernel32!DeviceIoControl`
forwarder-resolution miss earlier).

**Final file sizes (v2 run)**: `usbpcap1_35min_v2.pcapng` 288 B (empty,
header only -- consistent with prior runs), `usbpcap2_35min_v2.pcapng`
288 B (empty), `usbpcap3_35min_v2.pcapng` **249,980 B** -- real captured
traffic, and notably larger than the interrupted first 35-min attempt's
66,792 B `usbpcap3` segment. USBPcap3 remains the interface that
consistently captures real data across all runs so far. All files
confirmed on disk as of 21:17:51 IST, none deleted.

Across the whole session, capture files now on disk: `usbpcap{1,2,3}_8min`
(clean-strip run), `usbpcap{1,2,3}_35min` (interrupted by install
blocker), `usbpcap{1,2,3}_35min_v2` (this run, post-reboot, actual
install attempt). USBPcap3 has data in all three: 485,736 B / 66,792 B /
249,980 B respectively.

**Installer used (confirmed after the fact)**: the small **Armoury Crate
& Aura Creator Installer** (2.2 MB bootstrapper, v3.3.6.0) -- the one
originally recommended for catching the live first-touch handshake, not
the 4.99 GB Full Package. So `usbpcap3_35min_v2.pcapng` (249,980 B)
should contain the actual bootstrapper-driven install + live device
detection traffic this experiment was after from the start.

## Broad WPR kernel trace -- sixth method, same negative result (2026-08-16 14:16 IST)

Follow-up after the named-provider ETW trace above came back negative:
user relayed that the Linux side "didn't find our findings helpful,
suggested for kernel level trace" -- read as the narrower attempt not
being convincing enough, since it depended on guessing the right
provider name up front (`AccessBroker` producing literally nothing was
ambiguous: genuinely inactive, or just the wrong name?). This time, no
guessing: a broad Windows Performance Recorder capture using the classic
kernel-logger profiles (`GeneralProfile` + `CPU` + `FileIO` + `Registry`
+ `Handle`) captures essentially everything the kernel logs, then the
result gets searched afterwards for our device instead of betting on a
provider name in advance.

Full method, the false-positive lesson from the first search pass, and
the complete category breakdown:
[software_investigation/wpr/README.md](../software_investigation/wpr/README.md).

Short version: 6.9 million total events captured across a real mode
change. Searching for the device (`VID_0B05`/`PID_19B6`) narrowed to 267
genuine matches (after ruling out ~1400 false positives from a
coincidental hex substring in unrelated `DiskIo` pointer values -- same
class of mistake as the earlier `kernel32!DeviceIoControl` forwarder
miss, logged again here as a repeat lesson). All 267 matches break down
into three flavors, none of them a per-request I/O event:

- ~240 `Object` (CloseHandle/HandleDCEnd) -- handle lifecycle bookkeeping
  from several already-exited PIDs, no I/O payload.
- 13 `Microsoft-Windows-Kernel-Power` -- a PnP device-tree power-state
  snapshot listing every collection's bound driver (`kbdhid`, `mouhid`,
  `WUDFRd`, `HidUsb`, `usbccgp`).
- 13 `SystemConfig` -- the kernel logger's own PnP device inventory,
  friendly names and driver stack paths per collection.
- 1 `FileIo`, checked individually -- not a write, no HID path in it.

Also explicitly checked, since they're the categories most likely to
carry real I/O detail: zero `FileIo` events anywhere in the full 6.9M
are both a write operation *and* reference the device; zero of the 675
`WdfTraceLoggingProvider` events (the WDF framework-level provider
`hidclass.sys` itself would use) mention the device's VID/PID at all.

**Sixth independent method (two corrected WinDbg breakpoints, Process
Monitor, handle enumeration, named-provider ETW, and now this broad
kernel trace), same result every time**: every device-specific event
found across all six methods is some flavor of inventory, rundown, or
lifecycle bookkeeping -- never a discrete event carrying the actual
`SET_REPORT`/`IOCTL_HID_SET_FEATURE` write. This is now treated as the
practical ceiling of what's traceable from the Windows side without a
kernel debugger and driver symbols: the write happens inside a boundary
(most likely the `Windows.Devices.Lights.LampArray` broker's own
compiled code) that conventional tracing -- including a genuinely broad,
non-provider-guessing kernel trace -- cannot see past.

Raw files (`wpr_broad_trace.etl` 978MB, `wpr_broad_trace.csv` 3.76GB)
excluded from git, too large. Filtered extract committed instead:
[wpr/wpr_device_matches.csv](../software_investigation/wpr/wpr_device_matches.csv)
(267 rows) and
[wpr/wpr_broad_summary.txt](../software_investigation/wpr/wpr_broad_summary.txt)
(tracerpt's own per-provider event-count summary).
