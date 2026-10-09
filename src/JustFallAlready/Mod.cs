using HarmonyLib;
using KMod;
using UnityEngine;

namespace JustFallAlready
{
	public sealed class JustFallAlreadyMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			Debug.Log("[JustFallAlready] Loaded version " + typeof(JustFallAlreadyMod).Assembly.GetName().Version);
		}
	}
}
