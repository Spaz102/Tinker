using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Board : MonoBehaviour {
	public int boardWidth = 7;
	public int boardHeight = 7;
	public int animationLength;

	public GameObject parentboard;
	public Text textbox;
	public AudioSource audiosrc;

	public Tile[,] tile; // Implicitly a ref
	public string[,] state;
	public List<Storage> storagelist;
	public List<Dependency> dependencies;

	public void Start() {
		Game.Awake();
		tile = new Tile[boardWidth, boardHeight];
		state = new string[boardWidth, boardHeight];
		dependencies = new List<Dependency>();
		CreateBoard();
		animationLength = 20;
	}

	public void Update() {
		Game.Update();
	}

	public void CreateBoard() { // Warning, only run once! (Or fix to clear old tiles)
		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				GameObject newtile = UnityEngine.Object.Instantiate(Data.tiletemplate, new Vector3(), new Quaternion()) as GameObject;
				newtile.GetComponent<Tile>().transform.SetParent(parentboard.transform);
				newtile.GetComponent<RectTransform>().sizeDelta = new Vector2((this.GetComponentInParent<RectTransform>().rect.width / boardWidth) + newtile.GetComponent<RectTransform>().sizeDelta.x, (this.GetComponentInParent<RectTransform>().rect.height / boardHeight) + newtile.GetComponent<RectTransform>().sizeDelta.y);
				newtile.GetComponent<RectTransform>().transform.localPosition = new Vector3(0, 0, 0);
				newtile.GetComponent<RectTransform>().anchoredPosition = new Vector3((this.GetComponentInParent<RectTransform>().rect.width / boardWidth) * (x + .5f) - (this.GetComponentInParent<RectTransform>().rect.width * .5f), (this.GetComponentInParent<RectTransform>().rect.height / boardWidth) * (y + .5f) - (this.GetComponentInParent<RectTransform>().rect.height * .5f));
				newtile.GetComponent<RectTransform>().localScale = Vector3.one;
				newtile.GetComponent<BoxCollider2D>().size = new Vector2((this.GetComponentInParent<RectTransform>().rect.width / boardWidth), (this.GetComponentInParent<RectTransform>().rect.width / boardWidth));
				newtile.GetComponent<Interactive>().index = new Coord(x, y);
				newtile.GetComponent<Interactive>().type = "Tile";
				tile[x, y] = newtile.GetComponent<Tile>();

				GameObject newunderlay = UnityEngine.Object.Instantiate(Data.ghosttemplate, new Vector3(), new Quaternion()) as GameObject;
				newunderlay.GetComponent<Tile>().transform.SetParent(newtile.transform);
				newunderlay.GetComponent<RectTransform>().sizeDelta = new Vector2((this.GetComponentInParent<RectTransform>().rect.width / boardWidth) + newunderlay.GetComponent<RectTransform>().sizeDelta.x, (this.GetComponentInParent<RectTransform>().rect.height / boardHeight) + newunderlay.GetComponent<RectTransform>().sizeDelta.y);
				newunderlay.GetComponent<RectTransform>().transform.localPosition = new Vector3(0, 0, 0);
				newunderlay.GetComponent<RectTransform>().localScale = Vector3.one;
				newunderlay.GetComponent<Ghost>().lifespan = -1;
				tile[x,y].underlay = newunderlay.GetComponent<Tile>();

				newunderlay.GetComponent<Tile>().ShowSprite("Empty");
				Set(new Coord (x,y), "Empty");
			}
		}
		Clutter();
	}


	public void Clutter() {
		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				int remaining = Game.rng.Next(Game.startingPoolSize); // Pulls a random tile, with respect to relative chances
				foreach (string key in Data.tiledefs.Keys) {
					remaining -= Data.tiledefs[key].startingChance;
					if (remaining < 0) {
						Set(new Coord(x, y), key);
						break;
					}
				}
			}
		}
	}

	public void Set(Coord target, string setto) {
		if (state[target.x,target.y] == setto) { // Not strictly necessary to throw away redundant calls, but saves on redrawing
			return;
		} else {
			this.state[target.x,target.y] = setto;
			this.tile[target.x,target.y].ShowSprite(setto);
		}
		if (setto == "Rat" || setto == "NewRat") {
			tile[target.x,target.y].breathing = true;
			tile[target.x,target.y].scale = Random.Range(0, 0.15f);
		} else {
			tile[target.x,target.y].breathing = false;
		}

		foreach (Dependency checkme in dependencies) {
			if (target.x == checkme.dependson.x && target.y == checkme.dependson.y) {
				GameObject.Destroy(checkme.target); //TODO: Spawn and animate ghost to show destruction
			}
		}
		dependencies.RemoveAll(dep => dep.target == null);

		if (!Data.playerseen[setto]) {
			Data.playerseen[setto] = true;
			Game.blueprints.Recalc();
		}
	}

	public void SetBoard(string[,] setto) {
		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				Set(new Coord(x,y), setto[x,y]);
			}
		}
	}

	public void SetBoard(string setto) {
		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				Set(new Coord(x,y), setto);
			}
		}
	}

	public bool CheckGameOver() { //TODO: Change to void; interpret from here instead of main loop
		if (Game.hand == "Special") {
			for (int y = 0; y < boardHeight; y++) {
				for (int x = 0; x < boardWidth; x++) {
					if (state[x, y] != "Empty") { //TODO: Also check if part of a storage bin
						return false;
					}
				}
			}
			foreach (Storage stored in storagelist) {
				if (stored.stored != "Special") {
					return false;
				}
			}
			// Else all spaces empty or storage; all storage filled with tools. The true game over
		} else if (Game.hand == "Rat") {
			//TODO: Check if the whole screen is protected storage filled with rats??
			return false; // Can always either eat or be placed
		} else {
			for (int y = 0; y < boardHeight; y++) {
				for (int x = 0; x < boardWidth; x++) {
					if (state[x, y] == "Empty") {
						return false;
					}
				}
			}
			foreach (Storage stored in storagelist) {
				if (stored.stored == "Empty" || stored.stored == "Rat" || stored.stored == "Special") {
					return false;
				}
			}
		}
		Debug.Log("You lose. Good day, sir!");
		return true;
	}

	public void CleanUp() { // TODO: Push storage to corner, de-weird method
		List<string> shuffleme = new List<string>();
		List<Coord> empties = new List<Coord>();

		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				switch (state[x,y]) {
				case "Empty":
					empties.Add(new Coord(x,y));
					break;
				case "Dirt":
				case "Seed":
				case "Junk1":
				case "Junk2":
				case "Junk3":
				case "Rat": // Remove
					Set(new Coord(x,y), "Empty");
					empties.Add(new Coord(x,y));
					break;
				case "Stick":
				case "Wood":
				case "Plank":
				case "Rock":
				case "Metal": // Maybe remove
					if (Random.Range(0,4) > 0) { // 25% chance to remove
						shuffleme.Add(state[x,y]);
					}
					Set(new Coord(x,y), "Empty");
					empties.Add(new Coord(x,y));
					break;
				case "Panel": // Keep if storage
					bool isStorage = false;
					foreach (Dependency dep in Game.board.dependencies) {
						if (dep.dependson.x == x && dep.dependson.y == y) {
							isStorage = true;
						}
					}
					if (!isStorage) {
						shuffleme.Add(state[x,y]);
						Set(new Coord(x,y), "Empty");
						empties.Add(new Coord(x,y));
					} // Else leave it where it is
					break;
				default: // Keep but shuffle
					shuffleme.Add(state[x,y]);
					Set(new Coord(x,y), "Empty");
					empties.Add(new Coord(x,y));
					break;
				}
			}
		}
			
		if (shuffleme.Count > empties.Count) {
			Debug.Log("How is this possible??");
		}
		foreach (string placeme in shuffleme) {
			int rand = Random.Range(0,empties.Count);
			Set(empties[rand],placeme);
			empties.RemoveAt(rand);
		}
	}

	public void ShowText(string stuff) {
		textbox.text = stuff;
	}

	public void ResolvePatterns(Coord target, ref string[,] oldboard, bool test) { // Resolves all patterns surrounding the target, animates and applies if actually happening
		string[,] newboard = new string[boardWidth,boardHeight]; // Found tiles set to "Empty" to prepare for results and avoid double-counting
		newboard = (string[,]) oldboard.Clone();
		List<Coord> tilestocheck = new List<Coord>();
		List<Coord> newtilestocheck = new List<Coord>(); // Cannot modify list while using it, so use secondary list
		List<Coord> newstorage = new List<Coord>();
		tilestocheck.Add(target);
		int resolved = 0;

		for (int n = 0; n < Data.patterns.Count; n++) {
			foreach (Coord currenttarget in tilestocheck) {
				switch (Data.patterns[n].type) {
				case "3Cont":
				case "4Cont":
					int counter = 0;
					string[,] newnewboard = CheckContiguous(Data.patterns[n].value[0,0], currenttarget, ref counter, newboard);
					if (counter >= int.Parse(""+Data.patterns[n].type[0])) { // string[0] gets the number as a char, ""+char makes it a string, int.Parse converts it to an int. There really should be a better way
						for (int y = 0; y < boardHeight; y++) {
							for (int x = 0; x < boardWidth; x++) {
								if ((newnewboard[x,y] != newboard[x,y]) && (x != currenttarget.x || y != currenttarget.y)) { // Don't animate or highlight the target for compounded patterns
									if (test) {
										Highlight(x, y, "Highlight");
									} else {
										Animate(new Coord(x,y), currenttarget, resolved, Data.patterns[n].value[0,0], "Slide");
									}
								}
							}
						}
						newboard = newnewboard;
						newboard[currenttarget.x,currenttarget.y] = Data.patterns[n].result;
						if (!test) {
							tile[currenttarget.x, currenttarget.y].Hide((resolved+1)*animationLength);
							// Play combination sound
						}
						resolved++;
					}
					break;
				case "Static":
					List<Coord> staticresults = CheckStatic(Data.patterns[n].value, currenttarget, newboard);
					if (staticresults.Count == 0) { // No patterns found
						break;
					} else {
						int random = 0;
						if (!test) {
							random = Game.rng.Next(staticresults.Count);
						} else {
							// Highlight only first found. What else would look right?
						}

						for (int y = 0; y < Data.patterns[n].value.GetLength(1); y++) {
							for (int x = 0; x < Data.patterns[n].value.GetLength(0); x++) {
								if (Data.patterns[n].value[x,y] != "") { //TODO: Add check for other patterns that don't need an animation?
									newboard[staticresults[random].x + x, staticresults[random].y + Data.patterns[n].value.GetLength(1) - 1 - y] = "Empty";
									if (test) {
										Highlight(staticresults[random].x + x, staticresults[random].y + Data.patterns[n].value.GetLength(1) - 1 - y, "Highlight");
									} else {
										Animate(new Coord(staticresults[random].x + x, staticresults[random].y + Data.patterns[n].value.GetLength(1) - 1 - y), new Coord(staticresults[random].x + 1, staticresults[random].y + 1), resolved, Data.patterns[n].value[x,y], "Slide");
									}
								}
							}
						}
						newboard[staticresults[random].x + 1, staticresults[random].y + 1] = Data.patterns[n].result;
						newtilestocheck.Add(new Coord(staticresults[random].x + 1, staticresults[random].y + 1));
						if (!test) {
							tile[staticresults[random].x + 1, staticresults[random].y + 1].Hide((resolved+1)*animationLength);
							// Play combination sound
						}
					}
					resolved++;
					break;
				case "Storage":
					List<Coord> specialresults = CheckStatic(Data.patterns[n].value, currenttarget, newboard);
					if (specialresults.Count == 0) { // No patterns found
						break;
					}
					foreach (Coord found in specialresults) {
						if (test) {
							Highlight(found.x, found.y, "Highlight");
							Highlight(found.x + 1, found.y, "Highlight");
							Highlight(found.x, found.y + 1, "Highlight");
							Highlight(found.x + 1, found.y + 1, "Highlight");

						} else {
							Animate(new Coord(found.x, found.y), new Coord(found.x, found.y), resolved, "Storage", "Slide");
							Animate(new Coord(found.x + 1, found.y), new Coord(found.x + 1, found.y), resolved, "Storage", "Slide");
							Animate(new Coord(found.x, found.y + 1), new Coord(found.x, found.y + 1), resolved, "Storage", "Slide");
							Animate(new Coord(found.x + 1, found.y + 1), new Coord(found.x + 1, found.y + 1), resolved, "Storage", "Slide");

							newstorage.Add(found);
							// Play combination sound
						}
					}
					break;
				case "Set":
					List<string> values = new List<string>();
					for (int x = 0; x < Data.patterns[n].value.GetLength(0); x++) { // Is there a cleaner way?
						values.Add(Data.patterns[n].value[x,0]);
					}
					List<List<Coord>> results = CheckSet(values, currenttarget, ref newboard);
					if (results.Count == 0) {
						break;
					} else if (test) {
						if (newboard[target.x, target.y] != "NewRat") { // Otherwise the special recipes to double-merge junk (set and 3cont) cause handrats to show as ingredients
							foreach (List<Coord> result in results) {
								foreach (Coord spot in result) {
									Highlight(spot.x, spot.y, "Highlight");
									newboard[spot.x, spot.y] = "Empty";
								}
							}
						}
					} else {
						int random = Game.rng.Next(results.Count);
						foreach (Coord spot in results[random]) {
							Animate(spot, currenttarget, resolved, newboard[spot.x, spot.y], "Slide"); //TODO: Different animation type(s)?
							newboard[spot.x, spot.y] = "Empty";
						}
						tile[currenttarget.x, currenttarget.y].Hide((resolved+1)*animationLength);
						// Play combination sound
					}
					resolved++;
					newboard[currenttarget.x,currenttarget.y] = Data.patterns[n].result;
					break;
				default:
					Debug.Log("Unrecognized pattern type");
					break;
				}
			}

			foreach (Coord newtarget in newtilestocheck) {
				tilestocheck.Add(newtarget);
			}
			newtilestocheck.Clear();
		}

		if (!test) {
			SetBoard(newboard);
		}

		foreach (Coord newstore in newstorage) {
			CreateStorage(newstore);
		}
	}

	public string[,] CheckContiguous(string findme, Coord target, ref int counter, string[,] oldboard) { // Do NOT look for contiguous "Empty"!
		string[,] newboard; // Found tiles set to "Empty" to prepare for results and avoid double-counting
		newboard = (string[,]) oldboard.Clone(); // Will have all matching tiles changed to empty
		counter = 0;

		CheckNeighbors(findme, target, ref newboard, ref counter); // Warning: recursive

		return newboard;
	}
	private void CheckNeighbors(string findme, Coord target, ref string[,] newboard, ref int counter) { // Warning: recursive - clears found tiles and spreads checks to adjacent tiles
		if (target.InBounds() && newboard[target.x,target.y] == findme) { // Newly found tile
			newboard[target.x,target.y] = "Empty";
			counter++;
			CheckNeighbors(findme, target.Left(), ref newboard, ref counter);
			CheckNeighbors(findme, target.Right(), ref newboard, ref counter);
			CheckNeighbors(findme, target.Up(), ref newboard, ref counter);
			CheckNeighbors(findme, target.Down(), ref newboard, ref counter);
		}
	}

	public List<Coord> CheckStatic(string[,] findme, Coord target, string[,] oldboard) { // Checks for a static pattern involving the origin
		List<Coord> found = new List<Coord>(); // Coordinates of bottomleft corner, relative to oldboard

		for (int y = target.y - findme.GetLength(1) + 1; y <= target.y; y++) { // Out-of-bounds (illegal coordinates) checking deferred to SmartCheck()
			for (int x = target.x - findme.GetLength(0) + 1; x <= target.x; x++) {
				if (CheckStaticAt(ref findme, new Coord(x, y), ref oldboard)) {
					found.Add(new Coord(x, y));
				}
			}
		}
		return found;
	}
	public bool CheckStaticAt(ref string[,] findme, Coord bottomleft, ref string[,] oldboard) { // Checks for a pattern in one specific location, even if partially off the board
		for (int y = 0; y < findme.GetLength(1); y++) {
			for (int x = 0; x < findme.GetLength(0); x++) {
				if (!SmartCheck(oldboard, new Coord(bottomleft.x + x, bottomleft.y + findme.GetLength(1) - 1 - y), findme[x,y])) {
					return false;
				}
			}
		}
		return true;
	}

	public bool SmartCheck(string[,] findon, Coord location, string checkfor) { // Deferred bounds checking, logical order is critical //TODO implement in static pattern checking
		return ((checkfor == "") || (location.InBounds() && (checkfor == findon[location.x,location.y])));
	}

	public List<List<Coord>> CheckSet(List<string> findme, Coord target, ref string[,] oldboard) { // Returns a list of all complete sets
		List<List<Coord>> results = new List<List<Coord>>();
		FindSets(findme, new List<Coord>(), target, ref oldboard, ref results);
		return results;
	}
	public void FindSets(List<string> findme, List<Coord> found, Coord target, ref string[,] oldboard, ref List<List<Coord>> results) { // True iff complete pattern found, false iff dead end or only leads to dead ends //TODO determine if probabilities are skewed for certain shapes
		if (findme.Remove(oldboard[target.x, target.y])) { // Tile not double checked, and contains an entry - else this is a dead end and immediately ends the thread
			found.Add(target);
			if (findme.Count == 0) { // Complete set found
				results.Add(found);
			} else { // Incomplete set, branch out search to all adjacent tiles
				List<Coord> searchme = new List<Coord>();
				foreach (Coord live in found) { // For each found tile...
					List<Coord> temp = live.Neighbors(); // ...Get all of its (not out of bounds) neighbors...
					foreach(Coord potential in temp) {
						if (!found.Contains(potential) && !searchme.Contains(potential)) { // ...That aren't duplicates...
							searchme.Add(potential); // ...And add them to the list of tiles yet to be searched...
						}
					}
				}
				for (int n = 0; n < searchme.Count; n++) {
					List<string> newfindme = new List<string>(findme);
					List<Coord> newfound = new List<Coord>(found);
					FindSets(newfindme, newfound, searchme[n], ref oldboard, ref results);
				}
			}
		}
	}

	public void MoveRats() {
		List<Coord> rats = new List<Coord>(); // Avoids double-moving rats when moving into an unchecked space
		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				if (this.state[x,y] == "NewRat") { // Fresh rats don't move
					this.state[x,y] = "Rat";
				} else if (this.state[x,y] == "Rat") {
					rats.Add(new Coord(x,y));
				}
			}
		}
		while (rats.Count > 0) {
			int pick = Random.Range(0, rats.Count); // Rats move in random order
			MoveRat(rats[pick]);
			rats.RemoveAt(pick);
		}
	}

	private void MoveRat(Coord rat) { //TODO: Upgrade ai as needed for difficulty (Period of time before they start eating, aim towards high value)
		Coord target = rat.Neighbors()[Game.rng.Next(rat.Neighbors().Count)]; // Guaranteed to always have at least two options (No board walls), so no worries there

		if (this.state[target.x,target.y] == "Empty") {
			Set(target, "Rat");
			Set(rat, "Empty");
			Animate(rat, target, 0, "Rat", "Slide");
			tile[target.x,target.y].Hide(animationLength);
		} else {
			string poop = Data.tiledefs[this.state[target.x,target.y]].edible;
			if (poop != "") { // The target is edible
				Set(rat, "Empty"); // To keep the rat's old position clear, set this after the spread
				Set(target, "Empty");
				Game.PlaySound("Rat");
				AnimatePoof(target, 1);
				foreach (Coord spot in target.Neighbors()) {
					if (state[spot.x,spot.y] == "Empty") {
						Set(spot, poop);
						Animate(target, spot, 0, poop, "Slide");
						tile[spot.x,spot.y].Hide(10);
					}
				}
			} else {
				Animate(rat, target, 0, "Rat", "Bump");
				tile[rat.x, rat.y].Hide(animationLength);
			}
		}
	}

	public void Animate(Coord start, Coord finish, int delay, string sprite, string type) {
		if (sprite == "Empty") {
			return;
		}
		
		GameObject newghost = Instantiate(Data.ghosttemplate, tile[start.x, start.y].gameObject.transform.position, tile[start.x, start.y].gameObject.transform.rotation) as GameObject;

		newghost.GetComponent<Tile>().transform.SetParent(parentboard.transform);
		newghost.GetComponent<RectTransform>().sizeDelta = new Vector2((this.GetComponentInParent<RectTransform>().rect.width / boardWidth) + newghost.GetComponent<RectTransform>().sizeDelta.x, (this.GetComponentInParent<RectTransform>().rect.height / boardHeight) + newghost.GetComponent<RectTransform>().sizeDelta.y);
		newghost.GetComponent<RectTransform>().localScale = Vector3.one;

		newghost.GetComponent<UnityEngine.UI.Image>().sprite = Data.tiledefs[sprite].sprite;
		newghost.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, Data.tiledefs[sprite].opacity);

		newghost.GetComponent<Ghost>().Spawn(type, tile[finish.x, finish.y].gameObject.transform.position, delay * animationLength, true);
		newghost.GetComponent<Tile>().Hide(tile[start.x, start.y].hidden); // Delay this animation until after it unhides
	}



	public void AnimatePoof(Coord target, float size) {
		if (!Game.settings.dust) {
			return;
		}
		GameObject newghost = UnityEngine.Object.Instantiate(Data.ghosttemplate, tile[target.x,target.y].gameObject.transform.position, tile[target.x,target.y].gameObject.transform.rotation) as GameObject;

		newghost.GetComponent<Tile>().transform.SetParent(parentboard.transform);
		newghost.GetComponent<RectTransform>().sizeDelta = new Vector2((this.GetComponentInParent<RectTransform>().rect.width / boardWidth) + newghost.GetComponent<RectTransform>().sizeDelta.x, (this.GetComponentInParent<RectTransform>().rect.height / boardHeight) + newghost.GetComponent<RectTransform>().sizeDelta.y);
		newghost.GetComponent<RectTransform>().localScale = Vector3.one;

		newghost.GetComponent<UnityEngine.UI.Image>().sprite = Data.dustsprites[Random.Range(4,5)];
		newghost.transform.rotation = Quaternion.Euler(0,0,Random.Range(0,360));

		newghost.GetComponent<UnityEngine.UI.Image>().color = new Color(1,1,1,1);

		newghost.GetComponent<Ghost>().Spawn("Poof", tile[target.x,target.y].gameObject.transform.position, 0, false);
	}

	public void ClearHighlights() {
		for (int x = 0; x < tile.GetLength(0); x++) {
			for (int y = 0; y < tile.GetLength(0); y++) {
				tile[x,y].underlay.ShowSprite("Empty");
			}
		}
	}
	public void Highlight(int x, int y, string show) {
		if (x >= 0 && y >= 0 && x < tile.GetLength(0) && y < tile.GetLength(1)) {
			if (Game.mouseover== null || x != Game.mouseover.x || y != Game.mouseover.y) {
				tile[x,y].underlay.ShowSprite(show);
			}
		}
	}

	public void CreateStorage(Coord bottomleft) { // Assumes 2x2 pattern for dependencies //TODO: Modify panel appearance when used in storage
		GameObject newstorage = UnityEngine.Object.Instantiate(Data.storagetemplate, new Vector3(), new Quaternion()) as GameObject;
		
		newstorage.GetComponent<Tile>().transform.SetParent(parentboard.transform);
		newstorage.GetComponent<RectTransform>().anchoredPosition = new Vector2(-10 + 4*bottomleft.x,-10 + 4*bottomleft.y);
		newstorage.GetComponent<RectTransform>().sizeDelta = new Vector2(-24,-24);
		newstorage.GetComponent<Transform>().position = new Vector3(newstorage.GetComponent<Transform>().position.x, newstorage.GetComponent<Transform>().position.y, -0.01f);

		storagelist.Add(newstorage.GetComponent<Storage>());

		Game.board.dependencies.Add(new Dependency(newstorage, new Coord(bottomleft.x, bottomleft.y)));
		Game.board.dependencies.Add(new Dependency(newstorage, new Coord(bottomleft.x+1, bottomleft.y)));
		Game.board.dependencies.Add(new Dependency(newstorage, new Coord(bottomleft.x, bottomleft.y+1)));
		Game.board.dependencies.Add(new Dependency(newstorage, new Coord(bottomleft.x+1, bottomleft.y+1)));
	}

	public void AddDependency(GameObject target, Coord dependson) {
		Game.board.dependencies.Add(new Dependency(target, dependson));
	}
}

public class Dependency {
	public GameObject target;
	public Coord dependson;

	public Dependency(GameObject target, Coord dependson) {
		this.target = target;
		this.dependson = dependson;
	}
}