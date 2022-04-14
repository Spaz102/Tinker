using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Game logic. Control tiles/rats on the board, save/load/new game, game state, pattern matches, animations
/// </summary>
public class Board : MonoBehaviour {
	public int boardWidth = 7;
	public int boardHeight = 7;
	public int animationLength;

	public GameObject parentboard;
	public Text textbox;

	public Tile[,] tile; // Implicitly a ref
	public string[,] state;
	public List<Storage> storagelist;
	public List<Dependency> dependencies;

	public void Start() {
		Core.Awake(); // Make sure it is instantiated before moving on
		tile = new Tile[boardWidth, boardHeight];
		state = new string[boardWidth, boardHeight];
		dependencies = new List<Dependency>();
		//PlayerPrefs.DeleteAll();
		CreateBoard();
		if (!TryLoadGame()) {
			ResetBoard();
			//SetBoard("Empty");
		}
		animationLength = 20;
	}

	public void Update() {
		Core.Update();
	}

	public void CreateBoard() { // Warning, only run once! (Or fix to clear old tiles)
		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				GameObject newtile = UnityEngine.Object.Instantiate(Data.tiletemplate, new Vector3(), new Quaternion(), parentboard.transform) as GameObject;
				newtile.transform.localScale = Vector3.one;
				newtile.GetComponent<RectTransform>().transform.localPosition = new Vector3(0, 0, 0);
				newtile.GetComponent<RectTransform>().anchoredPosition = new Vector3((this.GetComponentInParent<RectTransform>().rect.width / boardWidth) * (x + .5f) - (this.GetComponentInParent<RectTransform>().rect.width * .5f), (this.GetComponentInParent<RectTransform>().rect.height / boardWidth) * (y + .5f) - (this.GetComponentInParent<RectTransform>().rect.height * .5f), 0f);
				
				newtile.GetComponent<Interactive>().index = new Coord(x, y);
				newtile.GetComponent<Interactive>().type = "Tile";
				tile[x, y] = newtile.GetComponent<Tile>();

				GameObject newunderlay = UnityEngine.Object.Instantiate(Data.ghosttemplate, new Vector3(), new Quaternion(), newtile.transform) as GameObject;
				newunderlay.GetComponent<RectTransform>().transform.localPosition = new Vector3(0, 0, 0);
				newunderlay.GetComponent<RectTransform>().localScale = Vector3.one;
				newunderlay.GetComponent<Ghost>().lifespan = -1;
				tile[x,y].underlay = newunderlay.GetComponent<Tile>();

				newunderlay.GetComponent<Tile>().SetTile("Empty");
			}
		}
	}

	public void ResetBoard() {
		SetBoard("Clutter");
		Core.SetHand("Random");
		Audio.PlaySound("Crunch");
		SaveGame();
	}

	public void EmptyBoard() {
		SetBoard("Empty");
		Audio.PlaySound("Byebye");
		SaveGame();
	}

	public bool TryLoadGame() {
		if (!PlayerPrefs.HasKey("board")) {
			return false;
		}
		string seenRaw = PlayerPrefs.GetString("seen");
		string readRaw = PlayerPrefs.GetString("read");
		int index = 0; // Let's just hope tiledata is consistently ordered (It can't be iterated over numerically)
		foreach (string key in Data.tiledefs.Keys) {
			Data.playerseen[key] = seenRaw.ToCharArray()[index] == '1';
			Data.playerread[key] = readRaw.ToCharArray()[index] == '1';
			//Data.playerread[key] = Data.playerread[key] || readRaw.ToCharArray()[index] == '1';
			index++;
		}
		Core.SetHand(PlayerPrefs.GetString("hand"));
		SetBoardFromCSV(PlayerPrefs.GetString("board"));
		string[] stored = PlayerPrefs.GetString("storage").Split(',');
		for (int n = 0; n < stored.Length; n++) {
			if (!string.IsNullOrEmpty(stored[n])) {
				storagelist[n].Set(stored[n]);
			}
		}
		return true;
	}

	public void SaveGame() {
		PlayerPrefs.SetString("board", GetBoardAsCSV());
		PlayerPrefs.SetString("hand", Core.hand);
		string storageString = "";
		foreach (Storage stored in storagelist) {
			storageString += stored.stored + ",";
		}
		storageString = storageString.Trim(new char[] {','});
		PlayerPrefs.SetString("storage", storageString);

		string seenRaw = "";
		string readRaw = "";
		foreach (string entry in Data.tiledefs.Keys) {
			seenRaw += Data.playerseen[entry] ? '1' : '0';
			readRaw += Data.playerread[entry] ? '1' : '0';
		}
		PlayerPrefs.SetString("seen", seenRaw);
		PlayerPrefs.SetString("read", readRaw);

		PlayerPrefs.Save();
	}

	public void DeleteSaveData() {
		PlayerPrefs.DeleteAll(); // Will need to exit the game directly after running this; before triggering a new SaveGame call
	}

	public void Set(Coord target, string setto) { // Does not check for new patterns formed
		if (state[target.x,target.y] == setto) { // Not strictly necessary to throw away redundant calls, but saves on redrawing
			return;
		}
		if (!Data.playerseen[setto]) {
			Data.playerseen[setto] = true;
			Core.codex.Recalc();
		}
		
		this.state[target.x,target.y] = setto;
		this.tile[target.x,target.y].SetTile(setto);
		
		if (setto == "Rat" || setto == "NewRat") {
			tile[target.x, target.y].newState = Tile.States.breathing;
		} else {
			tile[target.x, target.y].newState = Tile.States.normal;
		}

		foreach (Dependency checkMe in dependencies) { // Check if any storage was using this (panel) tile
			if (target.x == checkMe.dependsOn.x && target.y == checkMe.dependsOn.y && checkMe.dependent != null) { // If this tile was being depended on, its dependents die
				if (storagelist.Remove(checkMe.dependent.GetComponent<Storage>())) { // No longer a valid storage area; returns true if it was considered one
					//RedrawAllStorage();
					//TODO: Unique storage-destruction animation
				}
				GameObject.Destroy(checkMe.dependent); // Warning: The object must purge itself from the dependencies list during OnDestroy() (Can't be done here, as the object is not yet null, and there is no exposed list of objects soon to be destroyed)
			}
		}
		//TODO: Somehow force redraw storage shapes here, instead of on storage object deletion (Which is when the board knows which dependencies are still active)
	}

	public void SetBoard(string[,] setTo) {
		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				Set(new Coord(x,y), setTo[x,y]);
			}
		}
	}

	public void SetBoard(string setTo) {
		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				if (setTo == "Clutter") {
					int remaining = Core.rng.Next(Core.startingPoolSize); // Pulls a random tile, with respect to relative chances
					foreach (string key in Data.tiledefs.Keys) {
						remaining -= Data.tiledefs[key].startingChance;
						if (remaining < 0) {
							Set(new Coord(x, y), key);
							break;
						}
					}
				} else {
					Set(new Coord(x, y), setTo);
				}
			}
		}
	}

	public void SetBoardFromCSV(string setTo) {
		string[] split = setTo.Split(',');
		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				Set(new Coord(x, y), split[y * boardWidth + x]);
			}
		}
		RebuildStorage();

	}

	public string GetBoardAsCSV() {
		string output = "";
		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				output += state[x, y] + ",";
			}
		}
		return output;
	}

	public bool CheckGameOver() { //TODO: Change to void; interpret from here instead of main loop
		if (Core.hand == "Special") {
			for (int y = 0; y < boardHeight; y++) {
				for (int x = 0; x < boardWidth; x++) {
					if (state[x, y] != "Empty") { //TODO: Also check if part of a storage bin
						return false;
					}
				}
			} // Else all spaces empty The true game over
		} else if (Core.hand == "Rat") {
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
		return true;
	}

	public void RebuildStorage() {
		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				if (CheckStaticAt(ref Data.patternresults["CreateStorage"].value, new Coord(x, y), ref state)) {
					CreateStorage(new Coord(x, y));
				}
			}
		}
		RedrawAllStorage();
	}

	public void CleanUp() {
		List<string> shuffleMe = new List<string>();
		int storagePanels = 0;
		List<string> storedResources = new List<string>();
		foreach (Storage stored in storagelist) {
			storedResources.Add(stored.stored);
		}

		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				switch (state[x,y]) {
				case "Empty":
					break;
				case "Dirt":
				case "Rock":
				case "Seed":
				case "Stick":
				case "Junk1":
				case "Junk2":
				case "Junk3":
				case "Rat": // Remove
					break;
				case "Wood":
				case "Plank":
				case "Metal":
				case "Pin": // Maybe remove
					if (Random.Range(0, 2) > 0) { // 50% chance to remove
						shuffleMe.Add(state[x,y]);
					}
					break;
				case "Panel": // Sort to corner if storage; treat as semi-valuable resoure otherwise
					if (Random.Range(0, 9) >= storagePanels || Random.Range(0, 9) >= storagePanels) { // Exponential odds in favor of (re)building storage
						storagePanels++;
					} else {
						if (Random.Range(0, 4) > 0) { // 25% chance to remove
							shuffleMe.Add(state[x, y]);
						}
					}
					break;
				default: // Keep but shuffle
					shuffleMe.Add(state[x,y]);
					break;
				}
				Set(new Coord(x, y), "Empty");
			}
		}

		List<Coord> preferredPanelLocations = new List<Coord>();
		preferredPanelLocations.Add(new Coord(0, 0));
		preferredPanelLocations.Add(new Coord(0, 1));
		preferredPanelLocations.Add(new Coord(1, 0));
		preferredPanelLocations.Add(new Coord(1, 1));
		preferredPanelLocations.Add(new Coord(2, 0));
		preferredPanelLocations.Add(new Coord(2, 1));
		preferredPanelLocations.Add(new Coord(0, 2));
		preferredPanelLocations.Add(new Coord(1, 2));
		preferredPanelLocations.Add(new Coord(2, 2));
		for (int n = 0; n < storagePanels; n++) {
			Set(preferredPanelLocations[n], "Panel");
		}
		RebuildStorage(); // Warning: Randomly placed panels will not correctly complete storage patterns if they happen to land into them
		for (int n = 0; n < storedResources.Count; n++) {
			if (n < storagelist.Count) {
				storagelist[n].stored = storedResources[n];
			} else { // More things were stored than can now be contained
				shuffleMe.Add(storedResources[n]); // Dump them onto the ground
			}
		}

		while (shuffleMe.Count + storagePanels < boardHeight * boardWidth) { // Pad out all remaining space with empties
			shuffleMe.Add("Empty");
		}
		for (int n = 0; n < shuffleMe.Count; n++) { // Everybody do the Fischer-Yates~
			int swap = Random.Range(n, shuffleMe.Count);
			string temp = shuffleMe[swap];
			shuffleMe[swap] = shuffleMe[n];
			shuffleMe[n] = temp;
		} // Warning: If shuffleMe > 49 (technically possible with deferred storage and non-priority-placed panels), some resources won't get placed on the board
		for (int y = 0; y < boardHeight; y++) { // Populate the board with the survivors
			for (int x = 0; x < boardWidth; x++) {
				if (state[x,y] != "Panel") { // If it isn't a preferred-placed panel
					Set(new Coord(x, y), shuffleMe[0]);
					shuffleMe.RemoveAt(0);
				}
			}
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
			bool soundMade = false; // Sounds are attached to animations, but only one needs to be used instead of all ingredient->results animations
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
										string thisSound = (soundMade) ? null : Data.patterns[n].result;
										soundMade = true;
										Animate(new Coord(x,y), currenttarget, resolved, Data.patterns[n].value[0,0], "Slide", thisSound);
									}
								}
							}
						}
						newboard = newnewboard;
						newboard[currenttarget.x,currenttarget.y] = Data.patterns[n].result;
						if (!test) {
							tile[currenttarget.x, currenttarget.y].Hide((resolved+1)*animationLength);
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
							random = Core.rng.Next(staticresults.Count);
						} else {
							// Highlight only first found. What else would look right?
						}

						for (int y = 0; y < Data.patterns[n].value.GetLength(1); y++) {
							for (int x = 0; x < Data.patterns[n].value.GetLength(0); x++) {
								if (Data.patterns[n].value[x,y] != "") {
									newboard[staticresults[random].x + x, staticresults[random].y + Data.patterns[n].value.GetLength(1) - 1 - y] = "Empty";
									if (test) {
										Highlight(staticresults[random].x + x, staticresults[random].y + Data.patterns[n].value.GetLength(1) - 1 - y, "Highlight");
									} else {
										string thisSound = (soundMade) ? null : Data.patterns[n].result;
										soundMade = true;
										Animate(new Coord(staticresults[random].x + x, staticresults[random].y + Data.patterns[n].value.GetLength(1) - 1 - y), new Coord(staticresults[random].x + 1, staticresults[random].y + 1), resolved, Data.patterns[n].value[x,y], "Slide", thisSound);
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
							Animate(new Coord(found.x, found.y), new Coord(found.x + 1, found.y), resolved, "Panel", "Slide", "Storage"); //TODO: Audio immediately instead of after animation?
							Animate(new Coord(found.x + 1, found.y), new Coord(found.x + 1, found.y + 1), resolved, "Panel", "Slide");
							Animate(new Coord(found.x, found.y + 1), new Coord(found.x, found.y), resolved, "Panel", "Slide");
							Animate(new Coord(found.x + 1, found.y + 1), new Coord(found.x, found.y + 1), resolved, "Panel", "Slide");

							newstorage.Add(found);
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
						int random = Core.rng.Next(results.Count);
						foreach (Coord spot in results[random]) {
							string thisSound = (soundMade) ? null : Data.patterns[n].result;
							soundMade = true;
							Animate(spot, currenttarget, resolved, newboard[spot.x, spot.y], "Slide", thisSound);
							newboard[spot.x, spot.y] = "Empty";
						}
						tile[currenttarget.x, currenttarget.y].Hide((resolved+1)*animationLength);
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
		if (newstorage.Count > 0) {
			RedrawAllStorage();
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

	public bool SmartCheck(string[,] findon, Coord location, string checkfor) { // Deferred bounds checking, logical order is critical
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

	private void MoveRat(Coord rat) {
		Coord target = rat.Neighbors()[Core.rng.Next(rat.Neighbors().Count)]; // Guaranteed to always have at least two options (No board walls), so no worries there

		if (this.state[target.x,target.y] == "Empty") {
			Set(target, "Rat");
			Set(rat, "Empty");
			Animate(rat, target, 0, "Rat", "Slide");
			tile[target.x,target.y].Hide(animationLength);
		} else {
			string poop = Data.tiledefs[this.state[target.x,target.y]].edible;
			if (poop != "" && Random.Range(0, 3) > 0) { // The target is edible, and passes 2/3 chance
				Set(rat, "Empty"); // To keep the rat's old position clear, set this after the spread
				Set(target, "Empty");
				Audio.PlaySound("Crunch");
				AnimatePoof(target, 1.5f);
				foreach (Coord spot in target.Neighbors()) {
					if (state[spot.x,spot.y] == "Empty") {
						Set(spot, poop);
						Animate(target, spot, 0, poop, "Slide");
						tile[spot.x,spot.y].Hide(10);
					}
				}
			} else {
				Animate(rat, target, 0, "Rat", "Bump");
				//Animate(rat, target, 0, "Rat", "Bump", "Rat"); // Adorable, but too noisy
				tile[rat.x, rat.y].Hide(animationLength);
			}
		}
	}

	public void Animate(Coord start, Coord finish, int delay, string sprite, string type, string sfx_name = null) {
		if (sprite == "Empty") {
			return;
		}
		
		GameObject newghost = Instantiate(Data.ghosttemplate, tile[start.x, start.y].gameObject.transform.position, tile[start.x, start.y].gameObject.transform.rotation, parentboard.transform) as GameObject;

		newghost.GetComponent<UnityEngine.UI.Image>().sprite = Data.tiledefs[sprite].sprite;
		newghost.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, Data.tiledefs[sprite].opacity);

		newghost.GetComponent<Ghost>().Spawn(type, tile[finish.x, finish.y].gameObject.transform.position, delay * animationLength, true, sfx_name);
		newghost.GetComponent<Tile>().Hide(tile[start.x, start.y].hiddenTransitionTime); // Delay this animation until after it unhides
	}



	public void AnimatePoof(Coord target, float size) { // TODO: Longer duration poofs
		if (!Core.settings.dust) {
			return;
		}
		GameObject newghost = UnityEngine.Object.Instantiate(Data.ghosttemplate, tile[target.x,target.y].gameObject.transform.position, tile[target.x,target.y].gameObject.transform.rotation, parentboard.transform) as GameObject;
		
		newghost.GetComponent<UnityEngine.UI.Image>().sprite = Data.dustsprites[Random.Range(4,5)];
		newghost.transform.rotation = Quaternion.Euler(0,0,Random.Range(0,360));

		newghost.GetComponent<UnityEngine.UI.Image>().color = new Color(1,1,1,1);

		newghost.GetComponent<Ghost>().Spawn("Poof", tile[target.x,target.y].gameObject.transform.position, 0, false);
	}

	public void ClearHighlights() {
		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				tile[x,y].underlay.SetTile("Empty");
			}
		}
	}
	public void Highlight(int x, int y, string show) {
		if (x >= 0 && y >= 0 && x < boardWidth && y < boardHeight) {
			if (Core.mouseover == null || x != Core.mouseover.index.x || y != Core.mouseover.index.y) {
				tile[x,y].underlay.SetTile(show);
			}
		}
	}

	public void CreateStorage(Coord bottomleft) { // Assumes 2x2 pattern for dependencies //TODO: Modify panel appearance when used in storage?
		GameObject newstorage = UnityEngine.Object.Instantiate(Data.storagetemplate, new Vector3(), new Quaternion(), parentboard.transform) as GameObject;
		
		newstorage.GetComponent<RectTransform>().transform.localPosition = new Vector3(0, 0, -0.1f); 
		newstorage.GetComponent<RectTransform>().anchoredPosition = new Vector3((this.GetComponentInParent<RectTransform>().rect.width / boardWidth) * (bottomleft.x + 1f) - (this.GetComponentInParent<RectTransform>().rect.width * .5f), (this.GetComponentInParent<RectTransform>().rect.height / boardWidth) * (bottomleft.y + 1f) - (this.GetComponentInParent<RectTransform>().rect.height * .5f), -0.1f);

		storagelist.Add(newstorage.GetComponent<Storage>());

		Core.board.dependencies.Add(new Dependency(newstorage, new Coord(bottomleft.x, bottomleft.y)));
		Core.board.dependencies.Add(new Dependency(newstorage, new Coord(bottomleft.x+1, bottomleft.y)));
		Core.board.dependencies.Add(new Dependency(newstorage, new Coord(bottomleft.x, bottomleft.y+1)));
		Core.board.dependencies.Add(new Dependency(newstorage, new Coord(bottomleft.x+1, bottomleft.y+1)));
	}

	public void RedrawAround(Coord target) { // Redraws all panels surrounding a newly placed panel (which contributed to the creation of new storage)
		for (int y = target.y - 1; y <= target.y + 1; y++) {
			for (int x = target.x - 1; x <= target.x + 1; x++) {
				Coord thisCoord = new Coord(x, y);
				if (thisCoord.InBounds() && Core.board.state[thisCoord.x, thisCoord.y] == "Panel") {
					RedrawPanel(thisCoord);
				}
			}
		}
	}

	public void RedrawAllStorage() {
		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				if (Core.board.state[x, y] == "Panel") {
					RedrawPanel(new Coord(x, y));
				}
			}
		}
	}

	public void RedrawPanel(Coord target) {
		if (!HasDependents(target)) {
			Core.board.tile[target.x, target.y].SetTile("Panel");
			return;
		}
		int magicNumber = 0;
		int spriteIndex = 0;
		if (IsStoragePanel(target.Up())) {
			magicNumber += 1;
		}
		if (IsStoragePanel(target.Down())) {
			magicNumber += 2;
		}
		if (IsStoragePanel(target.Left())) {
			magicNumber += 4;
		}
		if (IsStoragePanel(target.Right())) {
			magicNumber += 8;
		}

		spriteIndex = new int[] {-1, -1, -1, -1, -1, 8, 2, 5, -1, 6, 0, 3, -1, 7, 1, 4}[magicNumber];
		if (spriteIndex < 0) { // This should be impossible in any board's final state, but is possible when destroying multiple storage areas in one click (The dependency list only updates when the objects are actually destroyed)
			Core.board.tile[target.x, target.y].SetTile("Panel");
			return;
			//Debug.Log("Something went horribly wrong in determining which panel sprite to show at " + target.x + ", " + target.y + ". Good luck?");
		}

		Core.board.tile[target.x, target.y].GetComponent<UnityEngine.UI.Image>().sprite = Data.storageSprites[spriteIndex];
		Core.board.tile[target.x, target.y].GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 1f);
	}

	public bool IsStoragePanel(Coord target) {
		return (target.InBounds()
			&& Core.board.state[target.x, target.y] == "Panel"
			&& HasDependents(target));
	}

	public bool HasDependents(Coord target) { //TODO: Check if dependent is a storage object
		foreach (Dependency dependency in Core.board.dependencies) {
			if (dependency.dependsOn.Equals(target)) {
				return true;
			}
		}
		return false;
	}
}

public class Dependency {
	public GameObject dependent; // For example, a storage tile
	public Coord dependsOn; // One of that storage tile's component panels

	public Dependency(GameObject dependent, Coord dependsOn) {
		this.dependent = dependent;
		this.dependsOn = dependsOn;
	}
}