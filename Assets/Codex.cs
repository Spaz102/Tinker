using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// Handler for the blueprint ui element (loads the needed blueprint_page and hands off to it)
/// </summary>
public class Codex : MonoBehaviour {
	public BlueprintPage statBlueprint;
	public BlueprintPage setBlueprint;
	public BlueprintPage specBlueprint;

	public Tile[] buttons;

	/// <summary>
	/// Initial setup for codex, set the layout per data, generate buttons/paths
	/// </summary>
	void Start () {

		// create the codex tiles and lay them out per data
		foreach (var item in Data.tiledefs)
		{
			if (item.Key == null || item.Key == "" || !Data.tiledefs.ContainsKey(item.Key))
			{
				Debug.Log(item.Key);
				return;
			}
			if (item.Value.codexPos == new Vector2(-1, -1)) { continue; }
			GameObject newentry = UnityEngine.Object.Instantiate(Data.codexitemtemplate, new Vector3(), new Quaternion(), GameObject.Find("CodexContainer").transform) as GameObject;

			newentry.transform.localScale = Vector3.one;
			newentry.GetComponent<RectTransform>().anchoredPosition = new Vector2(item.Value.codexPos.x * 220, item.Value.codexPos.y * -300);
			newentry.GetComponentInChildren<Tile>().newTiletype = item.Key;
			newentry.GetComponentInChildren<Button>().onClick.AddListener(CodexClick);

			// Create paths to be drawn on the codex (as needed)
			if (Data.codexpaths.ContainsKey(item.Key))
			{
				foreach (Vector3 path in Data.codexpaths[item.Key])
				{
					GameObject newpath = UnityEngine.Object.Instantiate(Data.codexpathtemplate, new Vector3(), new Quaternion(), newentry.transform.GetChild(1)) as GameObject;
					
					// sprite sheet only goes down/right so you need to flip the image for up/left
					if (path.x < 0) {newpath.transform.localScale = new Vector3(-1, 1, 1);}
					if (path.y < 0) {newpath.transform.localScale = new Vector3(newpath.transform.localScale.x, -1, 1);}
					// get sprite number from x,y coords. 'z' coord is '0' for the default image or '1' for the alternative option.
					newpath.GetComponent<UnityEngine.UI.Image>().sprite = Data.pathsprites[(int)(Math.Abs(path.z * 15) + Math.Abs(path.y * 5) + Math.Abs(path.x * 2))];
					newpath.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
				}
			}
			
		}
		buttons = this.gameObject.GetComponentsInChildren<Tile>();
		Recalc();
	}

	/// <summary>
	/// After change, recalc which Codecs should be shown/highlit
	/// </summary>
	public void Recalc () {
		foreach (Tile butt in buttons) {
			if (butt.tiletype != butt.newTiletype) { 
				butt.Update();
			}
			if (!PlayerData.playerseen.ContainsKey(butt.tiletype))
			{ // Doesn't exist
				Debug.Log("Invalid blueprint button");
			}
			else {
				if (PlayerData.playerseen[butt.tiletype] && PlayerData.playerread[butt.tiletype])
				{
					butt.newState = Tile.States.normal;
					butt.transform.parent.Find("Paths").gameObject.SetActive(true);
				}
				else if (PlayerData.playerseen[butt.tiletype] && !PlayerData.playerread[butt.tiletype])
				{
					butt.newState = Tile.States.glowing;
					butt.transform.parent.Find("Paths").gameObject.SetActive(true);
				}
				else if (!PlayerData.playerseen[butt.tiletype] && CanMake(butt.tiletype))
				{
					butt.newState = Tile.States.sillhouette;
					butt.transform.parent.Find("Paths").gameObject.SetActive(true);
				}
				else if (!PlayerData.playerseen[butt.tiletype] && !CanMake(butt.tiletype))
				{
					butt.newState = Tile.States.invisible;
					butt.transform.parent.Find("Paths").gameObject.SetActive(false);
				}
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
			if (required != "" && required != "Empty" && !PlayerData.playerseen[required]) { // Contains an unseen ingredient
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
		if (!CanMake(tiletype) && !PlayerData.playerseen[tiletype])
		{
			Debug.Log("Invalid blueprint button2");
			return;
		}

		PlayerData.playerread[tiletype] = true;
		Recalc();
		if (DevMode.codexcloning) {
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
