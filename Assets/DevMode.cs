using UnityEngine;
using System;

/// <summary>
/// Handler for Dev menu/features
/// </summary>
// Enables some dev only features
// clicking on items in the codex puts them in your hand
public sealed class DevMode : MonoBehaviour {
    #region Singleton Block
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
			FirstStart();
		}
	}
	#endregion
	
	public static bool devmode = true;

	/// <summary>
	/// Acts as a constructor in a seperate function to keep the singleton code in a block
	/// </summary>
	private void FirstStart()
	{
		if (!devmode)
		{
			GameObject.Find("Dev Buttons").SetActive(false);

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
		Core.codex.Recalc();
	}

	/// <summary>
	/// Sets all items to unseen
	/// </summary>
	public void UnseeItems() {
		if (!devmode) { return; }
		foreach (string defkey in Data.tiledefs.Keys)
		{
			Data.playerseen[defkey] = false;
			Data.playerread[defkey] = false;
		}
		Core.codex.Recalc();
	}
	
}