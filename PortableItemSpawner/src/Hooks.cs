using Deli.H3VR.Api;
using FistVR;
using UnityEngine;

namespace PortableItemSpawner
{
	public class Hooks
	{
		private GameObject _itemSpawner;

		private FVRPhysicalObject _portableItemSpawner;
		public FVRPhysicalObject PortableItemSpawner
		{
			get
			{
				if (_portableItemSpawner != null)
				{
					return _portableItemSpawner;
				}

				// Get panel prefab from Deli
				var panelObject = LockablePanel.GetCleanLockablePanel();
				panelObject.name = "PortableItemSpawner";
				var panelTransform = panelObject.transform;
				panelTransform.localPosition = Vector3.zero;
				panelTransform.localRotation = Quaternion.identity;

				// Fix button placement
				var btnTransform = panelTransform.Find("LockButton");
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
			// Get the ItemSpawner gameobject
			On.FistVR.ItemSpawnerUI.Start += (orig, self) => {
				orig(self);
				_itemSpawner = self.transform.parent.parent.gameObject;
			};

			// Enable itemspawner on death to vault match gun
			On.FistVR.TNH_Manager.PlayerDied += (orig, self) => {
				orig(self);
				self.ItemSpawner.SetActive(true);
			};

			On.FistVR.FVRWristMenu.SpawnOptionsPanel += FVRWristMenu_SpawnOptionsPanel;
		}

		private void Unhook()
		{
			On.FistVR.FVRWristMenu.SpawnOptionsPanel -= FVRWristMenu_SpawnOptionsPanel;
		}

		// Spawn our panel whenever options panel is clicked
		private void FVRWristMenu_SpawnOptionsPanel(On.FistVR.FVRWristMenu.orig_SpawnOptionsPanel orig, FVRWristMenu self)
		{
			orig(self);

			if (_itemSpawner != null)
			{
				var maxDist = 2;
				if (PortableItemSpawner == null || Vector3.Distance(GM.CurrentPlayerBody.Head.position, PortableItemSpawner.transform.position) > maxDist)
				{
					PortableItemSpawner.transform.position = self.m_currentHand.transform.position + GM.CurrentPlayerBody.Head.transform.forward;
					PortableItemSpawner.transform.rotation = Quaternion.Euler(270, 0, 0);
				}
			}
		}
	}
}
