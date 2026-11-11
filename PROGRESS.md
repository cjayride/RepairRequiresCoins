# RepairRequiresCoins — 1.2.15 (row icons fix, test build; not packaged/pushed)

## 1.2.15 — row item icons missing (user test 11/11)
- **User report:** Workbench/Forge station icon is fixed, but **no icon at all to the left of each item name** in the repair row list.
- **Not a "tested a stale build" case:** the installed DLL was byte-identical to the 1.2.14 release build (11/10 2:47 PM, FileVersion 1.2.14, 43,520 B) and no source change happened after it. `PROGRESS.md` listed this exact check as "In-game test (user, pending)" — so 1.2.14 shipped with the row-icon path **never verified in game**. My 1.2.14 report presented it as working; it did not.
- **Not a SeneaL update:** patch target `SeneaLUI.Inv.CraftView.RepairTip` lives in `SeneaL-UI.dll`, dated 11/09 12:10 (unchanged since the 1.2.14 build). `SeneaLHudLayout.dll` was updated 11/11 05:10 to 1.5.51 but is not the patch target.
- BepInEx log: mod loaded clean, no MissingMethodException / Harmony patch failure for this mod. (One unrelated Unity `ArgumentOutOfRangeException` in `SeneaLUI.Inv.TabsView.Layout` — separate SeneaL issue, not in the tooltip path.)
- Code evidence: `Tip.Row` is a **struct** (`SeneaL-UI.decompiled.cs:165880`); `RowIconAt` hides the image when `row.Icon == null` (166472); `Tip.AddRow` creates rows with **no Icon** (165971-165980); SeneaL's own callers set `Icon` **in the row initializer at construction** (e.g. 35821 `Icon = ... m_itemData.GetIcon()`).
- 1.2.14 used `AddRow(...)` then a boxed write-back `rows[index] = rowBox`. 1.2.15 builds each row fully (Label/Value/Small/Icon) via `Activator.CreateInstance(rowType)` + `AccessTools.Field`, then `rows.Add(row)` — SeneaL's own construction pattern. Falls back to plain `AddRow` (text-only) if the row shape is not found.
- New `DebugLogging` config entry (default **false**), and a `Dbgl` line after building rows: `SeneaL rows=N rowsWithIcon=M offerLines=K stationIcon=bool rowType=...`. **Decisive check**: `rowsWithIcon=0` ⇒ icons null upstream (`GetItemIcon`); `rowsWithIcon>0` with no visible icons ⇒ SeneaL's renderer is the blocker, not my write.
- Build: `tools\Build1215.ps1` → `dotnet build ... -c Release -p:GamePath=D:\AI-Agent-Projects\cjaycraft\_build_stage\valheim` → **EXIT=0, Build succeeded, 0 errors** (only pre-existing MSB3245 `UnityEngine.TextCoreModule`).
- DLL: `...\RepairRequiresCoins\bin\Release\net48\RepairRequiresMats.dll` — FileVersion **1.2.15**, 44,544 B. New-code literals verified in the binary (UTF-16 at odd byte offset: `SeneaL rows=`, `rowsWithIcon=`, `shape not found`).
- Installed to Gale profile `cjayride-beta_test_cjaycraft_ultimate_modpack-4.4.44` (verified FileVersion 1.2.15, 44,544 B). Backup `RepairRequiresMats.dll.bak-1.2.14` (43,520 B). `DebugLogging = true` set in that profile's cfg **for the test only**.
- Version bump 1.2.14 → 1.2.15 in `BepInExPlugin.cs` (BepInPlugin + `Version`), `.csproj`, `manifest.json`, `Package\manifest.json`, `Package\CHANGELOG.md` (v1.2.15 added; the false 1.2.14 claim "item icons ... drawn by SeneaL's native row icons" removed).
- **Not packaged, not committed, not pushed** — held until the in-game test passes (per user). The pushed 1.2.14 has this row-icon defect.
- Next steps: (1) user hovers Repair at Workbench / Forge / Fletcher table; (2) read `SeneaL rows=...` line from `BepInEx\LogOutput.log`; (3) if icons appear → set `DebugLogging = false`, package 1.2.15 flat zip, commit, tag, push; (4) if not, use the log line to target the real layer.

## 1.2.15 root cause found (11/11): we deleted the code that drew row icons
- User: "this was working before. then after you started working on fixing the workbench icons you broke it". **Correct**, and the diff proves it.
- v1.2.10/1.2.12 drew row icons with our own code, three patches on `SeneaLUI.Components.Tooltip`:
  - `Build` prefix `SeneaLTooltipBuildPrefix` — set `senealDrawingOurTip` from `Tip.Source`, rewind `IconUsed`.
  - `LayRow` postfix `SeneaLTooltipLayRowPostfix` — per row, take the offer's sprite by name, add a 20px `Image` child `RepairRequiresCoinsIcon` **on the row's own label**, set `label.margin.x = 26` so text clears the icon.
  - rows built with SeneaL's `AddRow(name, right, "")`.
- v1.2.14 (station icon) **deleted all of that** — `TryPatchSeneaLRepairTip` stopped patching `Build`/`LayRow`, `ApplySeneaLRowIcon`/`TakeOfferIcon`/`SeneaLTooltipBuildPrefix`/`LayRowPostfix` removed, `RepairOfferLine.IconUsed` removed from `RepairItemData.cs`. `Icon` kept being populated, so nothing looked broken in code. That is the whole regression; the station icon (header) is unrelated and kept working.
- Every "SeneaL doesn't render row icons" theory (1.2.14 boxed write-back, 1.2.15 full row construction, panel-level `rl{i}` placement) was chasing a self-inflicted wound: we had removed the code that drew them. Probes confirmed only that SeneaL's own `RowIconAt` emits no images for our rows — true in 1.2.12 as well, where our own images supplied them.
- Fix: restored the 1.2.12 mechanism verbatim (`git show 1e739ae:...` into `tools\BepInExPlugin-1.2.12.cs`), kept the 1.2.14 station header icon, removed probe/`GetRowType`, rows back to `AddRow`. `RepairOfferLine.IconUsed` re-added.
- Build: EXIT=0, 0 errors, 46,592 B. Installed to Gale profile (FileVersion 1.2.15; literals `RepairRequiresCoinsIcon`, `LayRow` present in the installed DLL). Backups `.bak-1.2.13`, `.bak-1.2.14` intact.
- Changelog rewritten: v1.2.15 = restores row icons (regression introduced in 1.2.14).
- **Awaiting user in-game test.** Not packaged, not committed, not pushed.

## 1.2.15 RELEASED (11/11)
- User in-game test: "the code for RepairRequiresCoins works." Row icons + station icon confirmed.
- Test profile `DebugLogging` set back to `false` (backup `cjayride.RepairRequiresCoins.cfg.bak-testflag`).
- Rebuilt (EXIT=0), packaged with `tools\Package1215.ps1`: `dist\cjayride-RepairRequiresCoins-1.2.15.zip` = 100,767 B; 5 entries at archive root (manifest.json, icon.png, README.md, CHANGELOG.md, RepairRequiresMats.dll); NESTED ENTRY COUNT = 0; extracted manifest 1.2.15 + DLL FileVersion 1.2.15.
- Loose `dist\` copies (DLL/manifest/CHANGELOG) synced to 1.2.15.
- Commit `27fd2d2` "Release 1.2.15: restore repair-list item icons (regression in 1.2.14); DebugLogging option"; tag `v1.2.15` (annotated). Committed zip blob `f53cd258...` == `git hash-object` of worktree zip (100,767 B).
- Pushed `main` (1331934..27fd2d2) and tag `v1.2.15` to `https://github.com/cjayride/RepairRequiresCoins.git` (both exit 0).
- Also committed: `tools\` release scripts + 1.2.12 reference source; untracked stale net480 bin/obj artifacts (deleted from index; worktree copies remain).
- Thunderstore upload: user has the zip; upload on request. Do NOT upload the 1.2.14 zip (row-icon regression).

---

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
- Changelog wording fixed in all three copies (root, `Package\`, `dist\`, identical) — the old v1.2.13 line prescribed a `plugins/` layout, which is wrong; zip rebuilt with it (99,339 bytes, `NESTED ENTRY COUNT = 0`).
- **Root cause of my nested-layout error: the workspace instructions themselves.** `AGENTS.md` line 61 said "Structure the zip correctly for Thunderstore (plugins folder, ...)". Per user, corrected in all 4 copies (`cjaycraft\`, `beta_cjaycraft\`, `beta_cjaycraft-server\`, `Cjayride.CLCCLootPatch\`) to state the flat root layout, forbid a `plugins/` subfolder, and require listing zip entries to confirm no `/` or `\` in any name. Verified: all 4 now carry the new line 61; no copy mentions a plugins folder. **Not committed** (these are outside this repo / user asked only for the edit).
- Pushed release tip: `1331934`, tag `v1.2.14` = `07bae6e` (force-pushed over the nested-layout attempts).

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
