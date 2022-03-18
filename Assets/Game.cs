using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public static class Game {
	public static void Awake() {}
	public static System.Random rng;

	public static Canvas mainCanvas;
	public static Tile cursor;
	public static Board board;
	public static Blueprints blueprints;
	
	public static int handPoolSize;
	public static int startingPoolSize;
	public static string hand;
	public static Interactive mouseover;

	public static int animationEnd;
	public static Coord queuedClick;
	public static int idletime;
	
	public static Settings settings;

	static Game () {
		rng = new System.Random();
		Data.Awake();
		CalcHandPoolSize();
		
		UnityEngine.Object.Destroy(GameObject.Find("Tile"));
		UnityEngine.Object.Destroy(GameObject.Find("Storage"));
		board = GameObject.Find("PlayArea").GetComponent<Board>();
		mainCanvas = GameObject.Find("Main Canvas").GetComponent<Canvas>();
		cursor = GameObject.Find("Hand").GetComponent<Tile>();
		blueprints = GameObject.Find("CodexContainer").GetComponent<Blueprints>();
		
		mouseover = null;
		cursor.transform.localScale = new Vector3(0.75f, 0.75f, 1);
		settings = new Settings();
		Audio.PlaySound("Startup");
	}

	public static void Update () {
		Idle();
		MoveHand();
		if (board.CheckGameOver()) {
			Audio.PlaySound("GameOver");
			Debug.Log("You lose. Good day, sir!");
			board.CleanUp();
			SetHand("Random");
			board.SaveGame();
		}
		if (animationEnd > 0) {
			animationEnd--;
		} else {
			Game.Click();
		}
	}

	public static void MoveHand() {
		Vector3 mouse = Input.mousePosition;
		if (mouseover == null) { // Mouse is not over a clickable object (Blank area of interface)
			cursor.gameObject.SetActive(false); //TODO: Show hand on interface, as in mobile mode?
		} else if (mouseover.index != null) { // Mousing over a board tile
			if (CanClick(board.state[mouseover.index.x, mouseover.index.y]) != board.state[mouseover.index.x, mouseover.index.y]) { // Mouse is over a legal move
				cursor.gameObject.SetActive(true);
				cursor.transform.position = board.tile[mouseover.index.x, mouseover.index.y].transform.position; // Snap to position
			} else { // Mouse is over the board, but not a legal move
				Vector2 pos;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(mainCanvas.transform as RectTransform, mouse, mainCanvas.worldCamera, out pos);
				cursor.transform.position = mainCanvas.transform.TransformPoint(pos);
				cursor.gameObject.SetActive(true);
			}
		} else { // Mousing over an interface element (button, storage, etc)
			cursor.gameObject.SetActive(true);
			cursor.transform.position = mouseover.transform.position; // Snap to position
		}
	}	

	public static void Mouseover() { // Updates interface reactions, and highlights potential changes on click
		board.ClearHighlights();
		// Clear interface highlights
		if (mouseover != null) { // Mousing over something interactive
			if (mouseover.index == null) { // Mousing over an interface button, storage, etc
				//TODO: Add a second underlay for the mousover highlight (Needs to show the storage core, the highlight, AND the previous resource)
				foreach (Storage storage in board.storagelist) {
					//storage.gameObject.GetComponent<Tile>().ShowSprite("Storage");
				}
				//mouseover.gameObject.GetComponent<Tile>().underlay.ShowSprite("Mouseover");
			} else if (CanClick(board.state[mouseover.index.x, mouseover.index.y]) == "Win") { // Clicking a key onto a music box
				board.tile[mouseover.index.x, mouseover.index.y].underlay.ShowSprite("Mouseover"); // TODO: Something fancier?
			} else if (CanClick(board.state[mouseover.index.x, mouseover.index.y]) != board.state[mouseover.index.x, mouseover.index.y]) { // Clicking will cause changes on the board
				string[,] boardcopy = (string[,])board.state.Clone(); // Copy the board
				boardcopy[mouseover.index.x, mouseover.index.y] = CanClick(board.state[mouseover.index.x, mouseover.index.y]); // Apply the click to the clone
				board.ResolvePatterns(mouseover.index, ref boardcopy, true); // test = true will show valid patterns on the hypothetical board
				board.tile[mouseover.index.x, mouseover.index.y].underlay.ShowSprite("Mouseover"); // Override any pattern highlighting with mouseover indicator
			} // Else mousing over a board tile that can't be clicked
		}
		
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
		} else if (hand == "Key" && targetState == "MusicBox") {
			return "Win";
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
		if (result == "Win") {
			Audio.PlaySound("Byebye");
			Debug.Log("You win"); // TODO: Win popup
			SetHand("Random");
			board.Set(queuedClick, "Empty");
		} else if (result != board.state[queuedClick.x,queuedClick.y]) { // A click is possible
			Audio.PlaySound(hand);
			if (hand == "Special") {
				Data.playerseen["Special"] = true;
				SetHand(board.state[queuedClick.x,queuedClick.y]);
				board.Set(queuedClick, "Empty");
			} else if (hand == "Rat") {
				Data.playerseen["NewRat"] = true;
				Data.playerseen["Rat"] = true;
				SetHand("Random");
				if (board.state[queuedClick.x,queuedClick.y] == "Empty") {
					board.Set(queuedClick, "NewRat");
				} else {
					board.Set(queuedClick, "Empty");
				}
			} else { // Hand is a normal tile
				SetHand("Random");
				board.Set(queuedClick, result);
			}
				
			if (result != "NewRat") { // Anything other than placing a rat (which has recipes that should only happen mid-cascade
				board.ResolvePatterns(queuedClick, ref board.state, false);
			}

			if (result != "Empty") { // No time progressed for hand-rats, specials, or storage; even if they trigger a pattern (Only possible by putting a hole in a pattern where "Empty" is needed)
				board.MoveRats();
			}
			Game.Mouseover();
			board.SaveGame();
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
		startingPoolSize = 0; // Should sum to 100
		foreach (TileDef def in Data.tiledefs.Values) {
			handPoolSize += def.chanceToDraw;
			startingPoolSize += def.startingChance;
		}
	}
	public static string GetRandom() { // Seed is int from 0-sum of all chancetodraw in tiledefs
		int remaining = Game.rng.Next(handPoolSize); // Pulls a random tile, with respect to relative chances
		foreach (string key in Data.tiledefs.Keys) {
			remaining -= Data.tiledefs[key].chanceToDraw;
			if (remaining < 0) {
				return key;
			}
		}
		return "Error";
	}

	public static void Idle() {
		idletime++;
		if (idletime > 255) { //TODO: Ping suggested move?
			idletime = 0;
		}
	}
}

public class Coord { // [0,0] is bottom left corner
	public int x; // Left to right
	public int y; // Bottom to top

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
	private GameObject[] dustList;

	public Settings() {
		dustList = GameObject.FindGameObjectsWithTag("Dust");

		if (PlayerPrefs.HasKey("dust")) {
			dust = true;
			SetDust(PlayerPrefs.GetInt("dust") == 1);
		} else {
			dust = true;
			PlayerPrefs.SetInt("dust", 1);
			PlayerPrefs.Save();
		}

		if (PlayerPrefs.HasKey("sound")) {
			sound = PlayerPrefs.GetInt("sound") == 1;
		} else {
			sound = true;
			PlayerPrefs.SetInt("sound", 1);
			PlayerPrefs.Save();
		}
	}

	public void SetDust(bool setting) { //TODO: Update interface
		if (setting == dust) return;

		dust = setting;
		if (setting) {
			PlayerPrefs.SetInt("dust", 1);
			foreach (GameObject found in dustList) {
				found.SetActive(true);
			}
		} else {
			PlayerPrefs.SetInt("dust", 0);
			foreach (GameObject found in dustList) {
				found.SetActive(false);
			}
		}
		PlayerPrefs.Save();
	}
}
