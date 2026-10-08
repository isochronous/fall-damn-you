using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace FallDamnYou
{
	/// <summary>
	/// Makes the fall start promptly. Whether a critter should fall is a behaviour precondition its
	/// brain evaluates on the brain scheduler's round-robin, so after a door changes the critter could
	/// stand on nothing for a moment. Every navigation grid raises an event after it has updated a
	/// batch of cells; when that batch holds a cell a critter can no longer stand in because of a door,
	/// the brain of each critter in it is updated right away. The brain then runs its normal chore
	/// selection, in which the fall check decides as it always does.
	///
	/// Two door changes qualify: the door below the cell opened to critters (the fix in Mod.cs made the
	/// cell unwalkable), and the cell is itself a door that closed on the critter (the cell became
	/// impassable; the game then lets the critter fall if what is under it is not solid, which is the
	/// two-pneumatic-door drop players build on purpose).
	///
	/// Cost: nothing unless a door changed state within the last second (digging and building dirty
	/// cells all the time, and those batches are not even looked at). Within that window each dirty
	/// cell costs a few array reads, and the critter positions are gathered once per frame and shared
	/// by all grids, since every grid receives the same dirty cells.
	///
	/// The event fires on the main thread inside the game's own graph update, with and without Fast
	/// Track (its path cache keeps that call), and updating a critter brain directly from the main
	/// thread is what Fast Track itself does for non-duplicant brains it queues.
	/// </summary>
	[HarmonyPatch(typeof(Pathfinding), nameof(Pathfinding.AddNavGrid))]
	public static class FallPoke
	{
		/// <summary>How long after a door state change the navigation batches are inspected. An airlock's solid flag arrives through the sim a tick or two later.</summary>
		private const float DoorWindowSeconds = 1f;

		private static float lastDoorChange = float.NegativeInfinity;
		private static int cacheFrame = -1;
		private static readonly Dictionary<int, List<Brain>> crittersByCell = new Dictionary<int, List<Brain>>();

		public static void Postfix(NavGrid nav_grid)
		{
			nav_grid.OnNavGridUpdateComplete += cells => OnGridUpdated(nav_grid, cells);
		}

		/// <summary>A door changed its passable state: open the inspection window.</summary>
		[HarmonyPatch(typeof(Door), "SetPassableState")]
		public static class Door_SetPassableState_Patch
		{
			public static void Postfix()
			{
				lastDoorChange = Time.realtimeSinceStartup;
			}
		}

		private static void OnGridUpdated(NavGrid grid, List<int> cells)
		{
			if (Time.realtimeSinceStartup - lastDoorChange > DoorWindowSeconds || KMonoBehaviour.isLoadingScene || Components.Brains.Count == 0)
				return;
			foreach (int cell in cells)
			{
				if (!DoorChangedUnderfoot(cell) || grid.NavTable.IsValid(cell, NavType.Floor))
					continue;
				if (!CrittersAt(cell, out List<Brain> critters))
					continue;
				foreach (Brain brain in critters)
				{
					Navigator navigator = brain.GetComponent<Navigator>();
					if (navigator != null && navigator.NavGrid == grid && brain.IsRunning())
						brain.UpdateBrain();
				}
			}
		}

		/// <summary>The cell is a door cell that is closed to critters, or sits on a door that is open to them.</summary>
		private static bool DoorChangedUnderfoot(int cell)
		{
			if (Grid.HasDoor[cell])
				return Grid.CritterImpassable[cell];
			int below = Grid.CellBelow(cell);
			return Grid.IsValidCell(below) && Grid.HasDoor[below] && Grid.FakeFloor[below]
				&& !Grid.Solid[below] && !Grid.CritterImpassable[below];
		}

		/// <summary>Critters (brains with a creature fall monitor) by cell, gathered once per frame.</summary>
		private static bool CrittersAt(int cell, out List<Brain> critters)
		{
			if (cacheFrame != Time.frameCount)
			{
				cacheFrame = Time.frameCount;
				crittersByCell.Clear();
				foreach (Brain brain in Components.Brains.Items)
				{
					if (!brain.IsRunning())
						continue;
					StateMachineController controller = brain.GetComponent<StateMachineController>();
					if (controller == null || controller.GetSMI<CreatureFallMonitor.Instance>() == null)
						continue;
					int at = Grid.PosToCell(brain);
					if (!crittersByCell.TryGetValue(at, out List<Brain> list))
						crittersByCell[at] = list = new List<Brain>();
					list.Add(brain);
				}
			}
			return crittersByCell.TryGetValue(cell, out critters);
		}
	}
}
