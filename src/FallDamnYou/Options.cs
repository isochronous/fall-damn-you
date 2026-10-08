using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace FallDamnYou
{
	/// <summary>Mod options. Read once per game load (see Settings), so a change applies when a game is loaded.</summary>
	[JsonObject(MemberSerialization.OptIn)]
	[ConfigFile(SharedConfigLocation: true)]
	public sealed class Options
	{
		[Option("Open doors are open air to critters", "A door that critters could walk through no longer counts as floor for them: a critter standing on a door falls through when the door is opened, and critters do not path across open doors. Closed and locked doors, and automatic doors that only opened for a duplicant, still carry critters. Duplicants and robots are unaffected. Takes effect when a game is loaded.")]
		[JsonProperty]
		public bool OpenDoorsFall { get; set; } = true;

		[Option("Start falling at once", "A critter whose floor a door just took away, by opening under it or by closing on it, has its brain run the fall check right away instead of waiting for its turn on the brain schedule. Takes effect when a game is loaded.")]
		[JsonProperty]
		public bool PromptFall { get; set; } = true;
	}

	/// <summary>The options as they were when the current game was loaded. Both hot paths test a cached flag only.</summary>
	public static class Settings
	{
		private static Game game;
		private static Options current = new Options();

		public static bool OpenDoorsFall => Current.OpenDoorsFall;
		public static bool PromptFall => Current.PromptFall;

		private static Options Current
		{
			get
			{
				if (game != Game.Instance)
				{
					game = Game.Instance;
					current = POptions.ReadSettings<Options>() ?? new Options();
				}
				return current;
			}
		}
	}
}
