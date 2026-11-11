$stage = 'D:\AI-Agent-Projects\cjaycraft\_build_stage\valheim'
$proj = 'D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\RepairRequiresCoins\RepairRequiresMats.csproj'
$log = 'D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\tools\build-1.2.14.log'
dotnet build $proj -c Release -v minimal -p:GamePath=$stage *> $log
"EXIT=$LASTEXITCODE"
Get-Content $log | Select-String -Pattern 'error|warning CS|Build succeeded|Build FAILED' | Select-Object -First 25
$dll = 'D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\RepairRequiresCoins\bin\Release\net48\RepairRequiresMats.dll'
if (Test-Path $dll) {
  [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dll) | ForEach-Object { "FILEVERSION={0}  SIZE={1}  {2}" -f $_.FileVersion, (Get-Item $dll).Length, $_.FileName }
} else { "DLL MISSING" }
$zip = 'D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\RepairRequiresCoins\bin\RepairRequiresMats.zip'
if (Test-Path $zip) { Remove-Item $zip -Force; "removed internal bin zip" }
$stagePlugin = Join-Path $stage 'BepInEx\plugins'
if (Test-Path $stagePlugin) { Get-ChildItem $stagePlugin -Recurse -File | ForEach-Object { "stagecopy: {0}" -f $_.FullName } }
