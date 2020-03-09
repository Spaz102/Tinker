﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Game {
	public static void Awake() {}

	public static Tile cursor;
	public static string hand;
	public static Coord mouseover;
	public static Settings settings;

	public static int animationEnd;
	public static Coord queuedClick;
	public static int idletime;

	public static Board board;

	public static Blueprints blueprints;
	public static Options options;
	public static ComboSounds combosounds;

	public static System.Random rng;
	private static int handPoolSize;

	static Game () {
		Data.Awake();
		UnityEngine.Object.Destroy(GameObject.Find("Tile"));
		UnityEngine.Object.Destroy(GameObject.Find("Storage"));
		board = GameObject.Find("PlayArea").GetComponent<Board>();
		cursor = GameObject.Find("Hand").GetComponent<Tile>();
		combosounds = GameObject.Find("Combosounds").GetComponent<ComboSounds>();
		blueprints = GameObject.Find("BlueprintsMenu").GetComponent<Blueprints>();
		CalcHandPoolSize();
		rng = new System.Random();
		mouseover = null;
		SetHand("Random");
		settings = new Settings();
	}

	public static void Update () {
		Idle();
		MoveHand();
		if (board.CheckGameOver()) {
			//board.SetBoard("Empty");
			board.CleanUp();
			SetHand("Random");
		}
		if (animationEnd > 0) {
			animationEnd--;
		} else {
			Game.Click();
		}
	}

	public static void MoveHand() { //TODO mouseover snap for storage tiles, also use a more direct method of defining cursor position, for resolution flexibility
		Vector3 mouse = Input.mousePosition;
		if ((mouseover != null) && (CanClick(board.state[mouseover.x,mouseover.y]) != board.state[mouseover.x,mouseover.y])) {
			cursor.GetComponent<RectTransform>().anchoredPosition = new Vector2(board.tile[mouseover.x,mouseover.y].transform.position.x * 1 + 0, board.tile[mouseover.x,mouseover.y].transform.position.y * 1 + 0); // Cursor snaps to valid position
		} else {
			cursor.GetComponent<RectTransform>().anchoredPosition = new Vector2(mouse.x*30/480 - 15, mouse.y*50/800 - 25); // Otherwise cursor is freeform
		}
	}

	public static void Mouseover() { // Updates interface reactions, and highlights potential changes on click
		board.ClearHighlights();
		if ((mouseover == null) || (CanClick(board.state[mouseover.x,mouseover.y]) == board.state[mouseover.x,mouseover.y]) || hand == "Rat") { // Go no further if clicking won't cause changes on the board
			return;
		}

		string[,] boardcopy = (string[,]) board.state.Clone(); // Copy the board
		boardcopy[mouseover.x, mouseover.y] = CanClick(board.state[mouseover.x,mouseover.y]); // Apply the click to the clone
		board.ResolvePatterns(mouseover, ref boardcopy, true); // Show patterns on the hypothetical board
		board.tile[mouseover.x,mouseover.y].underlay.ShowSprite("Mouseover"); // Override any pattern highlighting with mouseover indicator
	}

	public static string CanClick(string targetState) { // For mouseover snap and game-over checking; returns new value //TODO: clean up, change to bool?
		if (targetState == "Storage") {
			return "StoreHand";
		} else if (hand == "Rat") {
			if (targetState != "Empty") { // Hand-rats are hungrier than board-rats
				return "Empty";
			} else { // Drop rat
				return "NewRat";
			}
		} else if (hand == "Special") { // Anything that must be clicked on a resource
			if (targetState != "Empty") { // Or a part of a construct?
				return "Empty";
			}
		} else { // Ordinary tile
			if (targetState == "Empty") { // Or storage tile
				return hand;
			}
		}
		return targetState; // Cannot click
	}

	public static void QueueClick(Coord target) {
		queuedClick = target;
		board.AnimatePoof(target, 0.75f);
	}

	public static void Click() { // Actually applies a valid click
		if (queuedClick == null) {
			return;
		}
		idletime = 0;
		string result = CanClick(board.state[queuedClick.x,queuedClick.y]);
		if (result != board.state[queuedClick.x,queuedClick.y]) { // A click is possible
			if (hand == "Special") {
				Data.playerseen["Special"] = true;
				PlaySound("Special");
				SetHand(board.state[queuedClick.x,queuedClick.y]);
				board.Set(queuedClick, "Empty");
			} else if (hand == "Rat") {
				Data.playerseen["NewRat"] = true;
				Data.playerseen["Rat"] = true;
				PlaySound("Rat");
				SetHand("Random");
				if (board.state[queuedClick.x,queuedClick.y] == "Empty") {
					board.Set(queuedClick, "NewRat");
				} else {
					board.Set(queuedClick, "Empty");
				}
			} else { // Hand is a normal tile
				PlaySound("PlaceTile");
				SetHand("Random");
				board.Set(queuedClick, result);
			}
				
			if (result != "Rat") { // Otherwise hand rats merge with placed junk
				board.ResolvePatterns(queuedClick, ref board.state, false);
			}

			if (result != "Empty") { // No time progressed for hand-rats, specials, or storage; even if they trigger a pattern (Only possible by putting a hole in a pattern where "Empty" is needed)
				board.MoveRats();
			}
			Game.Mouseover();
		}
		queuedClick = null;
	}

	public static void SetHand(string setto) { //TODO make less memory efficient but easier to work with? (list of strings with repetition, pull random entry from list)
		if (setto == "Random" || setto == "Empty" || setto == "" ) {
			hand = GetRandom();
			if (hand == "Error") {
				Debug.Log("Invalid tile drawn to hand");
			}
		} else {
			hand = setto;
		}
		cursor.ShowSprite(hand);
		cursor.Fade(0.75f);
		//board.ShowText(hand);
	}

	private static void CalcHandPoolSize() { // Run once on startup
		handPoolSize = 0;
		foreach (TileDef def in Data.tiledefs.Values) {
			handPoolSize += def.chancetodraw;
		}
	}
	public static string GetRandom() { // Seed is int from 0-sum of all chancetodraw in tiledefs
		int remaining = Game.rng.Next(handPoolSize); // Pulls a random tile, with respect to relative chances
		foreach (string key in Data.tiledefs.Keys) {
			remaining -= Data.tiledefs[key].chancetodraw;
			if (remaining < 0) {
				return key;
			}
		}
		return "Error";
	}

	public static void PlaySound(string name) {
		board.GetComponent<AudioSource>().PlayOneShot(Data.audiofiles[name]);
	}

	public static void Idle() { //TODO rat breathing
		idletime++;
		if (idletime > 255) { // Pulse random tile?
			idletime = 0;
		}
		//board.ShowText(idletime.ToString());
	}
}

public class Coord { // Warning~ Cannot yet be compared directly (Needs to override Equals())
	public int x;
	public int y;

	public Coord(int x, int y) {
		this.x = x;
		this.y = y;
	}

	public Coord Left() {
		return new Coord(this.x-1, this.y);
	}
	public Coord Right() {
		return new Coord(this.x+1, this.y);
	}
	public Coord Up() {
		return new Coord(this.x, this.y+1);
	}
	public Coord Down() {
		return new Coord(this.x, this.y-1);
	}

	public List<Coord> Neighbors() {
		List<Coord> results = new List<Coord>();
		if (this.x - 1 >= 0) {results.Add(this.Left());}
		if (this.x + 1 < Game.board.boardWidth) {results.Add(this.Right());}
		if (this.y - 1 >= 0) {results.Add(this.Down());}
		if (this.y + 1 < Game.board.boardHeight) {results.Add(this.Up());}
		return results;
	}

	public bool InBounds() {
		return (this.x >=0 && this.y >= 0 && this.x < Game.board.boardWidth && this.y < Game.board.boardHeight);
	}
}

public class Settings {
	public bool dust;
	public bool sound;

	public Settings() {
		dust = true;
		sound = true;
	}
}
