using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Options : MonoBehaviour {
	public Text headtext;
	public Text bodytext;
	private GameObject[] dustlist;

	void Start () {
		this.gameObject.transform.localPosition = new Vector3(0f, -15f, 0f);
		this.gameObject.SetActive(false);
		Game.options = this;
		dustlist = GameObject.FindGameObjectsWithTag("Dust");
	}

	void Update () {

	}

	public void ToggleDust() {
		if (Game.settings.dust) {
			Game.settings.dust = false;
			foreach (GameObject found in dustlist) {
				found.SetActive(false);
			}
		} else {
			Game.settings.dust = true;
			foreach (GameObject found in dustlist) {
				found.SetActive(true);
			}
		}
	}
}