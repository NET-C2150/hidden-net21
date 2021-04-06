using Sandbox;
using System.Threading.Tasks;

namespace HiddenGamemode
{
	[ClassLibrary( "sbox-hidden", Title = "Hidden" )]
	partial class Game : Sandbox.Game
	{
		internal static MilitaryTeam MilitaryTeam;
		internal static HiddenTeam HiddenTeam;

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

		[Net] public string RoundName { get; set; }
		[Net] public float RoundTimeLeft { get; set; }

		private static BaseRound _round;

		private int MinimumPlayers = 1; // 2

		public Game()
		{
			if ( IsServer )
			{
				_ = new Hud();
			}
		}

		public async Task StartSecondTimer()
		{
			await Task.DelaySeconds( 1 );


			if ( CurrentRound != null )
			{
				// TODO: I'm not a fan of this.
				RoundName = CurrentRound.RoundName;
				RoundTimeLeft = CurrentRound.TimeLeft;

				CurrentRound.OnSecond();
			}

			await StartSecondTimer();
		}

		public override void PostLevelLoaded()
		{
			MilitaryTeam = new();
			HiddenTeam = new();

			_ = StartSecondTimer();

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

			CurrentRound?.OnPlayerLeave( player as Player );

			CheckMinimumPlayers();

			base.PlayerDisconnected( player, reason );
		}

		public override Player CreatePlayer() => new Player();

		private void CheckMinimumPlayers()
		{
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
