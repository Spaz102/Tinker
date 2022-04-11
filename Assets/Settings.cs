﻿using UnityEngine;
public class Settings
{
	public bool dust;
	public bool muted;
	private GameObject[] dustList;

	public Settings()
	{
		dustList = GameObject.FindGameObjectsWithTag("Dust");

		if (PlayerPrefs.HasKey("dust"))
		{
			dust = true;
			SetDust(PlayerPrefs.GetInt("dust") == 1);
		}
		else
		{
			dust = true;
			PlayerPrefs.SetInt("dust", 1);
			PlayerPrefs.Save();
		}

		if (PlayerPrefs.HasKey("muted"))
		{
			muted = PlayerPrefs.GetInt("muted") == 1;
		}
		else
		{
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

	public void SetDust(bool setting)
	{ //TODO: Update interface
		if (setting == dust) return;

		dust = setting;
		if (setting)
		{
			PlayerPrefs.SetInt("dust", 1);
			foreach (GameObject found in dustList)
			{
				found.SetActive(true);
			}
		}
		else
		{
			PlayerPrefs.SetInt("dust", 0);
			foreach (GameObject found in dustList)
			{
				found.SetActive(false);
			}
		}
		PlayerPrefs.Save();
	}
}




/*using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Options : MonoBehaviour {
	public Text headtext;
	public Text bodytext;

	void Start () {
		this.gameObject.transform.localPosition = new Vector3(0f, -15f, 0f);
		this.gameObject.SetActive(false);
	}

	void Update () {

	}

	
}*/
