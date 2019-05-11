using nucs.JsonSettings;

namespace Arbatel
{
	/// <summary>
	/// Settings that are per-user and machine independent.
	/// </summary>
	public class RoamingSettings : JsonSettings
	{
		public override string FileName { get; set; }

		public string LastBuiltInPalette { get; set; } = "";

		public bool InvertMouseX { get; set; } = false;
		public bool InvertMouseY { get; set; } = false;

		public int MouseSensitivity { get; set; } = 50;

		/// <summary>
		/// The camera's movement speed, in units per frame.
		/// </summary>
		public int MovementSpeed { get; set; } = 64;
	}
}
