$inst = Get-CimInstance -Namespace root\wmi -ClassName AsusAtkWmi_WMNB
"Instance: $($inst.InstanceName)"
Write-Output ""
Write-Output "=== SFUN ==="
try {
  $r = Invoke-CimMethod -InputObject $inst -MethodName SFUN -ErrorAction Stop
  $r | Format-List *
} catch { "Error: $_" }

Write-Output "=== DSTS Device_ID=0x00050021 (keyboard backlight) ==="
try {
  $r = Invoke-CimMethod -InputObject $inst -MethodName DSTS -Arguments @{Device_ID=[uint32]0x00050021} -ErrorAction Stop
  $r | Format-List *
} catch { "Error: $_" }

Write-Output "=== DSTS Device_ID=0x00050025 (per-key/RGB kbd variant) ==="
try {
  $r = Invoke-CimMethod -InputObject $inst -MethodName DSTS -Arguments @{Device_ID=[uint32]0x00050025} -ErrorAction Stop
  $r | Format-List *
} catch { "Error: $_" }

$r = $inst | Format-List *
$inst | ConvertTo-Json | Out-File "C:\Users\Krushna\AppData\Local\Temp\claude\hidtool\atk_probe_result.txt"
