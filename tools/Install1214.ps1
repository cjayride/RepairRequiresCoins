$src = 'D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\RepairRequiresCoins\bin\Release\net48\RepairRequiresMats.dll'
$dir = 'C:\Users\cjrob\AppData\Roaming\com.kesomannen.gale\valheim\profiles\cjayride-beta_test_cjaycraft_ultimate_modpack-4.4.44\BepInEx\plugins\cjayride-RepairRequiresCoins'
$dst = Join-Path $dir 'RepairRequiresMats.dll'
if (-not (Test-Path $dst)) { "TARGET MISSING: $dst"; exit 1 }
$cur = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dst).FileVersion
"installed now = $cur"
$copy = "$dst.bak-$cur"
if (-not (Test-Path $copy)) { Copy-Item $dst $copy; "backup -> $copy" }
Copy-Item $src $dst -Force
$vi = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dst)
"AFTER = {0}  SIZE={1}  {2}" -f $vi.FileVersion, (Get-Item $dst).Length, $dst
"--- dir listing ---"
Get-ChildItem $dir | ForEach-Object { "{0}  {1}" -f $_.Name, $_.Length }
"--- stage plugins cleanup ---"
$stagePlugins = 'D:\AI-Agent-Projects\cjaycraft\_build_stage\valheim\BepInEx\plugins'
if (Test-Path $stagePlugins) { Remove-Item $stagePlugins -Recurse -Force; "removed stage plugins dir" }
Test-Path $stagePlugins
"--- game install untouched ---"
[System.Diagnostics.FileVersionInfo]::GetVersionInfo('D:\SteamLibrary\steamapps\common\Valheim\BepInEx\plugins\cjayride-SeneaLHudLayout\SeneaLHudLayout.dll').FileVersion
