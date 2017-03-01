using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using MiniJSON;


public class CityCreater : MonoBehaviour
{

	public GameObject building;     // ビル
	public Dictionary<string,object> city;  // Jsonファイルの中身の辞書

    public GameObject CircleBlock;   // 土台

    public GameObject earth;         // 地面

    public GameObject fire;          // 炎

    public GameObject street;        // 道

    private CameraMove mainCamera;  // メインカメラ

    public GameObject DirNamePlate;     // 土台のお名前カンバン
    public GameObject FileNamePlate;    // ビルのお名前カンバン

    public string jsonText = "";        // Jsonファイルの中身
    // Use this for initialization

    public List<String> dir = new List<String>(); // ディレクトリ一覧

    public Sensor sensor;               // レーダー

    public String rootDirName;          // ルートディレクトリのお名前
    public List<String> firstDirNameList = new List<String>();      // 周囲に置かれるディレクトリのお名前のリスト

    

    public Dictionary<String, List<Dictionary<String, object>>> firstBlockDictionary2;  // 周囲に置かれる土台の辞書

    public Dictionary<String, List<Dictionary<String, object>>> allDirectory = new Dictionary<string, List<Dictionary<string, object>>>();  // 全てのディレクトリの辞書

    Dictionary<String, List<Dictionary<String, object>>> arrangedBuildings;
    public String currentRoot;          // 現在中心にあるディレクトリのお名前


    public List<String> satdFilesList = new List<String>(); // SATDのあるファイルのリスト

    void Start()
    {
        sensor = GameObject.Find("Main Camera").GetComponent<Sensor>();
        earth = Instantiate(this.earth, new Vector3(0, 0, 0), transform.rotation) as GameObject;

// Unity以外で動かしたときにHTMLファイルのJavascriptを呼び出すようにする

#if UNITY_EDITOR
        //StartCityCreater("acra");
        //StartCityCreater("redis-py");
        //StartCityCreater("android-swipelistview");
        //StartCityCreater("activeadmin");
        //StartCityCreater("Activiti");
        //StartCityCreater("histrage");
        //StartCityCreater("lamtram");
        //StartCityCreater("test");
        //StartCityCreater("travatar");
        //StartCityCreater("cdec");
        //StartCityCreater("tensorflow");
        StartCityCreater("dynet");
        //StartCityCreater("discourse");
        //StartCityCreater("crawlers");
#else
			    Application.ExternalCall("OnUnityReady");
#endif
    }  

    // 都市を作っていく
    void CreateCity ()
	{
        // 最初にJsonファイルを読み込む
		this.city = Json.Deserialize (jsonText) as Dictionary<string,object>;
		var blocks = this.city ["blocks"] as IList;
		var buildings = this.city ["buildings"] as IList;
        var directories = this.city["directories"] as IList;

        var rootDir = this.city["root_depth"] as IList;
        var firstDir = this.city["first_depth"] as IList;

        var satdfiles = this.city["satdfiles"] as IList;

        // ディレクトリ一覧を作ってソートしておく
        for(int i=0; i< directories.Count; i++)
        {
            dir.Add(directories[i].ToString());
        }

        dir.Sort();

        // ディレクトリ一覧を辞書化
        foreach(String name in dir)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("name", name);
            allDirectory[name] = new List<Dictionary<String, object>>();
            allDirectory[name].Add(data);
        }

        // rootディレクトリの名前を取得しておく
        foreach (Dictionary<string, object> content in rootDir)
        {
            rootDirName = content["name"].ToString();
        }

        // SATDのあるファイルのリストを作っておく
        foreach(Dictionary<string, object> content in satdfiles)
        {
            satdFilesList.Add(content["name"].ToString());
            //Debug.Log(content["name"].ToString());
        }

        // 土台ごとにビルをまとめる
        arrangedBuildings = ArrangeByKey(buildings, "block");

        // 土台やビルを建てていく
        LocateBlockAndBuilding2(allDirectory, arrangedBuildings, rootDirName);
	}

	
    // 土台やビルのオブジェクトを立てていく関数
    // 再構成もこの関数を使う
    void LocateBlockAndBuilding2(Dictionary<string, List<Dictionary<string, object>>> blocks, Dictionary<String, List<Dictionary<String, object>>> buildings, String root)
    {
        // 現在のrootディレクトリ（中心にあるディレクトリ）を更新
        currentRoot = root;
        
        // rootに指定したディレクトリ下にあるディレクトリの一覧を作成する
        List<String> firstDirList = SearchFirstDirectory(root, dir);

        // rootに指定したディレクトリ用の辞書を作る
        List<String> tempRootList = new List<string>();
        tempRootList.Add(root);
        Dictionary<String, List<Dictionary<String, object>>> rootBlockDictionary = SearchDirectoryDictionary(blocks, tempRootList);

        // rootに指定したディレクトリ直下のディレクトリの辞書を作る
        firstBlockDictionary2 = SearchDirectoryDictionary(blocks, firstDirList);


        // ビルを並べて土台のサイズを決める
        int edge = 0;
        // rootに指定した土台の処理
        if (buildings.ContainsKey(root))
        {
            edge = SetBuildingLocation(buildings[root]);
        }

        // 円の半径を決める
        if (edge != 0)
            rootBlockDictionary[root][0]["radius"] = (edge) * Mathf.Sqrt(2) / 2 * 100;  // 1辺に置かれるビルの個数から土台の半径を決める
        else
            rootBlockDictionary[root][0]["radius"] = 100;                               // ビルが置かれない場合は固定値


        // 周囲の土台の処理
        foreach (String key in firstBlockDictionary2.Keys)
        {
            //Debug.Log("***"+key);
            edge = 0;

            // ビルを配置して1辺の個数を求める
            if (buildings.ContainsKey(key))
            {
                edge = SetBuildingLocation(buildings[key]);
            }
            //Debug.Log(edge);

            // 円の半径を決める
            if (edge != 0)
                firstBlockDictionary2[key][0]["radius"] = (edge) * Mathf.Sqrt(2) / 2 * 100;
            else
                firstBlockDictionary2[key][0]["radius"] = 100;
        }

        // 土台の配置を決める
        SetBlockCircleLocation2(rootBlockDictionary, firstBlockDictionary2, root);

        // 今あるオブジェクトをdestroyしていく
        ObjectDestroyer("Block");
        ObjectDestroyer("SATDBuilding");
        ObjectDestroyer("NormalBuilding");
        ObjectDestroyer("Fire");
        ObjectDestroyer("Street");
        //ObjectDestroyer("Ground");
        ObjectDestroyer("enemy");
        ObjectDestroyer("Plate");

        // 土台の上にビルが置けるようにビルの座標を決定する
        SetGlobalCircleLocation2(buildings, rootBlockDictionary, firstBlockDictionary2, root);


        // ここからビルと土台を建てる（オブジェクトを生成していく）

        // 中心のディレクトリの土台
        GameObject rootCircle = Instantiate(this.CircleBlock, new Vector3(float.Parse(rootBlockDictionary[root][0]["x"].ToString()), 3, float.Parse(rootBlockDictionary[root][0]["z"].ToString())), transform.rotation) as GameObject;
        rootCircle.transform.localScale = new Vector3(float.Parse(rootBlockDictionary[root][0]["radius"].ToString()) * 2, (float)2, float.Parse(rootBlockDictionary[root][0]["radius"].ToString()) * 2);
        //rootCircle.name = "Circle " + rootDirName;
        rootCircle.name = root.Substring(1);
        rootCircle.tag = "Block";

        rootCircle.GetComponent<Block>().SetMaterial(Color.cyan);

        // 中心のディレクトリの土台のメタ情報
        var rootData = rootCircle.GetComponent<BlockData>();
        rootData.pathname = root.Substring(1);
        rootData.center = true;

        if (root == rootDirName)
            rootData.blockname = "root";
        else
            rootData.blockname = root.Substring(rootDirName.Length);

        rootData.end = false;


        // 周囲の土台を置く
        foreach (String key in firstBlockDictionary2.Keys)
        {
            GameObject firstCircle = Instantiate(this.CircleBlock, new Vector3(float.Parse(firstBlockDictionary2[key][0]["x"].ToString()), 3, float.Parse(firstBlockDictionary2[key][0]["z"].ToString())), transform.rotation) as GameObject;
            firstCircle.transform.localScale = new Vector3(float.Parse(firstBlockDictionary2[key][0]["radius"].ToString()) * 2, (float)2, float.Parse(firstBlockDictionary2[key][0]["radius"].ToString()) * 2);

            //firstCircle.name = "Circle " + key;
            firstCircle.name = key.Substring(1);
            firstCircle.tag = "Block";

            // メタ情報
            var circleData = firstCircle.GetComponent<BlockData>();
            circleData.pathname = key.Substring(1);
            circleData.blockname = key.Substring(key.ToString().LastIndexOf("/") + 1);
            circleData.center = false;
            

            // その先のディレクトリ（サブディレクトリ）がないか調べる
            bool end = true;
            foreach(String name in dir)
            {
                if (name.Contains("/" + circleData.pathname + "/"))
                    end = false;
            }
            circleData.end = end;

            // その先にディレクトリがない場合は色を灰色に変える
            if (end)
                firstCircle.GetComponent<Block>().SetMaterial(Color.gray);

            // 土台の先にSATDがあるなら土台を黄色にする
            foreach (String name in satdFilesList)
            {
                
                if (name.Contains("/" + circleData.pathname + "/") && !end && "/" + circleData.pathname != name.Substring(0, name.LastIndexOf(name.Substring(name.LastIndexOf("/")))))
                {
                    circleData.insideSATD = true;
                    firstCircle.GetComponent<Block>().SetMaterial(Color.yellow);

                    break;
                }
            }


            // お名前カンバンを付ける
            GameObject dirtext = Instantiate(this.DirNamePlate, new Vector3(float.Parse(firstBlockDictionary2[key][0]["x"].ToString()), 200, float.Parse(firstBlockDictionary2[key][0]["z"].ToString())), transform.rotation) as GameObject;
            dirtext.GetComponent<DirName>().SetNameText(circleData.blockname);
            dirtext.GetComponent<DirName>().SetBackSize();
            dirtext.tag = "Plate";
        }


        // ビルを建てる
        foreach (String key in buildings.Keys)
        {

            if (key == root || firstDirList.Contains(key))
            {
                List<Dictionary<String, object>> buildingList = buildings[key];
                foreach (Dictionary<String, object> oneBuilding in buildingList)
                {

                    //GameObject temp = Instantiate(this.checkSATD, new Vector3(float.Parse(oneBuilding["globalX"].ToString()), 2 + float.Parse(oneBuilding["height"].ToString()) / 2, float.Parse(oneBuilding["globalZ"].ToString())), transform.rotation) as GameObject;
                    GameObject buildingObj = Instantiate(this.building, new Vector3(float.Parse(oneBuilding["globalX"].ToString()), 4, float.Parse(oneBuilding["globalZ"].ToString())), Quaternion.Euler(-90, 0, 0)) as GameObject;
                    //buildingObj.transform.localScale = new Vector3(50, float.Parse(oneBuilding["height"].ToString()), 50);
                    buildingObj.transform.localScale = new Vector3(5, 5, float.Parse(oneBuilding["height"].ToString()) * (float)0.1);
                    //buildingObj.name = "CircleBuilding" + oneBuilding["name"];

                    buildingObj.name = (oneBuilding["path"].ToString() + ":").Substring(1);
                    buildingObj.tag = "NormalBuilding";
                    buildingObj.layer = LayerMask.NameToLayer("Building");

                    // メタ情報
                    var buildingData = buildingObj.GetComponent<BuildingData>();
                    buildingData.filename = oneBuilding["name"].ToString();
                    buildingData.fullpath = oneBuilding["path"].ToString();
                    buildingData.pathname = buildingData.fullpath.Substring(rootDirName.Length);
                    buildingData.loc = int.Parse(oneBuilding["height"].ToString()) - 1;
                    buildingData.comment = (int.Parse(oneBuilding["widthX"].ToString()) - 1) / 10;
                   

                    // SATDがある場合だけ処理を増やす
                    IList sList = oneBuilding["SATD"] as IList;
                    if (sList.Count != 0)
                    {

                        // SATDあるビルだけお名前カンバン
                        GameObject filetext = Instantiate(this.FileNamePlate, new Vector3(float.Parse(oneBuilding["globalX"].ToString()), float.Parse(oneBuilding["height"].ToString()) / 2, float.Parse(oneBuilding["globalZ"].ToString())), Quaternion.Euler(-90, 0, 0)) as GameObject;
                        filetext.GetComponent<FileName>().SetNameText(buildingData.filename);
                        filetext.GetComponent<FileName>().SetBackSize();
                        filetext.tag = "Plate";

                        buildingObj.tag = "SATDBuilding";
                        buildingObj.GetComponent<Building>().SetMaterial(Color.blue);

                        for (int i = 0; i < sList.Count; i++)
                        {
                            buildingData.satd.Add(int.Parse(sList[i].ToString()) + 1);

                            // ビルのオブジェクトの名前を修正する
                            buildingObj.name = buildingObj.name + (int.Parse(sList[i].ToString()) + 1).ToString() + ",";


                            // 炎の目印を作る
                            GameObject particle = Instantiate(this.fire, new Vector3(0, 1, 0), transform.rotation) as GameObject;
                            var r = particle.GetComponent<ParticleSystem>().shape;
                            r.radius = 50;

                            var s = particle.GetComponent<ParticleSystem>();

                            s.startSize = 25;

                            s.startSpeed = 50;

                            particle.transform.Rotate(new Vector3((float)270, (float)0, (float)0));
                            particle.transform.position = new Vector3(float.Parse(oneBuilding["globalX"].ToString()), (float.Parse(oneBuilding["height"].ToString()) - float.Parse(sList[i].ToString())) * (float)0.8845 + 3, float.Parse(oneBuilding["globalZ"].ToString()));
                            particle.name = "sence:" + oneBuilding["name"] + (int.Parse(sList[i].ToString())).ToString();
                            particle.layer = LayerMask.NameToLayer("Building");
                            particle.tag = "Fire";

                        }
                        buildingObj.name = buildingObj.name.Substring(0, buildingObj.name.Length - 1);
                    }
                }
            }
        }



        // 道を作る
        BuildCircleStreet2(rootBlockDictionary, firstBlockDictionary2, root);

        // 地面を作ってカメラをリスタート
        SetCircleGround2(rootBlockDictionary, firstBlockDictionary2, root);

        // レーダー更新
        sensor.MakeSensorList();
    }


    // 指定したタグのオブジェクトを破壊していくメソッド
    void ObjectDestroyer(String tagName)
    {
        GameObject[] tagobjs = GameObject.FindGameObjectsWithTag(tagName);

        foreach(GameObject obj in tagobjs)
        {
            GameObject.DestroyImmediate(obj);
        }
    }


    
    // rootに指定したディレクトリ下にあるディレクトリを探す
    List<String> SearchFirstDirectory(String root, List<String> dirs)
    {
        List<String> result = new List<string>();

        //Debug.Log("#" + root);

        foreach(String name in dirs)
        {
            if(name.Contains(root + "/") && CountChar(name.Substring(root.Length), '/') == 1)
            {
                result.Add(name);
                //Debug.Log(name);
            }
        }


        return result;
    }


    // リストにある名前と一致するディレクトリの辞書を返す
    Dictionary<String, List<Dictionary<String, object>>> SearchDirectoryDictionary(Dictionary<string, List<Dictionary<string, object>>> target, List<String> names)
    {
        Dictionary<String, List<Dictionary<String, object>>> result = new Dictionary<string, List<Dictionary<String, object>>>();

        foreach(String name in names)
        {
            //Debug.Log("$"+ name);
            foreach (String key in target.Keys)
            {
                if(target[key][0]["name"].ToString() == name)
                {
                    if (result.ContainsKey(key))
                    {
                        // 既に辞書に存在する場合はaddだけする
                        result[name].Add(target[key][0]);
                    }
                    else
                    {
                        // 初めて出てきたらnewして辞書にaddする
                        result[name] = new List<Dictionary<String, object>>();
                        result[name].Add(target[key][0]);
                    }
                }
            }
        }
        return result;
    }
    

    // 周りの土台を大きさ順に並べた時のお名前のリストを返す
    List<String> SortFirstBlockName(List<String> nameList, Dictionary<String, List<Dictionary<String, object>>> blockList)
    {
        for(int i = 0; i < nameList.Count; i++)
        {
            for(int j = nameList.Count - 1; j > i; j--)
            {
                if(float.Parse(blockList[nameList[j]][0]["radius"].ToString()) > float.Parse(blockList[nameList[j - 1]][0]["radius"].ToString()))
                {
                    String temp = nameList[j];
                    nameList[j] = nameList[j - 1];
                    nameList[j - 1] = temp;
                }
            }
        }
        return nameList;
    }

	/**
	 * 
     * keyの値ごとにtargetをまとめるメソッド
     * 用途1：ビルを属する土台ごとにまとめる
	 * 用途2：土台を名前ごとにまとめる
	 *
	 */
	Dictionary<String,List<Dictionary<String, object>>> ArrangeByKey (IList target, String key)
	{
		Dictionary<String,List<Dictionary<String, object>>> arrangedTarget = new Dictionary<string, List<Dictionary<String, object>>>();
		
		foreach (Dictionary<string,object> contents in target) {
			String contentsName = contents[key].ToString();
			if(arrangedTarget.ContainsKey(contentsName))
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



    // ビルを正方形っぽく並べる
    int SetBuildingLocation(List<Dictionary<String, object>> target)
    {
        //target.Sort((b, a) => string.Compare(b["block"].ToString(), a["block"].ToString()));
        // 土台名で比較して同じだったらファイル名で比較してソート
        target.Sort((b, a) => string.Compare(b["block"].ToString(), a["block"].ToString()) != 0 ? string.Compare(b["block"].ToString(), a["block"].ToString()) : string.Compare(b["name"].ToString(), a["name"].ToString()));

        int edgeNum = 0;
        for (int i = 0; ; i++)
        {
            // ビル数の平方根より大きい整数値を一辺の個数にする
            if (i * i > target.Count)
            {
                edgeNum = i;
                break;
            }
        }

        int z = 0;
        int x = 0;

        // ビルの座標を決めていく
        for(int i = 0; i < target.Count; i++)
        {
            target[i]["x"] = 100 * x;
            target[i]["z"] = 100 * z;

            //Debug.Log(target[i]["x"] + "," + target[i]["z"]);

            // 右に1個進む
            x++;

            // 右端まで行ったら0に戻す
            if (x >= edgeNum)
            {
                x = 0;

                // 下に1個進む
                z++;
            }       
        }

        // 1辺の長さ（正方形の辺に置くビルの個数）
        return edgeNum;
    }


    // 土台を円形に配置していく（最適解ではないのでご注意を）
    void SetBlockCircleLocation2(Dictionary<String, List<Dictionary<String, object>>> root, Dictionary<String, List<Dictionary<String, object>>> first, String rootName)
    {
        //firstDirNameList

        // 中央にrootを置く
        root[rootName][0]["x"] = 0;
        root[rootName][0]["z"] = 0;


        // 角度
        float deg = 360 / (float)first.Count;
        double rad = deg * Mathf.PI / 180.0;

        float max = 0;
        float min = 0;

        Boolean start = true;

        List<String> keyList = new List<string>();

        // 一番大きい・小さい半径を取得する
        foreach (String key in first.Keys)
        {
            if (start)
            {
                max = float.Parse(first[key][0]["radius"].ToString());
                min = float.Parse(first[key][0]["radius"].ToString());
                start = false;
            }
            else {
                if (float.Parse(first[key][0]["radius"].ToString()) >= max)
                    max = float.Parse(first[key][0]["radius"].ToString());

                if (float.Parse(first[key][0]["radius"].ToString()) <= min)
                    min = float.Parse(first[key][0]["radius"].ToString());
            }
            keyList.Add(key);
        }

        
        // 1階層目の座標を決めていく
        int radnumber = 0;

        // 周りの土台が4つ以下なら各土台の直径分離せば重ならない
        if (first.Count <= 4)
        {
            foreach (String key in first.Keys)
            {
                first[key][0]["x"] = Mathf.Cos((float)rad * radnumber) * (float.Parse(root[rootName][0]["radius"].ToString()) * 2 + float.Parse(first[key][0]["radius"].ToString()) * 2);
                first[key][0]["z"] = Mathf.Sin((float)rad * radnumber) * (float.Parse(root[rootName][0]["radius"].ToString()) * 2 + float.Parse(first[key][0]["radius"].ToString()) * 2);
                radnumber++;
            }
        }
        else
        {
            // お名前のリストを半径順にする
            keyList = SortFirstBlockName(keyList, first);

            // お名前のリストの要素数
            int listSize = keyList.Count;
            int[] nameOrder = new int[listSize];


            // 大小大小…となるようにお名前のリストの添字を並べていく
            int number = 0;
            int num2 = (int)Math.Ceiling((double)listSize / 2);

            for (int i = 0; i < listSize; i += 2)
            {
                nameOrder[i] = number;

                if (i + 1 < listSize)
                    nameOrder[i + 1] = num2 + number;

                number++;
            }

            radnumber = 0;
            int plusMinus = 1;
            for (int i = 0; i < first.Count; i++)
            {
                //Debug.Log(first[keyList[nameOrder[i]]][0]["name"]);

                float minDistance = float.Parse(root[rootName][0]["radius"].ToString()) * 2 + float.Parse(first[keyList[nameOrder[i]]][0]["radius"].ToString()) * 2;
                float distance = float.Parse(root[rootName][0]["radius"].ToString()) * 2 + (max * (2 + plusMinus * 1 + (max - min) * (plusMinus + 1) / 100000) +  float.Parse(first[keyList[nameOrder[i]]][0]["radius"].ToString()) * (2 + first.Count / 2 + plusMinus * 15 * (max - min) / 10000 - (max - min) / 100));

                // 差分のマイナスが大きすぎる場合はルートの直径＋その土台の直径分離す
                if (distance < minDistance)
                {
                    first[keyList[nameOrder[i]]][0]["x"] = Mathf.Cos((float)rad * radnumber) * minDistance;
                    first[keyList[nameOrder[i]]][0]["z"] = Mathf.Sin((float)rad * radnumber) * minDistance;
                }

                // 土台の半径の最大・最小値の差が大きいほど・土台の個数が大きいほど外に向かう 差が大きいほど内側と外側の差が広がる
                // でも正直結構テキトー。偉い数学の先生に最適解を聞いた方がよさそう
                else
                {
                    first[keyList[nameOrder[i]]][0]["x"] = Mathf.Cos((float)rad * radnumber) * distance;
                    first[keyList[nameOrder[i]]][0]["z"] = Mathf.Sin((float)rad * radnumber) * distance;
                }
                plusMinus *= -1;
                radnumber++;
            }
        }
    }


    // 決めておいたビルの配置を実際の土台の上に合わせていく
    void SetGlobalCircleLocation2(Dictionary<String, List<Dictionary<String, object>>> building, Dictionary<String, List<Dictionary<String, object>>> rootBlock, Dictionary<String, List<Dictionary<String, object>>> firstBlock, String root)
    {
        // 土台ごとに決めていく
        foreach (String key in building.Keys)
        {
            float blockX = 0;
            float blockZ = 0;
            float radius = 0;

            // 中心の土台の場合
            if (key == root)
            {
                blockX = float.Parse(rootBlock[root][0]["x"].ToString());
                blockZ = float.Parse(rootBlock[root][0]["z"].ToString());
                radius = float.Parse(rootBlock[root][0]["radius"].ToString());
            }
            // 周囲の土台の場合
            else if(firstBlock.ContainsKey(key))
            {
                blockX = float.Parse(firstBlock[key][0]["x"].ToString());
                blockZ = float.Parse(firstBlock[key][0]["z"].ToString());
                radius = float.Parse(firstBlock[key][0]["radius"].ToString());
            }

            List<Dictionary<String, object>> buildingList = building[key];

            // ビルの座標を一つ一つ順に決めていく
            foreach (Dictionary<String, object> oneBuilding in buildingList)
            {
                if(firstBlock.ContainsKey(key) || key == root)
                {
                    oneBuilding["globalX"] = float.Parse(blockX.ToString()) - Mathf.Sqrt(2) / 2 * radius + 50 + float.Parse(oneBuilding["x"].ToString());
                    oneBuilding["globalZ"] = float.Parse(blockZ.ToString()) - Mathf.Sqrt(2) / 2 * radius + 50 + float.Parse(oneBuilding["z"].ToString());
                }
            }
        }
    }
            
   
    // 地面を作ってメインカメラをスタートさせる
    void SetCircleGround2(Dictionary<String, List<Dictionary<String, object>>> rootBlock, Dictionary<String, List<Dictionary<String, object>>> firstBlock, String root)
    {
        float maxX = float.Parse(rootBlock[root][0]["x"].ToString()) + float.Parse(rootBlock[root][0]["radius"].ToString());
        float minX = float.Parse(rootBlock[root][0]["x"].ToString()) - float.Parse(rootBlock[root][0]["radius"].ToString());

        float maxZ = float.Parse(rootBlock[root][0]["z"].ToString()) + float.Parse(rootBlock[root][0]["radius"].ToString());
        float minZ = float.Parse(rootBlock[root][0]["z"].ToString()) - float.Parse(rootBlock[root][0]["radius"].ToString()); ;

        string maxXKey = root;
        string minXKey = root;

        string maxZKey = root;
        string minZKey = root;

        // X軸とZ軸が一番大きい＆一番小さい土台を調べて地面がそれより外側に存在するようにする
        foreach (String key in firstBlock.Keys)
        {
            if (maxX < float.Parse(firstBlock[key][0]["x"].ToString()) + float.Parse(firstBlock[key][0]["radius"].ToString()))
            {
                maxX = float.Parse(firstBlock[key][0]["x"].ToString()) + float.Parse(firstBlock[key][0]["radius"].ToString());
                maxXKey = key;
            }

            if (maxZ < float.Parse(firstBlock[key][0]["z"].ToString()) + float.Parse(firstBlock[key][0]["radius"].ToString()))
            {
                maxZ = float.Parse(firstBlock[key][0]["z"].ToString()) + float.Parse(firstBlock[key][0]["radius"].ToString());
                maxZKey = key;
            }

            if (minX > float.Parse(firstBlock[key][0]["x"].ToString()) - float.Parse(firstBlock[key][0]["radius"].ToString()))
            {
                minX = float.Parse(firstBlock[key][0]["x"].ToString()) - float.Parse(firstBlock[key][0]["radius"].ToString());
                minXKey = key;
            }

            if (minZ > float.Parse(firstBlock[key][0]["z"].ToString()) - float.Parse(firstBlock[key][0]["radius"].ToString()))
            {
                minZ = float.Parse(firstBlock[key][0]["z"].ToString()) - float.Parse(firstBlock[key][0]["radius"].ToString());
                minZKey = key;
            }
        }
        //GameObject ground = Instantiate(this.CircleGround, new Vector3(0,0,0), transform.rotation) as GameObject;

        
        maxX = maxX + 50;
       
        maxZ = maxZ + 50;

        minX = minX - 50;
        
        minZ = minZ - 50;

        earth.transform.localScale = new Vector3(maxX - minX, 2, maxZ - minZ);
        earth.transform.localPosition = new Vector3((maxX + minX) / 2, 0, (maxZ + minZ) / 2);

        earth.name = "CircleGround";
        earth.tag = "Ground";

        //var a = earth.GetComponent<Renderer>().material;
        //a.mainTextureScale = new Vector2(maxX - minX / 1000, maxZ - minZ / 1000);

        // カメラをスタートする
        GameObject obj = GameObject.Find("Main Camera");
        mainCamera = obj.GetComponent<CameraMove>();
        mainCamera.StartCamera();
    }
    

    // 中心からのびる道路を作る関数
    void BuildCircleStreet2(Dictionary<String, List<Dictionary<String, object>>> rootBlock, Dictionary<String, List<Dictionary<String, object>>> firstBlock, String root)
    {
        float centerX = float.Parse(rootBlock[root][0]["x"].ToString());
        float centerZ = float.Parse(rootBlock[root][0]["z"].ToString());

        //int num = 0;

        // 0度のベクトル
        Vector2 uVec = new Vector2(1, 0);

        // 周りに順番に道を伸ばしていく
        foreach (String key in firstBlock.Keys)
        {
            float x = float.Parse(firstBlock[key][0]["x"].ToString());
            float z = float.Parse(firstBlock[key][0]["z"].ToString());

            float midX = (centerX + x) / 2;
            float midZ = (centerZ + z) / 2;

            Vector2 vec = new Vector2(centerX, centerZ) - new Vector2(x, z);
            float distance = vec.magnitude;

            /*
            float deg = 360 / (float)firstBlock.Count;
            double rad = deg * Mathf.PI / 180.0;
            */

            // arcCos(ベクトルの内積 / ベクトルの大きさの積)で角度を出す
            float newDeg = Mathf.Acos(Vector2.Dot(vec, uVec) / (vec.magnitude * uVec.magnitude));

            // arcCosは0～πまでしか出せないのでz軸がマイナスなら負の値にする
            if (z < 0)
                newDeg = -newDeg;

            //GameObject street = Instantiate(this.street, new Vector3(midX, 2, midZ), Quaternion.Euler(0, -(float)rad * num * Mathf.Rad2Deg, 0)) as GameObject;

            GameObject street = Instantiate(this.street, new Vector3(midX, 2, midZ), Quaternion.Euler(0, newDeg * Mathf.Rad2Deg, 0)) as GameObject;

            street.transform.localScale = new Vector3(distance, 1, 50);
            street.name = "To:" + firstBlock[key][0]["name"].ToString();
            street.tag = "Street";

            var a = street.GetComponent<Renderer>().material;
            a.mainTextureScale = new Vector2(distance / 100, 1);

            //num++;
        }
    }


    // 文字の出現回数をカウント
    public static int CountChar(string s, char c)
    {
        return s.Length - s.Replace(c.ToString(), "").Length;
    }


    // Javascriptから街を作り始めるためのメソッド
    // ココがHTML側から呼び出される
    public void StartCityCreater(string id)
    {
        StartCoroutine(ReadFileOnline(id));
    }

    // サーバにあるJsonファイルを読み込むメソッド
    IEnumerator ReadFileOnline(string id)
    {
        // 以下の場合、163.221.29.171でアクセスするとJavascriptのエラーが起きるので注意！
        // rocat.naist.jpでアクセスすればOK

        //string url = "http://kataribe-dev.naist.jp:802/public/code_city.json?id=" + id;
        //string url = "http://163.221.29.246/json/" + id + ".json";
        //string url = "http://163.221.29.171/json/" + id + ".json";
        string url = "http://rocat.naist.jp/json/" + id + ".json";

        WWW www = new WWW(url);
        yield return www;

        if (www.error == null)
        {
            jsonText = www.text;

        }
        else {
            jsonText = SetDefaultText();
        }

        Camera.main.GetComponent<CameraMove>().isControlAvailable = true;
        CreateCity();

    }

    string SetDefaultText ()
	{
		return "cant read\n";
	}
	
    // 都市の情報（Jsonファイルの情報）を返す
	public Dictionary<string,object> GetCity(){
		return this.city;
	}
	
    // 地面のオブジェクトを返す
    public GameObject GetGround()
    {
        return this.earth as GameObject;
    }

    // 読み込んだJsonファイルの情報を返す
    public string GetJsonText()
    {
        return this.jsonText;
    }

    // 周囲に配置される土台（ディレクトリ）の辞書を返す
    public Dictionary<String, List<Dictionary<String, object>>> GetFirstBlockList()
    {
        //return this.firstBlockDictionary;
        return this.firstBlockDictionary2;
    }


    // 都市を再構成する関数
    public void RemakeCity(String root, Boolean directFlag)
    {
        // リストで飛ぶときか上へのボタンを押したときは直接行く
        if (directFlag)
        {
            //Debug.Log(root);
            LocateBlockAndBuilding2(allDirectory, arrangedBuildings, root);
        }
        // リストから飛んでいない、かつrootに指定した土台が現在のrootのときは1個上に行く
        else if(root == currentRoot)
        {
            // プロジェクトのルートディレクトリに当たる場合は上がないので行かない
            if(root != rootDirName)
            {
                LocateBlockAndBuilding2(allDirectory, arrangedBuildings, root.Substring(0, root.LastIndexOf("/")));
            }
        }
        // それ以外の場合は下に階層がある場合のみ行く
        else
        {
            foreach (String name in dir)
            {
                if (name.Contains(root + "/"))
                {
                    LocateBlockAndBuilding2(allDirectory, arrangedBuildings, root);
                    break;
                }
            }
        }       
    }

    // ルートディレクトリの名前を返す
    public String GetRootName()
    {
        return rootDirName;
    }

    // 現在中心にあるディレクトリの名前を返す
    public String GetCurrentDir()
    {
        if (currentRoot == rootDirName)
            return "/";
        else
            return currentRoot.Substring(rootDirName.Length);
    }
}
