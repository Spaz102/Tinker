using UnityEngine;
using System.Collections;

public class ComboSounds : MonoBehaviour {
	private int framecount;
	private int targetcount;
	public AudioSource src;

	public void Set(int combo) {
		framecount = 0;
		targetcount = combo*Game.board.animationLength;
	}

	void Start () {
		framecount = -1;
		targetcount = 0;
	}
	
	void Update () {
		if (framecount <= -1) {
			return;
		} else if (framecount == 0) {
			src.PlayOneShot(Data.audiofiles["Combo1"]);
		} else if (framecount == Game.board.animationLength) {
			src.PlayOneShot(Data.audiofiles["Combo2"]);
		} else if (framecount == 2*Game.board.animationLength) {
			src.PlayOneShot(Data.audiofiles["Combo3"]);
		} else if (framecount == 3*Game.board.animationLength) {
			src.PlayOneShot(Data.audiofiles["Combo4"]);
		} else if (framecount % Game.board.animationLength == 0) {
			src.PlayOneShot(Data.audiofiles["Combo5"]);
		}
		if (framecount >= targetcount) {
			framecount = -1;
		} else {
			framecount++;
		}
	}
}
