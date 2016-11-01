using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using KSP.UI.Screens;

using KIS;

namespace ShipYard {

	public class ShipYard : PartModule
	{
		[KSPField(isPersistant = true)]
		public string storageNodes = "";

		[KSPField(isPersistant = true)]
		public string konstructionNodes = "";

		[KSPField(isPersistant = true)]
		public string assemblyNodes = "";

		private Vessel craft = null;

		public override string GetInfo ()
		{
			string returnValue = string.Empty;
			returnValue += "Storage Nodes: " + storageNodes;
			returnValue += "\r\nKonstruction Nodes: " + konstructionNodes;
			returnValue += "\r\nAssembly Nodes: " + assemblyNodes;
			return returnValue;
		}

		public string GetModuleTitle ()
		{
			return "ShipYard";
		}
			
		public override void OnAwake ()
		{
			foreach (string nodeID in assemblyNodes.Split(','))
				if (part.FindAttachNode (nodeID.Trim ()) == null)
					Debug.Log ("ShipYard " + vessel.name + ": " + " assembly node " + nodeID + " not found on part."); 
			foreach (string nodeID in konstructionNodes.Split(','))
				if (part.FindAttachNode (nodeID.Trim ()) == null)
					Debug.Log ("ShipYard " + vessel.name + ": " + " konstruction node " + nodeID + " not found on part."); 
			foreach (string nodeID in storageNodes.Split(','))
				if (part.FindAttachNode (nodeID.Trim ()) == null)
					Debug.Log ("ShipYard " + vessel.name + ": " + " storage node " + nodeID + " not found on part.");
		}

		/******************* Context Menu ****************/

		private void updateMenu (string activeItem) {
			Events["startDismantlingVessel"].active = false;
			Events["stopDismantlingVessel"].active = false;

			Events["startUnloadCargo"].active = false;
			Events["stopUnloadCargo"].active = false;

			Events["startRebuildingVessel"].active = false;
			Events["stopRebuildingVessel"].active = false;

			Events["activateShipYard"].active = false;
			Events["deactivateShipYard"].active = false;

			Events ["loadVessel"].active = false;
			Events ["saveVessel"].active = false;

			Events[activeItem].active = true;
		}

		private void updateMenu () {
			Events["startDismantlingVessel"].active = (findVesselPart() != null);
			Events["stopDismantlingVessel"].active = false;

			Events["startUnloadCargo"].active = (findCargo() != null);
			Events["stopUnloadCargo"].active = false;

			Events["startRebuildingVessel"].active = (craft != null);
			Events["stopRebuildingVessel"].active = false;

			Events["activateShipYard"].active = false;
			Events["deactivateShipYard"].active = true;

			Events ["loadVessel"].active = true;
			Events ["saveVessel"].active = true;
		}

		[KSPEvent (guiActive = true, guiName = "Activate ShipYard", active = true)]
		public void activateShipYard ()
		{
			updateMenu ();
		}

		[KSPEvent (guiActive = true, guiName = "Deactivate ShipYard", active = false)]
		public void deactivateShipYard ()
		{
			updateMenu ("activateShipYard");
		}

		/*********** Save Vessel **************/

		[KSPEvent (guiActive = true, guiName = "Save Vessel", active = false)]
		public void saveVessel ()
		{
			//vessel.
		}

		/*********** Load Vessel **************/

		[KSPEvent (guiActive = true, guiName = "Load Vessel", active = false)]
		public void loadVessel ()
		{
			string strpath = HighLogic.SaveFolder;

			EditorFacility []facility = new EditorFacility[] {
				EditorFacility.VAB,
				EditorFacility.SPH,
				EditorFacility.None,
			};

			CraftBrowserDialog.Spawn(facility[1],
				strpath,
				craftSelectOkay,
				craftSelectCancel,
				false);
		}

		private void craftSelectOkay (string filename,
			CraftBrowserDialog.LoadType lt)
		{
			ConfigNode craftNode = ConfigNode.Load (filename);

			//bool lockedParts = false;
			ShipConstruct ship = new ShipConstruct ();

			//ProtoVessel cringle = new ProtoVessel (craftNode);

			//foreach (ProtoPartSnapshot ppart in cringle.protoPartSnapshots) {
			//	Debug.Log (ppart.partName);
			//}

			if (!ship.LoadShip (craftNode)) {
				return;
			}

			//GameObject ro = ship.parts[0].localRoot.gameObject;
			//Vessel craftVessel = ro.AddComponent<Vessel>();
			//craftVessel.Initialize (true);

			//craftVessel.Die ();

			//craft = craftVessel;
			//if (!ship.shipPartsUnlocked) {
			//	lockedParts = true;
			//}
			//GameObject ro = ship.parts[0].localRoot.gameObject;
			//craft = ro.AddComponent<Vessel>();
			//craft.Initialize (true);
			//			if (ExSettings.B9Wings_Present) {
			//				if (!InitializeB9Wings (craftVessel)
			//					&& ExSettings.FAR_Present) {
			//					InitializeFARSurfaces (craftVessel);
			//				}
			//			} else if (ExSettings.FAR_Present) {
			//				InitializeFARSurfaces (craftVessel);
			//			}

			//foreach (Part p in craftVessel.parts) {
			//	resources.addPart (p);
			//}
			//craftVessel.Die ();
		}

		private void craftSelectCancel ()
		{

		}

		/************* Storage *******************/

		private float calculateDelay(AvailablePart p, bool attached)
		{
			// TODO use volume and mass
			// float m = p.mass;
			return 2.0f;
		}

		private float calculateDelay(Part p, bool attached)
		{
			// TODO use volume and mass
			// float m = p.mass;
			return 2.0f;
		}

		private ModuleKISInventory findStorageFor(Part p, AttachNode search_at)
		{
			return search_at.attachedPart.GetComponent<ModuleKISInventory> ();
		}

		private ModuleKISInventory findStorageFor(Part p)
		{
			ModuleKISInventory found = null;

			foreach (string node in storageNodes.Split(',')) {
				AttachNode search_in = part.FindAttachNode (node.Trim ());
				found = findStorageFor (p, search_in);
				if (found)
					break;
			}
			return found;
			/* determine weight for each storageNode */
		}
			
		private Part findVesselPart() {
			// TODO search for the smallest one

			Part found = null;
			foreach (string node in assemblyNodes.Split(',')) {
				AttachNode search_in = part.FindAttachNode (node.Trim ());
				found = findVesselPart (search_in.attachedPart);
				if (found != null)
					break;
			}
			return found;
		}

		private Part findVesselPart(Part search_at) {
		
			if (search_at == null)
				return null;

			if (search_at.children.Count == 0)
				return search_at;

			foreach (Part new_search_at in search_at.children) {
				Part found = findVesselPart (new_search_at);
				if (found != null)
					return found;
			}

			return null;
		}

		private KIS_Item findCargo() {
			KIS_Item found = null;
			foreach (string node in assemblyNodes.Split(',')) {
				AttachNode search_in = part.FindAttachNode (node.Trim ());
				found = findCargo (search_in.attachedPart);
				if (found != null)
					break;
			}
			return found;
		}

		private KIS_Item findCargo(Part search_at, Part search_for = null)
		{
			if (search_at == null) {
				Debug.Log ("no search_at");
				return null;
			}

			ModuleKISInventory[] itemInventorys = search_at.GetComponents<ModuleKISInventory> ();

			foreach (ModuleKISInventory itemInventory in itemInventorys) {
				if (itemInventory.invType != ModuleKISInventory.InventoryType.Container)
					continue;
				if (!itemInventory.externalAccess)
					continue;

				if (!itemInventory.showGui)
					itemInventory.ShowInventory ();

				foreach (KeyValuePair<int, KIS_Item> item in itemInventory.items) {
					if (item.Value == null)
						continue;
					if (search_for == null)
						return item.Value;
					if (item.Value.availablePart.name == search_for.name)
						return item.Value;
				}
			}

			foreach (Part new_search_at in search_at.children) {
				KIS_Item found = findCargo (new_search_at, search_for);
				if (found != null)
					return found;
			}

			return null;
		}

		/***************** Rebuilding ***************************/

		[KSPEvent (guiActive = true, guiName = "Start Rebuilding Vessel", active = false)]
		public void startRebuildingVessel ()
		{
			updateMenu ("stopRebuildingVessel");
			rebuildingVessel ();
		}

		[KSPEvent (guiActive = true, guiName = "Stop Rebuilding Vessel", active = false)]
		public void stopRebuildingVessel ()
		{
			updateMenu ();
		}

		private void rebuildingVessel ()
		{
		}

		/************* Dismantling ****************/

		[KSPEvent (guiActive = true, guiName = "Start Dismantling Vessel", active = false)]
		public void startDismantlingVessel ()
		{
			updateMenu ("stopDismantlingVessel");
			dismantlingVessel ();
		}

		[KSPEvent (guiActive = true, guiName = "Stop Dismantling Vessel", active = false)]
		public void stopDismantlingVessel ()
		{
			Events["stopDismantlingVessel"].active = false;
		}

		private void dismantlingVessel ()
		{
			if (Events["stopDismantlingVessel"].active) {

				Part found = findVesselPart ();
				if (found == null) {
					updateMenu ();
					return;
				}
				found.decouple ();

				Invoke("dismantlingVessel", 5.0f);
			} else {
				updateMenu();
			}
		}

		/*********************** Unload Cargo ******************/

		[KSPEvent (guiActive = true, guiName = "Start Unload Cargo", active = false)]
		public void startUnloadCargo ()
		{
			updateMenu ("stopUnloadCargo");
			unloadCargo ();
		}

		[KSPEvent (guiActive = true, guiName = "Stop Unload Cargo", active = false)]
		public void stopUnloadCargo ()
		{
			Events["stopUnloadCargo"].active = false;
		}
			
		private void unloadCargo()
		{
			if (Events ["stopUnloadCargo"].active) {

				KIS_Item found = findCargo ();
				if (found == null) {
					updateMenu ();
					return;
				}
					
				ModuleKISInventory tgtInventory = findStorageFor (null);
				if (tgtInventory == null) {
					Debug.Log ("No target Inventory found");
					return;
				}
				if (!tgtInventory.showGui)
					tgtInventory.ShowInventory ();

				ModuleKISInventory.MoveItem (found, tgtInventory, tgtInventory.GetFreeSlot ());
				Invoke ("unloadCargo", calculateDelay(found.availablePart, false));
			} else
				updateMenu ();
		}
			
		/************************* Detach Vessel *********************************/

		[KSPEvent (guiActive = true, guiName = "Detach Vessel", active = false)]
		public void detachVessel ()
		{
			AttachNode node_stack_bottom = part.FindAttachNode ("bottom");
			if (node_stack_bottom != null) {
				Part attachedPart = node_stack_bottom.attachedPart;
				if (attachedPart != null) {
					attachedPart.SetHighlightColor (XKCDColors.LightSeaGreen);
					attachedPart.SetHighlight (true, false);
					attachedPart.decouple ();
				} else {
					Debug.Log ("attachedPart ist null!");					
				}
			} else {
				Debug.Log ("node_stack_bottom ist null!");
			}
		}
			
	}
}

