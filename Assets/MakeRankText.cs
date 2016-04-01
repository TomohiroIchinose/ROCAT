﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using MiniJSON;
using System.Collections.Generic;
using System;
using System.Text;

public class MakeRankText : MonoBehaviour {

    private string TARGET;           // jsonファイルのパスを入れる変数
    private string jsonText = "";    // jsonファイルの中身を入れる変数
    private Dictionary<string, object> jsonDictionary; // jsonファイルの中身を辞書にしたもの

    // Use this for initialization
    void Start () {

        this.GetComponent<Text>().text = "";

        // TARGETに指定したjsonファイルの中身を読みだす
        TARGET = Application.dataPath + "/target/test.json";
        ReadFile();

        MakeRankWindow();

    }
	
	// Update is called once per frame
	void Update () {
	
	}


    // ランキング用のテキストを作る関数
    void MakeRankWindow()
    {
        this.GetComponent<Text>().text = "★STADランキング★\n";

        // jsonファイルの中身を辞書にする
        this.jsonDictionary = Json.Deserialize(jsonText) as Dictionary<string, object>;

        // jsonDictionaryのrankの部分をリストにする
        var rankData = this.jsonDictionary["rank"] as IList;

        // おなまえごとの辞書にする
        Dictionary<String, List<Dictionary<String, object>>> rankDictionary = ArrangeByKey(rankData, "name");


        foreach (String person in rankDictionary.Keys)
        {
            this.GetComponent<Text>().text += person;

            // 1人ごとのデータのリストを取ってくる
            List<Dictionary<String, object>> onePersonData = rankDictionary[person];

            // 1人ごとのデータのリストから1つずつデータを見ていく
            foreach (Dictionary<String, object> oneData in onePersonData)
            {
                this.GetComponent<Text>().text += "   " + oneData["num"].ToString();
            }

            this.GetComponent<Text>().text += "\n";

        }
        //Debug.Log("%%%");
    }


    // jsonファイルの中身を読みだす関数
    void ReadFile()
    {
        FileInfo file = new FileInfo(TARGET);
        try
        {
            using (StreamReader sr = new StreamReader(file.OpenRead(), Encoding.UTF8))
            {
                jsonText = sr.ReadToEnd();
            }
        }
        catch (Exception e)
        {
            jsonText += SetDefaultText();
        }
    }



    // jsonファイルが読みだせなかったときの対応用の関数
    string SetDefaultText()
    {
        return "cant read\n";
    }


    // リストの中身をおなまえをキーにした辞書にする
    Dictionary<String, List<Dictionary<String, object>>> ArrangeByKey(IList target, String key)
    {
        Dictionary<String, List<Dictionary<String, object>>> arrangedTarget = new Dictionary<String, List<Dictionary<String, object>>>();

        // リストから1つずつとってきて辞書に入れていく
        foreach (Dictionary<string, object> contents in target)
        {
            // おなまえを取得
            String contentsName = contents[key].ToString();
            //Debug.Log(contentsName);
            if (arrangedTarget.ContainsKey(contentsName))
            {
                // 既に辞書に存在する場合はaddだけする
                arrangedTarget[contentsName].Add(contents);
            }
            else
            {
                // 初めて出てきたらnewして辞書にaddする
                arrangedTarget[contentsName] = new List<Dictionary<String, object>>();
                arrangedTarget[contentsName].Add(contents);
            }
        }

        return arrangedTarget;
    }
}
