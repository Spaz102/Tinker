using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Core functionality (controls, clicking, startup) and holds core definitions
/// </summary>
public static class Core {
	public static void Awake() {}
	public static System.Random rng;

	public static Canvas mainCanvas;
	public static Tile cursor;
	public static Board board;
	public static Codex codex;
	public static Popup popup;
	public static GameObject mainMenu;

	// Used in random drawing to hand
	public static int handPoolSize;
	public static int startingPoolSize;
	public static string hand;

	// Animations
	public static Interactive mouseover;
	public static int animationEnd;
	public static Coord queuedClick;
	public static int idletime;
	
	public static Settings settings;

	/// <summary>
	/// Core Constructor - attaches cs logic to unity objects, starts rand seed, plays startup sound
	/// </summary>
	static Core () {
		rng = new System.Random();
		Data.Awake();
		PlayerData.Awake();
		CalcHandPoolSize();
		
		UnityEngine.Object.Destroy(GameObject.Find("Tile"));
		UnityEngine.Object.Destroy(GameObject.Find("Storage"));
		board = GameObject.Find("PlayArea").GetComponent<Board>();
		mainCanvas = GameObject.Find("Main Canvas").GetComponent<Canvas>();
		cursor = GameObject.Find("Hand").GetComponent<Tile>();
		codex = UI.Codex.GetComponent<Codex>();
		popup = UI.Popup.GetComponent<Popup>();
		mainMenu = UI.MainMenu;

		mouseover = null;
		cursor.transform.localScale = new Vector3(0.75f, 0.75f, 1);
		settings = new Settings();
		Audio.PlaySound("Startup");
	}

	/// <summary>
	/// Game tick
	/// </summary>
	public static void Update () {
		Idle();
		MoveHand();
		if (board.CheckGameOver()) {
			Debug.Log("You lose. Good day, sir!");
			popup.Open(Popup.PopupTypes.lose);
			board.CleanUp();
			SetHand("Random");
			board.SaveGame();
		}
		if (animationEnd > 0) {
			animationEnd--;
		} else {
			Core.Click();
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
				board.tile[mouseover.index.x, mouseover.index.y].underlay.SetTile("Mouseover"); // TODO: Something fancier?
			} else if (CanClick(board.state[mouseover.index.x, mouseover.index.y]) != board.state[mouseover.index.x, mouseover.index.y]) { // Clicking will cause changes on the board
				string[,] boardcopy = (string[,])board.state.Clone(); // Copy the board
				boardcopy[mouseover.index.x, mouseover.index.y] = CanClick(board.state[mouseover.index.x, mouseover.index.y]); // Apply the click to the clone
				board.ResolvePatterns(mouseover.index, ref boardcopy, true); // test = true will show valid patterns on the hypothetical board
				board.tile[mouseover.index.x, mouseover.index.y].underlay.SetTile("Mouseover"); // Override any pattern highlighting with mouseover indicator
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
			Debug.Log("You win");
			popup.Open(Popup.PopupTypes.win);
			SetHand("Random");
			board.Set(queuedClick, "Empty");
		} else if (result != board.state[queuedClick.x,queuedClick.y]) { // A click is possible
			Audio.PlaySound(hand);
			if (hand == "Special") {
				PlayerData.playerseen["Special"] = true;
				SetHand(board.state[queuedClick.x,queuedClick.y]);
				board.Set(queuedClick, "Empty");
			} else if (hand == "Rat") {
				PlayerData.playerseen["NewRat"] = true;
				PlayerData.playerseen["Rat"] = true;
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
			Core.Mouseover();
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
		if (!PlayerData.playerseen[hand]) {
			PlayerData.playerseen[hand] = true;
			codex.Recalc();
		}
		if (hand == "NewRat" && !PlayerData.playerseen["Rat"]) {
			PlayerData.playerseen["Rat"] = true;
			codex.Recalc();
		}
		cursor.SetTile(hand);
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
		int remaining = Core.rng.Next(handPoolSize); // Pulls a random tile, with respect to relative chances
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

/// <summary>
/// Board coordinate system [0,0] is bottom left corner 
/// </summary>
public class Coord { 
	public int x; // Left to right
	public int y; // Bottom to top

	/// <summary>
	/// [0,0] is bottom left corner 
	/// </summary>
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
		if (this.x + 1 < Core.board.boardWidth) {results.Add(this.Right());}
		if (this.y - 1 >= 0) {results.Add(this.Down());}
		if (this.y + 1 < Core.board.boardHeight) {results.Add(this.Up());}
		return results;
	}

	public bool InBounds() {
		return (this.x >=0 && this.y >= 0 && this.x < Core.board.boardWidth && this.y < Core.board.boardHeight);
	}

	public bool Equals(Coord other) {
		return (this.x == other.x && this.y == other.y);
	}
}
