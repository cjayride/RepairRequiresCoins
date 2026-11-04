# RepairRequiresCoins — release 1.2.12

## Goal
- Confirm the mod compiles and matches Valheim 1.0.21 APIs.
- Cut per-frame repair-UI work so inventory open time has less hitching.

## Status
- **Completed** — Release build succeeded; Thunderstore zip in `/dist`.

## Verification
- Installed Valheim: Steam `buildid` 26249040, `valheim.exe` ProductVersion `6000.0.75f1`. Latest public patch is 1.0.21 (28 Oct 2026).
- `Inventory.CountItems(string, int, bool)` and `Inventory.RemoveItem(string, int, int, bool)` exist on this assembly.
- `dotnet build RepairRequiresCoins.sln -c Release` — success (MSB3245 UnityEngine.TextCoreModule missing reference, pre-existing).
- In-game FPS was not measured in this session (compile + API check only).

## Files changed
- `RepairRequiresCoins/BepInExPlugin.cs` — cache recipes/rates/tooltip/station access; skip full offer rebuild when inventory/durability/coins/station unchanged; CountItems/RemoveItem instead of reflection.
- `RepairRequiresCoins/RepairItemData.cs` — no longer subclasses `ItemDrop.ItemData`.
- Version 1.2.12: csproj, plugin, manifests, changelogs.

## Output
- DLL: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\RepairRequiresCoins\bin\Release\net48\RepairRequiresMats.dll`
- Dist DLL: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\dist\plugins\cjayride-RepairRequiresCoins\RepairRequiresMats.dll`
- ZIP: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\dist\cjayride-RepairRequiresCoins-1.2.12.zip`
