using Deli.Setup;
using System.Collections.Generic;
using UnityEngine;

namespace PortableItemSpawner
{
	public class Plugin : DeliBehaviour
	{
		public static Plugin Instance { get; private set; }

		private readonly Hooks _hooks;

		public Plugin()
		{
			Instance = this;
			_hooks = new Hooks();
		}

		private void OnDestroy()
		{
			_hooks?.Dispose();
		}

		public void DeleteAllBut(Transform parent, HashSet<string> exceptions)
		{
			for (var i = parent.childCount - 1; i >= 0; i--)
			{
				var child = parent.GetChild(i);
				if (!exceptions.Contains(child.name))
				{
					GameObject.Destroy(child.gameObject);
				}
			}
		}
	}
}