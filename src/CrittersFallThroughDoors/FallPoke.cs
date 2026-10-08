using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CrittersFallThroughDoors
{
	/// <summary>
	/// Makes the fall start promptly. Whether a critter should fall is a behaviour precondition its
	/// brain evaluates on the brain scheduler's round-robin, so after a door opens the critter could
	/// stand on nothing for a moment. Every navigation grid raises an event after it has updated a
	/// batch of cells; when that batch holds a cell above an open door that is no longer walkable, the
	/// brain of each critter standing in it, on that grid, is updated right away. The brain then runs
	/// its normal chore selection, in which the fall check now succeeds.
	///
	/// The event fires on the main thread inside the game's own graph update, with and without Fast
	/// Track (its path cache keeps that call), and updating a critter brain directly from the main
	/// thread is what Fast Track itself does for non-duplicant brains it queues.
	/// </summary>
	[HarmonyPatch(typeof(Pathfinding), nameof(Pathfinding.AddNavGrid))]
	public static class FallPoke
	{
		public static void Postfix(NavGrid nav_grid)
		{
			nav_grid.OnNavGridUpdateComplete += cells => OnGridUpdated(nav_grid, cells);
		}

		private static void OnGridUpdated(NavGrid grid, List<int> cells)
		{
			if (KMonoBehaviour.isLoadingScene || Components.Brains.Count == 0)
				return;
			HashSet<int> opened = null;
			foreach (int cell in cells)
			{
				int below = Grid.CellBelow(cell);
				if (!Grid.IsValidCell(below) || !Grid.HasDoor[below] || !Grid.FakeFloor[below])
					continue;
				if (Grid.Solid[below] || Grid.CritterImpassable[below] || grid.NavTable.IsValid(cell, NavType.Floor))
					continue;
				(opened ?? (opened = new HashSet<int>())).Add(cell);
			}
			if (opened == null)
				return;
			foreach (Brain brain in Components.Brains.Items)
			{
				if (!brain.IsRunning() || !opened.Contains(Grid.PosToCell(brain)))
					continue;
				Navigator navigator = brain.GetComponent<Navigator>();
				if (navigator == null || navigator.NavGrid != grid)
					continue;
				StateMachineController controller = brain.GetComponent<StateMachineController>();
				if (controller == null || controller.GetSMI<CreatureFallMonitor.Instance>() == null)
					continue;
				brain.UpdateBrain();
			}
		}
	}
}
