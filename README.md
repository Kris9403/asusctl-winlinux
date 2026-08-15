# G615LR Aura Lightbar — Windows working reference, Linux still stuck

ROG Strix G16 2025 (G615LR). Confirmed working, fully independent per-zone
chassis lightbar + keyboard control on Windows via raw HID, bypassing
Armoury Crate entirely. Same control **does not work on Linux** through
`asusctl` yet — this is what's blocking it.

## What's confirmed working (Windows)

- USB VID:PID `0B05:19B6` ("ROG N-Key Device" / N-Key device, shared
  across most recent ROG laptops).
- Control goes through **HID Feature report ID `0x04`**, sent via
  `HidD_SetFeature` (Windows) — **not** a plain output-report `write()`.
  Getting the report type right matters; the wrong one silently no-ops.
- 51-byte payload:

  | Offset | Meaning |
  |---|---|
  | 0 | Report ID (`0x04`) |
  | 1 | Zone count (1-8 per packet) |
  | 2 | Flag byte, `0x01` |
  | 3-18 | Up to 8 zone IDs, 2 bytes each (little-endian), unused zero |
  | 19-50 | Up to 8 colours, 4 bytes each: `[R, G, B, 0xFF]`, unused zero |

- 16 independently addressable zones total: 4 keyboard + 12 chassis
  lightbar segments. Full map in `g615lr_zone_map.png` (hex zone IDs
  `0x00`-`0x0F`).
- Live-demonstrated on video: individually coloured zones simultaneously,
  a custom tricolour layout across the chassis (see `aura_india.ps1`),
  and a live animation running on two specific zones while the rest of
  the chassis stayed static (see `aura_animate.ps1`).

## Files here

- `aura_core.ps1` — shared zone map + packet-building logic, loads
  `HidSend.cs` relative to its own folder location (portable, no
  hardcoded paths).
- `HidSend.cs` — the actual `HidD_SetFeature` P/Invoke wrapper.
- `aura_control.ps1` — direct single-zone/single-colour control.
- `aura_animate.ps1` — software-driven animations (rainbow, breathing,
  etc.) built by repeatedly resending computed-colour `0x04` packets.
- `aura_india.ps1` — worked example: custom multi-zone static layout.
- `g615lr_aura.py` — Python port of the same protocol, for reference
  outside PowerShell.

All of these work together as-is — the whole folder is self-contained,
no path edits needed.

## What's NOT working (Linux)

Ruled out so far, all clean negatives:
- Packet content, transport, interface selection, timing
- Two different real USB handshake sequences replayed from genuine
  captures — one produced a rainbow reaction, but colour was never
  actually unlocked
- A priming sequence mined directly out of a real working capture
- Continuous `0x04` streaming for several seconds
- The exact "`0x04` once, then continuous `0x0305` streaming" order,
  matching what a real wire capture shows the working sequence to be

Also checked: a sibling model's (G614JZ) driver in OpenRGB uses a
different raw-index per-key byte layout than assumed here. Swept that
range on this hardware — only the keyboard zones respond, chassis never
reacts anywhere in the swept range.

Bottom line: this is not a hardware limitation — proven working on
Windows exactly as described above. Something in how the Linux side
opens/writes the HID feature report, or an interface/init detail, is
still missing.
