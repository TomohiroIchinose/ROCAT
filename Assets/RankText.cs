using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using MiniJSON;
using System.Collections.Generic;
using System;
using System.Text;

public class RankText : MonoBehaviour {

    private string TARGET;           // jsonファイルのパスを入れる変数
    private string jsonText = "";    // jsonファイルの中身を入れる変数
    private Dictionary<string, object> jsonDictionary; // jsonファイルの中身を辞書にしたもの
    public GameObject rankView;
    private bool view = true;

    // Use this for initialization
    void Start () {

        #if UNITY_EDITOR
                StartRanking("2acra");
        #else
			    Application.ExternalCall("OnUnityReady");
        #endif

        //this.GetComponent<Text>().text = "start";

        // TARGETに指定したjsonファイルの中身を読みだす
        //TARGET = Application.dataPath + "/target/test.json";
        //ReadFile();

        // MakeRankWindow();

    }
	
	// Update is called once per frame
	void Update () {
        ControlByKeyboard();

    }


    // ランキング用のテキストを作る関数
    void MakeRankWindow()
    {
        this.GetComponent<Text>().text = "★STAD除去数ランキング★\n";

        // jsonファイルの中身を辞書にする
        this.jsonDictionary = Json.Deserialize(jsonText) as Dictionary<string, object>;

        // jsonDictionaryのrankの部分をリストにする
        var rankData = this.jsonDictionary["SATDRanking"] as IList;

        // おなまえごとの辞書にする
        Dictionary<String, List<Dictionary<String, object>>> rankDictionary = ArrangeByKey(rankData, "name");


        // ソート用におなまえとnum（除去数？）だけの辞書を作る
        Dictionary<string, int> sortDic = new Dictionary<string, int>();
        foreach (String person in rankDictionary.Keys)
        {
            // 1人ごとのデータのリストを取ってくる
            List<Dictionary<String, object>> onePersonData = rankDictionary[person];

            // 1人ごとのデータのリストから1つずつデータを見ていく
            foreach (Dictionary<String, object> oneData in onePersonData)
            {
                sortDic.Add(person, int.Parse(oneData["num"].ToString())); // 辞書に追加
            }
        }

        // ソート用の辞書をリストに変換して降順にソートする
        List<KeyValuePair<string, int>> print = new List<KeyValuePair<string, int>>(sortDic);
        print.Sort(CompareKeyValuePair);


        // ランキングのテキストを作る
        int rank = 1;
        foreach(KeyValuePair<string, int> pair in print)
        {
            this.GetComponent<Text>().text += rank.ToString() + "位  " +  pair.Key.ToString() + "さん   " + pair.Value.ToString() + "個\n";
            rank++;
        }


        /* jsonファイルの順に表示させるパターン */
        //foreach (String person in rankDictionary.Keys)
        //{
        //    this.GetComponent<Text>().text += person;
        //
        //    1人ごとのデータのリストを取ってくる
        //    List<Dictionary<String, object>> onePersonData = rankDictionary[person];
        //
        //    1人ごとのデータのリストから1つずつデータを見ていく
        //    foreach (Dictionary<String, object> oneData in onePersonData)
        //    {
        //       this.GetComponent<Text>().text += "   " + oneData["num"].ToString();
        //    }
        //
        //    this.GetComponent<Text>().text += "\n";
        //
        //}
        //Debug.Log("%%%");
    }

    /*
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
    */



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


    // 二つのKeyValuePair<string, int>を比較するためのメソッド
    static int CompareKeyValuePair(KeyValuePair<string, int> x, KeyValuePair<string, int> y)
    {
        // Valueで比較した結果を返す
        
        //降順
        return -x.Value.CompareTo(y.Value); 

        //昇順
        //return -x.Value.CompareTo(y.Value);

        // 単に次のようにしても同じ
        // return x.Value - y.Value;
    }


    // Javascriptから街を作り始めるためのメソッド
    public void StartRanking(string id)
    {
        this.GetComponent<Text>().text = "call";
        StartCoroutine(ReadFileOnline(id));
    }

    // サーバにあるJsonファイルを読み込むメソッド
    IEnumerator ReadFileOnline(string id)
    {

        //string url = "http://kataribe-dev.naist.jp:802/public/code_city.json?id=" + id;
        string url = "http://163.221.29.246/json/" + id + ".json";

        WWW www = new WWW(url);
        yield return www;

        if (www.error == null)
        {
            jsonText = www.text;
        }
        else {
            jsonText = SetDefaultText();
        }


        //Camera.main.GetComponent<CameraMove>().isControlAvailable = true;
        this.GetComponent<Text>().text = "read";
        MakeRankWindow();

    }

    // キーボード操作
    private void ControlByKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            rankView.GetComponent<Canvas>().enabled = !rankView.GetComponent<Canvas>().enabled;
        }
    }


}
