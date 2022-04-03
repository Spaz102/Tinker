using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unity container for a Popup
/// </summary>
public class Popup: MonoBehaviour {
	public enum PopupTypes { win = 0, lose = 1};
	
	public Text title;
	public Text msg;
	public Image image;

	/// <summary>
	/// Trigger a popup
	/// </summary>
	public void Open(PopupTypes popupType)
	{
		switch (popupType) {
		case PopupTypes.lose:
			title.text = "";
			msg.text = "Whaa... you lost...";
			image.sprite = Resources.Load<Sprite>("Sprites/lose");
			break;
		case PopupTypes.win:
			title.text = "";
			msg.text = "Woo! You won!";
			image.sprite = Resources.Load<Sprite>("Sprites/win");
			break;
		}

		this.gameObject.SetActive(true);
	}

}
