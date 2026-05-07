using System.Linq;
using Godot;
using TankDestroyer.API;
using TankDestroyer.Extensions;

namespace TankDestroyer;

public partial class TankNode : Node3D
{
    public ITank Tank { get; set; }

    [Export] Node3D TurretNode { get; set; }
    [Export] Node3D DestroyedTurretNode { get; set; }
    [Export] Node3D Smoke { get; set; }
    [Export] Node3D MuzzleFlash { get; set; }
    [Export] OrmMaterial3D ColorMaterial { get; set; }

    public override void _Ready()
    {
        base._Ready();
    }


    private Tween _rotateTween;

    public void DestroyTurret()
    {
        TurretNode.Visible = false;
        DestroyedTurretNode.Visible = true;
        Smoke.Visible = true;
    }

    private bool _initialized;

    public override void _Process(double delta)
    {
        if (!_initialized)
        {
            _initialized = true;
            var player = GetTree().GetGameNode().GetPlayers().Single(c => c.Tank.OwnerId == Tank.OwnerId);
            ColorMaterial.Set("albedo_color", Color.FromHtml(player.Color));
            TurretNode.Set("surface_material_override/1", ColorMaterial);
            DestroyedTurretNode.Set("surface_material_override/1", ColorMaterial);
        }

        base._Process(delta);
    }

    public void CorrectTurretRotation()
    {
        var targetRotation = DetermineRotation();
        if (TurretNode.GlobalRotationDegrees.EqualsWithMargin(targetRotation))
        {
            if (Tank.Fired)
            {
                PlayMuzzleFlash();
            }

            return;
        }

        _rotateTween?.Kill();
        _rotateTween = GetTree().CreateTween();
        _rotateTween.TweenProperty(this.TurretNode, "global_rotation_degrees",
            new Vector3(0, targetRotation.Y, 0), GetTree().GetGameNode().GameSpeed * 0.9f);
        _rotateTween.TweenCallback(Callable.From(() =>
        {
            if (Tank.Fired)
            {
                PlayMuzzleFlash();
            }
        }));
    }

    private void PlayMuzzleFlash()
    {
        MuzzleFlash.Call("play");
    }

    public float Difference(float targetAngle)
    {
        var angle = Mathf.Abs(targetAngle);
        if (angle > 180.0)
        {
            return 360.0f - angle;
        }

        return angle;
    }


    public void UpdateAll()
    {
        GlobalPosition = new Vector3((Tank.X * 2f) + 1f, 1f, Tank.Y * 2f + 1f);
        var targetRotation = Tank.TurretDirection.Get3DVector();
        if (!TurretNode.GlobalRotationDegrees.EqualsWithMargin(targetRotation))
        {
            this.TurretNode.GlobalRotationDegrees = new Vector3(0, targetRotation.Y, 0);
        }

        if (Tank.Destroyed)
        {
            DestroyTurret();
        }
    }
}