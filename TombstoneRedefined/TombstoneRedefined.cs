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

namespace TombstoneRedefined
{
    [BepInPlugin("net.bdew.valheim.tombstoneredefined", "TombstoneRedefined", "0.0.1")]
    public class TombstoneRedefined: BaseUnityPlugin
    {
        private static ManualLogSource LogSource;

        void Awake()
        {
            LogSource = Logger;
            LogSource.LogInfo(String.Format("TombstoneRedefined is Awake"));
            Harmony.CreateAndPatchAll(typeof(TombstoneRedefined));
            //Player 
        }
    }
}
