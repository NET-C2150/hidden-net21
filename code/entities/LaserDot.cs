﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiddenGamemode
{
	[ClassLibrary("laserdot")]
	public partial class LaserDot : Entity
	{
		private Particles _particles;

		public LaserDot()
		{
			Transmit = TransmitType.Always;

			if ( IsClient )
			{
				_particles = Particles.Create( "particles/laserdot.vpcf" );
				_particles.SetEntity( 0, this, true );
			}

			PhysicsClear();
		}

		protected override void OnDestroy()
		{
			_particles?.Destory( true );

			base.OnDestroy();
		}
	}
}