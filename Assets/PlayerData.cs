using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Remember that dictionaries must be initialized manually

/// <summary>
/// Handles player/saved data. Gamestate, playerstate, settings
/// </summary>
public static class PlayerData {
	public static void Awake() {}

	public static Dictionary<string, bool> playerseen;
	public static Dictionary<string, bool> playerread;

	static PlayerData() {
		FillLists();
	}

	private static void FillLists() {
        playerseen = new Dictionary<string, bool>();
		playerread = new Dictionary<string, bool>();
		foreach (string defkey in Data.tiledefs.Keys) {
			playerseen.Add(defkey, false);
			playerread.Add(defkey, false);
		}
	}
}