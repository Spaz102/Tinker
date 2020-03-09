using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Blueprints : MonoBehaviour {
	public Blueprint_Page statBlueprint;
	public Blueprint_Page setBlueprint;
	public Blueprint_Page specBlueprint;

	public Tile[] buttons;
	private string[] fullList;
	public string[] butts;

	public Text headtext;
	public Text bodytext;
	public void Awake(){}

	void Start () {
		buttons = this.gameObject.GetComponentsInChildren<Tile>();
		fullList = new string[] {"Seed","Stick","Wood","Plank","Panel","Dirt","Rock","Metal","Pin","Cylinder","Gear","Spring","Comb","Drum","Motor","MusicBox","Key","Storage","Special","Junk1","Rat"}; // Missing: gameover conditions
		butts = fullList;
		this.gameObject.transform.localPosition = new Vector3(0f, -15f, 0f);
		Recalc();
		this.gameObject.SetActive(false);
	}

	public void Recalc () {
		for (int n = 0; n < buttons.Length; n++) {
			if (!Data.playerseen.ContainsKey(butts[n])) { // Doesn't exist
				Debug.Log("Invalid blueprint button");
			} else if (!Data.patternresults.ContainsKey(butts[n]) || CanMake(butts[n]) || butts[n] == "Rat") { // Uncraftable or have ingredients
				buttons[n].SmartShow(butts[n]);
				if (!Data.playerseen[butts[n]]) {
					buttons[n].Recolour(0, 0, 0);
				}
			} else { // Craftable, but ingredients not seen
				buttons[n].SmartShow("Empty");
			}
		}
		//statBlueprint.Recalc();
		//setBlueprint.Recalc();
		//specBlueprint.Recalc();

	}

	public void Click(int index) {
		if (index >= 0 && index < butts.Length && (
		CanMake(butts[index]) || butts[index] == "Rat" ||
		(Data.playerseen.ContainsKey(butts[index]) && Data.playerseen[butts[index]]))) {
			Show(butts[index]);
		}
	}

	public void Show(string showme) { // Show entries for uncraftable tiles (Eg: Dirt)
		if (showme == "Storage") {
			statBlueprint.gameObject.SetActive(false);
			specBlueprint.gameObject.SetActive(true);
			setBlueprint.gameObject.SetActive(false);
			specBlueprint.Show(showme);
			this.gameObject.SetActive(false);
		} else if (!Data.patternresults.ContainsKey(showme)) { // Atomic tiles like dirt/tool
			statBlueprint.gameObject.SetActive(true);
			specBlueprint.gameObject.SetActive(false);
			setBlueprint.gameObject.SetActive(false);
			statBlueprint.Show(showme);
			this.gameObject.SetActive(false);
		} else {
			switch (Data.patternresults[showme].type) {
				case "3Cont":
				case "4Cont":
				case "Static":
				statBlueprint.gameObject.SetActive(true);
				specBlueprint.gameObject.SetActive(false);
				setBlueprint.gameObject.SetActive(false);
				statBlueprint.Show(showme);
					this.gameObject.SetActive(false);
				break;
				case "Set":
				statBlueprint.gameObject.SetActive(false);
				specBlueprint.gameObject.SetActive(false);
				setBlueprint.gameObject.SetActive(true);
				setBlueprint.Show(showme);
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
