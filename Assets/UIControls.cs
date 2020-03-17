using UnityEngine;

public class UIControls: MonoBehaviour {
	public static GameObject ClosedScroll;
	public static GameObject OpenedScroll;
	public static GameObject LightBox;

	public bool scrollopen = false;

	void Start() {
		ClosedScroll = GameObject.Find("ClosedScroll");
		OpenedScroll = GameObject.Find("OpenedScroll");
		LightBox = GameObject.Find("LightBox");

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
	}

	public void HideLightBox()
	{
		GameObject.Find("LightBox").SetActive(false);
	}

	public void ToggleScroll() {
		Game.PlaySound("Codex");
		//toggles
		scrollopen = !scrollopen;
		OpenedScroll.SetActive(scrollopen);
		ClosedScroll.SetActive(!scrollopen);

		if (scrollopen) {

		} else {

		}
	}

	public void ToggleDust() {
		Game.settings.SetDust(!Game.settings.dust);
		Game.PlaySound("Menu");
	}
}
