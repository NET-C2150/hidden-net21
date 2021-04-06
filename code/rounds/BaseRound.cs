using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiddenGamemode
{
    public abstract class BaseRound
	{
		public virtual int RoundDuration => 0;

		public List<Player> Players = new();

		public void Start()
		{
			if ( RoundDuration > 0 )
			{
				_ = WaitForTimeOver();
			}

			OnStart();
		}

		public void Finish()
		{
			Players.Clear();
			OnFinish();
		}

		public void AddPlayer( Player player )
		{
			if ( !Players.Contains(player) )
			{
				Players.Add( player );
			}
		}

		public virtual void OnPlayerSpawn( Player player ) { }

		public virtual void OnPlayerKilled( Player player ) { }

		public virtual void OnPlayerLeave( Player player )
		{
			Players.Remove( player );
		}

		protected virtual void OnStart() { }

		protected virtual void OnFinish() { }

		protected virtual void OnTimeUp() { }

		protected async Task WaitForTimeOver()
		{
			await Task.Delay( RoundDuration * 1000 );
			OnTimeUp();
		}
	}
}
