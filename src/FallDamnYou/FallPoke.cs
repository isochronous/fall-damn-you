using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace FallDamnYou
{
	/// <summary>
	/// Makes a critter fall the moment a door takes its footing away, instead of standing there until
	/// it figures out it is supposed to. Whether a critter should fall is a behaviour precondition its
	/// brain evaluates on the brain scheduler's round-robin, so the game itself can leave it hanging
	/// for a while, as in a standard pez dropper (a pneumatic door closing on a critter that stands on
	/// another pneumatic door).
	///
	/// Every navigation grid raises an event after it has updated a batch of cells. Within a second of
	/// a door changing state, the batch is scanned for cells that have a door in or next to them; a
	/// critter standing in such a cell whose own navigation type is no longer valid there has its brain
	/// updated right away, which runs the game's normal chore selection and with it the fall check.
	/// What counts as "floor" is left entirely to the game and to other mods: Sgt_Imalas's Critters
	/// Fall Through Open Doors makes open doors non-floor (and non-ceiling) for critters, and this
	/// triggers on the resulting navigation change just as it does on a door closing on a critter.
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
				if (!NearDoor(cell) || !CrittersAt(cell, out List<Brain> critters))
					continue;
				foreach (Brain brain in critters)
				{
					Navigator navigator = brain.GetComponent<Navigator>();
					if (navigator == null || navigator.NavGrid != grid || !brain.IsRunning())
						continue;
					if (!grid.NavTable.IsValid(cell, navigator.CurrentNavType))
						brain.UpdateBrain();
				}
			}
		}

		/// <summary>
		/// A door in the cell or in one of its four orthogonal neighbours: floor and ceiling anchors, and walls
		/// for crawlers. The door flag is set on every cell a door occupies, so each of the six cells around a
		/// two-cell door, horizontal or vertical, sees it; diagonals are never navigation anchors.
		/// </summary>
		private static bool NearDoor(int cell)
		{
			if (Grid.HasDoor[cell])
				return true;
			int other = Grid.CellBelow(cell);
			if (Grid.IsValidCell(other) && Grid.HasDoor[other])
				return true;
			other = Grid.CellAbove(cell);
			if (Grid.IsValidCell(other) && Grid.HasDoor[other])
				return true;
			other = Grid.CellLeft(cell);
			if (Grid.IsValidCell(other) && Grid.HasDoor[other])
				return true;
			other = Grid.CellRight(cell);
			return Grid.IsValidCell(other) && Grid.HasDoor[other];
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
