﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ghost : MonoBehaviour { // Independant non-interactive Tile-like entity; handles all animations
	GameObject target;
	Vector3 start;
	Vector3 finish;
	public string animationstyle;
	public int lifespan;
	//public List<Vector3> targets; //TODO change to coord, move tile hiding mechanics here (The ghost sets and resets the target's a)
	int delay;

	public void Spawn(string style, Vector3 target, int delay, bool freezeinputs) {
		this.animationstyle = style;
		this.delay = delay; //TODO animation queue
		this.start = this.gameObject.transform.position;

		if (style == "Slide") {
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
			Object.Destroy(this.gameObject);
			return;
		}
		lifespan--;
		if (delay > 0) { //TODO animation queue
			delay--;
			return;
		}
		if (animationstyle == "Slide") {
			SlideTowards();
			Pulse(0.6f);
		} else if (animationstyle == "Bump") {
			Bump();
		} else if (animationstyle == "Poof") {
			Grow(1.09f);
			Fade(0.8f);
		} else {
			//Pulse(1);
		}
	}

	public void SlideTowards() {
		this.gameObject.transform.position = Vector3.Lerp(finish, start, ((float)lifespan / (float)Game.board.animationLength)); // Reversed because it lerps from 1 to 0
	}

	public void Bump() { // Rats' gnawing
		this.gameObject.transform.position = Vector3.Lerp(finish, start, Mathf.PingPong((float)lifespan * 2 / (float)Game.board.animationLength, 1)); // Reversed because it lerps from 1 to 0
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

	public void HideTile() {

	}
}