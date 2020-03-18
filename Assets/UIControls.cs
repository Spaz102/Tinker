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
		rtOpenedScroll.offsetMax = new Vector2(-75, -50);
		rtOpenedScroll.offsetMin = new Vector2(75,0);
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
		Game.settings.SetDust(!Game.settings.dust);
		Game.PlaySound("Menu");
	}
}
