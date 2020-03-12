using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ghost : MonoBehaviour { // Independant non-interactive Tile-like entity; handles all animations
	GameObject target;
	Vector3 start;
	Vector3 finish;
	public string animationstyle;
	public int lifespan;
	int delay;
	string audioEvent; // On complete for slide, on turnaround for bump

	public void Spawn(string style, Vector3 target, int delay, bool freezeinputs, string audio = null) {
		this.animationstyle = style;
		this.delay = delay;
		this.start = this.gameObject.transform.position;
		this.audioEvent = audio;

		if (style == "Slide" || style == "Bump") {
			this.lifespan = Game.board.animationLength + delay + 1;
			this.finish = target;
		} else if (style == "Poof") {
			this.lifespan = 10;
		}

		if (freezeinputs) {
			Game.animationEnd = (int) Mathf.Max(Game.animationEnd, lifespan);
		}
	}
	
	public void Update () {
		if (lifespan == -1) {
			return; // Underlay; lasts forever
		} else if (lifespan == 1) {
			if (!string.IsNullOrEmpty(audioEvent) && animationstyle == "Slide") {
				Game.PlaySound(audioEvent);
			}
			Object.Destroy(this.gameObject);
			//TODO: Check if it should play an audio file??
			return;
		}
		lifespan--;
		if (delay > 0) {
			delay--;
			return;
		}
		if (animationstyle == "Slide") {
			SlideTowards();
			Pulse(0.6f);
		} else if (animationstyle == "Bump") {
			Bump();
		} else if (animationstyle == "Poof") {
			Grow(1.08f);
			Fade(0.8f);
		} else {
			//Pulse(1);
		}
	}

	public void SlideTowards() {
		this.gameObject.transform.position = Vector3.Lerp(finish, start, ((float)lifespan / (float)Game.board.animationLength)); // Reversed because it lerps from 1 to 0
	}

	public void Bump() { // Rats' gnawing
		if (!string.IsNullOrEmpty(audioEvent) && (float)lifespan / Game.board.animationLength > 0.5f) { // Bump at the halfway point?
			Game.PlaySound(audioEvent);
			this.audioEvent = null; // Make sure it only plays once
		}
		this.gameObject.transform.position = Vector3.Lerp(start, finish, Mathf.PingPong((float)lifespan * 2f / (float)Game.board.animationLength, 1f));
	}

	public void Grow(float amount) {
		this.gameObject.transform.localScale = this.gameObject.transform.localScale * amount;
	}

	public void Fade(float amount) {
		float newalpha = this.GetComponent<UnityEngine.UI.Image>().color.a * amount;
		this.GetComponent<UnityEngine.UI.Image>().color = new Color(1,1,1,newalpha);
	}

	public void Pulse(float amount) { // Small for mouseover, medium for tile placement, large for pattern resolution
		float scale = ((float)(Game.board.animationLength) - (float)lifespan) / (Game.board.animationLength); // 0 to 0.5f to 1
		if (lifespan > (Game.board.animationLength/2)) {
			this.gameObject.transform.localScale = new Vector3((1 + scale*amount*2), (1 + scale*amount*2), 1); // 1 to 1+amount
		} else {
			this.gameObject.transform.localScale = new Vector3((1 + (1-scale)*amount*2), (1 + (1-scale)*amount*2), 1); // 1+amount to 1
		}
	}

}
