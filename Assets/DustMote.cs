using UnityEngine;
using System.Collections;

public class DustMote : MonoBehaviour {
	public Vector3 momentum;
	float rotationalMomentum;
	int counter; // How long until its next spontaneous redirection
	float min; // Mimimum velocity
	float minRotation = 0.5f;
	void Start() { // Random starting position, momentum
		this.GetComponent<UnityEngine.SpriteRenderer>().sprite = Data.dustsprites[Random.Range(0, 3)]; // Warning: Depends of spritesheet size
		this.transform.position = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0.7f));
		this.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 360));
		min = Random.Range(0.01f, 0.05f);

		rotationalMomentum = Random.Range(minRotation  * -1f, minRotation);
		momentum = Random.insideUnitCircle.normalized * min; // Start at minimum velocity
		counter = Random.Range(2, 5); // Delay between next random angle change
	}

	void Update () {
		if (Input.GetMouseButtonDown(0)) { // Mouse was clicked; recalculate momentum
			Vector2 away = (Input.mousePosition - GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(this.transform.position));
			float nearness = Mathf.Min(2200 / away.sqrMagnitude, 0.3f); // Warning: Magic numbers

			momentum -= Quaternion.Euler(0, 0, Random.Range(-10, 10)) * away.normalized * nearness; // Moves away from the click, with some angular variance

			if (nearness > 0.04f) {
				min = Random.Range(0.01f, 0.05f); // New minimum velocity
				rotationalMomentum = Random.Range(minRotation * -1f, minRotation);
			}
		}

		if (GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(this.transform.position + momentum).x < - 10 || GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(this.transform.position + momentum).x > Screen.width + 10) {
			momentum.x *= -0.7f; // Slow down on collision
			rotationalMomentum = Random.Range(minRotation * -1f, minRotation);
			momentum = Quaternion.Euler(0,0,Random.Range(-10,10)) * momentum;
		}
		if (GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(this.transform.position + momentum).y < - 10 || GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(this.transform.position + momentum).y > Screen.height + 10) {
			momentum.y *= -0.7f; // Slow down on collision
			rotationalMomentum = Random.Range(minRotation * -1f, minRotation);
			momentum = Quaternion.Euler(0,0,Random.Range(-10,10)) * momentum;
		}

		this.transform.position += momentum;
		this.transform.eulerAngles += new Vector3(0f, 0f, rotationalMomentum);
		rotationalMomentum *= 0.995f;

		if (momentum.magnitude > min) {
			momentum = momentum * 0.92f; // Slow down over time
		} else if (momentum.magnitude < min) {
			momentum = momentum * 1.08f; // Return to min if somehow below
		}

		counter--;
		if (counter <= 0) {
			momentum = Quaternion.Euler(0,0,Random.Range(-5,5)) * momentum;
			counter = Random.Range(2, 5);
		}

	}
}