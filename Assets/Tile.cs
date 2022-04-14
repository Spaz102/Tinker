using UnityEngine;
using System.Collections;

/// <summary>
/// Tile graphics handler. Tiles include the cursor
/// </summary>
public class Tile : MonoBehaviour {
	public enum States { normal = 0, invisible = 1, sillhouette = 2, glowing = 3, breathing = 4};
	public States state;
	public States newState;

	public string newTiletype; // What type of tile (dirt, stick)
	public string tiletype;

	public int hiddenTransitionTime; // # of frames before unhiding (For animations/transitions), -1 for indefinite
	public Tile underlay; // null if already underlay // Is this the best way? If so, another layer for drop shadows? // This makes me super uneasy
	public float breathCycle; // Initialize to a random value between 0 and 1

	public void Start()
	{
		breathCycle = Random.Range(0f, 1f); //Random start point for breathing so tiles aren't all in sync
	}

	/// <summary>
	/// Update tiles. Handles animations and state changes
	/// </summary>
	public void Update() {

		if (hiddenTransitionTime == 1) { // Done this way so it only calls once, and preserves -1 as a special value
			hiddenTransitionTime = 0;
			Fade(1);
		} else if (hiddenTransitionTime > 1) {
			hiddenTransitionTime--;
		}

		if (newState != state || newTiletype != tiletype)
		{
			state = newState;
			tiletype = newTiletype;
			switch (state)
			{
				case States.glowing:
				case States.breathing:
				case States.normal:
				default:
					this.ShowSprite();
					break;
				case States.sillhouette:
					this.ShowSprite();
					this.Sillhouette();
					break;
				case States.invisible:
					this.Fade(0);
					break;
			}
		}
		switch (state){
			case States.glowing:
			case States.breathing:
				breathCycle = (breathCycle + 0.00625f) % 1f; // 160 frame animation
				float thisScaling = Mathf.Clamp(0.815f + 0.5f * Mathf.PingPong(breathCycle, 0.5f), 0.88f, 1.06f); // 0.815 to (0.88 to 1.06) to 1.065
				this.transform.localScale = new Vector3(thisScaling, thisScaling, 1);
				break;
		}
	}

	/// <summary>
	/// Convenient 1 line setter for tiles
	/// </summary>
	public void SetTile(string settiletype, States setstate = States.normal)
	{
		newTiletype = settiletype;
		newState = setstate;
	}

	/// <summary>
	/// Temporarily hide tiles (for animations)
	/// </summary>
	/// <param name="length">how many frames to hide</param>
	public void Hide(int length) { // Make this tile completely transparent, but still active, for n frames
		Fade(0);
		this.hiddenTransitionTime = length + 1;
	}

	/// <summary>
	/// Reset and normally display tile
	/// </summary>
	private void ShowSprite() {
		TileDef temp;
		if (Data.tiledefs.TryGetValue(tiletype, out temp)) {
			this.GetComponent<UnityEngine.UI.Image>().sprite = temp.sprite;
			this.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, temp.opacity);
			Fade(temp.opacity); // Makes sure hiddenTransitionTime tiles are still hiddenTransitionTime
		} else {
			Debug.Log("bad tiledef");
		}
	}

	/// <summary>
	/// Fade a tile (includes cursor part fading on mouseover)
	/// </summary>
	/// <param name="amount">0=invisible, 1=opaqu</param>
	public void Fade(float amount) {
		Color clr = this.GetComponent<UnityEngine.UI.Image>().color;
		if (hiddenTransitionTime > 0 || this.GetComponent<UnityEngine.UI.Image>().sprite == null) {
			clr.a = 0;
		} else {
			clr.a = amount;
		}
		this.GetComponent<UnityEngine.UI.Image>().color = clr;
	}

	/// <summary>
	/// Turns tile entirely black (preserves alpha layer)
	/// </summary>
	private void Sillhouette() {
		Color clr = this.GetComponent<UnityEngine.UI.Image>().color;
		clr.r = 0;
		clr.g = 0;
		clr.b = 0;
		this.GetComponent<UnityEngine.UI.Image>().color = clr;
	}
}