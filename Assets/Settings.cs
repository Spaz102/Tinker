using UnityEngine;

/// <summary>
/// Loads/saves set/get settings
/// </summary>
public class Settings
{
	public bool dust;
	public bool muted;
	private GameObject[] dustList;

	public Settings()
	{
		dustList = GameObject.FindGameObjectsWithTag("Dust");

		// Load prefs or else, use defaults
		if (PlayerPrefs.HasKey("dust"))
		{
			dust = (PlayerPrefs.GetInt("dust") == 1);
		}
		else
		{
			// defaults 
			dust = true;
			PlayerPrefs.SetInt("dust", 1);
			PlayerPrefs.Save();
		}
		foreach (GameObject found in dustList)
		{
			found.SetActive(dust);
		}

		if (PlayerPrefs.HasKey("muted"))
		{
			muted = (PlayerPrefs.GetInt("muted") == 1);
		}
		else
		{
			// defaults 
			muted = false;
			PlayerPrefs.SetInt("muted", muted ? 1 : 0);
			PlayerPrefs.Save();
		}
	}

	/// <summary>
	/// Toggle Mute
	/// </summary>
	public void ToggleMute()
	{
		muted = !muted;
		Audio.audiosrc.mute = muted;
		PlayerPrefs.SetInt("muted", muted ? 1 : 0);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Toggle Dust Motes
	/// </summary>
	public void ToggleDust()
	{
		dust = !dust;
		PlayerPrefs.SetInt("dust", dust ? 1 : 0);
		foreach (GameObject found in dustList)
		{
			found.SetActive(dust);
		}
		PlayerPrefs.Save();
	}
}