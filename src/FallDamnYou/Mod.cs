using HarmonyLib;
using KMod;
using UnityEngine;

namespace FallDamnYou
{
	public sealed class FallDamnYouMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			Debug.Log("[FallDamnYou] Loaded version " + typeof(FallDamnYouMod).Assembly.GetName().Version);
		}
	}
}
