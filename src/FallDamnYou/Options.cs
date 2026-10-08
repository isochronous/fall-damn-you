using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace FallDamnYou
{
	/// <summary>Mod options. Read once per game load (see Settings), so a change applies when a game is loaded.</summary>
	[JsonObject(MemberSerialization.OptIn)]
	[ConfigFile(SharedConfigLocation: true)]
	public sealed class Options
	{
		[Option("Critters fall through open doors", "Takes effect when a game is loaded.")]
		[JsonProperty]
		public bool OpenDoorsFall { get; set; } = true;

		[Option("Start falling at once", "No standing there until it realizes it should be falling. Takes effect when a game is loaded.")]
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
