using Godot;
using TankDestroyer.Engine.Objects;

namespace TankDestroyer;

public partial class AmmoNode : Node3D
{
	public MunitionBox MunitionBox { get; set; }
	private Tween _tween;
	private bool _deleteTween;


	public void Rotate()
	{
		this.Rotation += new Vector3(0,-22.5f,0);
	}

	public void Update()
	{
	}
}
