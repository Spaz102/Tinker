using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Blueprints : MonoBehaviour {
	public Blueprint_Page statBlueprint;
	public Blueprint_Page setBlueprint;
	public Blueprint_Page specBlueprint;

	public Tile[] buttons;

	//public Text headtext;
	//public Text bodytext;
	public void Awake(){}

	void Start () {
		buttons = this.gameObject.GetComponentsInChildren<Tile>();
		Recalc();
	}

	public void Recalc () {
		foreach (Tile butt in buttons) {
			if (!Data.playerseen.ContainsKey(butt.tiletype)) { // Doesn't exist
				Debug.Log("Invalid blueprint button");
			} else if (Data.playerseen[butt.tiletype] || CanMake(butt.tiletype)) { // Visible (Forcibly seen or properly buildable from seen parts)
				butt.SmartShow(butt.tiletype);
			} else { // Craftable, but ingredients not seen
				butt.SmartShow("Empty");
			}
		}
	}

	public void CodexClick() {
		string tiletype = EventSystem.current.currentSelectedGameObject.GetComponent<Tile>().tiletype;
		OpenRecipe(tiletype);
		
	}

	public void OpenRecipe(string tiletype) { // Also show entries for uncraftable tiles (Eg: Dirt)
		if (!CanMake(tiletype) && !Data.playerseen[tiletype])
		{
			Debug.Log("Invalid blueprint button2");
			return;
		}
		
		Data.playerread[tiletype] = true;
		Recalc();
		if (Data.devmode) { Game.SetHand(tiletype); }

		UIControls.LightBox.SetActive(true);
		if (tiletype == "Storage") {
			statBlueprint.gameObject.SetActive(false);
			specBlueprint.gameObject.SetActive(true);
			setBlueprint.gameObject.SetActive(false);
			specBlueprint.OpenRecipe(tiletype);
			if (this.gameObject.name != "CodexContainer") { this.gameObject.SetActive(false); }
		} else if (!Data.patternresults.ContainsKey(tiletype)) { // Atomic tiles like dirt/tool
			statBlueprint.gameObject.SetActive(true);
			specBlueprint.gameObject.SetActive(false);
			setBlueprint.gameObject.SetActive(false);
			statBlueprint.OpenRecipe(tiletype);
			if (this.gameObject.name != "CodexContainer") { this.gameObject.SetActive(false); }
		} else {
			switch (Data.patternresults[tiletype].type) {
				case "3Cont":
				case "4Cont":
				case "Static":
					statBlueprint.gameObject.SetActive(true);
					specBlueprint.gameObject.SetActive(false);
					setBlueprint.gameObject.SetActive(false);
					statBlueprint.OpenRecipe(tiletype);
					if (this.gameObject.name != "CodexContainer") { this.gameObject.SetActive(false); }
					break;
				case "Set":
					statBlueprint.gameObject.SetActive(false);
					specBlueprint.gameObject.SetActive(false);
					setBlueprint.gameObject.SetActive(true);
					setBlueprint.OpenRecipe(tiletype);
					this.gameObject.SetActive(false);
				break;
				default:
				break;
			}
		}
	}

	public bool CanMake (string showme) { // Can it be made with known ingredients?
		if (!Data.patternresults.ContainsKey(showme)) { // Not craftable, like dirt/tools
			return true;
		}
		foreach (string required in Data.patternresults[showme].value) {
			if (required != "" && required != "Empty" && !Data.playerseen[required]) { // Contains an unseen ingredient
				return false;
			}
		}
		return true;
	}
}
