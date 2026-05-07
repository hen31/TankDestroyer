using System.Linq;
using Godot;
using TankDestroyer.API;
using TankDestroyer.Engine;

namespace TankDestroyer;

public partial class BulletNode : Node3D
{
	public Bullet Bullet { get; set; }
	public bool Deleted { get; set; }
	private bool _deleteTweenFired;
	private Tween _tween;

	public void UpdateLocation(bool force = false)
	{
		if (Deleted)
		{
			if (!_deleteTweenFired)
			{
				_tween?.Kill();
				Vector3 target = new Vector3(Bullet.EndedAtX * 2f + 1f, GlobalPosition.Y, Bullet.EndedAtY * 2f + 1f);
				if (force)
				{
					GlobalPosition = target;
					SpawnExplosion();
				}
				else
				{
					_tween = GetTree().CreateTween();
					_tween.TweenProperty(this, "global_position",
						target,
						(double)(target.DistanceTo(GlobalPosition) / (5f * 2f)) * GetTree().GetGameNode().GameSpeed *
						0.2f);
					if (Bullet.Explode)
					{
						_tween.TweenCallback(Callable.From(SpawnExplosion));
					}
					_tween.TweenCallback(Callable.From(QueueFree));
				}

				GlobalRotationDegrees = new Vector3(0, GetAngle(Bullet.Direction), 0);

				_deleteTweenFired = true;
			}
		}
		else
		{
			_tween?.Kill();
			if (force)
			{
				GlobalPosition = new Vector3((Bullet.X * 2f) + 1f, 1f, Bullet.Y * 2f + 1f);
			}
			else
			{
				_tween = GetTree().CreateTween();
				_tween.TweenProperty(this, "global_position",
					new Vector3((Bullet.X * 2f) + 1f, 1f, Bullet.Y * 2f + 1f),
					GetTree().GetGameNode().GameSpeed * 0.1f);
			}

			GlobalRotationDegrees = new Vector3(0, GetAngle(Bullet.Direction), 0);
		}
	}

	private void SpawnExplosion()
	{
		var explosion = GD.Load<PackedScene>("res://Explosion/explosion_1.tscn").Instantiate<GpuParticles3D>();
		GetTree().Root.AddChild(explosion);
		explosion.GlobalPosition = new Vector3((Bullet.EndedAtX * 2f) + 1f, 2f, Bullet.EndedAtY * 2f + 1f);
		explosion.Emitting = true;

		var tanks = GetTree().Root.GetNode<GameNode>("Node3D/Game").TankContainer.GetChildren().OfType<TankNode>();
		var tankAtExplosion = tanks.FirstOrDefault(c => c.Tank.X == Bullet.EndedAtX && c.Tank.Y == Bullet.EndedAtY);
		if (tankAtExplosion != null && tankAtExplosion.Tank.Destroyed)
		{
			tankAtExplosion.DestroyTurret();
		}
	}

	private float GetAngle(TurretDirection bulletDirection)
	{
		switch (bulletDirection)
		{
			case TurretDirection.North:
				return 0;
			case TurretDirection.South:
				return 180;
			case TurretDirection.West:
				return 90;
			case TurretDirection.East:
				return -90;
			case TurretDirection.NorthEast:
				return -45;
			case TurretDirection.NorthWest:
				return 45;
			case TurretDirection.SouthEast:
				return -135;
			case TurretDirection.SouthWest:
				return 135;
			default:
				return 0;
		}
	}
}
