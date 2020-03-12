using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Options : MonoBehaviour {
	public Text headtext;
	public Text bodytext;

	void Start () {
		this.gameObject.transform.localPosition = new Vector3(0f, -15f, 0f);
		this.gameObject.SetActive(false);
	}

	void Update () {

	}

	
}