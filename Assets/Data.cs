using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Remember that dictionaries must be initialized manually

public static class Data {
	public static void Awake() {}

	public static GameObject tiletemplate;
	public static GameObject ghosttemplate;
	public static GameObject storagetemplate;

	public static List<Pattern> patterns;
	public static Dictionary<string, Pattern> patternresults;
	public static Dictionary<string, TileDef> tiledefs;
	public static Dictionary<string, AudioClip> audiofiles;
	public static Sprite dustsprite;
	public static Sprite[] dustsprites;
	public static Sprite[] storageSprites;
	public static Dictionary<string, bool> playerseen;
	public static Dictionary<string, bool> playerread;

	public static bool devmode = true;

	static Data() {
		FillLists();
	}

	private static void FillLists() {
		Data.tiletemplate = (GameObject) Resources.Load("TileTemplate");
		Data.ghosttemplate = (GameObject) Resources.Load("GhostTemplate");
		Data.storagetemplate = (GameObject) Resources.Load("StorageTemplate");

		audiofiles = new Dictionary<string, AudioClip>();
		dustsprites = Resources.LoadAll<Sprite>("Sprites/smokesheet");
		dustsprite = Resources.Load<Sprite>("Sprites/Dust");
		Sprite[] rawsprites = Resources.LoadAll<Sprite>("Sprites/Tinkerer");
		storageSprites = Resources.LoadAll<Sprite>("Sprites/storage spritesheet");
		tiledefs = new Dictionary<string, TileDef>();

		tiledefs.Add("Empty", new TileDef("Empty", 0, "", null, 0f, 76, "Hello World", ""));
		tiledefs.Add("NewRat", new TileDef("Temporary rat", 0, "", rawsprites[18], 1f, 0, "", "Rat")); // Correct/ideal sfx_name?
		
		tiledefs.Add("Seed", new TileDef("Sapling", 96, "Empty", rawsprites[0], 1f, 5, "Why does it take three seeds to grow a single stick? Taxes.", "Click"));
		tiledefs.Add("Stick", new TileDef("Stick", 28, "Seed", rawsprites[1], 1f, 3, "Stick with me, and we'll do great things", "Click"));
		tiledefs.Add("Wood", new TileDef("Raw lumber", 10, "Stick", rawsprites[2], 1f, 1, "Shall I compare thee to a summer's day? Thou art more... Uh... Look, it's just some wood, ok?", "Click"));
		tiledefs.Add("Plank", new TileDef("Wood plank", 4, "Wood", rawsprites[3], 1f, 0, "Resist the urge to imitate one", "Click"));
		tiledefs.Add("Panel", new TileDef("Sturdy panel", 1000, "Plank", rawsprites[4], 1f, 0, "It makes a decent coaster, if nothing else", "Click"));
		//tiledefs.Add("Panel", new TileDef("Sturdy panel", 1, "Plank", rawsprites[4], 1f, 0, "It makes a decent coaster, if nothing else", "Click"));

		tiledefs.Add("Dirt", new TileDef("Dirt", 127, "Empty", rawsprites[5], 1f, 5, "The humblest of beginnings. Clump together enough... and you'll still be disappointed", "Click"));
		tiledefs.Add("Rock", new TileDef("Rock", 30, "Dirt", rawsprites[6], 1f, 3, "One day, I'll treat you to my conglomeration of rock puns", "Click"));
		tiledefs.Add("Metal", new TileDef("Metal", 7, "Rock", rawsprites[7], 1f, 1, "Not included: black nail polish and pointy accessories", "Click"));
		tiledefs.Add("Gear", new TileDef("Gear", 0, "Metal", rawsprites[8], 1f, 0, "You might be thinking this is just an ordinary gear... Correct!", "Drop Metal Thing-SoundBible.com-401640954"));
		tiledefs.Add("Pin", new TileDef("Metal spike", 0, "Metal", rawsprites[9], 1f, 0, "This handsome stud is looking for love", "Drop Metal Thing-SoundBible.com-401640954"));

		tiledefs.Add("Cylinder", new TileDef("Metal cylinder", 0, "", rawsprites[10], 1f, 0, "Not a very good rolling pin", "Drop Metal Thing-SoundBible.com-401640954"));
		tiledefs.Add("Spring", new TileDef("Coil spring", 0, "", rawsprites[11], 1f, 0, "Use #493 for springs: hilarious facial accessory", "Drop Metal Thing-SoundBible.com-401640954"));
		tiledefs.Add("Key", new TileDef("Tiny key", 0, "Empty", rawsprites[12], 1f, 0, "What kind of key doesn't unlock anything?", "Drop Metal Thing-SoundBible.com-401640954"));
		tiledefs.Add("Comb", new TileDef("Fine metal comb", 0, "", rawsprites[13], 1f, 0, "What are you doing, checking for lice?", "Click"));
		tiledefs.Add("Drum", new TileDef("Spiked cylinder", 0, "", rawsprites[14], 1f, 0, "Modern art, perhaps?", "Click"));

		tiledefs.Add("Motor", new TileDef("Spring battery", 0, "", rawsprites[15], 1f, 0, "A device for storing and distributing kinetic energy", "Click"));
		tiledefs.Add("MusicBox", new TileDef("Mechanical box", 0, "", rawsprites[16], 1f, 0, "What kind of box needs a key, but isn't locked?", "Click"));
		tiledefs.Add("Special", new TileDef("Lift-o-matic", 19, "", rawsprites[17], 1f, 0, "You have in your hand, a great tool for picking things up. Also, you're holding something", "Wrench"));
		tiledefs.Add("Rat", new TileDef("Rattus norvegicus", 10, "Rat", rawsprites[18], 1f, 3, "Ugly, but also cute! Kind of like a pug, except it will eat anything and won't drool. Why do people like pugs?", "Rat"));
		tiledefs.Add("Storage", new TileDef("Storage space", 0, "", rawsprites[19], 1f, 0, "Put things down here, and you can pick them up again! This stackable storage is pretty much a miracle", "Pew_Pew-DKnight556-1379997159"));

		tiledefs.Add("Junk1", new TileDef("Junk 'n stuff", 17, "", rawsprites[20], 1f, 1, "Hoarding is a disgusting habit. This is how you get rats", "Mirror Breaking-SoundBible.com-73239746"));
		tiledefs.Add("Junk2", new TileDef("Scraps 'n stuff", 17, "", rawsprites[21], 1f, 1, "", "Glass Breaking-SoundBible.com-1765179538"));
		tiledefs.Add("Junk3", new TileDef("Junk 'n scraps", 17, "", rawsprites[22], 1f, 1, "", "Mirror Breaking-SoundBible.com-73239746"));
		tiledefs.Add("Highlight", new TileDef("Highlight", 0, "", rawsprites[23], 0.3f, 0, "", ""));
		tiledefs.Add("Mouseover", new TileDef("Mouseover", 0, "", rawsprites[23], 0.1f, 0, "", ""));

		patterns = new List<Pattern>(); // Warning: Patterns checked in order; be careful of conflicts
		#region continuous
		patterns.Add(new Pattern("Dirt", "4Cont", "Rock"));
		patterns.Add(new Pattern("Rock", "4Cont", "Metal"));
		patterns.Add(new Pattern("Seed", "3Cont", "Stick"));
		patterns.Add(new Pattern("Stick", "3Cont", "Wood"));
		patterns.Add(new Pattern("Wood", "3Cont", "Plank"));
		patterns.Add(new Pattern("Plank", "3Cont", "Panel"));
		#endregion
		#region gear
		patterns.Add(new Pattern(new string[]{
			"",			"Metal",	"",
			"Metal",	"Empty",	"Metal",
			"",			"Metal",	""},
			"Static", "Gear"));
		#endregion
		#region spring
		patterns.Add(new Pattern(new string[]{
			"Metal",	"Metal",	"",
			"Metal",	"Gear",		"",
			"",			"Metal",	"Metal"},
			"Static", "Spring"));
		patterns.Add(new Pattern(new string[]{
			"",			"Metal",	"Metal",
			"Metal",	"Gear",		"Metal",
			"Metal",	"",		""},
			"Static", "Spring"));
		patterns.Add(new Pattern(new string[]{
			"Metal",	"Metal",	"",
			"",			"Gear",		"Metal",
			"",			"Metal",	"Metal"},
			"Static", "Spring"));
		patterns.Add(new Pattern(new string[]{
			"",			"",			"Metal",
			"Metal",	"Gear",		"Metal",
			"Metal",	"Metal",	""},
			"Static", "Spring"));
		patterns.Add(new Pattern(new string[]{
			"Metal",	"Metal",	"",
			"Metal",	"Gear",		"Metal",
			"",			"",			"Metal"},
			"Static", "Spring"));
		patterns.Add(new Pattern(new string[]{
			"",			"Metal",	"Metal",
			"Metal",	"Gear",		"",
			"Metal",	"Metal",	""},
			"Static", "Spring"));
		patterns.Add(new Pattern(new string[]{
			"Metal",	"",			"",
			"Metal",	"Gear",		"Metal",
			"",			"Metal",	"Metal"},
			"Static", "Spring"));
		patterns.Add(new Pattern(new string[]{
			"",			"Metal",	"Metal",
			"",			"Gear",		"Metal",
			"Metal",	"Metal",	""},
			"Static", "Spring"));
		#endregion
		#region pin
		patterns.Add(new Pattern(new string[]{
			"",			"Metal",	"",
			"",			"Metal",	"",
			"",			"Metal",	""},
			"Static", "Pin"));
		patterns.Add(new Pattern(new string[]{
			"",			"",			"",
			"Metal",	"Metal",	"Metal",
			"",			"",			""},
			"Static", "Pin"));
		#endregion
		#region cylinder
		patterns.Add(new Pattern(new string[]{
			"",			"Pin",		"",
			"",			"Pin",		"",
			"",			"Pin",		""},
			"Static", "Cylinder"));
		patterns.Add(new Pattern(new string[]{
			"",			"",			"",
			"Pin",		"Pin",		"Pin",
			"",			"",			""},
			"Static", "Cylinder"));
		#endregion
		#region key
		patterns.Add(new Pattern(new string[]{
			"",			"Metal",	"Pin",
			"",			"Stick",	"Pin",
			"",			"Stick",	""},
			"Static", "Key"));
		patterns.Add(new Pattern(new string[]{
			"Pin",			"Metal",	"",
			"Pin",			"Stick",	"",
			"",				"Stick",	""},
			"Static", "Key"));
		patterns.Add(new Pattern(new string[]{
			"",			"Stick",	"",
			"Pin",		"Stick",	"",
			"Pin",		"Metal",	""},
			"Static", "Key"));
		patterns.Add(new Pattern(new string[]{
			"",			"Stick",	"",
			"",			"Stick",	"Pin",
			"",			"Metal",	"Pin"},
			"Static", "Key"));
		patterns.Add(new Pattern(new string[]{
			"Pin",		"Pin",		"",
			"Metal",	"Stick",	"Stick",
			"",			"",			""},
			"Static", "Key"));
		patterns.Add(new Pattern(new string[]{
			"",			"Pin",		"Pin",
			"Stick",	"Stick",	"Metal",
			"",			"",			""},
			"Static", "Key"));
		patterns.Add(new Pattern(new string[]{
			"",			"",			"",
			"Metal",	"Stick",	"Stick",
			"Pin",		"Pin",		""},
			"Static", "Key"));
		patterns.Add(new Pattern(new string[]{
			"",			"",			"",
			"Stick",	"Stick",	"Metal",
			"",			"Pin",		"Pin"},
			"Static", "Key"));

		#endregion
		#region comb
		patterns.Add(new Pattern(new string[]{
			"Cylinder",	"",			"",
			"Cylinder",	"Cylinder",	"",
			"Panel",	"Panel",	"Panel"},
			"Static", "Comb"));
		patterns.Add(new Pattern(new string[]{
			"",			"",			"Cylinder",
			"",			"Cylinder",	"Cylinder",
			"Panel",	"Panel",	"Panel"},
			"Static", "Comb"));
		patterns.Add(new Pattern(new string[]{
			"Panel",	"",			"",
			"Panel",	"Cylinder",	"",
			"Panel",	"Cylinder",	"Cylinder"},
			"Static", "Comb"));
		patterns.Add(new Pattern(new string[]{
			"Panel",	"Cylinder",	"Cylinder",
			"Panel",	"Cylinder",	"",
			"Panel",	"",			""},
			"Static", "Comb"));
		patterns.Add(new Pattern(new string[]{
			"Panel",	"Panel",	"Panel",
			"Cylinder",	"Cylinder",	"",
			"Cylinder",	"",			""},
			"Static", "Comb"));
		patterns.Add(new Pattern(new string[]{
			"Panel",	"Panel",	"Panel",
			"",			"Cylinder",	"Cylinder",
			"",			"",			"Cylinder"},
			"Static", "Comb"));
		patterns.Add(new Pattern(new string[]{
			"Cylinder",	"Cylinder",	"Panel",
			"",			"Cylinder",	"Panel",
			"",			"",			"Panel"},
			"Static", "Comb"));
		patterns.Add(new Pattern(new string[]{
			"",			"",			"Panel",
			"",			"Cylinder",	"Panel",
			"Cylinder",	"Cylinder",	"Panel"},
			"Static", "Comb"));
		#endregion
		#region drum
		patterns.Add(new Pattern(new string[]{
			"Pin",		"Cylinder",	"Pin",
			"",			"Cylinder",	"",
			"Pin",		"Cylinder",	"Pin"},
			"Static", "Drum"));
		patterns.Add(new Pattern(new string[]{
			"Pin",		"",			"Pin",
			"Cylinder",	"Cylinder",	"Cylinder",
			"Pin",		"",			"Pin"},
			"Static", "Drum"));
		#endregion
		#region motor
		patterns.Add(new Pattern(new string[]{
			"Panel",	"Gear",		"Panel",
			"Gear",		"Spring",	"Gear",
			"Panel",	"Gear",		"Panel"},
			"Static", "Motor"));
		#endregion
		#region musicbox
		patterns.Add(new Pattern(new string[]{
			"Panel",	"Panel",	"Panel",
			"Motor",	"Drum",		"Comb",
			"Panel",	"Panel",	"Panel"},
			"Static", "MusicBox"));
		patterns.Add(new Pattern(new string[]{
			"Panel",	"Panel",	"Panel",
			"Comb",		"Drum",		"Motor",
			"Panel",	"Panel",	"Panel"},
			"Static", "MusicBox"));
		patterns.Add(new Pattern(new string[]{
			"Panel",	"Comb",		"Panel",
			"Panel",	"Drum",		"Panel",
			"Panel",	"Motor",	"Panel"},
			"Static", "MusicBox"));
		patterns.Add(new Pattern(new string[]{
			"Panel",	"Motor",	"Panel",
			"Panel",	"Drum",		"Panel",
			"Panel",	"Comb",		"Panel"},
			"Static", "MusicBox"));
		#endregion
		#region storage
		patterns.Add(new Pattern(new string[]{ //TODO protect storage panels from other patterns
			"Panel",	"Panel",
			"Panel",	"Panel"},
			"Storage", "CreateStorage"));
		#endregion
		#region junk
		patterns.Add(new Pattern("Junk1", "3Cont", "NewRat"));
		patterns.Add(new Pattern("Junk2", "3Cont", "NewRat"));
		patterns.Add(new Pattern("Junk3", "3Cont", "NewRat"));
		patterns.Add(new Pattern(new string[] {"NewRat", "Junk2", "Junk3"}, "Set", "NewRat")); // Allow simultaneous 3-match and variety-match to get fewer rats per junk
		patterns.Add(new Pattern(new string[] {"Junk1", "NewRat", "Junk3"}, "Set", "NewRat"));
		patterns.Add(new Pattern(new string[] {"Junk1", "Junk2", "NewRat"}, "Set", "NewRat"));
		Pattern temp = new Pattern(new string[] {"Junk1", "Junk2", "Junk3"}, "Set", "NewRat"); // Done to force blueprints to show rat pattern correctly
		patterns.Add(temp);
		#endregion

		patternresults = new Dictionary<string, Pattern>();
		patternresults.Add("Rat", temp);
		foreach (Pattern pattern in patterns) {
			if (!patternresults.ContainsKey(pattern.result)) {
				patternresults.Add(pattern.result, pattern);
			}
		}

		audiofiles.Add("Startup", Resources.Load<AudioClip>("Audio/snd_quest_complete"));
		audiofiles.Add("Menu", Resources.Load<AudioClip>("Audio/323402__gosfx__sound-2"));
		audiofiles.Add("Crunch", Resources.Load<AudioClip>("Audio/Crunch"));
		audiofiles.Add("Codex", Resources.Load<AudioClip>("Audio/Page_Turn-Mark_DiAngelo-1304638748"));
		audiofiles.Add("Byebye", Resources.Load<AudioClip>("Audio/bye_bye_son-Mike_Koenig-1260922981"));
		audiofiles.Add("GameOver", Resources.Load<AudioClip>("Audio/Sad_Trombone-Joe_Lamb-665429450"));

		foreach(string resource in tiledefs.Keys) { // Warning: Loads the same sfx_name file multiple times, if used for multiple resources
			if (!string.IsNullOrWhiteSpace(tiledefs[resource].sfx_name)) {
				audiofiles.Add(resource, Resources.Load<AudioClip>("Audio/" + tiledefs[resource].sfx_name));
			}
		}

		playerseen = new Dictionary<string, bool>();
		playerread = new Dictionary<string, bool>();
		foreach (string defkey in tiledefs.Keys) {
			playerseen.Add(defkey, false);
			playerread.Add(defkey, false);
		}
		playerseen["Empty"] = true;
		playerseen["NewRat"] = true;
		//playerseen["Rat"] = true;
		playerseen["Junk1"] = true;
		playerseen["Junk2"] = true;
		playerread["Junk2"] = true;
		playerseen["Junk3"] = true;
		playerread["Junk3"] = true;
		playerseen["Seed"] = true;
		playerseen["Dirt"] = true;
		playerseen["Special"] = true;
		playerseen["Storage"] = true;
	}
}

public class Pattern {
	public string type; // "Cont" for contiguous, "Static" for static, etc. Technically unnecessary, as dimensions of types are distinct
	public string[,] value; // Eg: ("Dirt", "Rock") or an array for static. "" for irrelevant tiles
	//public string[,] result; // Always a square (1, 4, or 9), same dimensions as value if static pattern
	public string result; // Always a single tile??

	public Pattern(string input, string type, string result) {
		string[,] newinput = new string[1,1];
		newinput[0,0] = input;

		this.value = newinput;
		this.type = type;
		this.result = result;
	}
	public Pattern(string[] input, string type, string result) {
		string[,] newinput;
		if ((type == "Static" || type == "Storage") && input.Length == 4) {
			newinput = new string[2, 2];
		} else if (type == "Static" && input.Length == 9) {
			newinput = new string[3, 3];
		} else {
			newinput = new string[input.Length,1];
		}
		for (int y = 0; y < newinput.GetLength(1); y++) {
			for (int x = 0; x < newinput.GetLength(0); x++) {
				newinput[x,y] = input[x + newinput.GetLength(0)*y]; //Warning: Board space has 0,0 at bottom left - pattern has 0,0 at top left
			}
		}

		this.value = newinput;
		this.type = type;
		this.result = result;
	}
	public Pattern(string[,] input, string type, string result) {
		this.value = input;
		this.type = type;
		this.result = result;
	}
}

public class TileDef {
	public string name;
	public string description;
	public int chanceToDraw; // Sum of all values used to determine actual chances
	public string edible;
	public Sprite sprite;
	public float opacity;
	public int startingChance; // The weight of this resource on the initial board state' distribution
	public string sfx_name; // The filename in the Audio resources folder

	public TileDef(string name, int chanceToDraw, string edible, Sprite sprite, float opacity, int startingChance, string description, string sfx_name) {
		this.name = name;
		this.description = description;
		this.chanceToDraw = chanceToDraw;
		this.edible = edible;
		this.sprite = sprite;
		this.opacity = opacity;
		this.startingChance = startingChance;
		this.sfx_name = sfx_name;
	}
}