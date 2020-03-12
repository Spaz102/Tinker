using UnityEngine;

public class UIControls : MonoBehaviour
{
    public static GameObject ClosedScroll;
    public static GameObject OpenedScroll;

    public bool scrollopen = false;

    void Start()
    {
        ClosedScroll = GameObject.Find("ClosedScroll");
        OpenedScroll = GameObject.Find("OpenedScroll");

        // refit LowerArea to fit resolution
        RectTransform rtLowerArea = GameObject.Find("LowerArea").GetComponent<RectTransform>();
        rtLowerArea.offsetMax = new Vector2(rtLowerArea.offsetMax.x, -1080);
        
        // Scroll state handling (resize needed to allow it to be displayed in the editor)        
        OpenedScroll.SetActive(false);
        RectTransform rtOpenedScroll = OpenedScroll.GetComponent<RectTransform>();
        rtOpenedScroll.anchoredPosition = Vector2.zero;
        rtOpenedScroll.anchorMin = Vector2.zero;
        rtOpenedScroll.anchorMax = Vector2.one;
        rtOpenedScroll.sizeDelta = Vector2.zero;
    }

    public void ToggleScroll()
    {
        //toggles
        scrollopen = !scrollopen;
        OpenedScroll.SetActive(scrollopen); 
        ClosedScroll.SetActive(!scrollopen);
        
        if (scrollopen)
        {

        }
        else
        {

        }
    }
}
