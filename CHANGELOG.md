# v1.1.0
- Removed terminal reload
- Added compatibility for v0.217.46

# v1.0.2 - 2024/4/7
- Passed initial test seems to work. Just remember, an item can only be repaired by the same type of workbench it was created.
- No longer requires the depreciated mod: Extended Item Data Framework

# v1.0.1 - 2024/4/7
- Fixed possible issue with missing assembly files

# v1.0.0 - 2024/4/7
- Moved all configuration to one file: cjayride.RepairRequiresMats-Coins-CJAYCRAFT-fix.cfg

# v0.8.1 - 2024/4/7
- Re-uploaded because package was denied on Thunderstore. No changes to files.

# v0.8.0 - 2024/4/7
- Changed the method in code for text transformation to GetComponent<TMP_Text>() on lines 170 - 172. Committed by "aedenthorn" on Oct 6, 2023.

# v0.7.2 - 2023/7/13
- Added Newtonsoft.Json.dll to the plugins folder, seems to be required for mod to work.

# v0.7.1 - 2023/7/13
- Removed the dependency RandyKnapp-ExtendedItemDataFramework-1.0.11 because it is depreciated (breaks everything) and I believe is no longer needed.

# v0.7.0 - 2023/7/10
- MISTLANDS UPDATE
- Updated dependencies list
- Configuration now allows for easier placement of configuration file. It can be placed anywhere in the BepInEx folder that you specify.
- See cjayride.RepairREquiresMats-Coins-CJAYCRAFT-fix.cfg

# v0.6.4 - 2022/2/23
- Removed the RecipeCusomization dependency and file GoldRubyRing.json because it turns out that this workaround doesn't work on dedicated servers, only on client player run games. So in order to get this to work, you're now required to make sure that the recipes.json file in the EpicLoog plugins folder includes a GoldRubyRing with Coins as a required recipe part like follows:

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

# v0.6.3 - 2022/2/18
- Forgot to include RecipeCustomization mod as dependecy. It's actually not required though so long as you have EpicLoot installed AND your EpicLoot recipe.json file includes a recipe for "Recipe_GoldRubyRing" with Coins as a required resource to craft it. Some servers may want to change the recipe.

# v0.6.2 - 2022/2/18
- Fixed critical bug with dependencies. It been difficult getting this mod on Thunderstore from my community server because I didn't know it relied on so many things.

# v0.6.1 - 2022/2/17
- Removed test code mod config enforcer, because it was incomplete and prohibiting the mod from running at all.

# v0.6.0 - 2022/2/16
- First upload to Thunderstore