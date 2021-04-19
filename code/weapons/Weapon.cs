﻿using Sandbox;
using System;

namespace HiddenGamemode
{
	partial class Weapon : BaseWeapon
	{
		public virtual AmmoType AmmoType => AmmoType.Pistol;
		public virtual int ClipSize => 16;
		public virtual float ReloadTime => 3.0f;
		public virtual bool IsMelee => false;
		public virtual int Bucket => 1;
		public virtual int BucketWeight => 100;
		public virtual bool UnlimitedAmmo => false;
		public virtual bool HasFlashlight => false;
		public virtual bool HasLaserDot => false;
		public virtual int BaseDamage => 10;
		public virtual int HoldType => 1;
		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

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

		public override void ActiveStart( Entity owner )
		{
			base.ActiveStart( owner );

			TimeSinceDeployed = 0;
		}

		public override void Spawn()
		{
			base.Spawn();

			AmmoClip = ClipSize;

			SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
		}

		public override void Reload( Sandbox.Player owner )
		{
			if ( IsMelee || IsReloading )
				return;

			if ( AmmoClip >= ClipSize )
				return;

			TimeSinceReload = 0;

			if ( Owner is Player player )
			{
				if ( !UnlimitedAmmo )
				{
					if ( player.AmmoCount( AmmoType ) <= 0 )
						return;
				}
			}

			IsReloading = true;

			Owner.SetAnimParam( "b_reload", true );

			DoClientReload();
		}

		public override void TickPlayerAnimator( PlayerAnimator anim )
		{
			anim.SetParam( "holdtype", HoldType );
			anim.SetParam( "aimat_weight", 1.0f );
		}

		public override void OnPlayerControlTick( Sandbox.Player owner )
		{
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
				if ( !UnlimitedAmmo )
				{
					var ammo = player.TakeAmmo( AmmoType, ClipSize - AmmoClip );

					if ( ammo == 0 )
						return;

					AmmoClip += ammo;
				}
				else
				{
					AmmoClip = ClipSize;
				}
			}
		}

		[ClientRpc]
		public virtual void DoClientReload()
		{
			ViewModelEntity?.SetAnimParam( "reload", true );
		}

		public override void AttackPrimary( Sandbox.Player owner )
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			ShootEffects();
			ShootBullet( 0.05f, 1.5f, BaseDamage, 3.0f );
		}

		[ClientRpc]
		protected virtual void ShootEffects()
		{
			Host.AssertClient();

			if (!IsMelee)
			{
				Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
			}

			if ( Owner == Sandbox.Player.Local )
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

		public override void CreateViewModel()
		{
			Host.AssertClient();

			if ( string.IsNullOrEmpty( ViewModelPath ) )
				return;

			ViewModelEntity = new ViewModel
			{
				WorldPos = WorldPos,
				Owner = Owner,
				EnableViewmodelRendering = true
			};

			ViewModelEntity.SetModel( ViewModelPath );
		}

		public override void CreateHudElements()
		{
			if ( Sandbox.Hud.CurrentPanel == null ) return;

			if ( !HasLaserDot )
			{
				CrosshairPanel = new Crosshair
				{
					Parent = Sandbox.Hud.CurrentPanel
				};

				CrosshairPanel.AddClass( ClassInfo.Name );
			}
		}

		public bool IsUsable()
		{
			if ( IsMelee || ClipSize == 0 || AmmoClip > 0 )
			{
				return true;
			}

			return AvailableAmmo() > 0;
		}
	}
}
