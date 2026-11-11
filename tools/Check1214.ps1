$log = 'D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\tools\build-1.2.14.log'
(Get-Content $log | Select-String -Pattern 'warning CS' | Measure-Object).Count
(Get-Content $log | Select-String -Pattern 'warning CS') | ForEach-Object { $_.Line.Trim() } | Select-Object -First 10
"--- leftovers in source ---"
Get-Content 'D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\RepairRequiresCoins\BepInExPlugin.cs' | Select-String -Pattern 'senealDrawingOurTip|IconUsed|ApplySeneaLRowIcon|TakeOfferIcon|tip\.Type' | ForEach-Object { "L{0}: {1}" -f $_.LineNumber, $_.Line.Trim() }
"(none above = clean)"
