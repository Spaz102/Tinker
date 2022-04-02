using System.Linq;
using UnityEngine;

public class UIControls: MonoBehaviour {
	public static GameObject ClosedScroll;
	public static GameObject OpenedScroll;
	public static GameObject LightBox;
	public static GameObject Codex;
	public static GameObject Popup;

	public bool scrollopen = false;

	void Start() {
		ClosedScroll = GameObject.Find("ClosedScroll");
		OpenedScroll = GameObject.Find("OpenedScroll");
		LightBox = GameObject.Find("LightBox");
		Popup = GameObject.Find("Popup");
		Codex = GameObject.Find("CodexContainer");

		// refit LowerArea to fit resolution
		RectTransform rtLowerArea = GameObject.Find("LowerArea").GetComponent<RectTransform>();
		rtLowerArea.offsetMax = new Vector2(rtLowerArea.offsetMax.x, -1080);

		// prep LightBox
		LightBox.SetActive(false);
		RectTransform rtLB = LightBox.GetComponent<RectTransform>();
		rtLB.anchoredPosition = Vector2.zero;
		rtLB.anchorMin = Vector2.zero;
		rtLB.anchorMax = Vector2.one;
		rtLB.sizeDelta = Vector2.zero;

		// Scroll state handling (resize needed to allow it to be displayed in the editor)        
		OpenedScroll.SetActive(false);
		RectTransform rtOpenedScroll = OpenedScroll.GetComponent<RectTransform>();
		rtOpenedScroll.anchoredPosition = Vector2.zero;
		rtOpenedScroll.anchorMin = Vector2.zero;
		rtOpenedScroll.anchorMax = Vector2.one;
		rtOpenedScroll.sizeDelta = Vector2.zero;
		rtOpenedScroll.offsetMax = new Vector2(-75, -25);
		rtOpenedScroll.offsetMin = new Vector2(75,0);

		// Resize needed to allow it to be displayed in the editor
		Popup.SetActive(false);
		RectTransform rtPopup = Popup.GetComponent<RectTransform>();
		rtPopup.anchoredPosition = Vector2.zero;
		rtPopup.anchorMin = Vector2.zero;
		rtPopup.anchorMax = Vector2.one;
		rtPopup.sizeDelta = Vector2.zero;
	}

	void Update()
	{
		// TODO: change this to a more 'unity' way (not in update)
		//Debug.Log(Data.playerseen.Values.Count(x => x) + " - " + Data.playerread.Values.Count(x => x));
		if (Data.playerseen.Values.Count(v => v) > Data.playerread.Values.Count(v => v)) {
			ClosedScroll.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, .5f);
		}
		else
		{
			ClosedScroll.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0);
		}
	}
		
	public void HideLightBox()
	{
		GameObject.Find("LightBox").SetActive(false);
	}

	public void HidePopup()
	{
		GameObject.Find("Popup").SetActive(false);
	}

	public void ToggleScroll() {
		Audio.PlaySound("Codex");
		//toggles
		scrollopen = !scrollopen;
		OpenedScroll.SetActive(scrollopen);
		ClosedScroll.SetActive(!scrollopen);

		// Woosh all the dust motes below the game board (in the path of the scroll)
		GameObject[] dustList = GameObject.FindGameObjectsWithTag("Dust");
		foreach (GameObject dust in dustList)
		{
			if (GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(dust.transform.position).y < Screen.width) { //using screen.width since the game board is always this high
				dust.GetComponent<DustMote>().momentum -= Quaternion.Euler(0, 0, Random.Range(-10, 10)) * new Vector3(0, 1f * (scrollopen?1:-1), 0);
			}
		}
	}

	public void ToggleDust() {
		Core.settings.SetDust(!Core.settings.dust);
		Audio.PlaySound("Menu");
	}
}
