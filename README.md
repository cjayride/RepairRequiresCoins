# RepairRequiresCoins

Updated for Valheim 1.0

A fork of [aedenthorn/RepairRequiresMats](https://github.com/aedenthorn/ValheimMods/tree/master/RepairRequiresMats)

[ThunderStore](https://thunderstore.io/c/valheim/p/cjayride/RepairRequiresCoins) | [GitHub](https://github.com/cjayride/RepairRequiresCoins)

# Repair with coins!

- Anything with durability that can be repaired (armor, weapons, etc) now requires Coins to repair it.
- Items still repair at the station they were crafted at (for example a wooden hammer at a workbench, an iron axe at a forge).

# Setup

- There is only 1 config file: *cjayride.RepairRequiresCoins.cfg*

- The config file contains values for materials used to build items.

- The more expensive the material, the more costly in Coins to repair an item. A value of -1 means no cost to repair that material of an item. It works by counting the number of materials necessary to create the item, and then multiplies that by the configured exchange rate. This gives the Coin cost to repair.

- You may need to adjust the values to match the economy and Coin drops of your server.

- The default values were configured to match the coin economy of a cjaycraft modpack server https://valheim.thunderstore.io/package/cjayride/cjaycraft_ultimate_modpack/

- EpicLoot is optional. If it is installed, magic items include extra enchant-cost materials in the coin calculation.

# Contact
- Twitter: twitter.com/cjayride

- Discord: discord.gg/cjayride (find me at the top of the user list) "cjayride"

- Twitch: twitch.tv/cjayride



