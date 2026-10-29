# 🪙 RepairRequiresCoins

### Updated for Valheim 1.0

**Make repairs cost Coins.** RepairRequiresCoins adds a configurable coin-based repair economy to Valheim, letting you decide how much it costs to maintain your weapons, armor, tools, and other repairable equipment.

A fork of [aedenthorn's RepairRequiresMats](https://github.com/aedenthorn/ValheimMods/tree/master/RepairRequiresMats), updated and maintained for Valheim 1.0.

**Links:** [Thunderstore](https://thunderstore.io/c/valheim/p/cjayride/RepairRequiresCoins/) · [GitHub](https://github.com/cjayride/RepairRequiresCoins)

## ⚒️ How It Works

- **Repairs cost Coins.** Repairable items require Coins based on the materials used to craft them.
- **Use the correct crafting station.** Items must be repaired at the station type where they were crafted, such as a Workbench for a wooden hammer or a Forge for an iron axe.
- **Vanilla station-level rules apply.** Repairs require the correct station at the recipe's minimum level. Item quality does not increase the station level required to repair it.
- **Customize the economy.** Adjust material exchange rates to match your server's progression and coin drops.
- **Optional material costs.** Choose between coin-only repairs or charging Coins alongside the original repair materials.

## ⚙️ Configuration

The mod uses a single configuration file:

`BepInEx/config/cjayride.RepairRequiresCoins.cfg`

### Material Exchange Rates

The configuration defines the Coin value of materials used to craft items. Repair costs are calculated using the quantities of crafting materials required by the item's recipe and their configured exchange rates.

- More expensive materials can be assigned higher Coin values.
- Set a material's value to `-1` to make that material contribute no repair cost.
- Wood, Stone, and Fine Wood have default values configured for the mod's economy.
- You may need to adjust exchange rates to suit your server's economy and Coin drop rates.

The default values were designed for the [cjaycraft modpack](https://valheim.thunderstore.io/package/cjayride/cjaycraft_ultimate_modpack/).

### Coin-Only Repairs

The setting `CoinOnly` controls whether repairs consume only Coins or also require the original crafting materials.

| Setting | Behavior |
|---|---|
| `CoinOnly = true` | Repairs consume Coins only. **Default.** |
| `CoinOnly = false` | Repairs consume Coins and the original repair materials. |

When material costs are enabled, the required materials are shown in the repair tooltip.

## 🧩 Optional Mod Compatibility

These integrations are optional:

- **[EpicLoot](https://thunderstore.io/c/valheim/p/RandyKnapp/EpicLoot/)** — Includes additional enchantment-related material costs in the Coin calculation for magic items when EpicLoot is installed.
- **[Swmarly Valheim QOL](https://thunderstore.io/c/valheim/p/Swmarly/SwmarlyValheimQOL/)** — Repairs can use Coins stored in its Coin Purse.
- **[Azumatt CurrencyPocket](https://valheim.hexium.gg/mods/Azumatt/CurrencyPocket)** — Repairs can also spend Coins stored in CurrencyPocket.
- **[SeneaL UI](https://thunderstore.io/c/valheim/p/seneaL/SeneaL_UI/)** — The repair tooltip can display this mod's repair costs, including each item's icon.

When supported coin storage mods are installed, repair payments use Coins in your inventory first, followed by Coins available in the supported purse or pocket.

## 📋 Repair Tooltip

Hover over the repair button to view worn items at the current crafting station and their repair costs.

The tooltip uses color-coded information to make costs easy to read:

| Display | Meaning |
|---|---|
| White item name | Item that needs repair |
| **Green `Free`** | No Coin cost |
| Green material amount | You have enough materials |
| Red material amount | You lack the required materials |
| Yellow Coin amount | You can afford the repair |
| Red Coin amount | You cannot afford the repair |
| White `Coins` label | Currency name |
| Red `Needs Workbench Lvl: 1` or similar | Wrong station or insufficient station level |

Items that cannot be repaired under vanilla rules are excluded from the repair list.

## 🛠️ Build

Build from the project directory:

```bash
dotnet build -c Release
```

## 🙏 Credits

- **Original mod:** [aedenthorn — RepairRequiresMats](https://github.com/aedenthorn/ValheimMods/tree/master/RepairRequiresMats)
- **Fork and Valheim 1.0 update:** cjayride

## 💬 Contact

- **X:** [@cjayride](https://x.com/cjayride)
- **Discord:** [discord.gg/cjayride](https://discord.gg/cjayride) — Find `cjayride` near the top of the user list.
- **Twitch:** [twitch.tv/cjayride](https://twitch.tv/cjayride)

## 🤖 AI Disclosure

The code was **not AI-generated**. AI was used to help verify compatibility with the updated game version.

