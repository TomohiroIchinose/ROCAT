﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UpperButton : MonoBehaviour {

    public CityCreater cc;

    // Use this for initialization
    void Start () {
        cc = GameObject.Find("CityCreater").GetComponent<CityCreater>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    // ボタンクリック時に都市中央にある土台をクリックした時と同じ動作をする
    public void OnClick()
    {
        //Debug.Log(cc.GetRootName() + this.transform.root.gameObject.GetComponentInChildren<Text>().text.Substring(0, this.transform.root.gameObject.GetComponentInChildren<Text>().text.LastIndexOf("/")));
        cc.RemakeCity(cc.GetRootName() + this.transform.root.gameObject.GetComponentInChildren<Text>().text.Substring(0, this.transform.root.gameObject.GetComponentInChildren<Text>().text.LastIndexOf("/")), true);        
    }
}
