using System.Collections.Generic;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace FallDamnYou
{
	/// <summary>Mod options. Read once per game load (see Settings), so a change applies when a game is loaded.</summary>
	[JsonObject(MemberSerialization.OptIn)]
	[ConfigFile(SharedConfigLocation: true)]
	public sealed class Options : IOptions
	{
		[Option("Open doors are open air to critters", "Takes effect when a game is loaded.")]
		[JsonProperty]
		public bool OpenDoorsFall { get; set; } = true;

		[Option("Start falling at once", "Takes effect when a game is loaded.")]
		[JsonProperty]
		public bool PromptFall { get; set; } = true;

		// PLib's text block does not wrap, so the line breaks are part of the text.
		private const string Help =
			"Open doors are open air: a door that critters could walk through no longer counts as floor for them.\n"
			+ "A critter standing on a door falls when the door is opened, and critters do not path across open doors.\n"
			+ "Closed and locked doors, and automatic doors that only opened for a duplicant, still carry critters.\n"
			+ "Duplicants and robots are not affected.\n"
			+ "\n"
			+ "Start falling at once: when a door takes a critter's floor away, by opening under it or closing on it,\n"
			+ "the critter's brain runs its fall check right away instead of waiting for its turn on the brain schedule.";

		public IEnumerable<IOptionsEntry> CreateOptions()
		{
			yield return new TextBlockOptionsEntry("Help", new OptionAttribute(Help, "", ""));
		}

		public void OnOptionsChanged()
		{
		}
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
