using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace RF4Plow;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
	internal static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("RF4Plow");

	public override void Load()
	{
		Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} is loading!");

		Harmony.CreateAndPatchAll(typeof(RF4PlowBehaviour));

		Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} is loaded!");
	}

	class NewGroundItemData
	{
		public bool createNew;
		public ItemData itemData;
		public Vector3 pos;
		public NewGroundItemData(ItemData item, Vector3 pos, bool create)
		{
			this.itemData = item;
			this.pos = pos;
			this.createNew = create;
		}
	}

	[HarmonyPatch]
	class RF4PlowBehaviour
	{
		[HarmonyPatch(typeof(OnGroundItem), nameof(OnGroundItem.DoPlow))]
		[HarmonyPrefix]
		public static void PopCorns(OnGroundItem __instance, out NewGroundItemData __state)
		{
			var itemData = __instance.ItemData;
			var itemCount = itemData.Amount;
			var createNew = itemCount > 1;
			var popItem = createNew ? itemData.Pop(itemData.Amount - 1) : itemData;
			__state = new NewGroundItemData(popItem, __instance.transform.position, createNew);
		}

		[HarmonyPatch(typeof(OnGroundItem), nameof(OnGroundItem.DoPlow))]
		[HarmonyPostfix]
		public static void SpawnCorns(NewGroundItemData __state)
		{
			if (__state.createNew)
			{
				var newItem = OnGroundItem.CreateOnGroundItem(__state.itemData, OnGroundItem.SpawnReason.FromFarm);
				newItem.DropStartPoint = __state.pos;
				newItem.DropEndPoint = __state.pos;
			}
		}
	}
}
