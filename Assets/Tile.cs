using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour { // Is also either a ghost, or interactive
	public int hidden; // # of frames before unhiding (For animations/transitions), -1 for indefinite
	public Tile underlay; // null if already underlay // Is this the best way? If so, another layer for drop shadows? // This makes me super uneasy
	public bool breathing; // Rats pulse to show agency, interface pulses to show new information
	public float breathCycle; // Initialize to a random value between 0 and 1
	public string tiletype; // What type of tile (dirt, stick)

	public void Update() {
		if (hidden == 1) { // Done this way so it only calls once, and preserves -1 as a special value
			hidden = 0;
			Fade(1);
		} else if (hidden > 1) {
			hidden--;
		}
		if (breathing) {
			breathCycle = (breathCycle + 0.00625f) % 1f; // 160 frame animation
			float thisScaling = Mathf.Clamp(0.815f + 0.5f * Mathf.PingPong(breathCycle, 0.5f), 0.88f, 1.06f); // 0.815 to (0.88 to 1.06) to 1.065
			this.transform.localScale = new Vector3(thisScaling, thisScaling, 1);
		}
	}

	public void Hide(int length) { // Make this tile completely transparent, but still active, for n frames
		Fade(0);
		this.hidden = length + 1;
	}

	public void ShowSprite(string state) { // Try to use only when state changes, or unhiding
		TileDef temp;
		if (Data.tiledefs.TryGetValue(state, out temp)) {
			this.GetComponent<UnityEngine.UI.Image>().sprite = temp.sprite;
			this.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, temp.opacity);
			Fade(temp.opacity);
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

	public void Sillhouette() { // Make this tile all black (except transparent pixels) // Keeps alpha
		Color clr = this.GetComponent<UnityEngine.UI.Image>().color;
		clr.r = 0;
		clr.g = 0;
		clr.b = 0;
		this.GetComponent<UnityEngine.UI.Image>().color = clr;
	}

	public void SmartShow(string state) {
		this.ShowSprite(state);
		if (!Data.playerseen[state]) {
			this.Sillhouette();
		}
		if (!Data.playerread[state]) {
			StartBreathing();
		} else {
			this.breathing = false;
		}
	}

	public void StartBreathing() {
		this.breathing = true;
		this.breathCycle = Random.Range(0f, 1f);
	}
}