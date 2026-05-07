using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TankDestroyer.API;
using TankDestroyer.Engine;

namespace TankDestroyer;

public partial class GameNode : Node
{
	[Export] public WorldNode WorldNode { get; set; }

	[Export] public PackedScene TankScene { get; set; }
	[Export] public PackedScene AmmoScene { get; set; }
	[Export] public PackedScene BulletScene { get; set; }
	[Export] public float GameSpeed { get; set; } = 1f;

	[Export] public Node3D TankContainer { get; set; }
	[Export] public Node3D AmmoContainer { get; set; }
	[Export] public Node3D BulletContainer { get; set; }
	public bool HasGame => _gameRunner != null;

	private TankNode[] _tanks;
	private AmmoNode[] _ammoBoxes;


	private GameRunner _gameRunner;
	private GameTurn _currentTurn;

	public override void _Ready()
	{
	}

	public void StartGame(GameRunner gameRunner)
	{
		_playing = false;
		_tanks = null;
		_currentTurn = null;
		_ammoBoxes = null;
		_gameRunner = gameRunner;
		InitializeGame();
	}

	private void InitializeGame()
	{
		TankContainer.RemoveAndClearChilderen();
		BulletContainer.RemoveAndClearChilderen();
		AmmoContainer.RemoveAndClearChilderen();
		WorldNode.GridMap.RemoveAndClearChilderen();
		WorldNode.World = _gameRunner.GetWorld();
		foreach (var tank in _gameRunner.GetTanks())
		{
			var tankNode = TankScene.Instantiate<TankNode>();
			tankNode.Tank = tank;
			TankContainer.AddChild(tankNode);
			tankNode.GlobalPosition = new Vector3((tank.X * 2f) + 1f, 1f, tank.Y * 2f + 1f);
		}

		_tanks = TankContainer.GetChildren().OfType<TankNode>().ToArray();
		WorldNode.InitializeWorld();
		_currentTurn = _gameRunner.GetTurns().First();
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (inputEvent.IsActionPressed("do_turn"))
		{
			DoTurn();
		}
	}

	[Signal]
	public delegate void ChangedTurnEventHandler();

	public void DoTurn()
	{
		if (_currentTurn == _gameRunner.GetTurns().Last())
		{
			_gameRunner.DoTurn();

			var nextTurn = _gameRunner.GetTurns().Last();
			_currentTurn = nextTurn;

			UpdateGameWorld(nextTurn);

			if (_gameRunner.Finished)
			{
				_playing = false;
			}
		}
		else
		{
			_currentTurn = _gameRunner.GetNextTurn(_currentTurn);
			UpdateGameWorld(_currentTurn);
		}

		EmitSignalChangedTurn();
	}

	public void UpdateGameToTurnWithoutAnimation(GameTurn gameTurn)
	{
		foreach (var tankNode in _tanks)
		{
			var tankInTurn = gameTurn.Tanks.FirstOrDefault(c => c.OwnerId == tankNode.Tank.OwnerId);
			if (tankInTurn != null)
			{
				tankNode.Tank = tankInTurn;
			}

			tankNode.UpdateAll();
		}

		BulletContainer.RemoveAndClearChilderen();

		var bullets = gameTurn.Bullets;
		foreach (var bullet in bullets)
		{
			var bulletNode = BulletScene.Instantiate<BulletNode>();
			bulletNode.Bullet = bullet;
			BulletContainer.AddChild(bulletNode);
			bulletNode.GlobalPosition = new Vector3((bullet.StartingX * 2f) + 1f, 1f, bullet.StartingY * 2f + 1f);
			bulletNode.Deleted = bullet.Destroyed;
			bulletNode.UpdateLocation(true);
		}

		AmmoContainer.RemoveAndClearChilderen();

		var ammoBoxes = gameTurn.MunitionBoxes;
		foreach (var ammo in ammoBoxes)
		{
			var ammoNode = AmmoScene.Instantiate<AmmoNode>();
			ammoNode.MunitionBox = ammo;
			AmmoContainer.AddChild(ammoNode);
			ammoNode.GlobalPosition = new Vector3((ammo.X * 2f) + 1f, 1f, ammo.Y * 2f + 1f);
			ammoNode.Update();
		}
	}

	private void UpdateGameWorld(GameTurn turn)
	{
		_currentTurn = turn;
		var newBullets = new List<BulletNode>();
		var bulletNodes = BulletContainer.GetChildren().OfType<BulletNode>().ToArray();
		var bullets = turn.Bullets;
		foreach (var bullet in bullets)
		{
			var currentBulletNode = bulletNodes.FirstOrDefault(c => c.Bullet.Id.Equals(bullet.Id));
			if (currentBulletNode == null)
			{
				var bulletNode = BulletScene.Instantiate<BulletNode>();
				bulletNode.Bullet = bullet;
				bulletNode.Deleted = bullet.Destroyed;
				BulletContainer.AddChild(bulletNode);
				bulletNode.GlobalPosition =
					new Vector3((bullet.StartingX * 2f) + 1f, 1f, bullet.StartingY * 2f + 1f);
				bulletNode.Visible = false;
				newBullets.Add(bulletNode);
				continue;
			}

			currentBulletNode.Bullet = bullet;
			currentBulletNode.Deleted = bullet.Destroyed;
			currentBulletNode.UpdateLocation();
		}

		foreach (var bulletNode in bulletNodes)
		{
			if (bullets.All(c => c.Id != bulletNode.Bullet.Id))
			{
				bulletNode.Deleted = true;
				bulletNode.UpdateLocation();
			}
		}

		foreach (var tankNode in _tanks)
		{
			var tankInTurn = turn.Tanks.Single(c => c.OwnerId == tankNode.Tank.OwnerId);
			tankNode.Tank = tankInTurn;
			if (tankNode.Tank.Destroyed)
			{
				var tankDestroyTween = GetTree().CreateTween();
				tankDestroyTween.TweenInterval(GameSpeed * 0.9f);
				tankDestroyTween.TweenCallback(Callable.From(tankNode.DestroyTurret));
				continue;
			}

			var tween = GetTree().CreateTween();
			tween.TweenProperty(tankNode, "global_position",
				new Vector3((tankNode.Tank.X * 2f) + 1f, 1f, tankNode.Tank.Y * 2f + 1f), GameSpeed * 0.9f);
			tankNode.CorrectTurretRotation();
		}

		var bulletVisibleTween = GetTree().CreateTween();
		bulletVisibleTween.TweenInterval(GameSpeed * 0.9f);
		bulletVisibleTween.TweenCallback(Callable.From(() =>
		{
			foreach (var bulletNode in newBullets)
			{
				if (!bulletNode.IsValid())
				{
					continue;
				}

				bulletNode.Visible = true;
				bulletNode.UpdateLocation();
			}
		}));

		UpdateAmmoBoxes();
	}

	private void UpdateAmmoBoxes()
	{
		if (AmmoScene == null) return;
		
		var ammoNodes = AmmoContainer.GetChildren().OfType<AmmoNode>().ToArray();
		var ammoBoxes = _currentTurn.MunitionBoxes;

		foreach (var ammoNode in ammoNodes)
		{
			if (ammoBoxes.All(c => c.Id != ammoNode.MunitionBox.Id))
			{
				AmmoContainer.RemoveChild(ammoNode);
			}
		}

		foreach (var munitionBox in ammoBoxes)
		{
			var currentAmmoNode = ammoNodes.FirstOrDefault(c => c.MunitionBox.Id.Equals(munitionBox.Id));
			if (currentAmmoNode != null) continue;

			var ammoNode = AmmoScene.Instantiate<AmmoNode>();
			ammoNode.MunitionBox = munitionBox;
			AmmoContainer.AddChild(ammoNode);
			ammoNode.GlobalPosition = new Vector3((munitionBox.X * 2f) + 1f, 1f, munitionBox.Y * 2f + 1f);
			ammoNode.Visible = true;
			ammoNode.Update();
		}
		
		_ammoBoxes = AmmoContainer.GetChildren().OfType<AmmoNode>().ToArray();
	}

	private double _timeElapsed = 0f;
	private bool _playing = false;

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		if (!_playing)
		{
			return;
		}

		_timeElapsed += delta;
		if (_timeElapsed >= GameSpeed)
		{
			DoTurn();
			_timeElapsed = 0f;
		}
	}

	public void StartPlay()
	{
		_timeElapsed = GameSpeed;
		_playing = true;
	}

	public PlayerInfo[] GetPlayers()
	{
		return _currentTurn.Tanks.Select(c => new PlayerInfo()
		{
			Tank = c,
			Name = _gameRunner.GetPlayerName(c),
			Creator = _gameRunner.GetCreatorName(c),
			Color = _gameRunner.GetPlayerColor(c),
		}).ToArray();
	}

	public GameRunner GetRunner() => _gameRunner;

	public int GetCurrentTurnIndex() => _gameRunner.GetTurns().IndexOf(_currentTurn);

	public void PausePlay()
	{
		_playing = false;
	}

	public bool IsPlaying() => _playing;

	public void StepBack()
	{
		_currentTurn = _gameRunner.GetTurns()[Math.Max(0, GetCurrentTurnIndex() - 1)];
		UpdateGameToTurnWithoutAnimation(_currentTurn);
		EmitSignalChangedTurn();
	}

	public void SetStepIndex(int value)
	{
		_currentTurn = _gameRunner.GetTurns()[value];
		UpdateGameToTurnWithoutAnimation(_currentTurn);
		EmitSignalChangedTurn();
	}
}

public class PlayerInfo
{
	public string Name { get; set; }
	public string Creator { get; set; }
	public Tank Tank { get; set; }
	public string Color { get; set; }
}
