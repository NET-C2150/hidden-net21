
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace HiddenGamemode
{
	public class RoundInfo : Panel
	{
		public Panel Container;
		public Label RoundName;
		public Label TimeLeft;

		public RoundInfo()
		{
			Container = Add.Panel( "roundContainer" );
			RoundName = Container.Add.Label( "Round", "roundName" );
			TimeLeft = Container.Add.Label( "00:00", "timeLeft" );
		}

		public override void Tick()
		{
			var player = Sandbox.Player.Local;
			if ( player == null ) return;

			var game = GameBase.Current as Game;
			if ( game == null ) return;

			RoundName.Text = game.RoundName;

			var timeLeft = game.RoundTimeLeft;

			if ( timeLeft  > 0f )
			{
				var mins = Math.Round( timeLeft / 60 );
				var secs = Math.Round( timeLeft % 60 );
				TimeLeft.Text = string.Format( "{0}:{1}", mins, secs );
				TimeLeft.SetClass( "hidden", false );
			}
			else
			{
				TimeLeft.SetClass( "hidden", true );
			}
		}
	}
}
