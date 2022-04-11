using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handler for the blueprint ui element (loads the needed blueprint_page and hands off to it)
/// </summary>
public class Codex : MonoBehaviour {
	public BlueprintPage statBlueprint;
	public BlueprintPage setBlueprint;
	public BlueprintPage specBlueprint;

	public Tile[] buttons;

	void Start () {
		buttons = this.gameObject.GetComponentsInChildren<Tile>();
		Recalc();
	}

	/// <summary>
	/// After change, recalc which Codecs should be shown/highlit
	/// </summary>
	public void Recalc () {
		foreach (Tile butt in buttons) {
			if (!Data.playerseen.ContainsKey(butt.tiletype)) { // Doesn't exist
				Debug.Log("Invalid blueprint button");
			} else if (Data.playerseen[butt.tiletype] || CanMake(butt.tiletype)) { // Visible (Forcibly seen or properly buildable from seen parts)
				butt.SmartShow(butt.tiletype);
				if (!Data.playerread[butt.tiletype]) {
					butt.StartBreathing();
				} else {
					butt.breathing = false;
				}
			} else { // Craftable, but ingredients not seen
				butt.SmartShow("Empty");
			}
		}
	}

	/// <summary>
	/// Can it be made with known ingredients?
	/// </summary>
	public bool CanMake(string showme) {
		if (!Data.patternresults.ContainsKey(showme))
		{ // Not craftable, like dirt/tools
			return true;
		}
		foreach (string required in Data.patternresults[showme].value)
		{
			if (required != "" && required != "Empty" && !Data.playerseen[required]) { // Contains an unseen ingredient
				return false;
			}
		}
		return true;
	}

	public void CodexClick() {
		string tiletype = EventSystem.current.currentSelectedGameObject.GetComponent<Tile>().tiletype;
		OpenBlueprint(tiletype);
	}

	/// <summary>
	/// Given selected tiletype (through click or other script), open the appropriate bp page
	/// </summary>
	public void OpenBlueprint(string tiletype) { 
		if (!CanMake(tiletype) && !Data.playerseen[tiletype])
		{
			Debug.Log("Invalid blueprint button2");
			return;
		}

		Data.playerread[tiletype] = true;
		Recalc();
		if (DevMode.devmode) {
			if (tiletype == "Storage") { Core.SetHand("Panel"); }
			else { Core.SetHand(tiletype); }
		}

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
					if (this.gameObject.name != "CodexContainer") { this.gameObject.SetActive(false); }
					break;
				default:
				break;
			}
		}
		UI.LightBox.SetActive(true);
	}
}
