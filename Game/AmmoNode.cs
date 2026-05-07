using System;
using Godot;
using TankDestroyer.Engine.Objects;

namespace TankDestroyer;

public partial class AmmoNode : Node3D
{
	public MunitionBox MunitionBox { get; set; }
	private float _time = 0f;
	private float? _startY;
	private Vector3 _startPosition;
	

	public override void _Process(double delta)
	{
		_startY ??= GlobalPosition.Y;
		_time += (float)delta;
		RotateY((float)delta * 1.5f);
		GlobalPosition = GlobalPosition with { Y = _startY.Value + MathF.Sin(_time * 2f) * 0.2f };
	}
	
	public void Update()
	{
	}
}
