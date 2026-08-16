# Cross-verification: ASUS's own Windows lamp-geometry CSV vs. live Linux LampArray enumeration

Closes the "Windows-side cross-check pending" item in the companion
`Asus-wintheory` repo, which found that the G615LR chassis lightbar (12
zones) and keyboard (4 zones) are addressed through the standard USB-IF
HID "Lighting And Illumination" usage page (`0x59`, LampArray) on
interface `MI_01` -- not through ASUS's proprietary `0x04`/`0x5D` Aura
path. See that repo's `README.md` and `evidence/lamp_enumeration_output.txt`
for the full discovery writeup.

## What's being compared

- **`0B0519B6_lamp_layout.csv`** (this folder) -- ASUS's own lamp
  geometry table for this exact laptop model, pulled from
  `C:\ProgramData\ASUS\ROG Live Service\DeviceContent\0B0519B6\0B0519B6.csv`
  during the Windows-side investigation. Physical coordinates in
  centimeters (`PhyWidth`/`PhyHeight` header fields, `phy_x`/`phy_y`/`phy_z`
  per LED row).
- **`lamp_enumeration_output.txt`** (`Asus-wintheory/evidence/`) -- live
  `LampArrayAttributesReport`/`LampAttributesRequestReport`/`ResponseReport`
  query against the real hardware's `MI_01` HID collection on Linux,
  entirely independent of the Windows CSV -- different OS, different
  toolchain (`usbhid-dump` + a Rust HID harness vs. ASUS's own installed
  software), captured weeks apart. Physical coordinates in micrometers
  (per the HID LampArray spec's `LampArrayAttributesReport` units).

## Method

Converted every populated CSV row's `(phy_x, phy_y, phy_z)` from
centimeters to micrometers (×10000) and matched by `lamp_id` against the
Linux enumeration's `(X, Y, Z)` columns for the same lamp ID.

## Result: all 16 lamps match exactly

| lamp_id | CSV phy (cm) | CSV converted (µm) | Linux enum (µm) | Match |
|---|---|---|---|---|
| 0  | 6.5, 9.9, 0.9   | 65000, 99000, 9000    | 65000, 99000, 9000    | Yes |
| 1  | 14, 9.9, 0.9    | 140000, 99000, 9000   | 140000, 99000, 9000   | Yes |
| 2  | 21.4, 9.9, 0.9  | 214000, 99000, 9000   | 214000, 99000, 9000   | Yes |
| 3  | 28.9, 9.9, 0.9  | 289000, 99000, 9000   | 289000, 99000, 9000   | Yes |
| 4  | 35.4, 0, 2      | 354000, 0, 20000      | 354000, 0, 20000      | Yes |
| 5  | 0.6, 0, 2       | 6000, 0, 20000        | 6000, 0, 20000        | Yes |
| 6  | 36, 0, 2        | 360000, 0, 20000      | 360000, 0, 20000      | Yes |
| 7  | 0, 0, 2         | 0, 0, 20000           | 0, 0, 20000           | Yes |
| 8  | 36, 0.6, 2      | 360000, 6000, 20000   | 360000, 6000, 20000   | Yes |
| 9  | 0, 0.6, 2       | 0, 6000, 20000        | 0, 6000, 20000        | Yes |
| 10 | 36, 25.9, 2     | 360000, 259000, 20000 | 360000, 259000, 20000 | Yes |
| 11 | 0, 25.9, 2      | 0, 259000, 20000      | 0, 259000, 20000      | Yes |
| 12 | 36, 26.5, 2     | 360000, 265000, 20000 | 360000, 265000, 20000 | Yes |
| 13 | 0, 26.5, 2      | 0, 265000, 20000      | 0, 265000, 20000      | Yes |
| 14 | 35.4, 26.5, 2   | 354000, 265000, 20000 | 354000, 265000, 20000 | Yes |
| 15 | 0.6, 26.5, 2    | 6000, 265000, 20000   | 6000, 265000, 20000   | Yes |

16/16 lamp positions match exactly, and the `lamp_id` numbering itself
matches between the two sources (not just the coordinates under some
different index) -- ASUS's own Windows-side geometry table and the
live Linux HID query describe the literal same lamp array, indexed
identically. This also confirms the zone-group boundary already known
from the packet-level Windows capture analysis (`live_capture/TIMELINE.md`):
lamp IDs 0-3 = keyboard, 4-15 = chassis lightbar (4 front + 4+4 left/right
side strips).

## Secondary consistency check: flag-byte semantics

`Asus-wintheory/code/g615lr-lamparray-write.rs` sets the `LampMultiUpdateReport`
flag byte to `0x00` on all but the final packet of a multi-packet update, and
`0x01` (`FLAG_COMPLETE`) on the last one. This independently matches the
corrected flag-byte finding from this repo's own packet-level analysis of
Report `0x04` in `live_capture/TIMELINE.md` (`0x00` = more packets follow,
`0x01` = final packet / commit now) -- found by a completely different route
(cross-checking multiple raw Windows captures) months before this repo even
existed. Two independent analyses of two different report IDs on two
different OSes landed on the same flag convention.

## Conclusion

This is a strong, byte-level cross-validation between the two
investigations, not just a conceptual fit: the Linux-side LampArray
discovery is confirmed correct against ASUS's own authoritative,
Windows-only geometry data, lamp-by-lamp. Recommended as the piece of
evidence to lead with when taking this back to the asus-linux community --
it's independently reproducible by anyone who still has the ASUS
software installed (or a copy of `DeviceContent`) to check the CSV
against.
