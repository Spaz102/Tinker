using UnityEngine;
using System.Collections;

public class Storage : MonoBehaviour { // Always accompanied by a Tile class
	public string stored; // The state of the actual tile being stored and displayed

	void Start () {
		stored = "";
		GameObject newunderlay = UnityEngine.Object.Instantiate(Data.ghosttemplate, this.gameObject.transform.position, this.gameObject.transform.rotation) as GameObject;
		newunderlay.GetComponent<Tile>().transform.SetParent(this.gameObject.transform);
		newunderlay.GetComponent<RectTransform>().transform.localPosition = new Vector3(0,0,0);
		newunderlay.GetComponent<RectTransform>().sizeDelta = new Vector2(0,0);
		newunderlay.GetComponent<RectTransform>().localScale = Vector3.one;
		newunderlay.GetComponent<Ghost>().lifespan = -1;
		this.gameObject.GetComponent<Tile>().underlay = newunderlay.GetComponent<Tile>();
		this.gameObject.GetComponent<Tile>().ShowSprite("Empty");
		this.gameObject.GetComponent<Tile>().underlay.ShowSprite("Storage"); //TODO: Use a special storage sprite
	}

	public void OnMouseUpAsButton() { // Super naive implementation - just swaps hand and stored
		if (stored == "" || stored == "Empty") {
			stored = Game.hand;
			Game.SetHand("Random");
		} else {
			string temp = Game.hand;
			Game.SetHand(stored);
			stored = temp;
		}
		this.gameObject.GetComponent<Tile>().ShowSprite(stored);
	}

	public void OnMouseEnter() { // Seems to work via raytracing, so no need to worry about layers or z-fighting if it isn't happening visually
		this.gameObject.GetComponent<Tile>().underlay.ShowSprite("Mouseover");
	}
	public void OnMouseExit() { //TODO: Add check for if program focus lost/regained? (Add a pause screen)
		this.gameObject.GetComponent<Tile>().underlay.ShowSprite("Storage");
	}

	public void OnDestroy() { // Unity calls this just before the object is actually destroyed
		if (this.gameObject.GetComponent<Tile>().underlay != null) {
			GameObject.Destroy(this.gameObject.GetComponent<Tile>().underlay.gameObject);
		}
	}
}
