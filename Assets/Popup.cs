using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Popup: MonoBehaviour {
	public enum PopupTypes { win = 0, lose = 1};
	
	public Text title;
	public Text msg;
	public Image image;

	void Start() {

	}
		
	public void Open(PopupTypes popupType)
	{
		if (popupType == PopupTypes.lose) {
			title.text = "";
			msg.text = "Whaa... you lost...";
			image.sprite = Resources.Load<Sprite>("Sprites/lose");
		}
		else
		{
			title.text = "";
			msg.text = "Woo! You won!";
			image.sprite = Resources.Load<Sprite>("Sprites/win");
		}

		this.gameObject.SetActive(true);
	}

}
