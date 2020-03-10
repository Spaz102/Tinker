using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour { // Is also either a ghost, or interactive
	public int hidden; // # of frames before unhiding (For animations/transitions), -1 for indefinite
	public Tile underlay; // null if already underlay // Is this the best way? If so, another layer for drop shadows? // This makes me super uneasy
	public bool breathing; // Rats pulse to show agency
	public float scale;

	public void Update() {
		if (hidden == 1) { // Done this way so it only calls once, and preserves -1 as a special value
			hidden = 0;
			Fade(1);
		} else if (hidden > 1) {
			hidden--;
		}
		if (breathing) {
			scale += 0.0025f;
			this.transform.localScale = new Vector3(0.875f + Mathf.Clamp(Mathf.PingPong(scale, 0.2f), 0.05f, 0.185f), 0.875f + Mathf.Clamp(Mathf.PingPong(scale, 0.2f), 0.05f, 0.185f), 1);
			//this.transform.localScale.x = 0.5f + Mathf.PingPong(scale, 1.5f);
			//this.transform.localScale.y = 0.5f + Mathf.PingPong(scale, 1.5f);
		} else {
			scale = 0.2f;
			//this.transform.localScale = new Vector3(1, 1, 1);

		}
	}

	public void Hide(int length) {
		Fade(0);
		this.hidden = length + 1;
	}

	public void ShowSprite(string state) { // Try to use only when state changes, or unhiding
		TileDef temp = null;
		if (Data.tiledefs.TryGetValue(state, out temp)) {
			this.GetComponent<UnityEngine.UI.Image>().sprite = temp.sprite;
			this.GetComponent<UnityEngine.UI.Image>().color = temp.color;
			Fade(temp.color.a);
		} else {
			Debug.Log("bad tiledef");
		}
	}

	public void Fade(float amount) { // Currently only used to hide tiles during animations - and for the hand
		Color clr = this.GetComponent<UnityEngine.UI.Image>().color;
		if (hidden > 0 || this.GetComponent<UnityEngine.UI.Image>().sprite == null) {
			clr.a = 0;
		} else {
			clr.a = amount;
		}
		this.GetComponent<UnityEngine.UI.Image>().color = clr;
	}

	public void Recolour(float r, float g, float b) { //TODO: Phase out usage
		Color clr = this.GetComponent<UnityEngine.UI.Image>().color;
		clr.r = r;
		clr.g = g;
		clr.b = b;
		this.GetComponent<UnityEngine.UI.Image>().color = clr;
	}

	public void SmartShow(string state) {
		this.ShowSprite(state);
		if (!Data.playerseen[state]) {
			this.Recolour(0,0,0);
		}
	}
}