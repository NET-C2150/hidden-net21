using Sandbox;

namespace HiddenGamemode
{
	[ClassLibrary( "sbox-hidden", Title = "Hidden" )]
	partial class Game : Sandbox.Game
	{
		internal static MilitaryTeam MilitaryTeam = new();
		internal static HiddenTeam HiddenTeam = new();

		internal static BaseRound CurrentRound
		{
			get => _round;

			set
			{
				if ( value != null )
				{
					_round?.Finish();
					_round = value;
					_round?.Start();
				}
			}
		}

		private static BaseRound _round;

		private int MinimumPlayers = 1; // 2

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

		public override void PlayerKilled( Sandbox.Player player )
		{
			CurrentRound?.OnPlayerKilled( player as Player );

			base.PlayerKilled( player );
		}

		public override void PlayerJoined( Sandbox.Player player )
		{
			Log.Info( player.Name + " joined, checking minimum player count..." );

			CheckMinimumPlayers();

			base.PlayerJoined( player );
		}

		public override void PlayerDisconnected( Sandbox.Player player, NetworkDisconnectionReason reason )
		{
			Log.Info( player.Name + " left, checking minimum player count..." );

			CheckMinimumPlayers();

			base.PlayerDisconnected( player, reason );
		}

		public override Player CreatePlayer() => new Player();

		private void CheckMinimumPlayers()
		{
			Log.Info( "Player Count: " + Sandbox.Player.All.Count );

			if ( Sandbox.Player.All.Count >= MinimumPlayers)
			{
				if ( CurrentRound is LobbyRound || CurrentRound == null )
				{
					CurrentRound = new HideRound();
				}
			}
			else if ( CurrentRound is not LobbyRound )
			{
				CurrentRound = new LobbyRound();
			}
		}
	}
}
