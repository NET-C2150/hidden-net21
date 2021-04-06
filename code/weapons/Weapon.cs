﻿using Sandbox;

namespace HiddenGamemode
{
	partial class Weapon : BaseWeapon
	{
		public virtual AmmoType AmmoType => AmmoType.Pistol;
		public virtual int ClipSize => 16;
		public virtual float ReloadTime => 3.0f;
		public virtual int Bucket => 1;

		public virtual int BucketWeight => 100;

		[NetPredicted]
		public int AmmoClip { get; set; }

		[NetPredicted]
		public TimeSince TimeSinceReload { get; set; }

		[NetPredicted]
		public bool IsReloading { get; set; }

		[NetPredicted]
		public TimeSince TimeSinceDeployed { get; set; }

		public int AvailableAmmo()
		{
			if ( Owner is not Player owner ) return 0;
			return owner.AmmoCount( AmmoType );
		}

		public override void ActiveStart( Entity ent )
		{
			base.ActiveStart( ent );

			TimeSinceDeployed = 0;
		}

		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
		}

		public override void Reload( Sandbox.Player owner )
		{
			if ( IsReloading )
				return;

			if ( AmmoClip >= ClipSize )
				return;

			TimeSinceReload = 0;

			if ( Owner is Player player )
			{
				if ( player.AmmoCount( AmmoType ) <= 0 )
					return;

				StartReloadEffects();
			}

			IsReloading = true;
			Owner.SetAnimParam( "b_reload", true );
			StartReloadEffects();
		}

		public override void OnPlayerControlTick( Sandbox.Player owner )
		{
			if ( TimeSinceDeployed < 0.6f )
				return;

			if ( !IsReloading )
			{
				base.OnPlayerControlTick( owner );
			}

			if ( IsReloading && TimeSinceReload > ReloadTime )
			{
				OnReloadFinish();
			}
		}

		public virtual void OnReloadFinish()
		{
			IsReloading = false;

			if ( Owner is Player player )
			{
				var ammo = player.TakeAmmo( AmmoType, ClipSize - AmmoClip );
				if ( ammo == 0 )
					return;

				AmmoClip += ammo;
			}
		}

		[ClientRpc]
		public virtual void StartReloadEffects()
		{
			ViewModelEntity?.SetAnimParam( "reload", true );
		}

		public override void AttackPrimary( Sandbox.Player owner )
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			ShootEffects();

			foreach ( var tr in TraceBullet( owner.EyePos, owner.EyePos + owner.EyeRot.Forward * 5000 ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				using ( Prediction.Off() )
				{
					var damage = DamageInfo.FromBullet( tr.EndPos, owner.EyeRot.Forward * 100, 15 )
						.UsingTraceResult( tr )
						.WithAttacker( owner )
						.WithWeapon( this );

					tr.Entity.TakeDamage( damage );
				}
			}
		}

		[ClientRpc]
		protected virtual void ShootEffects()
		{
			Host.AssertClient();

			Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

			if ( Owner == Player.Local )
			{
				_ = new Sandbox.ScreenShake.Perlin();
			}

			ViewModelEntity?.SetAnimParam( "fire", true );
			CrosshairPanel?.OnEvent( "fire" );
		}

		public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
		{
			var forward = Owner.EyeRot.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			foreach ( var tr in TraceBullet( Owner.EyePos, Owner.EyePos + forward * 5000, bulletSize ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				using ( Prediction.Off() )
				{
					var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * 100 * force, damage )
						.UsingTraceResult( tr )
						.WithAttacker( Owner )
						.WithWeapon( this );

					tr.Entity.TakeDamage( damageInfo );
				}
			}
		}

		public bool TakeAmmo( int amount )
		{
			if ( AmmoClip < amount )
				return false;

			AmmoClip -= amount;
			return true;
		}

		[ClientRpc]
		public virtual void DryFire()
		{
			
		}

		public override void CreateViewModel()
		{
			Host.AssertClient();

			if ( string.IsNullOrEmpty( ViewModelPath ) )
				return;

			ViewModelEntity = new ViewModel();
			ViewModelEntity.WorldPos = WorldPos;
			ViewModelEntity.Owner = Owner;
			ViewModelEntity.EnableViewmodelRendering = true;
			ViewModelEntity.SetModel( ViewModelPath );
		}

		public override void CreateHudElements()
		{
			if ( Hud.CurrentPanel == null ) return;

			CrosshairPanel = new Crosshair();
			CrosshairPanel.Parent = Hud.CurrentPanel;
			CrosshairPanel.AddClass( ClassInfo.Name );
		}

		public bool IsUsable()
		{
			if ( AmmoClip > 0 ) return true;
			return AvailableAmmo() > 0;
		}
	}
}
