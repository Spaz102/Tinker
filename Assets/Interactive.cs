using UnityEngine;
using System.Collections;

public class Interactive : MonoBehaviour { //TODO phase out; use unity's ui interface
	public string type;
	public Coord index; // Unused if not a tile

	public void OnMouseUpAsButton() {
		if (type == "Tile") {
			Game.QueueClick(this.index);
		}
	}
	public void OnMouseEnter() { // Seems to work via raytracing, so no need to worry about layers or z-fighting if it isn't happening visually
		if (type == "Tile") { //TODO: Mouseover other things, such as blueprint tiles and buttons
			Game.mouseover = this.index;
			Game.Mouseover();
		}
	}
	public void OnMouseExit() { //TODO: Add check for if program focus lost/regained? (Add a pause screen)
		if (type == "Tile") {
			if (Game.mouseover == this.index) {
				Game.mouseover = null;
			}
			Game.board.tile[index.x,index.y].underlay.ShowSprite("Empty");
			Game.Mouseover();
		}
	}
}
