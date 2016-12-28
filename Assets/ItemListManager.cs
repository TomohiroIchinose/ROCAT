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

    void OnGUI () {

        // そのままだとループするのでcheckで1回だけ処理
        if (!check)
        {
            // jsonファイルのデータを取得
            string json = cc.GetJsonText();
            this.city = Json.Deserialize(json) as Dictionary<string, object>;
            var satd = this.city["satdfiles"] as IList;

            List<Dictionary<string, object>> arrangedList = new List<Dictionary<string, object>>();

            // IListからListへ
            foreach (Dictionary<string, object> contents in satd)
            {
                arrangedList.Add(contents);
            }

            // SATD数でソート
            arrangedList.Sort((b, a) => int.Parse(a["num"].ToString()) - int.Parse(b["num"].ToString()));

            for (int i = 0; i < arrangedList.Count; i++)
            {
                // リスト用のButtonを生成
                Button node = Instantiate(_itemNode) as Button;
                node.transform.localPosition = new Vector3(0f, 0f, 0f);
                node.transform.localScale = new Vector3(1f, 1f, 1f);
                node.transform.SetParent(_itemListContent.transform);

                // ノードの子オブジェクトを取得
                _itemName = node.transform.FindChild("FileName").GetComponent<Text>();
                _itemNum = node.transform.FindChild("Num").GetComponent<Text>();

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
                    // 値を設定
                    string text = arrangedList[i]["name"].ToString().Substring(arrangedList[i]["name"].ToString().IndexOf(".git") + 5);

                    _itemName.text = text;
                    _itemNum.text = arrangedList[i]["num"].ToString();

                    // クリック時のイベントを付加
                    string fullDir = arrangedList[i]["name"].ToString();
                    node.onClick.AddListener(() => ClickButton(fullDir));
                }
            }
            check = true;
        }
	}

    // クリックしたらCameraMoveのメソッドを呼び出して処理
    private void ClickButton(string filename)
    {
        // ディレクトリ名を渡す
        cm.SATDListClick(filename.Substring(1, filename.LastIndexOf("/")));
    }
	

}
