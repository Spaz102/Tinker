using UnityEngine;
using System.Collections;

public class Storage : MonoBehaviour { // Always accompanied by a Tile class
	public string stored; // The state of the actual tile being stored and displayed

	void Start () { // Warning: Sometimes gets called after the game has loaded and set each storage
		stored = (string.IsNullOrEmpty(stored))? "Empty" : stored;
		GameObject newunderlay = UnityEngine.Object.Instantiate(Data.ghosttemplate, this.gameObject.transform.position, this.gameObject.transform.rotation, this.gameObject.transform) as GameObject;
		newunderlay.GetComponent<Ghost>().lifespan = -1;
		
		this.gameObject.GetComponent<Tile>().underlay = newunderlay.GetComponent<Tile>();
		this.gameObject.GetComponent<Tile>().ShowSprite(stored);
		this.gameObject.GetComponent<Tile>().underlay.ShowSprite("Storage"); //TODO: Use a special storage sprite
	}

	public void OnMouseUpAsButton() { // Super naive implementation - just swaps hand and stored; then saves the game
		if (stored == "Empty") {
			Set(Game.hand);
			Game.SetHand("Random");
		} else {
			string temp = stored;
			Set(Game.hand);
			Game.SetHand(temp);
		}
		Game.board.SaveGame();
	}

	public void Set(string setTo) {
		stored = setTo;
		this.gameObject.GetComponent<Tile>().ShowSprite(stored);
	}

	public void OnMouseEnter() { // Seems to work via raytracing, so no need to worry about layers or z-fighting if it isn't happening visually
		this.gameObject.GetComponent<Tile>().underlay.ShowSprite("Mouseover");
	}
	public void OnMouseExit() { //TODO: Add check for if program focus lost/regained? (Add a pause screen)
		this.gameObject.GetComponent<Tile>().underlay.ShowSprite("Storage");
	}

	public void OnDestroy() { // Unity calls this just before the object is actually destroyed
		Game.board.dependencies.RemoveAll(dep => dep.dependent == this.gameObject);
		if (this.gameObject.GetComponent<Tile>().underlay != null) {
			GameObject.Destroy(this.gameObject.GetComponent<Tile>().underlay.gameObject);
		}
		Game.board.RedrawAllStorage();
	}
}
