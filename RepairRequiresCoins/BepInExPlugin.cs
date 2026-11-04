using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RepairRequiresMats {
    [BepInPlugin("cjayride.RepairRequiresCoins", "Repair Requires Coins", "1.2.12")]
    [BepInDependency("Swmarly.ValheimQOL", BepInDependency.DependencyFlags.SoftDependency)]
    public class BepInExPlugin : BaseUnityPlugin {
        public const string Version = "1.2.12";
        public const string ModName = "Repair Requires Coins";

        private static bool isDebug = false;

        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> showAllRepairsInToolTip;
        public static ConfigEntry<float> materialRequirementMult;
        public static ConfigEntry<string> titleTooltipColor;
        public static ConfigEntry<string> hasEnoughTooltipColor;
        public static ConfigEntry<string> notEnoughTooltipColor;
        private static List<ItemDrop.ItemData> orderedWornItems = new List<ItemDrop.ItemData>();
        private static int paidRepairFrame = -1;
        private static ItemDrop.ItemData paidRepairItem;
        private static bool allowUnpaidCanRepair = true;
        private static string repairTooltipTopic = "Repair an Item";
        private static string repairTooltipBody = "";
        private static string repairTooltipBodyPlain = "";
        private static readonly List<RepairOfferLine> senealOfferLines = new List<RepairOfferLine>();
        private static bool senealDrawingOurTip;
        private static string senealStationLine = "";
        private static int senealCoinCount = 0;

        private static BepInExPlugin context;

        private static Assembly epicLootAssembly;
        private static MethodInfo epicLootIsMagic;
        private static MethodInfo epicLootGetRarity;
        private static MethodInfo epicLootGetEnchantCosts;
        private static MethodInfo swmarlyGetPocketCoins;
        private static MethodInfo swmarlySetPocketCoins;
        private static readonly object[] invokeOne = new object[1];
        private static readonly object[] invokeTwo = new object[2];
        private const string SwmarlyQolGuid = "Swmarly.ValheimQOL";
        private const string CurrencyPocketGuid = "Azumatt.CurrencyPocket";
        private const string SwmarlyCoinKey = "SwmarlyValheimQOL_Coins";
        private const string CoinPocketCountKey = "CoinPocket_CoinCount";
        private static bool currencyPocketLoaded;

        private static readonly Dictionary<string, float> materialRates = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<ItemDrop.ItemData, Recipe> recipeCache = new Dictionary<ItemDrop.ItemData, Recipe>(ItemRefComparer.Instance);
        private static int recipeCacheFrame = -1;
        private static ItemDrop cachedCoinsDrop;
        private static int cachedCoinsDropFrame = -1;
        private static int lastOfferStamp;
        private static FieldInfo inventoryGuiRepairButton;
        private static FieldInfo uiTooltipCurrent;
        private static FieldInfo uiTooltipObject;
        private static FieldInfo stationExtensionTimer;
        private static FieldInfo stationAttachedExtensions;
        private static MethodInfo stationGetExtensions;
        private static TMP_Text cachedTooltipText;
        private static TMP_Text cachedTopicText;
        private static GameObject cachedTooltipGo;
        private static string lastStyledBody;

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
        public static ConfigEntry<float> FineWood;
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
            Wood = Config.Bind<float>("Item Values", "Wood", 1, "Wood exchange rate");
            FineWood = Config.Bind<float>("Item Values", "FineWood", 1, "FineWood exchange rate");
            RoundLog = Config.Bind<float>("Item Values", "RoundLog", 1, "RoundLog exchange rate");
            SerpentScale = Config.Bind<float>("Item Values", "SerpentScale", 3, "SerpentScale exchange rate");
            WorldTreeFragment = Config.Bind<float>("Item Values", "WorldTreeFragment", 8, "WorldTreeFragment exchange rate");
            BurningWorldTreeFragment = Config.Bind<float>("Item Values", "BurningWorldTreeFragment", 13, "BurningWorldTreeFragment exchange rate");
            Stone = Config.Bind<float>("Item Values", "Stone", 1, "Stone exchange rate");

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
                    savedValues.FineWood = FineWood.Value;
                    savedValues.RoundLog = RoundLog.Value;
                    savedValues.SerpentScale = SerpentScale.Value;
                    savedValues.WorldTreeFragment = WorldTreeFragment.Value;
                    savedValues.BurningWorldTreeFragment = BurningWorldTreeFragment.Value;
                    savedValues.Root = Root.Value;
                    savedValues.Stone = Stone.Value;

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
                savedValues.Wood = 1;
                savedValues.FineWood = 1;
                savedValues.RoundLog = 1;
                savedValues.SerpentScale = 3;
                savedValues.WorldTreeFragment = 8;
                savedValues.BurningWorldTreeFragment = 13;
                savedValues.Stone = 1;

            }

            RebuildMaterialRates();
            CacheStationAccess();
        }
        private void Start() {
            if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.epicloot")) {
                epicLootAssembly = Chainloader.PluginInfos["randyknapp.mods.epicloot"].Instance.GetType().Assembly;
                epicLootIsMagic = epicLootAssembly.GetType("EpicLoot.ItemDataExtensions").GetMethod("IsMagic", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(ItemDrop.ItemData) }, null);
                epicLootGetRarity = epicLootAssembly.GetType("EpicLoot.ItemDataExtensions").GetMethod("GetRarity", BindingFlags.Public | BindingFlags.Static);
                epicLootGetEnchantCosts = epicLootAssembly.GetType("EpicLoot.Crafting.EnchantHelper").GetMethod("GetEnchantCosts", BindingFlags.Public | BindingFlags.Static);
                Dbgl($"Loaded Epic Loot assembly; epicLootIsMagic {epicLootIsMagic != null}, epicLootGetRarity {epicLootGetRarity != null}, epicLootGetEnchantCosts {epicLootGetEnchantCosts != null}");
            }

            TryPatchSeneaLRepairTip();

            currencyPocketLoaded = Chainloader.PluginInfos.ContainsKey(CurrencyPocketGuid);
            if (currencyPocketLoaded)
                Dbgl("Detected Azumatt CurrencyPocket; repairs can spend CoinPocket_CoinCount.");

            if (Chainloader.PluginInfos.ContainsKey(SwmarlyQolGuid)) {
                Type pluginType = Chainloader.PluginInfos[SwmarlyQolGuid].Instance.GetType();
                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                swmarlyGetPocketCoins = pluginType.GetMethod("GetPocketCoins", flags, null, new[] { typeof(Player) }, null);
                swmarlySetPocketCoins = pluginType.GetMethod("SetPocketCoins", flags, null, new[] { typeof(Player), typeof(int) }, null);
                Dbgl($"Loaded Swmarly Valheim QOL coin pocket; GetPocketCoins {swmarlyGetPocketCoins != null}, SetPocketCoins {swmarlySetPocketCoins != null}");
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

                RefreshRepairOffer(___m_tempWornItems);
                CopyWornOrder(___m_tempWornItems);
                ApplyRepairTooltipSource();
                StyleRepairTooltipText();
            }
        }

        [HarmonyPatch(typeof(UITooltip), "UpdateTextElements")]
        static class UITooltip_UpdateTextElements_Patch {
            static void Postfix(UITooltip __instance) {
                if (!modEnabled.Value || __instance == null || string.IsNullOrEmpty(repairTooltipBody))
                    return;
                if (!IsRepairButtonTooltip(__instance))
                    return;

                __instance.m_topic = repairTooltipTopic;
                __instance.m_text = repairTooltipBody;
                StyleRepairTooltipText();
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "CanRepair")]
        static class InventoryGui_CanRepair_Patch {
            static void Postfix(ItemDrop.ItemData item, ref bool __result) {
                if (!modEnabled.Value)
                    return;

                // Valheim 1.0 CanRepair can return true at the wrong station via the world-level fallback.
                if (!CanEverBeRepaired(item) || !IsAtRequiredRepairStation(item))
                    __result = false;

                if (paidRepairFrame == Time.frameCount && paidRepairItem == item)
                    return;

                if (!allowUnpaidCanRepair && __result && HasPositiveCost(RepairReqs(item)) && (paidRepairFrame != Time.frameCount || paidRepairItem != item))
                    __result = false;
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "RepairOneItem")]
        static class InventoryGui_RepairOneItem_Patch {
            static bool Prefix() {
                if (!modEnabled.Value)
                    return true;
                allowUnpaidCanRepair = false;
                try {
                    return TryPayForNextRepair();
                } catch {
                    return false;
                }
            }

            static void Postfix() {
                allowUnpaidCanRepair = true;
            }
        }

        private static bool HasEnoughRequirements(List<Piece.Requirement> reqs) {
            if (reqs == null || Player.m_localPlayer == null)
                return true;
            foreach (Piece.Requirement requirement in reqs) {
                if (!requirement.m_resItem)
                    continue;
                if (CountNamedItems(Player.m_localPlayer.GetInventory(), requirement.m_resItem.m_itemData.m_shared.m_name) < requirement.m_amount)
                    return false;
            }
            return true;
        }

        private static bool HasPositiveCost(List<Piece.Requirement> reqs) {
            if (reqs == null)
                return false;
            foreach (Piece.Requirement req in reqs) {
                if (req != null && req.m_amount > 0)
                    return true;
            }
            return false;
        }

        private static void CopyWornOrder(List<ItemDrop.ItemData> dest) {
            if (dest == null)
                return;
            dest.Clear();
            for (int i = 0; i < orderedWornItems.Count; i++)
                dest.Add(orderedWornItems[i]);
        }

        private static int ComputeOfferStamp(IEnumerable<ItemDrop.ItemData> wornItems) {
            int stamp = 17;
            if (wornItems != null) {
                foreach (ItemDrop.ItemData item in wornItems) {
                    if (item == null)
                        continue;
                    stamp = (stamp * 31) + RuntimeHelpers.GetHashCode(item);
                    stamp = (stamp * 31) + item.m_quality;
                    stamp = (stamp * 31) + (int)(item.m_durability * 10f);
                }
            }
            stamp = (stamp * 31) + CountNamedItems(Player.m_localPlayer != null ? Player.m_localPlayer.GetInventory() : null, "$item_coins");
            CraftingStation current = Player.m_localPlayer != null ? Player.m_localPlayer.GetCurrentCraftingStation() : null;
            stamp = (stamp * 31) + (current ? current.GetInstanceID() : 0);
            stamp = (stamp * 31) + GetLiveStationLevel(current);
            stamp = (stamp * 31) + (showAllRepairsInToolTip.Value ? 1 : 0);
            stamp = (stamp * 31) + (coinOnly.Value ? 1 : 0);
            return stamp;
        }

        private static void RefreshRepairOffer(IEnumerable<ItemDrop.ItemData> wornItems) {
            int stamp = ComputeOfferStamp(wornItems);
            if (stamp == lastOfferStamp && orderedWornItems.Count >= 0 && repairTooltipBody.Length > 0)
                return;
            lastOfferStamp = stamp;

            List<RepairItemData> freeRepairs = new List<RepairItemData>();
            List<RepairItemData> enoughRepairs = new List<RepairItemData>();
            List<RepairItemData> notEnoughRepairs = new List<RepairItemData>();
            List<RepairItemData> unableRepairs = new List<RepairItemData>();
            List<string> richLines = new List<string>();
            senealOfferLines.Clear();
            orderedWornItems.Clear();

            if (wornItems != null) {
                foreach (ItemDrop.ItemData item in wornItems) {
                    if (!CanEverBeRepaired(item))
                        continue;
                    if (!IsAtRequiredRepairStation(item)) {
                        unableRepairs.Add(new RepairItemData(item));
                        continue;
                    }
                    List<Piece.Requirement> reqs = RepairReqs(item);
                    if (reqs == null) {
                        freeRepairs.Add(new RepairItemData(item));
                        continue;
                    }
                    List<string> reqRich = new List<string>();
                    List<string> reqPlain = new List<string>();
                    foreach (Piece.Requirement req in reqs) {
                        if (req.m_amount == 0)
                            continue;
                        reqRich.Add(FormatRequirementLine(req, true));
                        reqPlain.Add(FormatRequirementLine(req, false));
                    }
                    RepairItemData row = new RepairItemData(item, reqRich, reqPlain);
                    if (!HasEnoughRequirements(reqs))
                        notEnoughRepairs.Add(row);
                    else
                        enoughRepairs.Add(row);
                }
            }

            foreach (RepairItemData rid in freeRepairs) {
                string name = Localization.instance.Localize(rid.item.m_shared.m_name);
                richLines.Add("<color=#FFFFFFFF>" + name + "</color>: <color=#00FF00FF>Free</color>");
                senealOfferLines.Add(new RepairOfferLine { Name = name, Cost = "Free", Note = "", Icon = GetItemIcon(rid.item) });
                orderedWornItems.Add(rid.item);
            }
            foreach (RepairItemData rid in enoughRepairs)
                AddCostLines(rid, richLines);
            foreach (RepairItemData rid in notEnoughRepairs)
                AddCostLines(rid, richLines);
            foreach (RepairItemData rid in unableRepairs) {
                string name = Localization.instance.Localize(rid.item.m_shared.m_name);
                string need = GetRepairNeedLabel(rid.item);
                richLines.Add("<color=#FFFFFFFF>" + name + "</color> - <color=#FF0000FF>" + need + "</color>");
                senealOfferLines.Add(new RepairOfferLine { Name = name, Cost = "", Note = need, Icon = GetItemIcon(rid.item) });
                orderedWornItems.Add(rid.item);
            }

            senealCoinCount = Player.m_localPlayer != null ? CountNamedItems(Player.m_localPlayer.GetInventory(), "$item_coins") : 0;
            senealStationLine = GetCurrentStationLevelLabel();

            if (richLines.Count == 0)
                richLines.Add("No worn items at this station.");

            richLines.Add("-------------------- \r\n <b><color=#FFFFFFFF>Coins</color>: <color=#FFFF00FF>" + senealCoinCount + "</color></b>");

            repairTooltipTopic = "Repair an Item";
            repairTooltipBody = (string.IsNullOrEmpty(senealStationLine) ? "" : senealStationLine + "\r\n -------------------- \r\n") + string.Join("\r\n", richLines);
            repairTooltipBodyPlain = BuildSeneaLPlainBody();

            if (!showAllRepairsInToolTip.Value) {
                repairTooltipBody = "Repair costs coins.\r\nCoins: " + senealCoinCount;
                repairTooltipBodyPlain = repairTooltipBody;
            }
        }

        private static void AddCostLines(RepairItemData rid, List<string> richLines) {
            string name = Localization.instance.Localize(rid.item.m_shared.m_name);
            List<string> richReqs = rid.reqstring ?? new List<string>();
            List<string> plainReqs = rid.reqplain ?? new List<string>();
            richLines.Add("<color=#FFFFFFFF>" + name + "</color>: " + string.Join(", ", richReqs.ToArray()));
            senealOfferLines.Add(new RepairOfferLine { Name = name, Cost = string.Join(", ", plainReqs.ToArray()), Note = "", Icon = GetItemIcon(rid.item) });
            orderedWornItems.Add(rid.item);
        }

        private static string BuildSeneaLPlainBody() {
            if (senealOfferLines.Count == 0)
                return "No worn items at this station.";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < senealOfferLines.Count; i++) {
                RepairOfferLine line = senealOfferLines[i];
                if (i > 0)
                    sb.Append("\n\n");
                sb.Append(line.Name);
                if (!string.IsNullOrEmpty(line.Note) && string.IsNullOrEmpty(line.Cost))
                    sb.Append("\n    ").Append(line.Note);
                else if (!string.IsNullOrEmpty(line.Cost))
                    sb.Append("\n    ").Append(line.Cost);
            }
            return sb.ToString();
        }

        private static void RebuildWornItemsFromInventory() {
            lastOfferStamp = int.MinValue;
            List<ItemDrop.ItemData> worn = new List<ItemDrop.ItemData>();
            if (Player.m_localPlayer != null)
                Player.m_localPlayer.GetInventory().GetWornItems(worn);
            RefreshRepairOffer(worn);
        }

        private static bool TryPayForNextRepair() {
            paidRepairItem = null;
            paidRepairFrame = -1;
            if (Player.m_localPlayer == null)
                return true;

            lastOfferStamp = int.MinValue;
            lastExtRefreshFrame = -999;
            List<ItemDrop.ItemData> worn = new List<ItemDrop.ItemData>();
            Player.m_localPlayer.GetInventory().GetWornItems(worn);
            RefreshRepairOffer(worn);

            ItemDrop.ItemData freeItem = null;
            foreach (ItemDrop.ItemData item in worn) {
                if (!CanEverBeRepaired(item) || !IsAtRequiredRepairStation(item))
                    continue;

                List<Piece.Requirement> reqs = RepairReqs(item, true);
                if (!HasPositiveCost(reqs)) {
                    if (freeItem == null)
                        freeItem = item;
                    continue;
                }

                List<string> reqstring = new List<string>();
                foreach (Piece.Requirement req in reqs) {
                    if (req?.m_resItem?.m_itemData?.m_shared == null)
                        continue;
                    reqstring.Add(FormatRequirementLine(req, false));
                }

                if (!HasEnoughRequirements(reqs)) {
                    Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Require " + string.Join(", ", reqstring) + " to repair " + Localization.instance.Localize(item.m_shared.m_name), 0, null);
                    return false;
                }

                ConsumeRepairRequirements(Player.m_localPlayer, reqs);
                paidRepairItem = item;
                paidRepairFrame = Time.frameCount;
                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Used " + string.Join(", ", reqstring) + " to repair " + Localization.instance.Localize(item.m_shared.m_name), 0, null);
                return true;
            }

            if (freeItem != null) {
                paidRepairItem = freeItem;
                paidRepairFrame = Time.frameCount;
                return true;
            }

            if (worn.Count > 0) {
                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Require coins to repair", 0, null);
                return false;
            }

            return true;
        }

        private static bool IsCoinName(string itemName) {
            return itemName == "$item_coins";
        }

        private static int CountNamedItems(Inventory inventory, string itemName) {
            if (inventory == null || string.IsNullOrEmpty(itemName))
                return 0;

            int count = inventory.CountItems(itemName, -1, true);
            if (IsCoinName(itemName) && Player.m_localPlayer != null && inventory == Player.m_localPlayer.GetInventory())
                count += GetPocketCoins(Player.m_localPlayer);
            return count;
        }

        private static int GetPocketCoins(Player player) {
            if (player == null)
                return 0;

            if (swmarlyGetPocketCoins != null) {
                try {
                    invokeOne[0] = player;
                    return Math.Max(0, (int)swmarlyGetPocketCoins.Invoke(null, invokeOne));
                } catch { }
            }

            if (player.m_customData == null)
                return 0;

            if (currencyPocketLoaded && TryReadCustomCoins(player, CoinPocketCountKey, out int pocket))
                return pocket;
            if (TryReadCustomCoins(player, SwmarlyCoinKey, out int coins))
                return coins;
            if (TryReadCustomCoins(player, CoinPocketCountKey, out coins))
                return coins;
            return 0;
        }

        private static bool TryReadCustomCoins(Player player, string key, out int coins) {
            coins = 0;
            return player.m_customData.TryGetValue(key, out string value) && int.TryParse(value, out coins) && coins >= 0;
        }

        private static void SetPocketCoins(Player player, int coins) {
            if (player == null)
                return;

            coins = Math.Max(0, coins);
            if (swmarlySetPocketCoins != null) {
                try {
                    invokeTwo[0] = player;
                    invokeTwo[1] = coins;
                    swmarlySetPocketCoins.Invoke(null, invokeTwo);
                    return;
                } catch { }
            }

            if (player.m_customData == null)
                return;

            if (currencyPocketLoaded || player.m_customData.ContainsKey(CoinPocketCountKey))
                player.m_customData[CoinPocketCountKey] = coins.ToString();
            if (!currencyPocketLoaded || player.m_customData.ContainsKey(SwmarlyCoinKey))
                player.m_customData[SwmarlyCoinKey] = coins.ToString();
        }

        private static void SpendCoins(Player player, int amount) {
            if (player == null || amount <= 0)
                return;

            Inventory inventory = player.GetInventory();
            int inventoryCoins = inventory != null ? inventory.CountItems("$item_coins", -1, true) : 0;

            int fromInventory = Math.Min(inventoryCoins, amount);
            if (fromInventory > 0)
                RemoveNamedItems(inventory, "$item_coins", fromInventory);

            int remainder = amount - fromInventory;
            if (remainder > 0)
                SetPocketCoins(player, GetPocketCoins(player) - remainder);
        }

        private static void RemoveNamedItems(Inventory inventory, string itemName, int amount) {
            if (inventory == null || string.IsNullOrEmpty(itemName) || amount <= 0)
                return;
            inventory.RemoveItem(itemName, amount, -1, true);
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
                string itemName = requirement.m_resItem.m_itemData.m_shared.m_name;
                if (IsCoinName(itemName))
                    SpendCoins(player, requirement.m_amount);
                else
                    RemoveNamedItems(inventory, itemName, requirement.m_amount);
            }
        }

        private static void TryPatchSeneaLRepairTip() {
            Type craftView = AccessTools.TypeByName("SeneaLUI.Inv.CraftView");
            MethodInfo repairTip = craftView != null ? AccessTools.Method(craftView, "RepairTip") : null;
            if (repairTip == null)
                return;

            Harmony harmony = new Harmony("cjayride.RepairRequiresCoins.seneal");
            harmony.Patch(repairTip, postfix: new HarmonyMethod(typeof(BepInExPlugin), nameof(SeneaLRepairTipPostfix)));

            Type tooltipType = AccessTools.TypeByName("SeneaLUI.Components.Tooltip");
            MethodInfo build = tooltipType != null ? AccessTools.Method(tooltipType, "Build") : null;
            MethodInfo layRow = tooltipType != null ? AccessTools.Method(tooltipType, "LayRow") : null;
            if (build != null)
                harmony.Patch(build, prefix: new HarmonyMethod(typeof(BepInExPlugin), nameof(SeneaLTooltipBuildPrefix)));
            if (layRow != null)
                harmony.Patch(layRow, postfix: new HarmonyMethod(typeof(BepInExPlugin), nameof(SeneaLTooltipLayRowPostfix)));

            Dbgl("Patched SeneaL CraftView.RepairTip");
        }

        private static Sprite GetItemIcon(ItemDrop.ItemData item) {
            if (item == null)
                return null;
            try {
                return item.GetIcon();
            } catch {
                return item.m_shared != null && item.m_shared.m_icons != null && item.m_shared.m_icons.Length > 0
                    ? item.m_shared.m_icons[0]
                    : null;
            }
        }

        private static void SeneaLTooltipBuildPrefix(object tip) {
            senealDrawingOurTip = false;
            foreach (RepairOfferLine line in senealOfferLines)
                line.IconUsed = false;
            if (tip == null)
                return;
            string source = Traverse.Create(tip).Field("Source").GetValue<string>();
            senealDrawingOurTip = source == "RepairRequiresCoins";
        }

        private static void SeneaLTooltipLayRowPostfix(object row, object r) {
            if (!senealDrawingOurTip)
                return;

            try {
                object data = row;
                object ui = r;
                if (!(Traverse.Create(ui).Field("Label").GetValue() is TMP_Text)) {
                    if (Traverse.Create(data).Field("Label").GetValue() is TMP_Text) {
                        object swap = data;
                        data = ui;
                        ui = swap;
                    } else {
                        return;
                    }
                }

                string name = Traverse.Create(data).Field("Label").GetValue() as string;
                TMP_Text label = Traverse.Create(ui).Field("Label").GetValue() as TMP_Text;
                if (label == null)
                    return;

                ApplySeneaLRowIcon(label, TakeOfferIcon(name));
            } catch {
            }
        }

        private static Sprite TakeOfferIcon(string name) {
            if (string.IsNullOrEmpty(name))
                return null;
            foreach (RepairOfferLine line in senealOfferLines) {
                if (line.IconUsed || line.Name != name)
                    continue;
                line.IconUsed = true;
                return line.Icon;
            }
            return null;
        }

        private static void ApplySeneaLRowIcon(TMP_Text label, Sprite icon) {
            if (label == null)
                return;

            const string childName = "RepairRequiresCoinsIcon";
            const float size = 20f;
            const float gap = 6f;
            Transform existing = label.transform.Find(childName);
            if (icon == null) {
                if (existing)
                    existing.gameObject.SetActive(false);
                label.margin = new Vector4(0f, label.margin.y, label.margin.z, label.margin.w);
                return;
            }

            RectTransform iconRt;
            Image image;
            if (existing == null) {
                GameObject go = new GameObject(childName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.layer = label.gameObject.layer;
                go.transform.SetParent(label.transform, false);
                iconRt = go.GetComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0f, 0.5f);
                iconRt.anchorMax = new Vector2(0f, 0.5f);
                iconRt.pivot = new Vector2(0f, 0.5f);
                iconRt.sizeDelta = new Vector2(size, size);
                iconRt.anchoredPosition = new Vector2(0f, 0f);
                image = go.GetComponent<Image>();
                image.raycastTarget = false;
                image.preserveAspect = true;
                image.type = Image.Type.Simple;
            } else {
                iconRt = existing as RectTransform;
                image = existing.GetComponent<Image>();
                existing.gameObject.SetActive(true);
            }

            if (image != null) {
                image.sprite = icon;
                image.overrideSprite = icon;
                image.color = Color.white;
                image.material = null;
            }
            if (iconRt != null) {
                iconRt.sizeDelta = new Vector2(size, size);
                iconRt.anchoredPosition = new Vector2(0f, 0f);
                iconRt.localScale = Vector3.one;
            }

            label.margin = new Vector4(size + gap, label.margin.y, label.margin.z, label.margin.w);
        }

        private static void SeneaLRepairTipPostfix(object __result) {
            if (!modEnabled.Value || __result == null)
                return;

            if (senealOfferLines.Count == 0 && string.IsNullOrEmpty(repairTooltipBodyPlain))
                RebuildWornItemsFromInventory();

            Traverse tip = Traverse.Create(__result);
            if (tip.Method("Reset", new[] { typeof(string), typeof(int) }).MethodExists())
                tip.Method("Reset", new[] { typeof(string), typeof(int) }).GetValue("Repair", UnityEngine.Random.Range(1, int.MaxValue));

            if (tip.Field("Title").FieldExists())
                tip.Field("Title").SetValue("Repair");
            if (tip.Field("Sub").FieldExists()) {
                string sub = senealStationLine;
                if (string.IsNullOrEmpty(sub))
                    sub = "Coins: " + senealCoinCount;
                else
                    sub = sub + "   ·   Coins: " + senealCoinCount;
                tip.Field("Sub").SetValue(sub);
            }
            if (tip.Field("Warn").FieldExists())
                tip.Field("Warn").SetValue("");
            if (tip.Field("RichText").FieldExists())
                tip.Field("RichText").SetValue(false);
            if (tip.Field("Source").FieldExists())
                tip.Field("Source").SetValue("RepairRequiresCoins");
            if (tip.Field("Icon").FieldExists())
                tip.Field("Icon").SetValue(null);

            IList rows = tip.Field("Rows").FieldExists() ? tip.Field("Rows").GetValue() as IList : null;
            rows?.Clear();
            IList footer = tip.Field("Footer").FieldExists() ? tip.Field("Footer").GetValue() as IList : null;
            footer?.Clear();

            if (senealOfferLines.Count == 0) {
                if (tip.Field("Body").FieldExists())
                    tip.Field("Body").SetValue("No worn items at this station.");
                return;
            }

            if (tip.Field("Body").FieldExists())
                tip.Field("Body").SetValue("");

            foreach (RepairOfferLine line in senealOfferLines) {
                string right = !string.IsNullOrEmpty(line.Cost) ? line.Cost : line.Note;
                if (tip.Method("AddRow", new[] { typeof(string), typeof(string), typeof(string) }).MethodExists())
                    tip.Method("AddRow", new[] { typeof(string), typeof(string), typeof(string) }).GetValue(line.Name, right, "");
            }

            if (rows == null || rows.Count == 0) {
                if (tip.Field("Body").FieldExists())
                    tip.Field("Body").SetValue(BuildSeneaLPlainBody());
            }
        }

        private static UITooltip GetRepairButtonTooltip() {
            InventoryGui gui = InventoryGui.instance;
            if (gui == null)
                return null;
            if (inventoryGuiRepairButton == null)
                inventoryGuiRepairButton = AccessTools.Field(typeof(InventoryGui), "m_repairButton");
            Button button = inventoryGuiRepairButton != null ? inventoryGuiRepairButton.GetValue(gui) as Button : null;
            return button ? button.GetComponent<UITooltip>() : null;
        }

        private static bool IsRepairButtonTooltip(UITooltip tooltip) {
            if (tooltip == null)
                return false;
            if (tooltip.transform.name == "RepairButton")
                return true;
            UITooltip repair = GetRepairButtonTooltip();
            return repair != null && tooltip == repair;
        }

        private static void ApplyRepairTooltipSource() {
            UITooltip tooltip = GetRepairButtonTooltip();
            if (tooltip == null)
                return;
            tooltip.m_topic = repairTooltipTopic;
            tooltip.m_text = repairTooltipBody;
        }

        private static void StyleRepairTooltipText() {
            if (uiTooltipObject == null)
                uiTooltipObject = AccessTools.Field(typeof(UITooltip), "m_tooltip");
            if (uiTooltipCurrent == null)
                uiTooltipCurrent = AccessTools.Field(typeof(UITooltip), "m_current");
            GameObject go = uiTooltipObject != null ? uiTooltipObject.GetValue(null) as GameObject : null;
            UITooltip current = uiTooltipCurrent != null ? uiTooltipCurrent.GetValue(null) as UITooltip : null;
            if (go == null || current == null || !IsRepairButtonTooltip(current))
                return;

            if (cachedTooltipGo != go) {
                cachedTooltipGo = go;
                Transform textChild = Utils.FindChild(go.transform, "Text", Utils.IterativeSearchType.DepthFirst);
                cachedTooltipText = textChild != null ? textChild.GetComponent<TMP_Text>() : null;
                Transform topicChild = Utils.FindChild(go.transform, "Topic", Utils.IterativeSearchType.DepthFirst);
                cachedTopicText = topicChild != null ? topicChild.GetComponent<TMP_Text>() : null;
                lastStyledBody = null;
            }

            TMP_Text tooltipText = cachedTooltipText;
            if (tooltipText == null)
                return;

            string styled = "<b><color=#" + titleTooltipColor.Value + ">" + repairTooltipTopic + "</color></b>\r\n" + repairTooltipBody;
            if (lastStyledBody == styled)
                return;
            lastStyledBody = styled;

            tooltipText.richText = true;
            tooltipText.alignment = TextAlignmentOptions.Bottom;
            tooltipText.fontSize = 20;
            tooltipText.text = styled;

            if (cachedTopicText != null)
                cachedTopicText.text = repairTooltipTopic;
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

        private static bool CanEverBeRepaired(ItemDrop.ItemData item) {
            if (item?.m_shared == null || !item.m_shared.m_canBeReparied)
                return false;

            Recipe recipe = GetCachedRecipe(item);
            if (recipe == null)
                return false;

            return recipe.m_repairStation || recipe.m_craftingStation;
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

            Recipe recipe = GetCachedRecipe(item);
            if (recipe == null || !RecipeRepairsAtStation(recipe, current))
                return false;

            return GetLiveStationLevel(current) >= GetRequiredRepairStationLevel(recipe);
        }

        private static CraftingStation liveLevelStation;
        private static int liveLevelValue;
        private static int liveLevelFrame;
        private static int lastExtRefreshFrame = -999;

        // Count only extensions that still exist. Destroyed bellows/anvils can linger in the cached list.
        private static int GetLiveStationLevel(CraftingStation station) {
            if (!station)
                return 0;

            int frame = Time.frameCount;
            if (liveLevelFrame == frame && liveLevelStation == station)
                return liveLevelValue;

            if (frame - lastExtRefreshFrame >= 20) {
                lastExtRefreshFrame = frame;
                if (stationExtensionTimer != null)
                    stationExtensionTimer.SetValue(station, 999f);
                if (stationGetExtensions != null)
                    stationGetExtensions.Invoke(station, null);
            }

            IList list = stationAttachedExtensions != null ? stationAttachedExtensions.GetValue(station) as IList : null;
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
            Recipe recipe = GetCachedRecipe(item);
            if (recipe == null)
                return "Needs recipe";

            CraftingStation required = GetRequiredRepairStation(recipe);
            if (!required)
                return "cannot repair here";

            string stationName = Localization.instance.Localize(required.m_name);
            return "Needs " + stationName + " Lvl: " + GetRequiredRepairStationLevel(recipe);
        }

        private static void CacheStationAccess() {
            stationExtensionTimer = AccessTools.Field(typeof(CraftingStation), "m_updateExtensionTimer");
            stationGetExtensions = AccessTools.Method(typeof(CraftingStation), "GetExtensions");
            stationAttachedExtensions = AccessTools.Field(typeof(CraftingStation), "m_attachedExtensions");
            inventoryGuiRepairButton = AccessTools.Field(typeof(InventoryGui), "m_repairButton");
            uiTooltipCurrent = AccessTools.Field(typeof(UITooltip), "m_current");
            uiTooltipObject = AccessTools.Field(typeof(UITooltip), "m_tooltip");
        }

        private static void RebuildMaterialRates() {
            materialRates.Clear();
            if (savedValues == null)
                return;
            foreach (PropertyInfo prop in typeof(WeaponAndArmorValues).GetProperties())
                materialRates[prop.Name] = Convert.ToSingle(prop.GetValue(savedValues, null));
        }

        private static Recipe GetCachedRecipe(ItemDrop.ItemData item) {
            if (item == null || ObjectDB.instance == null)
                return null;
            int frame = Time.frameCount;
            if (recipeCacheFrame != frame) {
                recipeCache.Clear();
                recipeCacheFrame = frame;
            }
            Recipe recipe;
            if (recipeCache.TryGetValue(item, out recipe))
                return recipe;
            recipe = ObjectDB.instance.GetRecipe(item);
            recipeCache[item] = recipe;
            return recipe;
        }

        private static ItemDrop GetCoinsItemDrop() {
            int frame = Time.frameCount;
            if (cachedCoinsDropFrame == frame && cachedCoinsDrop)
                return cachedCoinsDrop;

            GameObject prefab = ObjectDB.instance != null ? ObjectDB.instance.GetItemPrefab("Coins") : null;
            if (prefab == null && ZNetScene.instance != null)
                prefab = ZNetScene.instance.GetPrefab("Coins");
            cachedCoinsDrop = prefab != null ? prefab.GetComponent<ItemDrop>() : null;
            cachedCoinsDropFrame = frame;
            return cachedCoinsDrop;
        }

        private static List<Piece.Requirement> RepairReqs(ItemDrop.ItemData item, bool log = false) {
            float percent = (item.GetMaxDurability() - item.m_durability) / item.GetMaxDurability();
            Recipe fullRecipe = GetCachedRecipe(item);
            if (fullRecipe is null)
                return null;
            Piece.Requirement[] resources = fullRecipe.m_resources;
            List<Piece.Requirement> fullReqs = new List<Piece.Requirement>(resources != null ? resources.Length : 0);
            if (resources != null) {
                for (int r = 0; r < resources.Length; r++)
                    fullReqs.Add(resources[r]);
            }

            int calculatedRepairCoinCost = 0;

            bool isMagic = false;
            if (epicLootAssembly != null) {
                try {
                    invokeOne[0] = item;
                    isMagic = (bool)epicLootIsMagic.Invoke(null, invokeOne);
                } catch { }
            }
            if (isMagic) {
                try {
                    invokeOne[0] = item;
                    int rarity = (int)epicLootGetRarity.Invoke(null, invokeOne);
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
            bool hasValuedMaterial = false;
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
                    amount += req.GetAmount(j);
                }

                int fraction = Mathf.RoundToInt(amount * percent * materialRequirementMult.Value);
                req.m_amount = fraction;
                ApplyMaterialRepairCost(req, reqs, ref calculatedRepairCoinCost, ref hasValuedMaterial);
            }

            if (calculatedRepairCoinCost == 0 && hasValuedMaterial && percent > 0)
                calculatedRepairCoinCost = 1;

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

        private static void ApplyMaterialRepairCost(Piece.Requirement req, List<Piece.Requirement> reqs, ref int calculatedRepairCoinCost, ref bool hasValuedMaterial) {
            float rate;
            bool known = TryGetMaterialRate(req.m_resItem, out rate);
            if (known && rate == -1)
                return;

            if (known)
                hasValuedMaterial = true;

            if (known && req.m_amount > 0) {
                if (rate * req.m_amount <= 1)
                    calculatedRepairCoinCost += 1;
                else
                    calculatedRepairCoinCost += Mathf.RoundToInt(rate * req.m_amount);
            }

            if (!coinOnly.Value && req.m_amount > 0)
                reqs.Add(req);
        }

        private static bool TryGetMaterialRate(ItemDrop drop, out float rate) {
            rate = 0;
            if (drop == null)
                return false;

            string prefabName = drop.name;
            if (!string.IsNullOrEmpty(prefabName)) {
                prefabName = prefabName.Replace("(Clone)", "").Trim();
                if (TryGetMaterialRateByProperty(prefabName, out rate))
                    return true;
            }

            string sharedName = drop.m_itemData?.m_shared?.m_name;
            if (string.IsNullOrEmpty(sharedName))
                return false;
            return TryGetMaterialRateByProperty(SharedTokenToPropertyName(sharedName), out rate);
        }

        private static string SharedTokenToPropertyName(string sharedName) {
            if (sharedName.StartsWith("$item_"))
                sharedName = sharedName.Substring(6);
            string[] parts = sharedName.Split('_');
            string property = "";
            foreach (string part in parts) {
                if (part.Length == 0)
                    continue;
                property += char.ToUpperInvariant(part[0]) + (part.Length > 1 ? part.Substring(1) : "");
            }
            return property;
        }

        private static bool TryGetMaterialRateByProperty(string propertyName, out float rate) {
            rate = 0;
            return !string.IsNullOrEmpty(propertyName) && materialRates.TryGetValue(propertyName, out rate);
        }
    }

    internal sealed class ItemRefComparer : IEqualityComparer<ItemDrop.ItemData> {
        public static readonly ItemRefComparer Instance = new ItemRefComparer();
        public bool Equals(ItemDrop.ItemData x, ItemDrop.ItemData y) { return ReferenceEquals(x, y); }
        public int GetHashCode(ItemDrop.ItemData obj) { return RuntimeHelpers.GetHashCode(obj); }
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
    public float FineWood { get; set; }
    public float RoundLog { get; set; }
    public float SerpentScale { get; set; }
    public float WorldTreeFragment { get; set; }
    public float BurningWorldTreeFragment { get; set; }
    public float Stone { get; set; }
}