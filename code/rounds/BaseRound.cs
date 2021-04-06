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
		public virtual string RoundName => "";

		public List<Player> Players = new();

		public float RoundEndTime;

		public float TimeLeft
		{
			get
			{
				return RoundEndTime - Sandbox.Time.Now;
			}
		}

		public string TimeLeftFormatted
		{
			get
			{
				var timeLeft = TimeLeft;
				var mins = Math.Round( timeLeft / 60 );
				var secs = Math.Round( timeLeft % 60 );

				//var span = TimeSpan.FromSeconds( TimeLeft );
				//return span.ToString( "mm:ss" );

				return string.Format( "{0}:{1}", mins, secs );
			}
		}

		public void Start()
		{
			if ( RoundDuration > 0 )
			{
				RoundEndTime = Sandbox.Time.Now + RoundDuration;
			}
			
			OnStart();
		}

		public void Finish()
		{
			RoundEndTime = 0f;
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

		public virtual void OnSecond()
		{
			if ( RoundEndTime > 0 && Sandbox.Time.Now >= RoundEndTime )
			{
				RoundEndTime = 0f;
				OnTimeUp();
			}
		}

		protected virtual void OnStart() { }

		protected virtual void OnFinish() { }

		protected virtual void OnTimeUp() { }
	}
}
