using UnityEngine;

/// <summary>
/// Sounds and Music
/// </summary>
public static class Audio
{
	public static AudioSource audiosrc;
	public static bool muted;

	static Audio()
	{
		audiosrc = GameObject.Find("Main Canvas").GetComponent<AudioSource>();
		muted = false;
	}

	/// <summary>
	/// Play short sounds/fx
	/// </summary>
	public static void PlaySound(string name)
	{
		if (Core.settings.sound && !string.IsNullOrWhiteSpace(name) && Data.audiofiles.ContainsKey(name))
		{
			audiosrc.PlayOneShot(Data.audiofiles[name]);
		}
	}

	/// <summary>
	/// Toggle Mute
	/// </summary>
	public static void ToggleMute()
	{
		muted = !muted;
		audiosrc.mute = muted;
	}
}