using FistVR;
using System.Collections.Generic;
using UnityEngine;

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

				// Get the wristmenu
				var bod = GM.CurrentPlayerBody;
				var wristMenu = bod.RightHand.GetComponent<FVRViveHand>().m_wristmenu ? null : bod.LeftHand.GetComponent<FVRViveHand>().m_wristmenu;

				// Get prefab from wristmenu
				var panelObject = GameObject.Instantiate(wristMenu.OptionsPanelPrefab, Vector3.zero, Quaternion.identity);
				panelObject.name = "PortableItemSpawner";

				// Delete all unneeded components & children
				GameObject.Destroy(panelObject.GetComponent<OptionsPanel_Screenmanager>());
				var panelTransform = panelObject.transform;
				Plugin.Instance.DeleteAllBut(panelTransform, _rootKeep);

				// Fix button placement
				var btnTransform = panelTransform.Find("LockButton").transform;
				btnTransform.localPosition = new Vector3(0.2185f, 0.1135f, -0.12f);
				btnTransform.rotation = Quaternion.Euler(270, 0, 180);

				// Add item spawner & position
				var itemSpawner = GameObject.Instantiate(_itemSpawner, new Vector3(0, -0.021f, -0.1775f), Quaternion.Euler(270, 180, 0), panelTransform);
				itemSpawner.transform.localScale = new Vector3(0.47f, 0.47f, 0.47f);
				GameObject.Destroy(itemSpawner.transform.Find("ItemSpawnerMover").gameObject);

				// Config the fvrObj
				_portableItemSpawner = panelObject.GetComponent<FVRPhysicalObject>();
				_portableItemSpawner.SetIsKinematicLocked(true);
				_portableItemSpawner.m_colliders = _portableItemSpawner.GetComponentsInChildren<Collider>(true);
				_portableItemSpawner.PoseOverride.transform.rotation = Quaternion.Euler(40, 0, 0);

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
			On.FistVR.TNH_Manager.PlayerDied += TNH_Manager_PlayerDied;
		}

		private void Unhook()
		{
			On.FistVR.ItemSpawnerUI.Start -= ItemSpawnerUI_Start;
			On.FistVR.FVRWristMenu.SpawnOptionsPanel -= FVRWristMenu_SpawnOptionsPanel;
			On.FistVR.TNH_Manager.PlayerDied -= TNH_Manager_PlayerDied;
		}

		// Spawn our panel whenever options panel is clicked
		private void FVRWristMenu_SpawnOptionsPanel(On.FistVR.FVRWristMenu.orig_SpawnOptionsPanel orig, FVRWristMenu self)
		{
			orig(self);

			if (_itemSpawner != null)
			{
				var maxDist = 2;
				if (_portableItemSpawner == null || Vector3.Distance(GM.CurrentPlayerBody.Head.position, PortableItemSpawner.transform.position) > maxDist)
				{
					PortableItemSpawner.transform.position = self.m_currentHand.transform.position + GM.CurrentPlayerBody.Head.transform.forward;
					PortableItemSpawner.transform.rotation = Quaternion.Euler(270, 0, 0);
				}
			}
		}

		// Get the ItemSpawner gameobject
		private void ItemSpawnerUI_Start(On.FistVR.ItemSpawnerUI.orig_Start orig, ItemSpawnerUI self)
		{
			orig(self);

			_itemSpawner = GameObject.Find("ItemSpawner");
			if (_itemSpawner == null)
			{
				_itemSpawner = GameObject.Find("_ItemSpawner");
			}
		}

		// Enable itemspawner on death to vault match gun
		private void TNH_Manager_PlayerDied(On.FistVR.TNH_Manager.orig_PlayerDied orig, TNH_Manager self)
		{
			orig(self);

			self.ItemSpawner.SetActive(true);
		}
	}
}
