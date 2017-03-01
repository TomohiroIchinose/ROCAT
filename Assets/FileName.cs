using UnityEngine;
using System.Collections;

public class FileName : MonoBehaviour {

    public GameObject MainCamera;
    public GameObject Text;
    public GameObject Back;

    // Use this for insitialization
    void Start()
    {
        MainCamera = GameObject.Find("Main Camera");
        Text = this.transform.FindChild("Name").gameObject;
        Back = Text.transform.FindChild("Back").gameObject;
        if (this.transform.localPosition.y > 300)
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, 300, this.transform.localPosition.z);
    }

	// Update is called once per frame
	void Update ()
    {
        // メインカメラの方を向く
        this.transform.LookAt(MainCamera.transform);
        this.transform.Rotate(new Vector3(0, -180, 0));

        //Debug.Log(Vector3.Distance(this.transform.localPosition, MainCamera.transform.localPosition));
        // すごく遠いorすごく近いと非表示にする
        if (Vector3.Distance(this.transform.localPosition, MainCamera.transform.localPosition) >= 2000 || Vector3.Distance(this.transform.localPosition, MainCamera.transform.localPosition) <= 300)
        {
            Text.GetComponent<Renderer>().enabled = false;
            Back.GetComponent<Renderer>().enabled = false;
            //Text.GetComponent<TextMesh>().color = new Color(Text.GetComponent<TextMesh>().color.r, Text.GetComponent<TextMesh>().color.g, Text.GetComponent<TextMesh>().color.b, 0);
            //Back.GetComponent<MeshRenderer>().material.color = new Color(Back.GetComponent<MeshRenderer>().material.color.r, Back.GetComponent<MeshRenderer>().material.color.g, Back.GetComponent<MeshRenderer>().material.color.b, 0);
        }
        // 程々の遠さのときに表示する
        else
        {
            Text.GetComponent<Renderer>().enabled = true;
            Back.GetComponent<Renderer>().enabled = true;
            //Text.GetComponent<TextMesh>().color = new Color(Text.GetComponent<TextMesh>().color.r, Text.GetComponent<TextMesh>().color.g, Text.GetComponent<TextMesh>().color.b, 255);
            //Back.GetComponent<MeshRenderer>().material.color = new Color(Back.GetComponent<MeshRenderer>().material.color.r, Back.GetComponent<MeshRenderer>().material.color.g, Back.GetComponent<MeshRenderer>().material.color.b, 255);
        }
    }

    // テキスト部分に名前をセットする
    public void SetNameText(string name)
    {
        Text.GetComponent<TextMesh>().text = name;
    }

    // テキスト部分のサイズに応じて背景部分のサイズを変更する
    public void SetBackSize()
    {
        Bounds textBounds = Text.GetComponent<TextMesh>().GetComponent<Renderer>().bounds;
        Back.transform.localPosition = new Vector3(textBounds.size.x / 2, -textBounds.size.z / 2, 1);
        Back.transform.localScale = new Vector3(textBounds.size.x, textBounds.size.z, (float)0.1);
    }
}
