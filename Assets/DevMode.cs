using UnityEngine;
using System;

/// <summary>
/// Handler for Dev menu/features
/// </summary>
// Enables some dev only features
// clicking on items in the codex puts them in your hand
public sealed class DevMode : MonoBehaviour
{
	public static bool devmode = true;
	
	private static DevMode _instance;
	public static DevMode Instance { get { return _instance; } }
	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			_instance = this;
			if (!devmode)
			{
				GameObject.Find("Dev Buttons").SetActive(false);
				
			}
		}
	}

	/// <summary>
	/// New Board
	/// </summary>
	public void ClearBoard() {
		if (!devmode) { return; }
		Core.board.EmptyBoard();
		UnseeItems();
	}

	/// <summary>
	/// Sets all items to 'seen'
	/// </summary>
	public void SeeItems() {
		if (!devmode) { return; }
		foreach (string defkey in Data.tiledefs.Keys) {
			Data.playerseen[defkey] = true;
		}
		Core.blueprints.Recalc();
	}

	/// <summary>
	/// Sets all items to unseen
	/// </summary>
	public void UnseeItems() {
		if (!devmode) { return; }
		foreach (string defkey in Data.tiledefs.Keys)
		{
			Data.playerseen[defkey] = false;
		}
		Core.blueprints.Recalc();
	}
	
}