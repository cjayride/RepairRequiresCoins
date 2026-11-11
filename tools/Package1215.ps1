# Builds the Thunderstore zip for RepairRequiresCoins 1.2.15.
# Layout: FLAT archive root only - manifest.json, icon.png, README.md, CHANGELOG.md,
# RepairRequiresMats.dll. No subfolders (Thunderstore rejects nested layouts).
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$root  = 'D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins'
$ver   = '1.2.15'
$dll   = Join-Path $root 'RepairRequiresCoins\bin\Release\net48\RepairRequiresMats.dll'
$stage = Join-Path $root "dist\stage-$ver"
$zip   = Join-Path $root "dist\cjayride-RepairRequiresCoins-$ver.zip"

# --- verify the DLL is the version we are releasing ---
$vi = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dll)
"DLL  FileVersion={0}  Size={1}" -f $vi.FileVersion, (Get-Item $dll).Length
if ($vi.FileVersion -notlike "$ver*") { "ABORT: DLL version mismatch"; exit 1 }

# --- fresh flat staging tree (no subfolders) ---
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Force -Path $stage | Out-Null

Copy-Item (Join-Path $root 'Package\manifest.json') (Join-Path $stage 'manifest.json') -Force
Copy-Item (Join-Path $root 'Package\icon.png')      (Join-Path $stage 'icon.png')      -Force
Copy-Item (Join-Path $root 'Package\README.md')     (Join-Path $stage 'README.md')     -Force
Copy-Item (Join-Path $root 'Package\CHANGELOG.md')  (Join-Path $stage 'CHANGELOG.md')  -Force
Copy-Item $dll (Join-Path $stage 'RepairRequiresMats.dll') -Force

"MANIFEST version_number: {0}" -f ((Get-Content (Join-Path $stage 'manifest.json') -Raw | ConvertFrom-Json).version_number)
$sm = [System.Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $stage 'RepairRequiresMats.dll'))
"STAGED DLL FileVersion={0}  Size={1}" -f $sm.FileVersion, (Get-Item (Join-Path $stage 'RepairRequiresMats.dll')).Length

# --- write zip: every entry is a bare file name at the archive root ---
if (Test-Path $zip) { Remove-Item $zip -Force }
$fs = [System.IO.File]::Open($zip, [System.IO.FileMode]::CreateNew)
$za = New-Object System.IO.Compression.ZipArchive($fs, [System.IO.Compression.ZipArchiveMode]::Create)
$names = @('manifest.json', 'icon.png', 'README.md', 'CHANGELOG.md', 'RepairRequiresMats.dll')
foreach ($n in $names) {
  if ($n -match '[\\/]') { "ABORT: nested entry name '$n'"; $za.Dispose(); $fs.Dispose(); exit 1 }
  $en = $za.CreateEntry($n, [System.IO.Compression.CompressionLevel]::Optimal)
  $es = $en.Open()
  $bytes = [System.IO.File]::ReadAllBytes((Join-Path $stage $n))
  $es.Write($bytes, 0, $bytes.Length)
  $es.Dispose()
}
$za.Dispose(); $fs.Dispose()
"WROTE: $zip  ({0} bytes)" -f (Get-Item $zip).Length

# --- verify the archive we just wrote ---
"--- zip entries ---"
$bad = 0
[System.IO.Compression.ZipFile]::OpenRead($zip).Entries | ForEach-Object {
  "  {0}  {1}" -f $_.FullName, $_.Length
  if ($_.FullName -match '[\\/]') { $bad++ }
}
"NESTED ENTRY COUNT = $bad"
if ($bad -gt 0) { "ABORT: archive contains subfolders"; exit 1 }

$tmp = Join-Path $env:TEMP "verify-flat-$ver"
if (Test-Path $tmp) { Remove-Item $tmp -Recurse -Force }
[System.IO.Compression.ZipFile]::ExtractToDirectory($zip, $tmp)
"--- extracted tree ---"
Get-ChildItem $tmp -Recurse | ForEach-Object { "  {0}" -f $_.FullName.Substring($tmp.Length) }
"EXTRACTED manifest version_number: {0}" -f ((Get-Content (Join-Path $tmp 'manifest.json') -Raw | ConvertFrom-Json).version_number)
$ev = [System.Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $tmp 'RepairRequiresMats.dll'))
"EXTRACTED DLL FileVersion={0}  Size={1}" -f $ev.FileVersion, (Get-Item (Join-Path $tmp 'RepairRequiresMats.dll')).Length
"CHANGELOG head: {0}" -f (Get-Content (Join-Path $tmp 'CHANGELOG.md') -TotalCount 1)
Remove-Item $tmp -Recurse -Force
