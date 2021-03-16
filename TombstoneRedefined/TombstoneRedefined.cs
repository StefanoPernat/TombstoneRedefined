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

namespace TombstoneRedefined {
    [BepInPlugin("net.bdew.valheim.tombstoneredefined", "TombstoneRedefined", "0.0.2")]
    public class TombstoneRedefined: BaseUnityPlugin {
        private static ManualLogSource LogSource;
        private static List<ItemDrop.ItemData> equipedItems;
        private static Player player;

        // Awake Phase, define the Logger
        void Awake() {
            LogSource = Logger;
            LogSource.LogInfo(string.Format("TombstoneRedefined is Awake"));
            Harmony.CreateAndPatchAll(typeof(TombstoneRedefined));
            //Player 
        }

        // When Player is spawned got his instance
        [HarmonyPatch(typeof(Player), "OnSpawned")]
        [HarmonyPrefix]
        static void GetPlayerInstanceWhenSpawned(Player __instance) {
            player = __instance;
        }

        // before the death of a Player I will filter his inventory 
        // looking at his equiped items
        [HarmonyPatch(typeof(Player), "OnDeath")]
        [HarmonyPrefix]
        static void GetInventoryItems(Player __instance) {
            equipedItems = filterWearableItemsFrom(__instance.GetInventory().GetEquipedtems());
            //PrintItems(equipedItems);
        }

        // After the takeall of a Tombstone happends
        // check if player is owner
        // equip the before dead armor
        [HarmonyPatch(typeof(TombStone), "OnTakeAllSuccess")]
        [HarmonyPostfix]
        static void EquipLastItems(TombStone __instance) {
            if (__instance.GetOwner() != player.GetPlayerID()) return;

            LogSource.LogInfo(string.Format("{0} has picked up all from TOMBSTONE", player.GetPlayerName()));
            if (player) {
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
                        i =>    i.m_shared.m_name.Equals(item.m_shared.m_name) &&
                                i.IsEquipable() == item.IsEquipable() &&
                                i.IsWeapon() == item.IsWeapon() &&
                                Mathf.Approximately(i.GetArmor(), item.GetArmor()) &&
                                Mathf.Approximately(i.m_shared.m_maxDurability, item.m_shared.m_maxDurability)
                    );

                    if (matchingList.Count > 0) {
                        ItemDrop.ItemData itemToEquip = matchingList.First();
                        player.EquipItem(itemToEquip, false);
                        alreadyEquiped.Add(itemToEquip.m_shared.m_name, true);
                    }

                    /*List<ItemDrop.ItemData> matchingList = currentInventory.FindAll(
                        i => i.m_shared.m_name.Equals(item.m_shared.m_name)
                    );

                    foreach (ItemDrop.ItemData itemInInventory in matchingList) {
                        if (CompareItem(item, itemInInventory)) {
                            player.EquipItem(itemInInventory, false);
                            alreadyEquiped.Add(itemInInventory.m_shared.m_name, true);
                            break;
                        }
                    }*/
                }
            }
        }

        // debug functions to print items
        private static void PrintItems(List<ItemDrop.ItemData> itemList) {
            foreach (ItemDrop.ItemData item in itemList) {
                LogSource.LogInfo(string.Format("{0} has armor {1}", item.m_shared.m_name, item.GetArmor()));

            }
        }
    }
}
