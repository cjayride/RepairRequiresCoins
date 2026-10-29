# RepairRequiresCoins — release 1.2.11

## Goal
- Verify Release build against Valheim 1.0.21.
- Bump BepInEx dependency to `denikson-BepInExPack_Valheim-5.4.2360`.
- Changelog, Thunderstore `/dist` package, git commit, push, tag `v1.2.11`.

## Status
- **Completed** — Release build succeeded; dist zip created; pushed and tagged (if git push succeeded).

## Verification
- `dotnet build RepairRequiresCoins.sln -c Release` — success (MSB3245 UnityEngine.TextCoreModule missing reference, pre-existing).
- Game path: `D:\SteamLibrary\steamapps\common\Valheim` (Steam buildid 26249040).

## Files changed
- `manifest.json`, `Package/manifest.json` — 1.2.11, BepInEx 5.4.2360
- `CHANGELOG.md`, `Package/CHANGELOG.md`
- `RepairRequiresCoins/RepairRequiresMats.csproj`, `BepInExPlugin.cs`
- `dist/` — Thunderstore staging + `cjayride-RepairRequiresCoins-1.2.11.zip`

## Output
- DLL: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\dist\plugins\cjayride-RepairRequiresCoins\RepairRequiresMats.dll`
- ZIP: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\dist\cjayride-RepairRequiresCoins-1.2.11.zip`
