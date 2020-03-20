using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Blueprint_Page : MonoBehaviour {
	public Tile[] displayTiles;
	public string[] showing;

	public Text titleText;
	public Text descText;
	public Text arrowText;
	public bool showgrid; // For static blueprints to show geometric significance

	void Start () {
		showing = new string[10];

		this.gameObject.SetActive(false);

		RectTransform rtPage = this.gameObject.GetComponent<RectTransform>();
		rtPage.anchoredPosition = Vector2.zero;
	}
	
	public void OpenRecipe(string tiletype) {
		showing[0] = (tiletype == "Junk2" || tiletype == "Junk3")? "Junk1" : tiletype;
		Recalc();
	}
	public void Recalc() {
		if (showing == null || showing[0] == null || showing[0] == "" || !Data.tiledefs.ContainsKey(showing[0])) {
			Debug.Log(showing[0]);
			return;
		}
			
		if (Data.playerseen[showing[0]]) {
			titleText.text = Data.tiledefs[showing[0]].name + ":";
			descText.text = Data.tiledefs[showing[0]].description;
		} else {
			titleText.text = "???:";
			descText.text = "";
		}
		if (showing[0] == "Junk1" || showing[0] == "Junk2" || showing[0] == "Junk3") { // Special case: junk
			arrowText.text = "";
			for (int n = 0; n <= 9; n++) {
				showing[n] = "Empty";
			}
			showing[4] = "Junk1";
			showing[5] = "Junk2";
			showing[6] = "Junk3";
		} else if (showing[0] == "Rat" || showing[0] == "NewRat") { // Special case: rat
			arrowText.text = ">>>";
			showing[0] = "Rat";
			showing[1] = "Junk1";
			showing[2] = "Junk1";
			showing[3] = "Junk1";
			showing[4] = "Junk1";
			showing[5] = "Junk2";
			showing[6] = "Junk3";
		} else if (showing[0] == "Storage") {
			arrowText.text = ">>>";
			for (int n = 1; n < 9; n++) {
				showing[n] = "Panel";
			}
		} else if (!Data.patternresults.ContainsKey(showing[0])) { // Atomic tiles like dirt/tools
			arrowText.text = "";
			string temp = showing[0];
			for (int n = 0; n <= 9; n++) {
				showing[n] = "Empty";
			}
			showing[5] = temp;
		} else { // Actual craftable recipe on display
			arrowText.text = ">>>";

			switch (Data.patternresults[showing[0]].type) {
			case "3Cont":
				for (int n = 1; n <= 3; n++) {
					showing[n] = (Data.patternresults[showing[0]].value[0,0]);
				}
				for (int n = 4; n <= 9; n++) {
					showing[n] = "Empty";
				}
				break;
			case "4Cont":
				for (int n = 1; n <= 4; n++) {
					showing[n] = (Data.patternresults[showing[0]].value[0,0]);
				}
				for (int n = 5; n <= 9; n++) {
					showing[n] = "Empty";
				}
				break;
			case "Static":
				for (int y = 0; y < 3; y++) {
					for (int x = 0; x < 3; x++) {
						if (Data.patternresults[showing[0]].value[x,y] == "" || Data.patternresults[showing[0]].value[x,y] == "Empty") {
							showing[3*y + x + 1] = "Empty";
						} else {
							showing[3*y + x + 1] = Data.patternresults[showing[0]].value[x,y];
						}
					}
				}
				break;
			default:
				Debug.Log("Invalid recipe for blueprint page");
				break;
			}
		}
		ReDisplay();
	}
	public void ReDisplay() {
		for (int n = 0; n < displayTiles.Length; n++) {
			displayTiles[n].SmartShow(showing[n]);
		}
	}

	public void Click(int index) {
		if (showing[index] == "Panel" && !Data.playerseen["Panel"]) {
			return;
		} else if (showing[index] != "Empty") {
			Game.blueprints.OpenRecipe(showing[index]);
		}
	}
}
