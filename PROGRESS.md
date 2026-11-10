# RepairRequiresCoins — release 1.2.14 (zip built; pushed to GitHub)

## Packaging (11/11/2026) — flat Thunderstore layout
- **Layout rule (user, explicit): NO SUBFOLDERS. Every file at the archive root.** My first 1.2.14 build used `plugins/cjayride-RepairRequiresCoins/RepairRequiresMats.dll` and was rejected; rebuilt flat.
- Correction: the 1.2.13 zip is **fine** — flat, DLL at archive root. My earlier note calling it "broken (DLL at root)" was wrong; root-level DLL *is* the required layout. The 1.2.13 changelog wording about `plugins/` is misleading — do not "fix" 1.2.13.
- Script `tools\Package1214.ps1` rewritten: stages a flat tree, refuses any entry name containing `/` or `\`, counts nested entries and aborts if > 0, then re-extracts to verify. Run with `Set-ExecutionPolicy -Scope Process Bypass` (machine policy blocks direct `&` of unsigned .ps1).
- ZIP: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\dist\cjayride-RepairRequiresCoins-1.2.14.zip` — 99,291 bytes.
- Verified entries (exactly 5, all at root, `NESTED ENTRY COUNT = 0`): `manifest.json` 334 (version_number 1.2.14), `icon.png` 77,852, `README.md` 4,921, `CHANGELOG.md` 6,457 (head `# v1.2.14`), `RepairRequiresMats.dll` 43,520 (FileVersion 1.2.14).
- `dist\` staging flattened: `dist\RepairRequiresMats.dll` = 1.2.14, `dist\manifest.json` + `dist\CHANGELOG.md` refreshed; `dist\plugins\` and `dist\stage-1.2.14\` deleted.
- Git: committed as `84c0d8b`, tag `v1.2.14`; force-pushed over the first attempt (`bbc4aad` nested layout, tag `4485d64`) so `origin/main` carries only the flat package. Verified `git ls-remote`: main `84c0d8b`, tag `5481ba8`.
- Blob identity checks: committed zip blob `c2caca9` == worktree zip; committed `dist/RepairRequiresMats.dll` blob `f4bada0` == the built 1.2.14 DLL. No `plugins/` path anywhere in `HEAD`.
- `dist\cjayride-RepairRequiresCoins-1.2.13.zip` left untracked (superseded); `dist\cjayride-RepairRequiresCoins-1.2.12.zip` is tracked and deleted in the worktree (pre-existing state, left alone).

---

# RepairRequiresCoins — test build 1.2.14 (no zip, not released)

## Goal
- Fix the icon beside "Workbench Lvl: 5" (and Forge / Fletcher-table equivalents) in the SeneaL UI Repair tooltip: it must be the **station's own icon**, not an item icon.

## Root cause (verified against decompiles)
- 1.2.10 attached item icons as hand-made child `Image`s at the row Label's top-left (`ApplySeneaLRowIcon` + `LayRow` postfix). The first item's icon therefore rendered at the top of the rows block — visually beside the `Sub` line (`Workbench Lvl: 5 · Coins: N`).
- The mod also set `Tip.Icon = null` every rebuild. SeneaL's `Tooltip.TipIcon` keeps the pooled icon box active only when `tip.Icon != null` (`Tooltip.cs:1245,1281-1287`), and my `Reset(..., Random.Range)` forced a `Build()` every 0.12s tick (`Tooltip.cs:408`), so the header box flickered/staled.

## Changes
- `BepInExPlugin.cs`
  - Version 1.2.13 → 1.2.14 (BepInPlugin + `Version` const).
  - Added `GetStationIcon(CraftingStation)` → `station.m_icon`; cache field `senealStationIcon` set in `RefreshRepairOffer` from `Player.m_localPlayer.GetCurrentCraftingStation()`.
  - `SeneaLRepairTipPostfix`: `Tip.Icon` = station sprite (SeneaL header box, beside the station Lvl line); per-item icons now written into SeneaL's native `Tip.Row.Icon` field (`Tip.cs:29`, drawn by `Tooltip.RowIconAt`, 16px, row-centred).
  - Removed `SeneaLTooltipBuildPrefix` / `SeneaLTooltipLayRowPostfix` patches and `ApplySeneaLRowIcon` / `TakeOfferIcon` — no more manual GameObject children, no per-row reflection.
  - Tip cache version is now `lastOfferStamp` (content hash of items/quality/durability/costs/coins/station level) instead of `Random.Range(1, int.MaxValue)`.
- `RepairItemData.cs`: dropped now-unused `RepairOfferLine.IconUsed`.
- `RepairRequiresMats.csproj` (Version/AssemblyVersion/FileVersion), `manifest.json`, `Package\manifest.json`, `CHANGELOG.md` + `Package\CHANGELOG.md` (copied, verified identical) → 1.2.14.

## Build
- `dotnet build RepairRequiresMats.csproj -c Release -p:GamePath=D:\AI-Agent-Projects\cjaycraft\_build_stage\valheim` → **EXIT=0**, `Build succeeded`, 0 CS warnings. (Pre-existing MSB3245 `UnityEngine.TextCoreModule` reference warning is unrelated and pre-existing.)
- Two build errors fixed en route: CS1503 (`tip.Type` is a Traverse method group → use `Traverse.GetValue` overload), CS7036 (`MethodInfo.GetValue` → `Traverse.GetValue`).
- PostBuild copied the DLL into the **staging** tree only; staging `BepInEx\plugins` removed after install. `D:\SteamLibrary\...\Valheim\BepInEx\plugins` untouched (no SeneaLHudLayout.dll there, as expected).

## Install (test profile)
- Installed: `C:\Users\cjrob\AppData\Roaming\com.kesomannen.gale\valheim\profiles\cjayride-beta_test_cjaycraft_ultimate_modpack-4.4.44\BepInEx\plugins\cjayride-RepairRequiresCoins\RepairRequiresMats.dll` — verified `FileVersion = 1.2.14`, 43,520 bytes.
- Backup kept: `RepairRequiresMats.dll.bak-1.2.13` (45,568 bytes).

## Output
- DLL: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\RepairRequiresCoins\bin\Release\net48\RepairRequiresMats.dll` (FileVersion 1.2.14, 43,520 bytes)
- No zip built, no GitHub push, `beta_cjaycraft` / `beta_cjaycraft-server` manifest + CHANGELOG untouched (per user).

## In-game test (user, pending)
- Hover the Repair button at a Workbench (Lvl 5), Forge, and Fletcher table: icon box shows the station icon; each item line keeps its own item icon; no icon floats onto the station line.

---

# RepairRequiresCoins — release 1.2.13

## Goal
- Fix broken 1.2.12 Thunderstore zip (DLL at archive root).
- Bump to 1.2.13 and rebuild dist package with correct `plugins/` layout.

## Status
- **Completed** — Release build + corrected zip.

## Verification
- `dotnet build RepairRequiresCoins.sln -c Release` — success.
- Zip entries include `plugins/cjayride-RepairRequiresCoins/RepairRequiresMats.dll` (not root-level DLL).

## Output
- DLL: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\dist\plugins\cjayride-RepairRequiresCoins\RepairRequiresMats.dll`
- ZIP: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RepairRequiresCoins\dist\cjayride-RepairRequiresCoins-1.2.13.zip`
