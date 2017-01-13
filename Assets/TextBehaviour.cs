using UnityEngine;
using System.Collections;

public class TextBehaviour : MonoBehaviour {

    public Camera MainCamera;
	
	// Update is called once per frame
	void Update () {
        this.transform.LookAt(MainCamera.transform);
        this.transform.Rotate(new Vector3(0, -180, 0));

        //Debug.Log(Vector3.Distance(this.transform.localPosition, MainCamera.transform.localPosition));
        if(Vector3.Distance(this.transform.localPosition, MainCamera.transform.localPosition) >= 1500)
        {
            this.transform.localScale = new Vector3(10, 10, 10);
            this.GetComponent<TextMesh>().color = new Color(this.GetComponent<TextMesh>().color.r, this.GetComponent<TextMesh>().color.g, this.GetComponent<TextMesh>().color.b, 255);
        }
        else if(Vector3.Distance(this.transform.localPosition, MainCamera.transform.localPosition) <= 300)
        {
            this.GetComponent<TextMesh>().color = new Color(this.GetComponent<TextMesh>().color.r, this.GetComponent<TextMesh>().color.g, this.GetComponent<TextMesh>().color.b, 0);
        }
        else
        {
            this.transform.localScale = new Vector3(1, 1, 1);
            this.GetComponent<TextMesh>().color = new Color(this.GetComponent<TextMesh>().color.r, this.GetComponent<TextMesh>().color.g, this.GetComponent<TextMesh>().color.b, 255);
        }
	}
}
