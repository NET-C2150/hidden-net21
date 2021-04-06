using Sandbox;

namespace HiddenGamemode
{
	[ClassLibrary( "sbox-hidden", Title = "Hidden" )]
	partial class Game : Sandbox.Game
	{
		internal static MilitaryTeam MilitaryTeam = new();
		internal static HiddenTeam HiddenTeam = new();

		public Game()
		{
			if ( IsServer )
			{
				_ = new Hud();
			}
		}

		public override void PostLevelLoaded()
		{
			MilitaryTeam = new();
			HiddenTeam = new();

			base.PostLevelLoaded();
		}

		public override Player CreatePlayer() => new Player();
	}
}
