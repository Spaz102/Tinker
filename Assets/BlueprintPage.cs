using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Unity container for a blueprint. There are 3 types: statBlueprint, setBlueprint, specBlueprint
/// </summary>
public class BlueprintPage : MonoBehaviour {
	// stat is a base level type, 'raw ingredient', not made
	// set is made with a pattern
	// spec is special... storage
	public enum BpTypes { statBlueprint = 1, setBlueprint = 2, specBlueprint = 3 }; 
	
	// Defines what the page is
	public string bpTiletype;
	public BpTypes bpType;

	// Content that gets displayed on the BP page
	public Text titleText;
	public Text descText;
	public Text arrowText;
	public bool showgrid; // For static blueprints to show geometric significance
	
	public string[] displayTiletypes;
	public Tile[] displayTiles;

	/// <summary>
	/// Awake runs on insantiation. 'Start' is a JIT feature
	/// </summary>
	void Awake () {
		displayTiletypes = new string[10];
		
		this.gameObject.SetActive(false);

		RectTransform rtPage = this.gameObject.GetComponent<RectTransform>();
		rtPage.anchoredPosition = Vector2.zero;
	}
	
	public void OpenRecipe(string tiletype) {
		bpTiletype = (tiletype == "Junk2" || tiletype == "Junk3")? "Junk1" : tiletype;
		LoadPage();
	}

	/// <summary>
	/// Given the selected bpTiletype this will rearrange what appears on the blueprint page popup
	/// </summary>
	public void LoadPage() {
		if (bpTiletype == null || bpTiletype == "" || !Data.tiledefs.ContainsKey(bpTiletype)) {
			Debug.Log(bpTiletype);
			return;
		}
			
		if (PlayerData.playerseen[bpTiletype]) {
			titleText.text = Data.tiledefs[bpTiletype].name + ":";
			descText.text = Data.tiledefs[bpTiletype].description + "";
		} else {
			titleText.text = "???:";
			descText.text = "";
		}
		
		for (int n = 0; n < displayTiletypes.Length; n++) { displayTiletypes[n] = "Empty"; }    // Reset display to blank

		if (bpTiletype == "Junk1" || bpTiletype == "Junk2" || bpTiletype == "Junk3") { // Special case: junk
			arrowText.text = "";	
			displayTiletypes[4] = "Junk1";
			displayTiletypes[5] = "Junk2";
			displayTiletypes[6] = "Junk3";
		} else if (bpTiletype == "Rat" || bpTiletype == "NewRat") { // Special case: rat
			arrowText.text = ">>>";
			displayTiletypes[0] = "Rat";
			displayTiletypes[1] = "Junk3";
			displayTiletypes[2] = "Junk2";
			displayTiletypes[3] = "Junk1";
			displayTiletypes[4] = "Junk1";
			displayTiletypes[5] = "Junk2";
			displayTiletypes[6] = "Junk3";
		} else if (bpTiletype == "Storage") { // Special case: storage
			arrowText.text = ">>>";
			displayTiletypes[0] = "Storage";
			for (int n = 1; n < displayTiletypes.Length; n++) {displayTiletypes[n] = "Panel";} // Box of 9 panels for a storage
		} else if (!Data.patternresults.ContainsKey(bpTiletype)) { // Atomic tiles like dirt/tools
			arrowText.text = "";
			displayTiletypes[5] = bpTiletype;
		} else { // Actual craftable recipe on display
			arrowText.text = ">>>";
			displayTiletypes[0] = bpTiletype;
			switch (Data.patternresults[bpTiletype].type) {
				case "3Cont":
					displayTiletypes[1] = (Data.patternresults[bpTiletype].value[0, 0]);
					displayTiletypes[2] = (Data.patternresults[bpTiletype].value[0, 0]);
					displayTiletypes[3] = (Data.patternresults[bpTiletype].value[0, 0]);
					break;
				case "4Cont":
					displayTiletypes[1] = (Data.patternresults[bpTiletype].value[0, 0]);
					displayTiletypes[2] = (Data.patternresults[bpTiletype].value[0, 0]);
					displayTiletypes[3] = (Data.patternresults[bpTiletype].value[0, 0]);
					displayTiletypes[4] = (Data.patternresults[bpTiletype].value[0, 0]); 
					break;
				case "Static":
					for (int y = 0; y < 3; y++) {
						for (int x = 0; x < 3; x++) {
							if (Data.patternresults[bpTiletype].value[x,y] == "" || Data.patternresults[bpTiletype].value[x,y] == "Empty") {
								displayTiletypes[3*y + x + 1] = "Empty";
							} else {
								displayTiletypes[3*y + x + 1] = Data.patternresults[bpTiletype].value[x,y];
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
			if (PlayerData.playerseen[displayTiletypes[n]]) { 
				displayTiles[n].SetTile(displayTiletypes[n]);
			}
			else {
				displayTiles[n].SetTile(displayTiletypes[n],Tile.States.sillhouette);
			}
		}
	}

	public void Click(int index) {
		if (displayTiletypes[index] == "Panel" && !PlayerData.playerseen["Panel"]) {
			return;
		} else if (displayTiletypes[index] != "Empty") {
			Core.codex.OpenBlueprint(displayTiletypes[index]);
		}
	}
}
