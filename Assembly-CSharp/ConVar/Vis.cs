namespace ConVar
{
	[Factory("vis")]
	public class Vis : ConsoleSystem
	{
		[Help("Turns on debug display of lerp")]
		[ClientVar]
		public static bool lerp;

		[ServerVar]
		[Help("Turns on debug display of damages")]
		public static bool damage;

		[Help("Turns on debug display of attacks")]
		[ClientVar]
		[ServerVar]
		public static bool attack;

		[ClientVar]
		[Help("Turns on debug display of protection")]
		[ServerVar]
		public static bool protection;

		[ServerVar]
		[Help("Turns on debug display of weakspots")]
		public static bool weakspots;

		[Help("Show trigger entries")]
		[ServerVar]
		public static bool triggers;

		[Help("Turns on debug display of hitboxes")]
		[ServerVar]
		public static bool hitboxes;

		[Help("Turns on debug display of line of sight checks")]
		[ServerVar]
		public static bool lineofsight;

		[ServerVar]
		[Help("Turns on debug display of senses, which are received by Ai")]
		public static bool sense;
	}
}
