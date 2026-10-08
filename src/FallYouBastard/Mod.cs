using HarmonyLib;
using KMod;
using UnityEngine;

namespace FallYouBastard
{
	public sealed class FallYouBastardMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			Debug.Log("[FallYouBastard] Loaded version " + typeof(FallYouBastardMod).Assembly.GetName().Version);
		}
	}
}
