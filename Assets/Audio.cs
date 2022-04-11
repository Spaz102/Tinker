using UnityEngine;

/// <summary>
/// Sounds and Music
/// </summary>
/// mute/volume control in settings.cs
public static class Audio
{
	public static AudioSource audiosrc;

	static Audio()
	{
		audiosrc = GameObject.Find("Main Canvas").GetComponent<AudioSource>();
		audiosrc.mute = Core.settings.muted;
	}

	/// <summary>
	/// Play short sounds/fx
	/// </summary>
	public static void PlaySound(string name)
	{
		if (!string.IsNullOrWhiteSpace(name) && Data.audiofiles.ContainsKey(name))
		{
			audiosrc.PlayOneShot(Data.audiofiles[name]);
		}
	}
}