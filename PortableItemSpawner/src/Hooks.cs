using FistVR;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace PortableItemSpawner
{
	public class Hooks
	{
		private GameObject _itemSpawner;
		private readonly HashSet<string> _rootKeep = new()
		{
			"OptionsPanelProto",
			"_PoseOverride",
			"Phys",
			"LockButton",
			"Cube",
		};

		private FVRPhysicalObject _portableItemSpawner;
		public FVRPhysicalObject PortableItemSpawner
		{
			get
			{
				if (_portableItemSpawner != null)
				{
					return _portableItemSpawner;
				}

				// ugly way to get wristmenu
				var wristMenu = UnityEngine.Resources.FindObjectsOfTypeAll<FVRWristMenu>().First();
				wristMenu.gameObject.SetActive(true);

				var panelObject = GameObject.Instantiate(wristMenu.OptionsPanelPrefab, Vector3.zero, Quaternion.identity);
				panelObject.name = "ItemPanel";

				// Delete all unneeded components & children
				GameObject.Destroy(panelObject.GetComponent<OptionsPanel_Screenmanager>());
				var panelTransform = panelObject.transform;
				Plugin.Instance.DeleteAllBut(panelTransform, _rootKeep);

				// Add item spawner & position
				var itemSpawner = GameObject.Instantiate(_itemSpawner, new Vector3(0, -0.013f, -0.185f), Quaternion.Euler(270, 180, 0), panelTransform);
				//itemSpawner.transform.position = new Vector3(0, -0.013f, -0.185f);
				//itemSpawner.transform.rotation = Quaternion.Euler(270, 180, 0);
				itemSpawner.transform.localScale = new Vector3(0.47f, 0.47f, 0.47f);
				GameObject.Destroy(itemSpawner.transform.Find("ItemSpawnerMover").gameObject);

				_portableItemSpawner = panelObject.GetComponent<FVRPhysicalObject>();
				_portableItemSpawner.SetIsKinematicLocked(true);
				_portableItemSpawner.m_colliders = _portableItemSpawner.GetComponentsInChildren<Collider>(true);

				return _portableItemSpawner;
			}
		}
		public Hooks()
		{
			Hook();
		}

		public void Dispose()
		{
			Unhook();
		}

		private void Hook()
		{
			On.FistVR.ItemSpawnerUI.Start += ItemSpawnerUI_Start;
			On.FistVR.FVRWristMenu.SpawnOptionsPanel += FVRWristMenu_SpawnOptionsPanel;
		}

		private void Unhook()
		{
			On.FistVR.ItemSpawnerUI.Start -= ItemSpawnerUI_Start;
			On.FistVR.FVRWristMenu.SpawnOptionsPanel -= FVRWristMenu_SpawnOptionsPanel;
		}

		private void FVRWristMenu_SpawnOptionsPanel(On.FistVR.FVRWristMenu.orig_SpawnOptionsPanel orig, FVRWristMenu self)
		{
			orig(self);

			if (_itemSpawner != null)
			{
				PortableItemSpawner.transform.position = new Vector3(0f, 1f, 0f) + GM.CurrentPlayerBody.transform.position;
				PortableItemSpawner.transform.rotation = Quaternion.Euler(270, 0, 0);
			}
		}

		private void ItemSpawnerUI_Start(On.FistVR.ItemSpawnerUI.orig_Start orig, ItemSpawnerUI self)
		{
			orig(self);

			_itemSpawner = GameObject.Find("ItemSpawner");
			if (_itemSpawner == null)
			{
				_itemSpawner = GameObject.Find("_ItemSpawner");
			}
		}
	}
}
