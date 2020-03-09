using UnityEngine;
using System.Collections;

public class DustMote : MonoBehaviour {
	Vector3 momentum;
	int counter;
	float min;

	void Start() { // Random starting position, momentum
		this.GetComponent<UnityEngine.SpriteRenderer>().sprite = Data.dustsprites[Random.Range(0, 3)]; // Warning: Depends of spritesheet size
		this.transform.rotation = Quaternion.Euler(0,0,Random.Range(0,360));
		counter = Random.Range(2, 5); // Delay between next random angle change
		min = Random.Range(0.01f, 0.05f);
		Vector2 temp = new Vector2();
		temp = Random.insideUnitCircle;
		momentum = new Vector3(temp.x, temp.y);
		momentum.Normalize();
		momentum = momentum * min;
	}

	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			Vector3 temp = (Input.mousePosition - GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(this.transform.position));
			temp.z = 0;
			float dist = Mathf.Min(2200/temp.sqrMagnitude, 0.3f);
			temp.Normalize();
			temp = Quaternion.Euler(0,0,Random.Range(-10,10)) * temp;
			momentum += temp * -1 * dist;

			min = Random.Range(0.01f, 0.05f);
		} else {
			Vector3 temp = (Input.mousePosition - GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(this.transform.position));
			temp.z = 0;
			temp.Normalize();
			temp = Quaternion.Euler(0,0,Random.Range(-15,15)) * temp;
		}

		if (GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(this.transform.position + momentum).x < -10 || GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(this.transform.position + momentum).x > 490) {
			momentum.x = momentum.x * -0.7f;
			momentum = Quaternion.Euler(0,0,Random.Range(-10,10)) * momentum;
		}
		if (GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(this.transform.position + momentum).y < -10 || GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(this.transform.position + momentum).y > 810) {
			momentum.y = momentum.y * -0.7f;
			momentum = Quaternion.Euler(0,0,Random.Range(-10,10)) * momentum;
		}
		this.transform.position += momentum;

		if (momentum.magnitude > min) {
			momentum = momentum * 0.92f;
		} else if (momentum.magnitude < min) {
			momentum = momentum * 1.08f;
		}

		counter--;
		if (counter <= 0) {
			momentum = Quaternion.Euler(0,0,Random.Range(-5,5)) * momentum;
			counter = Random.Range(2, 5);
		}

	}
}