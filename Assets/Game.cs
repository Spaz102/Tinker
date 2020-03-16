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
	public static Coord mouseover;

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
		PlaySound("Startup");
	}

	public static void Update () {
		Idle();
		MoveHand();
		if (board.CheckGameOver()) {
			PlaySound("GameOver");
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

	public static void MoveHand() { //TODO: Mouseover snap for storage tiles
		Vector3 mouse = Input.mousePosition;
		if (mouseover == null)
		{ // Mouse is not over the board
			cursor.gameObject.SetActive(false);
		}
		else if (CanClick(board.state[mouseover.x, mouseover.y]) != board.state[mouseover.x, mouseover.y])
		{ // Mouse is over a legal move
			cursor.gameObject.SetActive(true);
			cursor.transform.position = board.tile[mouseover.x, mouseover.y].transform.position;
		}
		else
		{ // Mouse is over the board, but not a legal move
			Vector2 pos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(mainCanvas.transform as RectTransform, mouse, mainCanvas.worldCamera, out pos);
			cursor.transform.position = mainCanvas.transform.TransformPoint(pos);
			cursor.gameObject.SetActive(true);
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
			PlaySound(hand);
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
			
				
			if (result != "Rat" && result != "NewRat") { // Otherwise hand rats merge with placed junk
				board.ResolvePatterns(queuedClick, ref board.state, false);
			}

			if (result != "Empty") { // No time progressed for hand-rats, specials, or storage; even if they trigger a pattern (Only possible by putting a hole in a pattern where "Empty" is needed)
				board.MoveRats();
			}
			Game.Mouseover();
			string test = board.GetBoardAsCSV();
			PlayerPrefs.SetString("board",test);
			PlayerPrefs.SetString("hand", hand);
			PlayerPrefs.Save();
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

	public static void PlaySound(string name) {
		if (settings.sound && !string.IsNullOrWhiteSpace(name) && Data.audiofiles.ContainsKey(name)) {
			board.GetComponent<AudioSource>().PlayOneShot(Data.audiofiles[name]);
		}
	}

	public static void Idle() {
		idletime++;
		if (idletime > 255) { //TODO: Ping suggested move?
			idletime = 0;
		}
		//board.ShowText(idletime.ToString());
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
