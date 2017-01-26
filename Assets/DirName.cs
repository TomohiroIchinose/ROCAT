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

        float distance = Vector3.Distance(new Vector3(this.transform.localPosition.x, 400, this.transform.localPosition.z), MainCamera.transform.localPosition);

        //Debug.Log(Vector3.Distance(this.transform.localPosition, MainCamera.transform.localPosition));
        if (distance <= 600)
        {
            //Text.GetComponent<Renderer>().enabled = false;
            //Back.GetComponent<Renderer>().enabled = false;

            this.transform.localScale = new Vector3((float)0.5, (float)0.5, (float)0.5);
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, 100, this.transform.localPosition.z);

            //Text.GetComponent<TextMesh>().color = new Color(Text.GetComponent<TextMesh>().color.r, Text.GetComponent<TextMesh>().color.g, Text.GetComponent<TextMesh>().color.b, 0);
            //Back.GetComponent<MeshRenderer>().material.color = new Color(Back.GetComponent<MeshRenderer>().material.color.r, Back.GetComponent<MeshRenderer>().material.color.g, Back.GetComponent<MeshRenderer>().material.color.b, 0);
        }
        else
        {
            //Text.GetComponent<Renderer>().enabled = true;
            //Back.GetComponent<Renderer>().enabled = true;

            if (distance >= 4000)
            {
                this.transform.localScale = new Vector3(5, 5, 5);
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, 800, this.transform.localPosition.z);
            }
            else if (distance >= 1500)
            {
                this.transform.localScale = new Vector3(3, 3, 3);
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, 400, this.transform.localPosition.z);
            }
            else
            {
                this.transform.localScale = new Vector3(1, 1, 1);
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
