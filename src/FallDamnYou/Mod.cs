using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using UnityEngine;

namespace FallDamnYou
{
	public sealed class FallDamnYouMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			PUtil.InitLibrary(false);
			new POptions().RegisterOptions(this, typeof(Options));
			Debug.Log("[FallDamnYou] Loaded version " + typeof(FallDamnYouMod).Assembly.GetName().Version);
		}
	}

	/// <summary>
	/// A door marks its top row of cells as "fake floor" for its whole life, open or closed, and the
	/// floor validator that builds every creature's navigation table treats any fake floor as ground.
	/// That is why a critter keeps standing on a door after it opens and can walk across an open one,
	/// while an already falling critter passes straight through (gravity only stops at solid cells).
	///
	/// For critters (the validator's is_dupe flag is false; duplicants and robots pass true) a door
	/// the critter could walk through, meaning open to critters and not solid, no longer counts as
	/// floor. The navigation table then marks the cell above as not walkable, the critter's fall check
	/// finds no floor there and no solid under its feet, and it falls through, the same way it would
	/// if the tile had been dug out. A closed or locked door, or an automatic door that merely opened
	/// for a duplicant, is still impassable to critters and stays floor.
	/// Option "Open doors are open air to critters".
	/// </summary>
	[HarmonyPatch(typeof(GameNavGrids.FloorValidator), nameof(GameNavGrids.FloorValidator.IsWalkableCell))]
	public static class FloorValidator_IsWalkableCell_Patch
	{
		public static void Postfix(int anchor_cell, bool is_dupe, ref bool __result)
		{
			if (!__result || is_dupe || !Settings.OpenDoorsFall)
				return;
			if (!Grid.HasDoor[anchor_cell] || !Grid.FakeFloor[anchor_cell])
				return;
			if (Grid.Solid[anchor_cell] || Grid.CritterImpassable[anchor_cell])
				return;
			__result = false;
		}
	}
}
