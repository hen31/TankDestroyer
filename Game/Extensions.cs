using System;
using System.Linq;
using System.Reflection;
using Godot;

namespace TankDestroyer
{
	public static class EnumExtensions
	{
		public static TAttribute GetAttribute<TAttribute>(this Enum value)
			where TAttribute : Attribute
		{
			var type = value.GetType();
			var name = Enum.GetName(type, value);
			if (name == null) return null;
			return type.GetField(name)?
				.GetCustomAttribute<TAttribute>();
		}

		public static T GetChildOfType<T>(this Node node) where T : Node
		{
			foreach (Node child in node.GetChildren())
			{
				if (child is T typedNode)
				{
					return typedNode;
				}
				else if (child.GetChildCount() > 0)
				{
					var childItem = GetChildOfType<T>(child);
					if (childItem != null)
					{
						return childItem;
					}
				}
			}

			return null;
		}

		public static GameNode GetGameNode(this SceneTree tree)
		{
			return tree.Root.GetNode<GameNode>("Node3D/Game");
		}

		public static bool EqualsWithMargin(this Vector3 compare, Vector3 compareTo, float margin = 0.001f)
		{
			return (compare.X.EqualsWithMargin(compareTo.X, margin) &&
					compare.Y.EqualsWithMargin(compareTo.Y, margin) && compare.Z.EqualsWithMargin(compareTo.Z, margin));
		}

		public static bool EqualsWithMargin(this float compare, float compareTo, float margin = 0.001f)
		{
			return Mathf.Abs(compare - compareTo) < margin;
		}

		public static bool EqualsWithMargin(this double compare, double compareTo, double margin = 0.001f)
		{
			return Mathf.Abs(compare - compareTo) < margin;
		}

		public static bool EqualsWithMargin(this Vector2 compare, Vector2 compareTo, float margin = 0.001f)
		{
			return (compare.X.EqualsWithMargin(compareTo.X, margin) && compare.Y.EqualsWithMargin(compareTo.Y, margin));
		}


		public static void SetNextPassMaterialForMeshesBelow(this Node node, Material materialForNextPass)
		{
			if (node is MeshInstance3D meshInstanceOfCurrentNode && meshInstanceOfCurrentNode.Mesh != null)
			{
				for (int i = 0; i < meshInstanceOfCurrentNode.Mesh.GetSurfaceCount(); i++)
				{
					var material = meshInstanceOfCurrentNode.GetSurfaceOverrideMaterial(i);
					if (material != null)
					{
						material.NextPass = materialForNextPass;
					}
				}
			}

			foreach (Node childNode in node.GetChildren())
			{
				childNode.SetNextPassMaterialForMeshesBelow(materialForNextPass);
			}
		}

		public static bool HasDirectChildOfType<T>(this Node node) where T : Node
		{
			foreach (Node child in node.GetChildren())
			{
				if (child is T)
				{
					return true;
				}
			}

			return false;
		}

		public static T SetNodeFromNodePath<T>(this Node node, NodePath nodePath) where T : Node
		{
			if (nodePath != null)
			{
				return node.GetNode<T>(nodePath);
			}

			return null;
		}


		public static TValue GetAttributeValue<TAttribute, TValue>(
			this Type type,
			Func<TAttribute, TValue> valueSelector)
			where TAttribute : Attribute
		{
			var att = type.GetCustomAttributes(
				typeof(TAttribute), true
			).FirstOrDefault() as TAttribute;
			if (att != null)
			{
				return valueSelector(att);
			}

			return default;
		}

		public static Vector3 ToVector3(this Vector2 vector, float y = 0f)
		{
			return new Vector3(vector.X, y, vector.Y);
		}

		public static Vector2 ToVector2(this Vector3 vector)
		{
			return new Vector2(vector.X, vector.Z);
		}

		public static Vector2 ToVector2XY(this Vector3 vector)
		{
			return new Vector2(vector.X, vector.Y);
		}

		public static T FindParentOfType<T>(this Node collidedWith) where T : class
		{
			if (collidedWith is T itemOfType)
			{
				return itemOfType;
			}
			else if (collidedWith.GetParent() != null)
			{
				return FindParentOfType<T>(collidedWith.GetParent());
			}
			else
			{
				return null;
			}
		}


		public static T FindChildOfType<T>(this Node node) where T : Node
		{
			if (node is T itemOfType)
			{
				return itemOfType;
			}

			var typedChild = node.GetChildren().OfType<T>().FirstOrDefault();
			if (typedChild != null)
			{
				return typedChild;
			}

			foreach (var child in node.GetChildren())
			{
				typedChild = FindChildOfType<T>(child);
				if (typedChild != null)
				{
					return typedChild;
				}
			}

			return null;
		}


		public static Vector3 ToDegrees(this Vector3 vector)
		{
			return new Vector3(Mathf.RadToDeg(vector.X), Mathf.RadToDeg(vector.Y), Mathf.RadToDeg(vector.Z));
		}

		public static Node GetMainNode(this SceneTree tree)
		{
			return tree?.Root.GetNode("Main");
		}

		public static Node GetSystemsNode(this SceneTree tree)
		{
			return tree?.GetMainNode()?.GetNode<Node>("Systems");
		}


		public static void ClearChilderen(this Node node)
		{
			foreach (var childNode in node.GetChildren())
			{
				foreach (var groupName in childNode.GetGroups())
				{
					childNode.RemoveFromGroup(groupName);
				}
				childNode.ClearChilderen();
				childNode.QueueFree();
			}
		}

		public static void RemoveAndClearChilderen(this Node node)
		{
			foreach (var childNode in node.GetChildren().ToArray())
			{
				node.RemoveChild(childNode);
				childNode.QueueFree();
			}
		}


		public static bool IsValid(this GodotObject obj)
		{
			return obj != null && Godot.GodotObject.IsInstanceValid(obj) && !obj.IsQueuedForDeletion();
		}
	}
}
