﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiddenGamemode
{
	public class StatsRound : BaseRound
	{
		public override int RoundDuration => 10;

		protected override void OnStart()
		{
			Log.Info( "Started Stats Round" );
		}

		protected override void OnFinish()
		{
			Log.Info( "Finished Stats Round" );
		}

		protected override void OnTimeUp()
		{
			Log.Info( "Stats Time Up!" );

			Game.CurrentRound = new HideRound();

			base.OnTimeUp();
		}

		public override void OnPlayerSpawn( Player player )
		{
			if ( Players.Contains( player ) ) return;

			player.SetModel( "models/citizen/citizen.vmdl" );
			player.Hide();

			AddPlayer( player );

			base.OnPlayerSpawn( player );
		}
	}
}
