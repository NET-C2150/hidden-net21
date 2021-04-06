
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace HiddenGamemode
{
	public class Crosshair : Panel
	{
		public Label Health;

		int fireCounter;

		public Crosshair()
		{
			StyleSheet = StyleSheet.FromFile( "/ui/Crosshair.scss" );

			for ( int i = 0; i < 5; i++ )
			{
				var p = Add.Panel( "element" );
				p.AddClass( $"el{i}" );
			}
		}

		public override void Tick()
		{
			base.Tick();
			this.PositionAtCrosshair();

			SetClass( "fire", fireCounter > 0 );

			if ( fireCounter > 0 )
				fireCounter--;
		}

		public override void OnEvent( string eventName )
		{
			if ( eventName == "fire" )
			{
				fireCounter += 2;
			}

			base.OnEvent( eventName );
		}
	}
}
