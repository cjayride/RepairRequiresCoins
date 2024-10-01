using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using TMPro;

namespace RepairRequiresMats {
    [BepInPlugin("cjayride.RepairRequiresCoins", "Repair Requires Coins", "1.1.1")]
    public class BepInExPlugin : BaseUnityPlugin {
        public const string Version = "1.1.1";
        public const string ModName = "Repair Requires Coins";

        private static bool isDebug = true;

        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> showAllRepairsInToolTip;
        public static ConfigEntry<float> materialRequirementMult;
        public static ConfigEntry<string> titleTooltipColor;
        public static ConfigEntry<string> hasEnoughTooltipColor;
        public static ConfigEntry<string> notEnoughTooltipColor;
        private static List<ItemDrop.ItemData> orderedWornItems = new List<ItemDrop.ItemData>();

        private static BepInExPlugin context;

        private static Assembly epicLootAssembly;
        private static MethodInfo epicLootIsMagic;
        private static MethodInfo epicLootGetRarity;
        private static MethodInfo epicLootGetEnchantCosts;

        // added by cjayride
        public static WeaponAndArmorValues savedValues;
        public static ConfigEntry<bool> coinOnly;

        // item values
        public static ConfigEntry<float> BlackMetalScrap;
        public static ConfigEntry<float> BlackMetal;
        public static ConfigEntry<float> Bronze;
        public static ConfigEntry<float> Chain;
        public static ConfigEntry<float> Chitin;
        public static ConfigEntry<float> Copper;
        public static ConfigEntry<float> CopperOre;
        public static ConfigEntry<float> Flametal;
        public static ConfigEntry<float> FlametalOre;
        public static ConfigEntry<float> Frometal;
        public static ConfigEntry<float> FrometalOre;
        public static ConfigEntry<float> FrostinfusedDarkmetal;
        public static ConfigEntry<float> HeatedIron;
        public static ConfigEntry<float> Heavymetal;
        public static ConfigEntry<float> HeavymetalOre;
        public static ConfigEntry<float> Heavyscale;
        public static ConfigEntry<float> Iron;
        public static ConfigEntry<float> ScrapIron;
        public static ConfigEntry<float> LeatherScraps;
        public static ConfigEntry<float> PrimordialIce;
        public static ConfigEntry<float> Silver;
        public static ConfigEntry<float> SilverOre;
        public static ConfigEntry<float> Tin;
        public static ConfigEntry<float> TinOre;
        public static ConfigEntry<float> DeerHide;
        public static ConfigEntry<float> TrollHide;
        public static ConfigEntry<float> WolfPelt;
        public static ConfigEntry<float> LoxPelt;
        public static ConfigEntry<float> WitheredBone;
        public static ConfigEntry<float> BoneFragments;
        public static ConfigEntry<float> LinenThread;
        public static ConfigEntry<float> ElderBark;
        public static ConfigEntry<float> Obsidian;
        public static ConfigEntry<float> FreezeGland;
        public static ConfigEntry<float> Crystal;
        public static ConfigEntry<float> YimirRemains;
        public static ConfigEntry<float> HardAntler;
        public static ConfigEntry<float> SalamanderFurTH;
        public static ConfigEntry<float> WolfFang;
        public static ConfigEntry<float> Root;
        public static ConfigEntry<float> Flint;
        public static ConfigEntry<float> Needle;
        public static ConfigEntry<float> Wood;
        public static ConfigEntry<float> RoundLog;
        public static ConfigEntry<float> SerpentScale;
        public static ConfigEntry<float> WorldTreeFragment;
        public static ConfigEntry<float> BurningWorldTreeFragment;
        public static ConfigEntry<float> Stone;

        public static void Dbgl(string str = "", bool pref = true) {
            if (isDebug)
                Debug.Log((pref ? typeof(BepInExPlugin).Namespace + " " : "") + str);
        }
        private void Awake() {
            context = this;
            modEnabled = Config.Bind<bool>("General", "Enabled", true, "Enable this mod");
            showAllRepairsInToolTip = Config.Bind<bool>("General", "ShowAllRepairsInToolTip", true, "Show all repairs in tooltip when hovering over repair button.");
            titleTooltipColor = Config.Bind<string>("General", "TitleTooltipColor", "FFFFFFFF", "Color to use in tooltip title.");
            hasEnoughTooltipColor = Config.Bind<string>("General", "HasEnoughTooltipColor", "FFFFFFFF", "Color to use in tooltip for items with enough resources to repair.");
            notEnoughTooltipColor = Config.Bind<string>("General", "NotEnoughTooltipColor", "FF0000FF", "Color to use in tooltip for items with enough resources to repair.");
            materialRequirementMult = Config.Bind<float>("General", "MaterialRequirementMult", 0.5f, "Multiplier for amount of each material required.");

            // added by cjayride
            coinOnly = Config.Bind<bool>("General", "CoinOnly", true, "Repair only using coins.");

            // item values
            savedValues = new WeaponAndArmorValues();
            BlackMetalScrap = Config.Bind<float>("Item Values", "BlackMetalScrap", 5, "BlackMetalScrap exchange rate");
            BlackMetal = Config.Bind<float>("Item Values", "BlackMetal", 5, "BlackMetal exchange rate");
            Bronze = Config.Bind<float>("Item Values", "Bronze", 2, "Bronze exchange rate");
            Chain = Config.Bind<float>("Item Values", "Chain", 2, "Chain exchange rate");
            Chitin = Config.Bind<float>("Item Values", "Chitin", 2, "Chitin exchange rate");
            Copper = Config.Bind<float>("Item Values", "Copper", 1, "Copper exchange rate");
            CopperOre = Config.Bind<float>("Item Values", "CopperOre", 1, "CopperOre exchange rate");
            Flametal = Config.Bind<float>("Item Values", "Flametal", 21, "Flametal exchange rate");
            FlametalOre = Config.Bind<float>("Item Values", "FlametalOre", 21, "FlametalOre exchange rate");
            Frometal = Config.Bind<float>("Item Values", "Frometal", 13, "Frometal exchange rate");
            FrometalOre = Config.Bind<float>("Item Values", "FrometalOre", 13, "FrometalOre exchange rate");
            FrostinfusedDarkmetal = Config.Bind<float>("Item Values", "FrostinfusedDarkmetal", 85, "FrostinfusedDarkmetal exchange rate");
            HeatedIron = Config.Bind<float>("Item Values", "HeatedIron", 2, "HeatedIron exchange rate");
            Heavymetal = Config.Bind<float>("Item Values", "Heavymetal", 8, "Heavymetal exchange rate");
            HeavymetalOre = Config.Bind<float>("Item Values", "HeavymetalOre", 8, "HeavymetalOre exchange rate");
            Heavyscale = Config.Bind<float>("Item Values", "Heavyscale", 8, "Heavyscale exchange rate");
            Iron = Config.Bind<float>("Item Values", "Iron", 2, "Iron exchange rate");
            ScrapIron = Config.Bind<float>("Item Values", "ScrapIron", 2, "ScrapIron exchange rate");
            LeatherScraps = Config.Bind<float>("Item Values", "LeatherScraps", 1, "LeatherScraps exchange rate");
            PrimordialIce = Config.Bind<float>("Item Values", "PrimordialIce", 13, "PrimordialIce exchange rate");
            Silver = Config.Bind<float>("Item Values", "Silver", 2, "Silver exchange rate");
            SilverOre = Config.Bind<float>("Item Values", "SilverOre", 2, "SilverOre exchange rate");
            Tin = Config.Bind<float>("Item Values", "Tin", 1, "Tin exchange rate");
            TinOre = Config.Bind<float>("Item Values", "TinOre", 1, "TinOre exchange rate");
            DeerHide = Config.Bind<float>("Item Values", "DeerHide", 1, "DeerHide exchange rate");
            TrollHide = Config.Bind<float>("Item Values", "TrollHide", 2, "TrollHide exchange rate");
            WolfPelt = Config.Bind<float>("Item Values", "WolfPelt", 3, "WolfPelt exchange rate");
            LoxPelt = Config.Bind<float>("Item Values", "LoxPelt", 5, "LoxPelt exchange rate");
            WitheredBone = Config.Bind<float>("Item Values", "WitheredBone", 3, "WitheredBone exchange rate");
            BoneFragments = Config.Bind<float>("Item Values", "BoneFragments", 1, "BoneFragments exchange rate");
            LinenThread = Config.Bind<float>("Item Values", "LinenThread", 5, "LinenThread exchange rate");
            ElderBark = Config.Bind<float>("Item Values", "ElderBark", 1, "ElderBark exchange rate");
            Obsidian = Config.Bind<float>("Item Values", "Obsidian", 5, "Obsidian exchange rate");
            FreezeGland = Config.Bind<float>("Item Values", "FreezeGland", 1, "FreezeGland exchange rate");
            Crystal = Config.Bind<float>("Item Values", "Crystal", 1, "Crystal exchange rate");
            YimirRemains = Config.Bind<float>("Item Values", "YimirRemains", 3, "YimirRemains exchange rate");
            HardAntler = Config.Bind<float>("Item Values", "HardAntler", 1, "HardAntler exchange rate");
            SalamanderFurTH = Config.Bind<float>("Item Values", "SalamanderFurTH", 9, "SalamanderFurTH exchange rate");
            WolfFang = Config.Bind<float>("Item Values", "WolfFang", 3, "WolfFang exchange rate");
            Root = Config.Bind<float>("Item Values", "Root", 2, "Root exchange rate");
            Flint = Config.Bind<float>("Item Values", "Flint", 1, "Flint exchange rate");
            Needle = Config.Bind<float>("Item Values", "Needle", 5, "Needle exchange rate");
            Wood = Config.Bind<float>("Item Values", "Wood", -1, "Wood exchange rate");
            RoundLog = Config.Bind<float>("Item Values", "RoundLog", 1, "RoundLog exchange rate");
            SerpentScale = Config.Bind<float>("Item Values", "SerpentScale", 3, "SerpentScale exchange rate");
            WorldTreeFragment = Config.Bind<float>("Item Values", "WorldTreeFragment", 8, "WorldTreeFragment exchange rate");
            BurningWorldTreeFragment = Config.Bind<float>("Item Values", "BurningWorldTreeFragment", 13, "BurningWorldTreeFragment exchange rate");
            Stone = Config.Bind<float>("Item Values", "Stone", -1, "Stone exchange rate");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);

            try {

                    savedValues.BlackMetalScrap = BlackMetalScrap.Value;
                    savedValues.BlackMetal = BlackMetal.Value;
                    savedValues.Bronze = Bronze.Value;
                    savedValues.BlackMetal = BlackMetal.Value;
                    savedValues.Bronze = Bronze.Value;
                    savedValues.Chain = Chain.Value;
                    savedValues.Chitin = Chitin.Value;
                    savedValues.Copper = Copper.Value;
                    savedValues.CopperOre = CopperOre.Value;
                    savedValues.Flametal = Flametal.Value;
                    savedValues.FlametalOre = FlametalOre.Value;
                    savedValues.Frometal = Frometal.Value;
                    savedValues.FrometalOre = FrometalOre.Value;
                    savedValues.FrostinfusedDarkmetal = FrostinfusedDarkmetal.Value;
                    savedValues.HeatedIron = HeatedIron.Value;
                    savedValues.Heavymetal = Heavymetal.Value;
                    savedValues.HeavymetalOre = HeavymetalOre.Value;
                    savedValues.Heavyscale = Heavyscale.Value;
                    savedValues.Iron = Iron.Value;
                    savedValues.LeatherScraps = LeatherScraps.Value;
                    savedValues.PrimordialIce = PrimordialIce.Value;
                    savedValues.ScrapIron = ScrapIron.Value;
                    savedValues.Silver = Silver.Value;
                    savedValues.SilverOre = SilverOre.Value;
                    savedValues.Tin = Tin.Value;
                    savedValues.TinOre = TinOre.Value;
                    savedValues.DeerHide = DeerHide.Value;
                    savedValues.TrollHide = TrollHide.Value;
                    savedValues.WolfPelt = WolfPelt.Value;
                    savedValues.LoxPelt = LoxPelt.Value;
                    savedValues.WitheredBone = WitheredBone.Value;
                    savedValues.BoneFragments = BoneFragments.Value;
                    savedValues.LinenThread = LinenThread.Value;
                    savedValues.ElderBark = ElderBark.Value;
                    savedValues.Obsidian = Obsidian.Value;
                    savedValues.FreezeGland = FreezeGland.Value;
                    savedValues.Crystal = Crystal.Value;
                    savedValues.YimirRemains = YimirRemains.Value;
                    savedValues.HardAntler = HardAntler.Value;
                    savedValues.SalamanderFurTH = SalamanderFurTH.Value;
                    savedValues.WolfFang = WolfFang.Value;
                    savedValues.Flint = Flint.Value;
                    savedValues.Needle = Needle.Value;
                    savedValues.Wood = Wood.Value;
                    savedValues.RoundLog = RoundLog.Value;
                    savedValues.SerpentScale = SerpentScale.Value;
                    savedValues.WorldTreeFragment = WorldTreeFragment.Value;
                    savedValues.BurningWorldTreeFragment = BurningWorldTreeFragment.Value;
                    savedValues.Stone = Stone.Value;

                //Dbgl("RRM: Values loaded from config file");
                //Dbgl("RRM: Test value: savedValues.BurningWorldTreeFragment = " + savedValues.BurningWorldTreeFragment.ToString());

            } catch {

                //Dbgl("RRM: Error loading config values for Repair Requires Coins (mats)");
               //Dbgl("RRM: Loading default coin values now...");

                savedValues.BlackMetalScrap = 5;
                savedValues.BlackMetal = 5;
                savedValues.Bronze = 2;
                savedValues.Chain = 2;
                savedValues.Chitin = 2;
                savedValues.Copper = 1;
                savedValues.CopperOre = 1;
                savedValues.Flametal = 21;
                savedValues.FlametalOre = 21;
                savedValues.Frometal = 13;
                savedValues.FrometalOre = 85;
                savedValues.FrostinfusedDarkmetal = 2;
                savedValues.HeatedIron = 8;
                savedValues.Heavymetal = 8;
                savedValues.HeavymetalOre = 8;
                savedValues.Heavyscale = 2;
                savedValues.Iron = 1;
                savedValues.LeatherScraps = 13;
                savedValues.PrimordialIce = 2;
                savedValues.ScrapIron = 2;
                savedValues.Silver = 2;
                savedValues.SilverOre = 2;
                savedValues.Tin = 1;
                savedValues.TinOre = 1;
                savedValues.DeerHide = 1;
                savedValues.TrollHide = 2;
                savedValues.WolfPelt = 3;
                savedValues.LoxPelt = 5;
                savedValues.WitheredBone = 3;
                savedValues.BoneFragments = 1;
                savedValues.LinenThread = 5;
                savedValues.ElderBark = 1;
                savedValues.Obsidian = 5;
                savedValues.FreezeGland = 1;
                savedValues.Crystal = 1;
                savedValues.YimirRemains = 3;
                savedValues.HardAntler = 1;
                savedValues.SalamanderFurTH = 9;
                savedValues.WolfFang = 3;
                savedValues.Root = 2;
                savedValues.Flint = 1;
                savedValues.Needle = 5;
                savedValues.Wood = -1;
                savedValues.RoundLog = 1;
                savedValues.SerpentScale = 3;
                savedValues.WorldTreeFragment = 8;
                savedValues.BurningWorldTreeFragment = 13;
                savedValues.Stone = -1;

            }

        }
        private void Start() {
            if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.epicloot")) {
                epicLootAssembly = Chainloader.PluginInfos["randyknapp.mods.epicloot"].Instance.GetType().Assembly;
                epicLootIsMagic = epicLootAssembly.GetType("EpicLoot.ItemDataExtensions").GetMethod("IsMagic", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(ItemDrop.ItemData) }, null);
                epicLootGetRarity = epicLootAssembly.GetType("EpicLoot.ItemDataExtensions").GetMethod("GetRarity", BindingFlags.Public | BindingFlags.Static);
                epicLootGetEnchantCosts = epicLootAssembly.GetType("EpicLoot.Crafting.EnchantHelper").GetMethod("GetEnchantCosts", BindingFlags.Public | BindingFlags.Static);
                Dbgl($"Loaded Epic Loot assembly; epicLootIsMagic {epicLootIsMagic != null}, epicLootGetRarity {epicLootGetRarity != null}, epicLootGetEnchantCosts {epicLootGetEnchantCosts != null}");
            }

        }


        [HarmonyPatch(typeof(UITooltip), "LateUpdate")]
        static class UITooltip_LateUpdate_Patch {
            static void Postfix(UITooltip __instance, UITooltip ___m_current, GameObject ___m_tooltip) {

                if (!modEnabled.Value)
                    return;
                if (___m_current == __instance && ___m_tooltip != null && ___m_current.transform.name == "RepairButton") {
                    ___m_tooltip.transform.position = Input.mousePosition + new Vector3(-200, -100);
                }
            }
        }



        [HarmonyPatch(typeof(InventoryGui), "UpdateRepair")]
        static class InventoryGui_UpdateRepair_Patch {
            static void Postfix(InventoryGui __instance, ref List<ItemDrop.ItemData> ___m_tempWornItems) {
                if (!modEnabled.Value)
                    return;

                if (!___m_tempWornItems.Any())
                    return;

                List<RepairItemData> freeRepairs = new List<RepairItemData>();
                List<RepairItemData> enoughRepairs = new List<RepairItemData>();
                List<RepairItemData> notEnoughRepairs = new List<RepairItemData>();
                List<RepairItemData> unableRepairs = new List<RepairItemData>();
                List<string> outstring = new List<string>();
                foreach (ItemDrop.ItemData item in ___m_tempWornItems) {
                    if (!Traverse.Create(__instance).Method("CanRepair", new object[] { item }).GetValue<bool>()) {
                        unableRepairs.Add(new RepairItemData(item));
                        continue;
                    }
                    List<Piece.Requirement> reqs = RepairReqs(item);
                    if (reqs == null) {
                        freeRepairs.Add(new RepairItemData(item));
                        continue;
                    }
                    List<string> reqstring = new List<string>();
                    foreach (Piece.Requirement req in reqs) {
                        if (req.m_amount == 0)
                            continue;

                        // changed by cjayride
                        //reqstring.Add($"{req.m_amount}/{Player.m_localPlayer.GetInventory().CountItems(req.m_resItem.m_itemData.m_shared.m_name)} {Localization.instance.Localize(req.m_resItem.m_itemData.m_shared.m_name)}");
                        reqstring.Add($"{req.m_amount} {Localization.instance.Localize(req.m_resItem.m_itemData.m_shared.m_name)}");
                    }
                    bool enough = true;
                    foreach (Piece.Requirement requirement in reqs) {
                        if (requirement.m_resItem) {
                            int amount = requirement.m_amount;
                            if (Player.m_localPlayer.GetInventory().CountItems(requirement.m_resItem.m_itemData.m_shared.m_name) < amount) {
                                enough = false;
                                break;
                            }
                        }
                    }
                    if (!enough)
                        notEnoughRepairs.Add(new RepairItemData(item, reqstring));
                    else
                        enoughRepairs.Add(new RepairItemData(item, reqstring));
                }
                orderedWornItems = new List<ItemDrop.ItemData>();
                foreach (RepairItemData rid in freeRepairs) {
                    outstring.Add($"<color=#{hasEnoughTooltipColor.Value}>{Localization.instance.Localize(rid.item.m_shared.m_name)}: Free</color>");
                    orderedWornItems.Add(rid.item);
                }
                foreach (RepairItemData rid in enoughRepairs) {
                    outstring.Add($"<color=#{hasEnoughTooltipColor.Value}>{Localization.instance.Localize(rid.item.m_shared.m_name)}: {string.Join(", ", rid.reqstring)}</color>");
                    orderedWornItems.Add(rid.item);
                }
                foreach (RepairItemData rid in notEnoughRepairs) {
                    outstring.Add($"<color=#{notEnoughTooltipColor.Value}>{Localization.instance.Localize(rid.item.m_shared.m_name)}: {string.Join(", ", rid.reqstring)}</color>");
                    orderedWornItems.Add(rid.item);
                }
                foreach (RepairItemData rid in unableRepairs) {
                    orderedWornItems.Add(rid.item);
                }
                ___m_tempWornItems = new List<ItemDrop.ItemData>(orderedWornItems);

                if (!showAllRepairsInToolTip.Value)
                    return;

                UITooltip tt = (UITooltip)typeof(UITooltip).GetField("m_current", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                GameObject go = (GameObject)typeof(UITooltip).GetField("m_tooltip", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

                if (go == null || tt.transform.name != "RepairButton")
                    return;



                // added by cjayride
                List<ItemDrop.ItemData> playerItems = Player.m_localPlayer.GetInventory().GetAllItems();
                int numberOfCoinsInInventory = 0;
                if (outstring.Count == 0) {
                    outstring.Add("Nothing to repair.");
                }

                int tempCoinCount = 0;

                foreach (ItemDrop.ItemData item in playerItems) {

                    if (item.m_shared.m_name == "$item_coins") {
                        tempCoinCount += item.GetValue();
                    }
                }

                numberOfCoinsInInventory = tempCoinCount;
                outstring.Add("-------------------- \r\n <b>Coins: " + numberOfCoinsInInventory.ToString() + "</b>");

                Utils.FindChild(go.transform, "Text").GetComponent<TMP_Text>().richText = true;
                Utils.FindChild(go.transform, "Text").GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Bottom;
                Utils.FindChild(go.transform, "Text").GetComponent<TMP_Text>().fontSize = 20;
                Utils.FindChild(go.transform, "Text").GetComponent<TMP_Text>().text = $"<b><color=#{titleTooltipColor.Value}>[{Localization.instance.Localize("$inventory_repairbutton")}]</color></b>\r\n -------------------- \r\n" + string.Join("\r\n", outstring);
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "CanRepair")]
        static class InventoryGui_CanRepair_Patch {
            static void Postfix(ItemDrop.ItemData item, ref bool __result) {
                if (!modEnabled.Value)
                    return;

                if (modEnabled.Value && Environment.StackTrace.Contains("RepairOneItem") && !Environment.StackTrace.Contains("HaveRepairableItems") && __result == true && item?.m_shared != null && Player.m_localPlayer != null && orderedWornItems.Count > 0) {
                    if (orderedWornItems[0] != item) {
                        __result = false;
                        return;
                    }
                    List<Piece.Requirement> reqs = RepairReqs(item, true);
                    if (reqs == null)
                        return;

                    List<string> reqstring = new List<string>();
                    foreach (Piece.Requirement req in reqs) {
                        if (req?.m_resItem?.m_itemData?.m_shared == null)
                            continue;


                        // changed by cjayride
                        //reqstring.Add($"{req.m_amount}/{Player.m_localPlayer.GetInventory().CountItems(req.m_resItem.m_itemData.m_shared.m_name)} {Localization.instance.Localize(req.m_resItem.m_itemData.m_shared.m_name)}");
                        reqstring.Add($"{req.m_amount} {Localization.instance.Localize(req.m_resItem.m_itemData.m_shared.m_name)}");
                    }
                    string outstring;

                    bool enough = true;
                    foreach (Piece.Requirement requirement in reqs) {
                        if (requirement.m_resItem) {
                            int amount = requirement.m_amount;
                            if (Player.m_localPlayer.GetInventory().CountItems(requirement.m_resItem.m_itemData.m_shared.m_name) < amount) {
                                enough = false;
                                break;
                            }
                        }
                    }
                    if (enough) {
                        Player.m_localPlayer.ConsumeResources(reqs.ToArray(), 1);
                        outstring = $"Used {string.Join(", ", reqstring)} to repair {Localization.instance.Localize(item.m_shared.m_name)}";
                        __result = true;
                    } else {
                        outstring = $"Require {string.Join(", ", reqstring)} to repair {item.m_shared.m_name}";
                        __result = false;
                    }

                    Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, outstring, 0, null);
                    Dbgl(outstring);
                }
            }
        }

        private static List<Piece.Requirement> RepairReqs(ItemDrop.ItemData item, bool log = false) {
            float percent = (item.GetMaxDurability() - item.m_durability) / item.GetMaxDurability();
            Recipe fullRecipe = ObjectDB.instance.GetRecipe(item);
            if (fullRecipe is null)
                return null;
            var fullReqs = new List<Piece.Requirement>(fullRecipe.m_resources);

            // added by cjayride
            // hack in the coin object becuase I don't know how else to do it
            // EssenceMagic breaks down into Coins, so I'm breaking it down to grab the Coin item
            // Then I can have the Coin object held in the recipe container, and hack it into the original coded Recipe container
            int calculatedRepairCoinCost = 0;
            GameObject prefab5 = ZNetScene.instance.GetPrefab("GoldRubyRing");
            ItemDrop.ItemData newItem5 = prefab5.GetComponent<ItemDrop>().m_itemData.Clone();
            Recipe cjayRecipe = ObjectDB.instance.GetRecipe(newItem5);
            var cjayFullReqs = new List<Piece.Requirement>(cjayRecipe.m_resources);

            bool isMagic = false;
            if (epicLootAssembly != null) {
                try {
                    isMagic = (bool)epicLootIsMagic.Invoke(null, new[] { item });
                } catch { }
            }
            if (isMagic) {
                try {
                    int rarity = (int)epicLootGetRarity.Invoke(null, new[] { item });
                    List<KeyValuePair<ItemDrop, int>> magicReqs = (List<KeyValuePair<ItemDrop, int>>)epicLootGetEnchantCosts.Invoke(null, new object[] { item, rarity });
                    foreach (var kvp in magicReqs) {
                        fullReqs.Add(new Piece.Requirement() {
                            m_amount = kvp.Value,
                            m_resItem = kvp.Key
                        });
                    }
                } catch { }
            }


            List<Piece.Requirement> reqs = new List<Piece.Requirement>();
            for (int i = 0; i < fullReqs.Count; i++) {

                Piece.Requirement req = new Piece.Requirement() {
                    m_resItem = fullReqs[i].m_resItem,
                    m_amount = fullReqs[i].m_amount,
                    m_amountPerLevel = fullReqs[i].m_amountPerLevel,
                    m_recover = fullReqs[i].m_recover
                };

                int amount = 0;
                for (int j = item.m_quality; j > 0; j--) {
                    //Dbgl($"{req.m_resItem.m_itemData.m_shared.m_name} req for level {j} {req.m_amount}, {req.m_amountPerLevel} {req.GetAmount(j)}");
                    amount += req.GetAmount(j);
                }

                int fraction = Mathf.RoundToInt(amount * percent * materialRequirementMult.Value);


                //Dbgl($"total {req.m_resItem.m_itemData.m_shared.m_name} reqs for {item.m_shared.m_name}, dur {item.m_durability}/{item.GetMaxDurability()} ({item.GetDurabilityPercentage()} {percent}): {fraction}/{amount}");

                /*if (fraction > 0)
                {*/
                // req.m_amount = fraction;
                // reqs.Add(req);

                req.m_amount = (int)fraction;

                switch (req.m_resItem.name) {
                    case "BlackMetalScrap":

                        if (savedValues.BlackMetalScrap == -1)
                            break;

                        if (savedValues.BlackMetalScrap * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.BlackMetalScrap * req.m_amount);
                        break;

                    case "BlackMetal":

                        if (savedValues.BlackMetal == -1)
                            break;

                        if (savedValues.BlackMetal * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.BlackMetal * req.m_amount);
                        break;

                    case "Bronze":

                        if (savedValues.Bronze == -1)
                            break;

                        if (savedValues.Bronze * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Bronze * req.m_amount);
                        break;

                    case "Chain":

                        if (savedValues.Chain == -1)
                            break;

                        if (savedValues.Chain * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Chain * req.m_amount);
                        break;

                    case "Chitin":

                        if (savedValues.Chitin == -1)
                            break;

                        if (savedValues.Chitin * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Chitin * req.m_amount);
                        break;

                    case "Copper":

                        if (savedValues.Copper == -1)
                            break;

                        if (savedValues.Copper * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Copper * req.m_amount);
                        break;

                    case "CopperOre":

                        if (savedValues.CopperOre == -1)
                            break;

                        if (savedValues.CopperOre * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.CopperOre * req.m_amount);
                        break;

                    case "Flametal":

                        if (savedValues.Flametal == -1)
                            break;

                        if (savedValues.Flametal * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Flametal * req.m_amount);
                        break;

                    case "FlametalOre":

                        if (savedValues.FlametalOre == -1)
                            break;

                        if (savedValues.FlametalOre * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.FlametalOre * req.m_amount);
                        break;

                    case "Frometal":

                        if (savedValues.Frometal == -1)
                            break;

                        if (savedValues.Frometal * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Frometal * req.m_amount);
                        break;

                    case "FrometalOre":

                        if (savedValues.FrometalOre == -1)
                            break;

                        if (savedValues.FrometalOre * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.FrometalOre * req.m_amount);
                        break;

                    case "FrostinfusedDarkmetal":

                        if (savedValues.FrostinfusedDarkmetal == -1)
                            break;

                        if (savedValues.FrostinfusedDarkmetal * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.FrostinfusedDarkmetal * req.m_amount);
                        break;

                    case "HeatedIron":

                        if (savedValues.HeatedIron == -1)
                            break;

                        if (savedValues.HeatedIron * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.HeatedIron * req.m_amount);
                        break;

                    case "Heavymetal":

                        if (savedValues.Heavymetal == -1)
                            break;

                        if (savedValues.Heavymetal * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Heavymetal * req.m_amount);
                        break;

                    case "HeavymetalOre":

                        if (savedValues.HeavymetalOre == -1)
                            break;

                        if (savedValues.HeavymetalOre * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.HeavymetalOre * req.m_amount);
                        break;

                    case "Heavyscale":

                        if (savedValues.Heavyscale == -1)
                            break;

                        if (savedValues.Heavyscale * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Heavyscale * req.m_amount);
                        break;

                    case "Iron":

                        if (savedValues.Iron == -1)
                            break;

                        if (savedValues.Iron * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Iron * req.m_amount);
                        break;

                    case "LeatherScraps":

                        if (savedValues.LeatherScraps == -1)
                            break;

                        if (savedValues.LeatherScraps * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.LeatherScraps * req.m_amount);
                        break;

                    case "PrimordialIce":

                        if (savedValues.PrimordialIce == -1)
                            break;

                        if (savedValues.PrimordialIce * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.PrimordialIce * req.m_amount);
                        break;

                    case "ScrapIron":

                        if (savedValues.ScrapIron == -1)
                            break;

                        if (savedValues.ScrapIron * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.ScrapIron * req.m_amount);
                        break;

                    case "Silver":
                        if (savedValues.Silver == -1)
                            break;

                        if (savedValues.Silver * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Silver * req.m_amount);
                        break;

                    case "SilverOre":

                        if (savedValues.SilverOre == -1)
                            break;

                        if (savedValues.SilverOre * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.SilverOre * req.m_amount);
                        break;

                    case "Tin":

                        if (savedValues.Tin == -1)
                            break;

                        if (savedValues.Tin * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Tin * req.m_amount);
                        break;

                    case "TinOre":

                        if (savedValues.TinOre == -1)
                            break;

                        if (savedValues.TinOre * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.TinOre * req.m_amount);
                        break;

                    case "DeerHide":

                        if (savedValues.DeerHide == -1)
                            break;

                        if (savedValues.DeerHide * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.DeerHide * req.m_amount);
                        break;

                    case "TrollHide":

                        if (savedValues.TrollHide == -1)
                            break;

                        if (savedValues.TrollHide * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.TrollHide * req.m_amount);
                        break;

                    case "WolfPelt":

                        if (savedValues.WolfPelt == -1)
                            break;

                        if (savedValues.WolfPelt * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.WolfPelt * req.m_amount);
                        break;

                    case "LoxPelt":

                        if (savedValues.LoxPelt == -1)
                            break;

                        if (savedValues.LoxPelt * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.LoxPelt * req.m_amount);
                        break;

                    case "WitheredBone":

                        if (savedValues.WitheredBone == -1)
                            break;

                        if (savedValues.WitheredBone * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.WitheredBone * req.m_amount);
                        break;

                    case "BoneFragments":

                        if (savedValues.BoneFragments == -1)
                            break;

                        if (savedValues.BoneFragments * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.BoneFragments * req.m_amount);
                        break;

                    case "LinenThread":

                        if (savedValues.LinenThread == -1)
                            break;

                        if (savedValues.LinenThread * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.LinenThread * req.m_amount);
                        break;

                    case "ElderBark":

                        if (savedValues.ElderBark == -1)
                            break;

                        if (savedValues.ElderBark * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.ElderBark * req.m_amount);
                        break;

                    case "Obsidian":

                        if (savedValues.Obsidian == -1)
                            break;

                        if (savedValues.Obsidian * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Obsidian * req.m_amount);
                        break;

                    case "FreezeGland":

                        if (savedValues.FreezeGland == -1)
                            break;

                        if (savedValues.FreezeGland * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.FreezeGland * req.m_amount);
                        break;

                    case "Crystal":

                        if (savedValues.Crystal == -1)
                            break;

                        if (savedValues.Crystal * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Crystal * req.m_amount);
                        break;

                    case "YimirRemains":

                        if (savedValues.YimirRemains == -1)
                            break;

                        if (savedValues.YimirRemains * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.YimirRemains * req.m_amount);
                        break;

                    case "HardAntler":

                        if (savedValues.HardAntler == -1)
                            break;

                        if (savedValues.HardAntler * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.HardAntler * req.m_amount);
                        break;

                    case "SalamanderFurTH":
                        if (savedValues.SalamanderFurTH == -1)
                            break;

                        if (savedValues.SalamanderFurTH * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.SalamanderFurTH * req.m_amount);
                        break;

                    case "WolfFang":
                        if (savedValues.WolfFang == -1)
                            break;

                        if (savedValues.WolfFang * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.WolfFang * req.m_amount);
                        break;

                    case "Root":
                        if (savedValues.Root == -1)
                            break;

                        if (savedValues.Root * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Root * req.m_amount);
                        break;

                    case "Flint":
                        if (savedValues.Flint == -1)
                            break;

                        if (savedValues.Flint * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Flint * req.m_amount);
                        break;

                    case "Needle":
                        if (savedValues.Needle == -1)
                            break;

                        if (savedValues.Needle * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Needle * req.m_amount);
                        break;

                    case "Wood":

                        if (savedValues.Wood == -1)
                            break;

                        if (savedValues.Wood * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.Wood * req.m_amount);
                        break;

                    case "RoundLog":

                        if (savedValues.RoundLog == -1)
                            break;

                        if (savedValues.RoundLog * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.RoundLog * req.m_amount);
                        break;

                    case "SerpentScale":

                        if (savedValues.SerpentScale == -1)
                            break;

                        if (savedValues.SerpentScale * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.SerpentScale * req.m_amount);
                        break;

                    case "WorldTreeFragment":

                        if (savedValues.WorldTreeFragment == -1)
                            break;

                        if (savedValues.WorldTreeFragment * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.WorldTreeFragment * req.m_amount);
                        break;

                    case "BurningWorldTreeFragment":

                        if (savedValues.BurningWorldTreeFragment == -1)
                            break;

                        if (savedValues.BurningWorldTreeFragment * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.BurningWorldTreeFragment * req.m_amount);
                        break;

                    case "Stone":

                        if (savedValues.Stone == -1)
                            break;

                        if (savedValues.BurningWorldTreeFragment * req.m_amount <= 1)
                            calculatedRepairCoinCost += 1;
                        else
                            calculatedRepairCoinCost += Mathf.RoundToInt(savedValues.BurningWorldTreeFragment * req.m_amount);
                        break;
                }
            }

            // added by cjayride
            // this is the 2nd part of the item hack using the EssenceMagic recipe
            // to get a coin item, and then modify the number of coins to add to the repair cost

            if (calculatedRepairCoinCost > 0) {
                List<Piece.Requirement> cjayReqs = new List<Piece.Requirement>();
                for (int i = 0; i < cjayFullReqs.Count; i++) {

                    Piece.Requirement cjayReq = new Piece.Requirement() {
                        m_resItem = cjayFullReqs[i].m_resItem,
                        m_amount = cjayFullReqs[i].m_amount,
                        m_amountPerLevel = cjayFullReqs[i].m_amountPerLevel,
                        m_recover = cjayFullReqs[i].m_recover
                    };

                    if (cjayReq.m_resItem.name == "Coins") {
                        cjayReq.m_amount = calculatedRepairCoinCost;
                        reqs.Add(cjayReq);
                    } else {
                        continue;
                    }
                }
            } else {
                // return a FREE repair
                return null;
            }

            // commented out by cjayride
            // this was original code by the dev
            /*
            if (!reqs.Any())
            {
                return null;
            }*/

            // left this return in, but it should never get called
            return reqs;
        }
    }
}

public class WeaponAndArmorValues {
    public float BlackMetalScrap { get; set; }
    public float BlackMetal { get; set; }
    public float Bronze { get; set; }
    public float Chain { get; set; }
    public float Chitin { get; set; }
    public float Copper { get; set; }
    public float CopperOre { get; set; }
    public float Flametal { get; set; }
    public float FlametalOre { get; set; }
    public float Frometal { get; set; }
    public float FrometalOre { get; set; }
    public float FrostinfusedDarkmetal { get; set; }
    public float HeatedIron { get; set; }
    public float Heavymetal { get; set; }
    public float HeavymetalOre { get; set; }
    public float Heavyscale { get; set; }
    public float Iron { get; set; }
    public float LeatherScraps { get; set; }
    public float PrimordialIce { get; set; }
    public float ScrapIron { get; set; }
    public float Silver { get; set; }
    public float SilverOre { get; set; }
    public float Tin { get; set; }
    public float TinOre { get; set; }
    public float DeerHide { get; set; }
    public float TrollHide { get; set; }
    public float WolfPelt { get; set; }
    public float LoxPelt { get; set; }
    public float WitheredBone { get; set; }
    public float BoneFragments { get; set; }
    public float LinenThread { get; set; }
    public float ElderBark { get; set; }
    public float Obsidian { get; set; }
    public float FreezeGland { get; set; }
    public float Crystal { get; set; }
    public float YimirRemains { get; set; }
    public float HardAntler { get; set; }
    public float SalamanderFurTH { get; set; }
    public float WolfFang { get; set; }
    public float Root { get; set; }
    public float Flint { get; set; }
    public float Needle { get; set; }
    public float Wood { get; set; }
    public float RoundLog { get; set; }
    public float SerpentScale { get; set; }
    public float WorldTreeFragment { get; set; }
    public float BurningWorldTreeFragment { get; set; }
    public float Stone { get; set; }
}