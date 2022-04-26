﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// Basic interaction handling for generated objects (tiles)
/// </summary>
public class Interactive : MonoBehaviour { // Warning: Needs the GameObject to also have a Tile component; ideally with an underlay as well
	public string type; // Generally set in the editor // "Tile", "Interface", "Codex", "Storage"
	public Coord index; // Unused if not a tile

	public void OnMouseUpAsButton() {
		if (type == "Tile") {
			Core.QueueClick(this.index);
		}
	}

	public void OnMouseEnter() { 
			Core.mouseover = this;
			Core.Mouseover();
	}

	public void OnMouseExit() { //TODO: Add check for if program focus lost/regained? (Add a pause screen)
		if (type == "Tile")
		{
			if (Core.mouseover == this)
			{
				Core.mouseover = null;
				Core.Mouseover();
			}
		}	
	}
}
