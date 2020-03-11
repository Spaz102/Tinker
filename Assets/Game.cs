using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public static class Game {
	public static void Awake() {}

	public static Canvas mainCanvas;
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

	public static System.Random rng;
	private static int handPoolSize; //TODO: Move to Data and calculate on Awake()

	static Game () {
		Data.Awake();
		UnityEngine.Object.Destroy(GameObject.Find("Tile"));
		UnityEngine.Object.Destroy(GameObject.Find("Storage"));
		board = GameObject.Find("PlayArea").GetComponent<Board>();
		mainCanvas = GameObject.Find("Main Canvas").GetComponent<Canvas>();
		cursor = GameObject.Find("Hand").GetComponent<Tile>();
		blueprints = GameObject.Find("BlueprintsMenu").GetComponent<Blueprints>();
		CalcHandPoolSize();
		rng = new System.Random();
		mouseover = null;
		SetHand("Random");
		cursor.transform.localScale = new Vector3(0.75f, 0.75f, 1);
		settings = new Settings();
	}

	public static void Update () {
		Idle();
		MoveHand();
		if (board.CheckGameOver()) {
			board.CleanUp();
			SetHand("Random");
		}
		if (animationEnd > 0) {
			animationEnd--;
		} else {
			Game.Click();
		}
	}

	public static void MoveHand() { //TODO: Mouseover snap for storage tiles, also use a more direct method of defining cursor position, for resolution flexibility
		Vector3 mouse = Input.mousePosition;
		if ((mouseover != null) && (CanClick(board.state[mouseover.x,mouseover.y]) != board.state[mouseover.x,mouseover.y])) {
			cursor.transform.position = board.tile[mouseover.x, mouseover.y].transform.position;
		} else {
			Vector2 pos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(mainCanvas.transform as RectTransform, mouse, mainCanvas.worldCamera, out pos);
			cursor.transform.position = mainCanvas.transform.TransformPoint(pos);
		}
	}

	public static void Mouseover() { // Updates interface reactions, and highlights potential changes on click
		board.ClearHighlights();
		if ((mouseover == null) || (CanClick(board.state[mouseover.x,mouseover.y]) == board.state[mouseover.x,mouseover.y])) { // Go no further if clicking won't cause changes on the board
		//if ((mouseover == null) || (CanClick(board.state[mouseover.x,mouseover.y]) == board.state[mouseover.x,mouseover.y]) || hand == "Rat") { // Go no further if clicking won't cause changes on the board
			return;
		}

		string[,] boardcopy = (string[,]) board.state.Clone(); // Copy the board
		boardcopy[mouseover.x, mouseover.y] = CanClick(board.state[mouseover.x,mouseover.y]); // Apply the click to the clone
		board.ResolvePatterns(mouseover, ref boardcopy, true); // Show patterns on the hypothetical board
		board.tile[mouseover.x,mouseover.y].underlay.ShowSprite("Mouseover"); // Override any pattern highlighting with mouseover indicator
	}

	public static string CanClick(string targetState) { // For mouseover snap and game-over checking; returns new value of the board tile (Returning its current state implies an invalid move)
		if (targetState == "Storage") {
			return "StoreHand";
		} else if (hand == "Rat") {
			if (targetState != "Empty") { // Hand-rats are hungrier than board-rats
				return "Empty";
			} else { // Drop rat
				return "NewRat";
			}
		} else if (hand == "Special") {
			return "Empty";
		} else { // Ordinary tile
			if (targetState == "Empty") { // Or storage tile
				return hand;
			}
		}
		return targetState; // Cannot click
	}

	public static void QueueClick(Coord target) {
		queuedClick = target;
		if ((board.state[target.x, target.y] == "Empty") != (hand == "Special")) { // Only dustpoof on placed tile
			board.AnimatePoof(target, 10.75f);
		}
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
				
			if (result != "Rat" && result != "NewRat") { // Otherwise hand rats merge with placed junk
				board.ResolvePatterns(queuedClick, ref board.state, false);
			}

			if (result != "Empty") { // No time progressed for hand-rats, specials, or storage; even if they trigger a pattern (Only possible by putting a hole in a pattern where "Empty" is needed)
				board.MoveRats();
			}
			Game.Mouseover();
		}
		queuedClick = null;
	}

	public static void SetHand(string setto) {
		if (setto == "Random" || setto == "Empty" || setto == "" ) {
			hand = GetRandom();
		} else {
			hand = setto;
		}
		if (!Data.playerseen[hand]) {
			Data.playerseen[hand] = true;
			blueprints.Recalc();
		}
		if (hand == "NewRat" && !Data.playerseen["Rat"]) {
			Data.playerseen["Rat"] = true;
			blueprints.Recalc();
		}
		cursor.ShowSprite(hand);
		cursor.Fade(0.75f);
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

	public static void Idle() {
		idletime++;
		if (idletime > 255) { //TODO: Ping suggested move?
			idletime = 0;
		}
		//board.ShowText(idletime.ToString());
	}
}

public class Coord {
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

	public bool Equals(Coord other) {
		return (this.x == other.x && this.y == other.y);
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
