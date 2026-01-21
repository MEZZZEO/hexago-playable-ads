using System.Runtime.CompilerServices;
using UnityEngine;

namespace Game.Utilities.Lifetimes.Extensions
{
	public static class GameObjectExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasComponent<TComponent>(this GameObject go) where TComponent : class
		{
			return go.TryGetComponent<TComponent>(out _);
		}

		public static TComponent AddOrGetComponent<TComponent>(this GameObject go) where TComponent : Component
		{
			if (!go.TryGetComponent<TComponent>(out var component))
			{
				component = go.AddComponent<TComponent>();
			}

			return component;
		}
	}
}
