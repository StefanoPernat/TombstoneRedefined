using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ValheimPlus.Configurations;

namespace TombstoneRedefined {
    [BepInPlugin("net.bdew.valheim.tombstoneredefined", "TombstoneRedefined", "0.0.1")]
    public class TombstoneRedefined: BaseUnityPlugin {
        private static ManualLogSource LogSource;
        private static List<ItemDrop.ItemData> equipedItems;
        private static Player player;

        void Awake() {
            LogSource = Logger;
            LogSource.LogInfo(string.Format("TombstoneRedefined is Awake"));
            Harmony.CreateAndPatchAll(typeof(TombstoneRedefined));
            //Player 
        }

        [HarmonyPatch(typeof(Player), "OnSpawned")]
        [HarmonyPrefix]
        static void GetPlayerInstanceWhenSpawned(Player __instance) {
            player = __instance;
            LogSource.LogInfo(string.Format("{0} is SPAWNED", player.GetPlayerName()));
        }

        // track the dead of a player
        [HarmonyPatch(typeof(Player), "OnDeath")]
        [HarmonyPrefix]
        static void GetInventoryItems(Player __instance) {
            LogSource.LogInfo(string.Format("{0} is DEAD", __instance.GetPlayerName()));
            equipedItems = filterWearableItemsFrom(__instance.GetInventory().GetEquipedtems());
            PrintItems(equipedItems);
        }

        [HarmonyPatch(typeof(TombStone), "OnTakeAllSuccess")]
        [HarmonyPostfix]
        static void EquipLastItems(TombStone __instance) {
            LogSource.LogInfo(string.Format("{0} has picked up all from TOMBSTONE", player.GetPlayerName()));
            if (player) {
                LogSource.LogInfo(string.Format("{0} is defined", player.GetPlayerName()));
                EquipLastItemsToRespownedPlayer(equipedItems, player);
            }
        }

        private static List<ItemDrop.ItemData> filterWearableItemsFrom(List<ItemDrop.ItemData> equipedItems) {
            List<ItemDrop.ItemData> filtered = new List<ItemDrop.ItemData>();

            foreach (ItemDrop.ItemData item in equipedItems) {
                short armor = (short)item.GetArmor();
                if (item.IsEquipable() && !item.IsWeapon() && armor > 0) {
                    filtered.Add(item);
                }
            }

            return filtered;
        }


        private static void EquipLastItemsToRespownedPlayer(List<ItemDrop.ItemData> list, Player player) {
            Dictionary<string, bool> alreadyEquiped = new Dictionary<string, bool>();
            List<ItemDrop.ItemData> currentInventory = player.GetInventory().GetAllItems();
            foreach (ItemDrop.ItemData item in list){
                if (alreadyEquiped.ContainsKey(item.m_shared.m_name)) {
                    continue;
                } else {
                    List<ItemDrop.ItemData> matchingList = currentInventory.FindAll(
                        i => i.m_shared.m_name.Equals(item.m_shared.m_name)
                    );

                    foreach (ItemDrop.ItemData itemInInventory in matchingList) {
                        if (CompareItem(item, itemInInventory)) {
                            player.EquipItem(itemInInventory, false);
                            alreadyEquiped.Add(itemInInventory.m_shared.m_name, true);
                            break;
                        }
                    }
                }
            }
        }

        private static void PrintItems(List<ItemDrop.ItemData> itemList) {
            foreach (ItemDrop.ItemData item in itemList) {
                LogSource.LogInfo(string.Format("{0} has armor {1}", item.m_shared.m_name, item.GetArmor()));

            }
        }

        private static bool CompareItem(ItemDrop.ItemData first, ItemDrop.ItemData second) {
            if (!first.m_shared.m_name.Equals(second.m_shared.m_name)) {
                return false;
            }

            if (first.IsEquipable() != second.IsEquipable()) {
                return false;
            }

            if (first.IsWeapon() != second.IsWeapon()) {
                return false;
            }

            short armor1 = (short) first.GetArmor();
            short armor2 = (short) second.GetArmor();

            if (armor1 != armor2) {
                return false;
            }

            short durability1 = (short) first.m_shared.m_maxDurability;
            short durability2 = (short) second.m_shared.m_maxDurability;

            if (durability1 != durability2) {
                return false;
            }

            return true;
        }
    }
}
