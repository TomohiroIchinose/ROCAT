using UnityEngine;
using System.Collections;

public class SpriteChange : MonoBehaviour {

	// Use this for initialization
	void Start () {
        float changeRed = 0.0f;
        float changeGreen = 1.0f;
        float changeBlue = 0.7f;
        float changeAlpha = 1.0f;
        this.GetComponent<SpriteRenderer>().color = new Color(changeRed, changeGreen, changeBlue, changeAlpha);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
