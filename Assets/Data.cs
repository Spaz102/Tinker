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
	public static Dictionary<string, bool> playerseen;

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
		tiledefs = new Dictionary<string, TileDef>();

		tiledefs.Add("Empty", new TileDef("Empty", 0, "", null, new Color(0,0,0,0), "Hello World"));
		tiledefs.Add("NewRat", new TileDef("Temporary rat", 0, "", rawsprites[18], new Color(1,1,1,1), ""));
		
		tiledefs.Add("Seed", new TileDef("Sapling", 96, "Empty", rawsprites[0], new Color(1,1,1,1), "Why does it take three seeds to grow a single stick? Taxes."));
		tiledefs.Add("Stick", new TileDef("Stick", 28, "Seed", rawsprites[1], new Color(1,1,1,1), "Stick with me, and we'll do great things"));
		tiledefs.Add("Wood", new TileDef("Raw lumber", 10, "Stick", rawsprites[2], new Color(1,1,1,1), "Shall I compare thee to a summer's day? Thou art more... Uh... Look, it's just some wood, ok?"));
		tiledefs.Add("Plank", new TileDef("Wood plank", 4, "Wood", rawsprites[3], new Color(1,1,1,1), "Resist the urge to imitate one"));
		tiledefs.Add("Panel", new TileDef("Sturdy panel", 1, "Plank", rawsprites[4], new Color(1,1,1,1), "It makes a decent coaster, if nothing else"));

		tiledefs.Add("Dirt", new TileDef("Dirt", 127, "Empty", rawsprites[5], new Color(1,1,1,1), "The humblest of beginnings. Clump together enough... and you'll still be disappointed"));
		tiledefs.Add("Rock", new TileDef("Rock", 30, "Dirt", rawsprites[6], new Color(1,1,1,1), "One day, I'll treat you to my conglomeration of rock puns"));
		tiledefs.Add("Metal", new TileDef("Metal", 7, "Rock", rawsprites[7], new Color(1,1,1,1), "Not included: black nail polish and pointy accessories"));
		tiledefs.Add("Gear", new TileDef("Gear", 0, "Metal", rawsprites[8], new Color(1,1,1,1), "You might be thinking this is just an ordinary gear... Correct!"));
		tiledefs.Add("Pin", new TileDef("Metal spike", 0, "Metal", rawsprites[9], new Color(1,1,1,1), "This handsome stud is looking for love"));

		tiledefs.Add("Cylinder", new TileDef("Metal cylinder", 0, "", rawsprites[10], new Color(1,1,1,1), "Not a very good rolling pin"));
		tiledefs.Add("Spring", new TileDef("Coil spring", 0, "", rawsprites[11], new Color(1,1,1,1), "Use #493 for springs: hilarious facial accessory"));
		tiledefs.Add("Key", new TileDef("Tiny key", 0, "Empty", rawsprites[12], new Color(1,1,1,1), "What kind of key doesn't unlock anything?"));
		tiledefs.Add("Comb", new TileDef("Fine metal comb", 0, "", rawsprites[13], new Color(1,1,1,1), "What are you doing, checking for lice?"));
		tiledefs.Add("Drum", new TileDef("Spiked cylinder", 0, "", rawsprites[14], new Color(1,1,1,1), "Modern art, perhaps?"));

		tiledefs.Add("Motor", new TileDef("Spring battery", 0, "", rawsprites[15], new Color(1,1,1,1), "A device for storing and distributing kinetic energy"));
		tiledefs.Add("MusicBox", new TileDef("Mechanical box", 0, "", rawsprites[16], new Color(1,1,1,1), "What kind of box needs a key, but isn't locked?"));
		tiledefs.Add("Special", new TileDef("Lift-o-matic", 19, "", rawsprites[17], new Color(0.75f,0.25f,0.75f,1), "You have in your hand, a great tool for picking things up. Also, you're holding something"));
		tiledefs.Add("Rat", new TileDef("Rattus norvegicus", 10, "Rat", rawsprites[18], new Color(1,1,1,1), "Ugly, but also cute! Kind of like a pug, except it will eat anything and won't drool. Why do people like pugs?"));
		tiledefs.Add("Storage", new TileDef("Storage space", 0, "", rawsprites[23], new Color(0.2f,0.4f,0.2f,0.3f), "Put things down here, and you can pick them up again! This stackable storage is pretty much a miracle"));

		tiledefs.Add("Junk1", new TileDef("Junk 'n stuff", 17, "", rawsprites[20], new Color(0.5f,0,0,1), "Hoarding is a disgusting habit. This is how you get rats"));
		tiledefs.Add("Junk2", new TileDef("Scraps 'n stuff", 17, "", rawsprites[21], new Color(0,0.5f,0,1), ""));
		tiledefs.Add("Junk3", new TileDef("Junk 'n scraps", 17, "", rawsprites[22], new Color(0,0,0.5f,1), ""));
		tiledefs.Add("Highlight", new TileDef("Highlight", 0, "", rawsprites[23], new Color(0.6f,0.8f,0.8f,0.3f), ""));
		tiledefs.Add("Mouseover", new TileDef("Mouseover", 0, "", rawsprites[23], new Color(1,1,1,0.1f), ""));

		patterns = new List<Pattern>(); // Warning: Patterns checked in order; be careful of conflicts

		patterns.Add(new Pattern("Dirt", "4Cont", "Rock"));
		patterns.Add(new Pattern("Rock", "4Cont", "Metal"));
		patterns.Add(new Pattern("Seed", "3Cont", "Stick"));
		patterns.Add(new Pattern("Stick", "3Cont", "Wood"));
		patterns.Add(new Pattern("Wood", "3Cont", "Plank"));
		patterns.Add(new Pattern("Plank", "3Cont", "Panel"));
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

		audiofiles.Add("PlaceTile", Resources.Load<AudioClip>("Audio/Click"));
		audiofiles.Add("Special", Resources.Load<AudioClip>("Audio/Wrench"));
		audiofiles.Add("Rat", Resources.Load<AudioClip>("Audio/Rat"));
		//TODO: One audio file per resource?

		playerseen = new Dictionary<string, bool>();
		foreach (string defkey in tiledefs.Keys) {
			playerseen.Add(defkey, false);
		}
		playerseen["Empty"] = true;
		playerseen["NewRat"] = true;
		//playerseen["Rat"] = true;
		playerseen["Junk1"] = true;
		playerseen["Junk2"] = true;
		playerseen["Junk3"] = true;
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
	public int chancetodraw; // Sum of all values used to determine actual chances
	public string edible;
	public Sprite sprite;
	public Color color; //TODO: Phase out coloured tiles for actual sprites

	public TileDef(string name, int chance, string edible, Sprite sprite, Color color, string desc) {
		this.name = name;
		this.description = desc;
		this.chancetodraw = chance;
		this.edible = edible;
		this.sprite = sprite;
		this.color = color;
	}
}