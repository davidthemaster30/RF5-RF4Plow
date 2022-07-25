using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.IL2CPP;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace RF4Plow {
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public class Plugin : BasePlugin {

		internal static new ManualLogSource Log;

		public override void Load() {
			// Plugin startup logic

			Log = base.Log;
			Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

			Harmony.CreateAndPatchAll(typeof(RF4PlowBehaviour));
		}

		class NewGroundItemData {
			public ItemData itemData;
			public Vector3 pos;
			public bool createNew;
			public NewGroundItemData(ItemData item, Vector3 pos, bool create) {
				this.itemData = item;
				this.pos = pos;
				this.createNew = create;
			}
		}

		[HarmonyPatch]
		class RF4PlowBehaviour {
			[HarmonyPatch(typeof(OnGroundItem), nameof(OnGroundItem.DoPlow))]
			[HarmonyPrefix]
			public static void PopCorns(OnGroundItem __instance, out NewGroundItemData __state) {
				var itemData = __instance.ItemData;
				var itemCount = itemData.Amount;
				var createNew = itemCount > 1;
				var popItem = createNew ? itemData.Pop(itemData.Amount - 1) : itemData;
				__state = new NewGroundItemData(popItem, __instance.transform.position, createNew);
			}

			[HarmonyPatch(typeof(OnGroundItem), nameof(OnGroundItem.DoPlow))]
			[HarmonyPostfix]
			public static void SpawnCorns(NewGroundItemData __state) {
				if (__state.createNew) {
					var newItem = OnGroundItem.CreateOnGroundItem(__state.itemData, OnGroundItem.SpawnReason.FromFarm);
					newItem.DropStartPoint = __state.pos;
					newItem.DropEndPoint = __state.pos;
				}
			}
		}
	}
}
