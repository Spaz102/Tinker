using UnityEngine;

public static class Audio {
	public static AudioSource audiosrc;

	static Audio()
	{
		audiosrc = GameObject.Find("Main Canvas").GetComponent<AudioSource>();
	}
	public static void PlaySound(string name)
	{
		if (Game.settings.sound && !string.IsNullOrWhiteSpace(name) && Data.audiofiles.ContainsKey(name))
		{
			audiosrc.PlayOneShot(Data.audiofiles[name]);
		}
	}


}