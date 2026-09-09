using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using TMPro;

namespace RepairRequiresMats {
    [BepInPlugin("cjayride.RepairRequiresCoins", "Repair Requires Coins", "1.2.2")]
    public class BepInExPlugin : BaseUnityPlugin {
        public const string Version = "1.2.2";
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
            hasEnoughTooltipColor = Config.Bind<string>("General", "HasEnoughTooltipColor", "00FF00FF", "Color to use in tooltip for repairable item names.");
            notEnoughTooltipColor = Config.Bind<string>("General", "NotEnoughTooltipColor", "FF0000FF", "Color to use in tooltip for items that cannot be repaired yet.");
            materialRequirementMult = Config.Bind<float>("General", "MaterialRequirementMult", 0.5f, "Multiplier for amount of each material required.");

            // added by cjayride
            coinOnly = Config.Bind<bool>("General", "CoinOnly", true, "If true, repair costs coins only. If false, repair costs coins plus the original materials.");

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
                    if (!IsAtRequiredRepairStation(item)) {
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
                        reqstring.Add(FormatRequirementLine(req));
                    }
                    bool enough = true;
                    foreach (Piece.Requirement requirement in reqs) {
                        if (requirement.m_resItem) {
                            int amount = requirement.m_amount;
                            if (CountNamedItems(Player.m_localPlayer.GetInventory(), requirement.m_resItem.m_itemData.m_shared.m_name) < amount) {
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
                    outstring.Add($"<color=#FFFFFFFF>{Localization.instance.Localize(rid.item.m_shared.m_name)}</color>: <color=#00FF00FF>Free</color>");
                    orderedWornItems.Add(rid.item);
                }
                foreach (RepairItemData rid in enoughRepairs) {
                    outstring.Add($"<color=#FFFFFFFF>{Localization.instance.Localize(rid.item.m_shared.m_name)}</color>: {string.Join(", ", rid.reqstring)}");
                    orderedWornItems.Add(rid.item);
                }
                foreach (RepairItemData rid in notEnoughRepairs) {
                    outstring.Add($"<color=#FFFFFFFF>{Localization.instance.Localize(rid.item.m_shared.m_name)}</color>: {string.Join(", ", rid.reqstring)}");
                    orderedWornItems.Add(rid.item);
                }
                foreach (RepairItemData rid in unableRepairs) {
                    string itemName = Localization.instance.Localize(rid.item.m_shared.m_name);
                    outstring.Add($"<color=#FFFFFFFF>{itemName}</color> - <color=#FF0000FF>{GetRepairNeedLabel(rid.item)}</color>");
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
                if (outstring.Count == 0) {
                    outstring.Add("Nothing to repair.");
                }

                int numberOfCoinsInInventory = CountNamedItems(Player.m_localPlayer.GetInventory(), "$item_coins");
                outstring.Add("-------------------- \r\n <b><color=#FFFFFFFF>Coins</color>: <color=#FFFF00FF>" + numberOfCoinsInInventory.ToString() + "</color></b>");

                Transform textChild = Utils.FindChild(go.transform, "Text", Utils.IterativeSearchType.DepthFirst);
                if (textChild == null)
                    return;
                TMP_Text tooltipText = textChild.GetComponent<TMP_Text>();
                if (tooltipText == null)
                    return;
                tooltipText.richText = true;
                tooltipText.alignment = TextAlignmentOptions.Bottom;
                tooltipText.fontSize = 20;
                string stationLine = GetCurrentStationLevelLabel();
                tooltipText.text = $"<b><color=#{titleTooltipColor.Value}>Repair an Item</color></b>\r\n{stationLine}\r\n -------------------- \r\n" + string.Join("\r\n", outstring);
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "CanRepair")]
        static class InventoryGui_CanRepair_Patch {
            static void Postfix(ItemDrop.ItemData item, ref bool __result) {
                if (!modEnabled.Value)
                    return;

                // Valheim 1.0 CanRepair can return true at the wrong station via the world-level fallback.
                if (!IsAtRequiredRepairStation(item))
                    __result = false;

                if (Environment.StackTrace.Contains("RepairOneItem") && !Environment.StackTrace.Contains("HaveRepairableItems") && __result == true && item?.m_shared != null && Player.m_localPlayer != null && orderedWornItems.Count > 0) {
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
                        reqstring.Add(FormatRequirementLine(req, false));
                    }
                    string outstring;

                    bool enough = true;
                    foreach (Piece.Requirement requirement in reqs) {
                        if (requirement.m_resItem) {
                            int amount = requirement.m_amount;
                            if (CountNamedItems(Player.m_localPlayer.GetInventory(), requirement.m_resItem.m_itemData.m_shared.m_name) < amount) {
                                enough = false;
                                break;
                            }
                        }
                    }
                    if (enough) {
                        ConsumeRepairRequirements(Player.m_localPlayer, reqs);
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

        private static int CountNamedItems(Inventory inventory, string itemName) {
            if (inventory == null || string.IsNullOrEmpty(itemName))
                return 0;

            int count = 0;
            foreach (ItemDrop.ItemData item in inventory.GetAllItems()) {
                if (item?.m_shared?.m_name == itemName)
                    count += item.m_stack;
            }
            return count;
        }

        private static MethodInfo inventoryRemoveByName;

        private static void RemoveNamedItems(Inventory inventory, string itemName, int amount) {
            if (inventory == null || string.IsNullOrEmpty(itemName) || amount <= 0)
                return;

            if (inventoryRemoveByName == null) {
                MethodInfo best = null;
                foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(Inventory))) {
                    if (method.Name != "RemoveItem")
                        continue;
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length < 2 || parameters[0].ParameterType != typeof(string) || parameters[1].ParameterType != typeof(int))
                        continue;
                    if (best == null || parameters.Length < best.GetParameters().Length)
                        best = method;
                }
                inventoryRemoveByName = best;
            }

            if (inventoryRemoveByName != null) {
                ParameterInfo[] parameters = inventoryRemoveByName.GetParameters();
                object[] args = new object[parameters.Length];
                args[0] = itemName;
                args[1] = amount;
                for (int i = 2; i < parameters.Length; i++) {
                    if (parameters[i].HasDefaultValue)
                        args[i] = parameters[i].DefaultValue;
                    else if (parameters[i].ParameterType == typeof(int))
                        args[i] = -1;
                    else if (parameters[i].ParameterType == typeof(bool))
                        args[i] = true;
                    else
                        args[i] = null;
                }
                inventoryRemoveByName.Invoke(inventory, args);
                return;
            }

            List<ItemDrop.ItemData> items = inventory.GetAllItems();
            for (int i = items.Count - 1; i >= 0 && amount > 0; i--) {
                ItemDrop.ItemData item = items[i];
                if (item?.m_shared?.m_name != itemName)
                    continue;
                int take = Mathf.Min(item.m_stack, amount);
                item.m_stack -= take;
                amount -= take;
                if (item.m_stack <= 0)
                    inventory.RemoveItem(item);
            }
        }

        // Player.ConsumeResources(Requirement[], int, int) was removed in later Valheim versions
        // (Call to Arms / 1.0 added extraAmount). Calling the old signature JIT-fails inside
        // InventoryGui.CanRepair and spams MissingMethodException every frame.
        private static void ConsumeRepairRequirements(Player player, List<Piece.Requirement> reqs) {
            if (player == null || reqs == null)
                return;

            Inventory inventory = player.GetInventory();
            foreach (Piece.Requirement requirement in reqs) {
                if (requirement?.m_resItem?.m_itemData?.m_shared == null)
                    continue;
                RemoveNamedItems(inventory, requirement.m_resItem.m_itemData.m_shared.m_name, requirement.m_amount);
            }
        }

        private static string FormatRequirementLine(Piece.Requirement req, bool colored = true) {
            string sharedName = req.m_resItem.m_itemData.m_shared.m_name;
            string name = Localization.instance.Localize(sharedName);
            bool isCoins = sharedName == "$item_coins" || req.m_resItem.name == "Coins";
            if (!colored)
                return req.m_amount + " " + (isCoins ? "Coins" : name);
            if (isCoins) {
                int coins = CountNamedItems(Player.m_localPlayer.GetInventory(), sharedName);
                string coinAmountColor = coins >= req.m_amount ? "FFFF00FF" : "FF0000FF";
                return "<color=#" + coinAmountColor + ">" + req.m_amount + "</color> <color=#FFFFFFFF>Coins</color>";
            }
            int have = CountNamedItems(Player.m_localPlayer.GetInventory(), sharedName);
            string amountColor = have >= req.m_amount ? "00FF00FF" : "FF0000FF";
            return "<color=#" + amountColor + ">" + req.m_amount + "</color> " + name;
        }

        private static bool RecipeRepairsAtStation(Recipe recipe, CraftingStation current) {
            if (recipe == null || !current)
                return false;
            if (recipe.m_repairStation && recipe.m_repairStation.m_name == current.m_name)
                return true;
            if (recipe.m_craftingStation && recipe.m_craftingStation.m_name == current.m_name)
                return true;
            return false;
        }

        private static CraftingStation GetRequiredRepairStation(Recipe recipe) {
            if (recipe == null)
                return null;
            if (recipe.m_repairStation)
                return recipe.m_repairStation;
            return recipe.m_craftingStation;
        }

        private static int GetRequiredRepairStationLevel(Recipe recipe) {
            if (recipe == null)
                return 1;
            return Mathf.Max(1, recipe.m_minStationLevel);
        }

        private static bool IsAtRequiredRepairStation(ItemDrop.ItemData item) {
            if (item == null || Player.m_localPlayer == null)
                return false;

            CraftingStation current = Player.m_localPlayer.GetCurrentCraftingStation();
            if (!current)
                return false;

            Recipe recipe = ObjectDB.instance != null ? ObjectDB.instance.GetRecipe(item) : null;
            if (recipe == null || !RecipeRepairsAtStation(recipe, current))
                return false;

            return GetLiveStationLevel(current) >= GetRequiredRepairStationLevel(recipe);
        }

        private static CraftingStation liveLevelStation;
        private static int liveLevelValue;
        private static int liveLevelFrame;

        // Count only extensions that still exist. Destroyed bellows/anvils can linger in the cached list.
        private static int GetLiveStationLevel(CraftingStation station) {
            if (!station)
                return 0;

            int frame = Time.frameCount;
            if (liveLevelFrame == frame && liveLevelStation == station)
                return liveLevelValue;

            FieldInfo timer = AccessTools.Field(typeof(CraftingStation), "m_updateExtensionTimer");
            if (timer != null)
                timer.SetValue(station, 999f);

            MethodInfo refresh = AccessTools.Method(typeof(CraftingStation), "GetExtensions");
            if (refresh != null)
                refresh.Invoke(station, null);

            FieldInfo listField = AccessTools.Field(typeof(CraftingStation), "m_attachedExtensions");
            IList list = listField != null ? listField.GetValue(station) as IList : null;
            int extras = 0;
            if (list != null) {
                foreach (object ext in list) {
                    UnityEngine.Object unityObj = ext as UnityEngine.Object;
                    if (unityObj)
                        extras++;
                }
            } else {
                extras = Math.Max(0, station.GetLevel(true) - 1);
            }

            liveLevelStation = station;
            liveLevelFrame = frame;
            liveLevelValue = 1 + extras;
            return liveLevelValue;
        }

        private static string GetCurrentStationLevelLabel() {
            CraftingStation current = Player.m_localPlayer != null ? Player.m_localPlayer.GetCurrentCraftingStation() : null;
            if (!current)
                return "";
            string name = Localization.instance.Localize(current.m_name);
            return name + " Lvl: " + GetLiveStationLevel(current);
        }

        private static string GetRepairNeedLabel(ItemDrop.ItemData item) {
            Recipe recipe = ObjectDB.instance != null ? ObjectDB.instance.GetRecipe(item) : null;
            if (recipe == null)
                return "Needs recipe";

            CraftingStation required = GetRequiredRepairStation(recipe);
            if (!required)
                return "cannot repair here";

            string stationName = Localization.instance.Localize(required.m_name);
            return "Needs " + stationName + " Lvl: " + GetRequiredRepairStationLevel(recipe);
        }

        private static ItemDrop GetCoinsItemDrop() {
            GameObject prefab = ObjectDB.instance != null ? ObjectDB.instance.GetItemPrefab("Coins") : null;
            if (prefab == null && ZNetScene.instance != null)
                prefab = ZNetScene.instance.GetPrefab("Coins");
            return prefab != null ? prefab.GetComponent<ItemDrop>() : null;
        }

        private static List<Piece.Requirement> RepairReqs(ItemDrop.ItemData item, bool log = false) {
            float percent = (item.GetMaxDurability() - item.m_durability) / item.GetMaxDurability();
            Recipe fullRecipe = ObjectDB.instance.GetRecipe(item);
            if (fullRecipe is null)
                return null;
            var fullReqs = new List<Piece.Requirement>(fullRecipe.m_resources);

            int calculatedRepairCoinCost = 0;

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
                if (fullReqs[i]?.m_resItem == null)
                    continue;

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
                ApplyMaterialRepairCost(req, reqs, ref calculatedRepairCoinCost);
            }

            if (calculatedRepairCoinCost > 0) {
                ItemDrop coins = GetCoinsItemDrop();
                if (coins == null && coinOnly.Value)
                    return reqs.Count > 0 ? reqs : null;

                if (coins != null && calculatedRepairCoinCost > 0) {
                    reqs.Add(new Piece.Requirement() {
                        m_resItem = coins,
                        m_amount = calculatedRepairCoinCost,
                        m_amountPerLevel = 0,
                        m_recover = false
                    });
                }
            }

            if (reqs.Count == 0)
                return null;

            return reqs;
        }

        private static void ApplyMaterialRepairCost(Piece.Requirement req, List<Piece.Requirement> reqs, ref int calculatedRepairCoinCost) {
            float rate;
            bool known = TryGetMaterialRate(req.m_resItem.name, out rate);
            if (known && rate == -1)
                return;

            if (known && req.m_amount > 0) {
                if (rate * req.m_amount <= 1)
                    calculatedRepairCoinCost += 1;
                else
                    calculatedRepairCoinCost += Mathf.RoundToInt(rate * req.m_amount);
            }

            if (!coinOnly.Value && req.m_amount > 0)
                reqs.Add(req);
        }

        private static bool TryGetMaterialRate(string prefabName, out float rate) {
            rate = 0;
            if (savedValues == null || string.IsNullOrEmpty(prefabName))
                return false;
            var prop = typeof(WeaponAndArmorValues).GetProperty(prefabName);
            if (prop == null)
                return false;
            rate = Convert.ToSingle(prop.GetValue(savedValues, null));
            return true;
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