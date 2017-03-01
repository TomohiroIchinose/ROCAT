using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using System;

public class ItemListManager : MonoBehaviour {

    /// <summary>
    /// ItemList内コンテンツオブジェクト
    /// </summary>
    [SerializeField]
    private GameObject _itemListContent = null;

    /// <summary>
    /// ItemNode
    /// </summary>
    [SerializeField]
    private Button _itemNode = null;

    /// <summary>
    /// ItemNode内オブジェクト
    /// </summary>
    private Text _itemName = null;
    private Text _itemNum = null;

    public CityCreater cc;
    public CameraMove cm;

    public Dictionary<string, object> city;

    public bool check = false;
    public bool first = true;

    void Start()
    {
        cc = GameObject.Find("CityCreater").GetComponent<CityCreater>();
        cm = GameObject.Find("Main Camera").GetComponent<CameraMove>();
    }

    // Startと同時にするとJsonファイルを読めずにエラーになるのでOnGUIで動かす
    void OnGUI () {

        // そのままだとループするのでcheckで1回だけ処理
        if (!check)
        {
            // jsonファイルのデータを取得
            string json = cc.GetJsonText();
            this.city = Json.Deserialize(json) as Dictionary<string, object>;
            //this.city = cc.GetCityData();
            var satd = this.city["satdfiles"] as IList;

            List<Dictionary<string, object>> arrangedList = new List<Dictionary<string, object>>();

            // IListからListへ
            foreach (Dictionary<string, object> contents in satd)
            {
                arrangedList.Add(contents);
            }

            // SATD数でソート
            arrangedList.Sort((b, a) => int.Parse(a["num"].ToString()) - int.Parse(b["num"].ToString()));

            for (int i = 0; i < arrangedList.Count + 1; i++)
            {
                //Debug.Log(arrangedList[i]["name"]);
                
                // リスト用のButtonを生成
                Button node = Instantiate(_itemNode) as Button;
                node.transform.localPosition = new Vector3(0f, 0f, 0f);
                node.transform.localScale = new Vector3(1f, 1f, 1f);
                node.transform.SetParent(_itemListContent.transform);

                // ノードの子オブジェクトを取得
                _itemName = node.transform.FindChild("FileName").GetComponent<Text>();
                _itemNum = node.transform.FindChild("Num").GetComponent<Text>();

                // 一行目の項目は説明なので別処理
                if (first)
                {
                    _itemName.text = "Files containing SATD";
                    _itemName.color = Color.red;
                    _itemNum.text = "#SATD";
                    _itemNum.color = Color.red;
                    first = false;
                }
                else
                {
                    // 値を設定(iにすると最初の1個を飛ばしちゃうのでi-1でしている)
                    string text = "/" + arrangedList[i - 1]["name"].ToString().Substring(arrangedList[i - 1]["name"].ToString().IndexOf(".git") + 5);

                    _itemName.text = text;
                    _itemNum.text = arrangedList[i -1]["num"].ToString();


                    // クリック時のイベントを付加するための情報生成
                    string fullDir = arrangedList[i - 1]["name"].ToString();    // ファイルのフルパス

                    IList satdList = arrangedList[i - 1]["SATD"] as IList;
                    string satdListString = ":";

                    // SATDのある行番号を順番に並べていく
                    for (int j = 0; j < satdList.Count; j++)
                    {
                        satdListString = satdListString + (int.Parse(satdList[j].ToString()) + 1).ToString() + ",";
                    }
                    satdListString = satdListString.Substring(0, satdListString.Length - 1);

                    // 項目をクリックしたときにClickButtonメソッドが呼ばれるようにする
                    // fullDir+satdListString はビルのオブジェクトの名前と一致するようにしている
                    node.onClick.AddListener(() => ClickButton(fullDir + satdListString));
                }
            }
            check = true;
        }
	}

    // クリックしたらCameraMoveのメソッドを呼び出して処理
    private void ClickButton(string filename)
    {
        // ディレクトリ名を渡す
        //cm.SATDListClick(filename.Substring(1, filename.LastIndexOf("/")));

        // フルパスを渡す
        cm.SATDListClick(filename.Substring(1));
    }


}
