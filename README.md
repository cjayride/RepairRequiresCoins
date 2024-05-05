# RepairRequiresCoins

Patched for Valheim v0.217.46+

A fork of [aedenthorn/RepairRequiresMats](https://github.com/aedenthorn/ValheimMods/tree/master/RepairRequiresMats) 

[ThunderStore](https://thunderstore.io/c/valheim/p/cjayride/RepairRequiresCoins) | [GitHub](https://github.com/cjayride/RepairRequiresCoins)

# Repair with coins!

- Anything with durability that can be repaired (armor, weapons, etc) now requires Coins to repair it.

# Setup

- There is only 1 config file: *cjayride.RepairRequiresCoins.cfg*

- The config file contains values for materials used to build items. 

- The more expensive the material, the more costly in Coins to repair an item. A value of -1 means no cost to repair that material of an item. It works by counting the number of materials necessary to create the item, and then multiplies that by the value set in item_values.json. This gives the Coin cost to repair.

- You may need to adjust the values to match the economy and Coin drops of your server.

- The default values were configured to match the coin economy of a cjaycraft modpack server https://valheim.thunderstore.io/package/cjayride/cjaycraft_ultimate_modpack/

- This mod requires EpicLoot. (see next section)

# Important EpicLoot Configuration

I'm very sorry I couldn't get this to work out-of-the-box. I'm NOT an experienced programmer and this was the only way I could figure it out.

So the problem was, on dedicated servers the recipes.json file in the EpicLoot plugins folder overwrites any custom recipes you might have loaded from the CustomRecipes mod.

To overcome this, you need to directly edit the recipes.json file for EpicLoot in your game.

The file is: BepInEx/plugins/RandyKnapp-EpicLoot/recipes.json

Near the top is a recipe for GoldRubyRing

    {
      "name": "Recipe_GoldRubyRing",
      "item": "GoldRubyRing",
      "craftingStation": "forge",
     "resources": [
	{ "item": "Coins", "amount": 100 },
        { "item": "Bronze", "amount": 4 },
        { "item": "Ruby", "amount": 2 }
      ]
    },

You just need to make sure that "Coins" are a recipe part for GoldRubyRing -- it can be any amount.

Again, sorry for the weird workaround!

# Contact
- Twitter: twitter.com/cjayride

- Discord: discord.gg/cjayride (find me at the top of the user list) "cjayride"

- Twitch: twitch.tv/cjayride


