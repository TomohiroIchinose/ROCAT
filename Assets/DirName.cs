using UnityEngine;
using System.Collections;

public class DirName : MonoBehaviour {

    public GameObject MainCamera;
    public GameObject Text;
    public GameObject Back;

    // Use this for insitialization
    void Start()
    {
        MainCamera = GameObject.Find("Main Camera");
        Text = this.transform.FindChild("Name").gameObject;
        Back = Text.transform.FindChild("Back").gameObject;
        this.transform.localScale = new Vector3(2, 2, 2);
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.LookAt(MainCamera.transform);
        this.transform.Rotate(new Vector3(0, -180, 0));

        //Debug.Log(Vector3.Distance(this.transform.localPosition, MainCamera.transform.localPosition));
        if (Vector3.Distance(this.transform.localPosition, MainCamera.transform.localPosition) <= 700)
        {
            Text.GetComponent<Renderer>().enabled = false;
            Back.GetComponent<Renderer>().enabled = false;
            //Text.GetComponent<TextMesh>().color = new Color(Text.GetComponent<TextMesh>().color.r, Text.GetComponent<TextMesh>().color.g, Text.GetComponent<TextMesh>().color.b, 0);
            //Back.GetComponent<MeshRenderer>().material.color = new Color(Back.GetComponent<MeshRenderer>().material.color.r, Back.GetComponent<MeshRenderer>().material.color.g, Back.GetComponent<MeshRenderer>().material.color.b, 0);
        }
        else
        {
            Text.GetComponent<Renderer>().enabled = true;
            Back.GetComponent<Renderer>().enabled = true;

            if (Vector3.Distance(this.transform.localPosition, MainCamera.transform.localPosition) >= 4000)
            {
                this.transform.localScale = new Vector3(6, 6, 6);
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, 800, this.transform.localPosition.z);
            }
            else if (Vector3.Distance(this.transform.localPosition, MainCamera.transform.localPosition) >= 2500)
            {
                this.transform.localScale = new Vector3(4, 4, 4);
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, 400, this.transform.localPosition.z);
            }
            else
            {
                this.transform.localScale = new Vector3(2, 2, 2);
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, 200, this.transform.localPosition.z);
            }

            //Text.GetComponent<TextMesh>().color = new Color(Text.GetComponent<TextMesh>().color.r, Text.GetComponent<TextMesh>().color.g, Text.GetComponent<TextMesh>().color.b, 255);
            //Back.GetComponent<MeshRenderer>().material.color = new Color(Back.GetComponent<MeshRenderer>().material.color.r, Back.GetComponent<MeshRenderer>().material.color.g, Back.GetComponent<MeshRenderer>().material.color.b, 255);
        }
    }

    public void SetNameText(string name)
    {
        Text.GetComponent<TextMesh>().text = name;
    }

    public void SetBackSize()
    {
        Bounds textBounds = Text.GetComponent<TextMesh>().GetComponent<Renderer>().bounds;
        Back.transform.localPosition = new Vector3(textBounds.size.x / 2, -textBounds.size.y / 2, 1);
        Back.transform.localScale = new Vector3(textBounds.size.x, textBounds.size.y, (float)0.1);
    }
}
