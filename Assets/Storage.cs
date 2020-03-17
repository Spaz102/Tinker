using UnityEngine;
using System.Collections;

public class Storage : MonoBehaviour { // Always accompanied by a Tile class
	public string stored; // The state of the actual tile being stored and displayed

	void Start () {
		stored = "Empty";
		GameObject newunderlay = UnityEngine.Object.Instantiate(Data.ghosttemplate, this.gameObject.transform.position, this.gameObject.transform.rotation) as GameObject;
		newunderlay.GetComponent<Tile>().transform.SetParent(this.gameObject.transform);
		newunderlay.GetComponent<RectTransform>().transform.localPosition = Vector3.zero;
		newunderlay.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
		newunderlay.GetComponent<RectTransform>().localScale = Vector3.one;
		newunderlay.GetComponent<Ghost>().lifespan = -1;
		this.gameObject.GetComponent<Tile>().underlay = newunderlay.GetComponent<Tile>();
		this.gameObject.GetComponent<Tile>().ShowSprite("Empty");
		this.gameObject.GetComponent<Tile>().underlay.ShowSprite("Storage"); //TODO: Use a special storage sprite
	}

	public void OnMouseUpAsButton() { // Super naive implementation - just swaps hand and stored
		if (stored == "Empty") {
			Set(Game.hand);
			Game.SetHand("Random");
		} else {
			string temp = stored;
			Set(Game.hand);
			Game.SetHand(temp);
		}
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
		if (this.gameObject.GetComponent<Tile>().underlay != null) {
			GameObject.Destroy(this.gameObject.GetComponent<Tile>().underlay.gameObject);
		}
	}
}
