using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using MiniJSON;

public class sort_block{
	public int block_ID{ get; set;}
	public string block_name{ get; set;}
	public float x_width { get; set;}
	public float z_width { get; set;}
	public float x_pos{ get; set;}
	public float y_pos{ get; set;}
	public float z_pos{ get; set;}
}

public class sort_building{
	public int block_ID { get; set;} 
	public string building_name { get; set;}
	public float width { get; set;}
	public float height { get; set;}
	public float x_pos{ get; set;}
	public float y_pos{ get; set;}
	public float z_pos{ get; set;}
	public float color_r { get; set;}
	public float color_g { get; set;}
	public float color_b { get; set;}
}

public class x_hold{
	public int block_ID { get; set;}
	public int building_step_cnt { get; set;}
	public float x { get; set;}
	public float width { get; set;}
}

public class z_hold{
	public int block_ID { get; set;}
	public int building_step_cnt { get; set;}
	public float z { get; set;}
	public float width { get; set;}
}

public class CityCreater : MonoBehaviour
{
	public string TARGET; 
	public GameObject ground;
	public GameObject testGround;
	public GameObject building; 
	public GameObject checkSATD;
	public Dictionary<string,object> city;

    public GameObject text;
    public TextMesh meshtext;
    public MeshRenderer textrender;

    public GameObject plate;

    public GameObject earth;    // 土台

    public GameObject sense;    // パーティクル

    public GameObject street;   // 道

    private CameraMove mainCamera;  // メインカメラ

    public GameObject DirNamePlate;
    public GameObject FileNamePlate;

    public string jsonText = "";
    // Use this for initialization

    public List<String> dir = new List<String>(); // ディレクトリ一覧
    public List<String> addedDir = new List<String>();  // 既に置かれたディレクトリの一覧

    public Sensor sensor;

    public String rootDirName;
    public List<String> firstDirNameList = new List<String>();
    public List<String> noBuildingDirNameList = new List<String>();
    public List<String> rootAndFirstDirNameList = new List<String>();

    public GameObject CircleBlock;

    public Dictionary<String, List<Dictionary<String, object>>> firstBlockDictionary;
    public Dictionary<String, List<Dictionary<String, object>>> firstBlockDictionary2;

    public Dictionary<String, List<Dictionary<String, object>>> allDirectory = new Dictionary<string, List<Dictionary<string, object>>>();

    Dictionary<String, List<Dictionary<String, object>>> arrangedBuildings;
    public String currentRoot;


    public float Size = 0;               // 広さ(建物の数に比例)
    public float Repulsion = 0;          // すべてのクラス同士の斥力
    public float Attraction_Link = 0;    // 呼び出し関係にあるクラス同士の引力
    public float Attraction_Package = 0; // 同じパッケージに存在するクラス同士の引力
    public float MinDistance = 0;        // 建物同士の距離が一定以下である場合、引き離すようにする
    public float T = 0;                  // 温度パラメータ(1回の移動で動く距離を制限する)
    public int N = 0;                    // 計算回数(ループ回数)
    public int Seed = 0;                 // 乱数シード

    public float Radius = 0;

    public List<String> satdFilesList = new List<String>();

    void Start()
    {
        sensor = GameObject.Find("Main Camera").GetComponent<Sensor>();
        earth = Instantiate(this.plate, new Vector3(0, 0, 0), transform.rotation) as GameObject;

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
        //StartCityCreater("dynet");
        StartCityCreater("discourse");
        //StartCityCreater("crawlers");
#else
			    Application.ExternalCall("OnUnityReady");
#endif
    }

    /*
    void Start ()
	{
        //TARGET = Application.dataPath + "/target/redmine_path.json";

        TARGET = Application.dataPath + "/target/guice.json";

        //TARGET = Application.dataPath + "/target/test.json";

        //TARGET = Application.dataPath + "/target/tumikiTest.json";


        ReadFile ();
		CreateCity ();
	} 
    */
    


    void CreateCity ()
	{
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
            rootAndFirstDirNameList.Add(rootDirName);
            // ビル数が0のとき
            if (int.Parse(content["filenum"].ToString()) == 0)
            {
                noBuildingDirNameList.Add(content["name"].ToString());
            }
        }

        // SATDのあるファイルのリストを作っておく
        foreach(Dictionary<string, object> content in satdfiles)
        {
            satdFilesList.Add(content["name"].ToString());
            //Debug.Log(content["name"].ToString());
        }

        /*
        // 1階層目のディレクトリ一覧を作る
        foreach(Dictionary<string, object> content in firstDir)
        {
            firstDirNameList.Add(content["name"].ToString());
            rootAndFirstDirNameList.Add(content["name"].ToString());

            // ビル数が0のとき
            if(int.Parse(content["filenum"].ToString()) == 0)
            {
                noBuildingDirNameList.Add(content["name"].ToString());
            }
        }
        */

        // ブロックごとにビルをまとめる *一時的にココでしてみる
        arrangedBuildings = ArrangeByKey(buildings, "block");

        LocateBlockAndBuilding2(allDirectory, arrangedBuildings, rootDirName);
        //LocateBlockAndBuilding2(allDirectory, arrangedBuildings, "/var/www/html/repository/clab/dynet.git");

        //LocateBlockAndBuilding(blocks, buildings, firstDir, rootDir);
		//nori_rogic_ver2 (blocks, buildings);
		/*
			Simple (blocks, buildings);
			 */
	}
	

    void LocateBlockAndBuilding2(Dictionary<string, List<Dictionary<string, object>>> blocks, Dictionary<String, List<Dictionary<String, object>>> buildings, String root)
    {
        // 現在のrootディレクトリを更新
        currentRoot = root;
        
        // rootに指定したディレクトリ下にあるディレクトリの一覧を作成する
        List<String> firstDirList = SearchFirstDirectory(root, dir);

        // rootに指定したディレクトリ用の辞書を作る
        List<String> tempRootList = new List<string>();
        tempRootList.Add(root);
        Dictionary<String, List<Dictionary<String, object>>> rootBlockDictionary = SearchDirectoryDictionary(blocks, tempRootList);

        // rootに指定したディレクトリ直下のディレクトリの辞書を作る
        firstBlockDictionary2 = SearchDirectoryDictionary(blocks, firstDirList);


        // ビルを並べてブロックのサイズを決める
        int edge = 0;
        // rootブロックの処理
        if (buildings.ContainsKey(root))
        {
            edge = SetBuildingLocation(buildings[root]);
        }

        if (edge != 0)
            rootBlockDictionary[root][0]["radius"] = (edge) * Mathf.Sqrt(2) / 2 * 100;
        else
            rootBlockDictionary[root][0]["radius"] = 100;


        // 1階層目ブロックの処理
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

        // ブロックの配置を決める
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


        SetGlobalCircleLocation2(buildings, rootBlockDictionary, firstBlockDictionary2, root);


        // ビルとブロックを建てる

        GameObject rootCircle = Instantiate(this.CircleBlock, new Vector3(float.Parse(rootBlockDictionary[root][0]["x"].ToString()), 3, float.Parse(rootBlockDictionary[root][0]["z"].ToString())), transform.rotation) as GameObject;
        rootCircle.transform.localScale = new Vector3(float.Parse(rootBlockDictionary[root][0]["radius"].ToString()) * 2, (float)2, float.Parse(rootBlockDictionary[root][0]["radius"].ToString()) * 2);
        //rootCircle.name = "Circle " + rootDirName;
        rootCircle.name = root.Substring(1);
        rootCircle.tag = "Block";

        // メタ情報
        var rootData = rootCircle.GetComponent<BlockData>();
        rootData.pathname = root.Substring(1);

        if (root == rootDirName)
            rootData.blockname = "root";
        else
            rootData.blockname = root.Substring(rootDirName.Length);

        rootData.end = false;

        /*
        // お名前カンバン
        //GameObject rootDirText = Instantiate(this.DirNamePlate, new Vector3(float.Parse(rootBlockDictionary[root][0]["x"].ToString()), 200, float.Parse(rootBlockDictionary[root][0]["z"].ToString())), transform.rotation) as GameObject;
        //rootDirText.GetComponent<DirName>().SetNameText(rootData.blockname);
        //rootDirText.GetComponent<DirName>().SetBackSize();
        //rootDirText.tag = "Plate";
        */

        // ブロックを置く
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

            bool end = true;
            foreach(String name in dir)
            {
                if (name.Contains("/" + circleData.pathname + "/"))
                    end = false;
            }
            circleData.end = end;

            // その先にディレクトリがない場合は色を変える
            if (end)
                firstCircle.GetComponent<Block>().SetMaterial(Color.gray);

            // ブロックの先にSATDがあるなら炎をつくる
            foreach (String name in satdFilesList)
            {
                
                if (name.Contains("/" + circleData.pathname) && !end && "/" + circleData.pathname != name.Substring(0, name.LastIndexOf(name.Substring(name.LastIndexOf("/")))))
                {
                    /*
                    //Debug.Log("/" + circleData.pathname + ", " + name.Substring(0, name.LastIndexOf(name.Substring(name.LastIndexOf("/")))));
                    GameObject fire = Instantiate(this.sense, new Vector3(0, 1, 0), transform.rotation) as GameObject;
                    var r = fire.GetComponent<ParticleSystem>().shape;
                    r.radius = float.Parse(firstBlockDictionary2[key][0]["radius"].ToString());

                    var s = fire.GetComponent<ParticleSystem>();

                    s.startSize = float.Parse(firstBlockDictionary2[key][0]["radius"].ToString()) / 2;

                    s.startSpeed = 70;


                    fire.transform.Rotate(new Vector3((float)270, (float)0, (float)0));
                    fire.transform.position = new Vector3(firstCircle.transform.localPosition.x, 2, firstCircle.transform.localPosition.z);
                    fire.name = "sence:" + firstCircle.name;
                    fire.layer = LayerMask.NameToLayer("Building");
                    fire.tag = "SATDBuilding";
                    */

                    circleData.insideSATD = true;
                    firstCircle.GetComponent<Block>().SetMaterial(Color.yellow);

                    break;
                }
            }


            // お名前カンバン
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
                    //Debug.Log(oneBuilding["name"]);
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

                            buildingObj.name = buildingObj.name + (int.Parse(sList[i].ToString()) + 1).ToString() + ",";

                            // パーティクルの目印を作る
                            GameObject particle = Instantiate(this.sense, new Vector3(0, 1, 0), transform.rotation) as GameObject;
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


    /**
	 *
     * ブロックの位置を決めるメソッド
	 *
	 */
    void LocateBlockAndBuilding (IList blocks, IList buildings, IList firstBlocks, IList rootBlock)
	{

        Dictionary<String, List<Dictionary<String, object>>> arrangedBlock = ArrangeByKey(buildings, "block"); // ブロックごとにビルをまとめる
        //Dictionary<String, List<Dictionary<String, object>>> blockDictionary = ArrangeByKey(blocks, "name"); // 名前ごとにブロックをまとめる
        SearchFirstDirectory(rootDirName, dir);


        // root直下または1階層目のディレクトリごとにビルをまとめる
        Dictionary<String, List<Dictionary<String, object>>> arrangedRootAndFirst = ArrangeByKey2(buildings, rootAndFirstDirNameList);
        
        // rootブロックの辞書
        Dictionary<String, List<Dictionary<String, object>>> rootBlockDictionary = ArrangeByKey(rootBlock, "name");
        //Debug.Log(rootBlockDictionary[rootDirName][0]["name"]);

        // 1階層目のブロックの辞書
        firstBlockDictionary = ArrangeByKey(firstBlocks, "name");
        /*
        foreach (String key in firstBlockDictionary.Keys)
        {
            foreach(Dictionary<String, object> a in firstBlockDictionary[key])
            {
                Debug.Log(a["name"].ToString());
            }
        }
        */

        int edge = 0;
        // rootブロックの処理
        if (!noBuildingDirNameList.Contains(rootDirName))
        {
            edge = SetBuildingLocation(arrangedRootAndFirst[rootDirName]);
        }

        if (edge != 0)
            rootBlockDictionary[rootDirName][0]["radius"] = (edge) * Mathf.Sqrt(2) / 2 * 100;
        else
            rootBlockDictionary[rootDirName][0]["radius"] = 100;


        // 1階層目ブロックの処理
        foreach (String key in firstBlockDictionary.Keys)
        {
            //Debug.Log(key);
            edge = 0;

            // ビルを配置して1辺の個数を求める
            if (!noBuildingDirNameList.Contains(key))
            {
                edge = SetBuildingLocation(arrangedRootAndFirst[key]);
            }
            //Debug.Log(edge);

            // 円の半径を決める
            if(edge != 0)
                firstBlockDictionary[key][0]["radius"] = (edge) * Mathf.Sqrt(2) / 2 * 100;
            else
                firstBlockDictionary[key][0]["radius"] = 50;
        }


        /*
        firstBlockDictionary2 = ArrangeByKey3(firstBlocks, "name");

        // 1階層目ブロックの処理
        foreach (String key in firstBlockDictionary2.Keys)
        {
            //Debug.Log(key);
            int edge1 = 0;

            // ビルを配置して1辺の個数を求める
            if (!noBuildingDirNameList.Contains(key) && arrangedRootAndFirst.ContainsKey(key))
            {
                edge1 = SetBuildingLocation(arrangedRootAndFirst[key]);
            }
            //Debug.Log(edge);

            // 円の半径を決める
            if (edge1 != 0)
                firstBlockDictionary2[key][0]["radius"] = 50;
            else
                firstBlockDictionary2[key][0]["radius"] = 50;
        }

        SetBlockCircleByForceDirectedGraph(rootBlockDictionary, firstBlockDictionary2);
        BuildCircle2(firstBlockDictionary2);
        */
        

        // ブロックの半径でソート（ブロックのお名前を並び替える）
        firstDirNameList = SortFirstBlockName(firstDirNameList, firstBlockDictionary);

        // ブロックを円上においていく座標を決める
        SetBlockCircleLocation(rootBlockDictionary, firstBlockDictionary);

        // ビルの座標を円の上に置き換える
        SetGlobalCircleLocation(arrangedRootAndFirst, rootBlockDictionary, firstBlockDictionary);

        // 円上に置いていく
        BuildCircle(arrangedRootAndFirst,rootBlockDictionary, firstBlockDictionary);

        // 道を置いていく
        BuildCircleStreet(rootBlockDictionary, firstBlockDictionary);

        // 地面を作る
        SetCircleGround(rootBlockDictionary, firstBlockDictionary);

        sensor.MakeSensorList();

        /*


        Dictionary<String,List<Dictionary<String, object>>> arrangedBlock = ArrangeByKey (buildings, "block"); // ブロックごとにビルをまとめる
		Dictionary<String,List<Dictionary<String, object>>> blockDictionary = ArrangeByKey (blocks, "name"); // 名前ごとにブロックをまとめる
		
		List<Dictionary<String, object>> blockList = new List<Dictionary<string, object>> ();
        //Debug.Log (new HashSet<String>(arrangedBlock.Keys).Equals(new HashSet<String>(blockDictionary.Keys)));

        // ブロックの座標を決めるときに出てくる追加分のブロックの辞書
        Dictionary<String, List<Dictionary<String, object>>> notBuildingBlockList = new Dictionary<String, List<Dictionary<String, object>>>();

        foreach (String key in blockDictionary.Keys) { // ブロックのkeyごとに実行する

            //SetLocation (arrangedBlock[key]); // 1.ビルの座標を決める

            //SetLocation2(arrangedBlock[key]); // 1.ビルの座標を決める

            SetLocation2_1(arrangedBlock[key]); // 1.ビルの座標を決める fixed

            //SetWidth (arrangedBlock[key], blockDictionary[key]); // 2.ブロックの幅を決める

            //SetWidth2(arrangedBlock[key], blockDictionary[key]); // 2.ブロックの幅を決める
            SetWidth2_1(arrangedBlock[key], blockDictionary[key]); // 2.ブロックの幅を決める

            blockList.Add(blockDictionary[key][0]);
            //Debug.Log(int.Parse(blockDictionary[key][0]["width"].ToString()));
        }

        // ブロックの座標を決める
        //SetLocation (blockList);
        SetLocation2(blockList);

        //notBuildingBlockList = SetBlockLocation(blockList);
        notBuildingBlockList = SetBlockLocation2(blockList);

        // ビルのリストに追加しておく
        foreach (String key in notBuildingBlockList.Keys)
        {
            blockList.Add(notBuildingBlockList[key][0]);
        }

        // ビルの実際の座標を決める
        SetGlobalLocation (arrangedBlock, blockDictionary);


		// ビルとブロックを建てていく
		BuildBuildings (arrangedBlock, blockDictionary, notBuildingBlockList);


        // 地面を設定
        SetGround(blockList);


        // 道を作る
        //BuildStreets(blockList);
        BuildStreets2(blockList);

        sensor.MakeSensorList();

        */

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
        /*
        foreach(String key in result.Keys)
        {
            Debug.Log(result[key][0]["name"]);
        }
        */
        return result;
    }


    void SetBlockCircleByForceDirectedGraph(Dictionary<String, List<Dictionary<String, object>>> root, Dictionary<String, List<Dictionary<String, object>>> other)
    {

        //root[rootDirName][0]["x"] = 0;
        //root[rootDirName][0]["z"] = 0;

        // ランダムに配置する
        {
            UnityEngine.Random.seed = Seed;

            foreach(String key in other.Keys)
            {
                float angle = UnityEngine.Random.Range(0, Mathf.PI * 2);
                float distance = UnityEngine.Random.Range(0, Radius);
                other[key][0]["x"] = distance * Mathf.Cos(angle);
                other[key][0]["z"] = distance * Mathf.Sin(angle);
            }
        }

        // ノードと速度の辞書
        Dictionary<String, Vector3> velocityDictionary = new Dictionary<String, Vector3>();
        foreach(String key in other.Keys)
        {
            velocityDictionary.Add(key, Vector3.zero);
        }

        // 各係数を求める
        float k = Mathf.Sqrt(Radius * Radius * Mathf.PI / other.Count);
        float k_Repulsion = Repulsion * k;
        float k_Attraction_Link = Attraction_Link * k;
        float k_Attraction_Package = Attraction_Package * k;

        for (int n = 0; n < N; n++)
        {
            // 温度パラメータ
            float tmp = Radius * T * (N - n) / N;
            
            // 速度をリセット
            foreach (String key in other.Keys)
            {
                velocityDictionary[key] = Vector3.zero;
            }

            // すべてのノードの速度を更新する
            foreach (String key in other.Keys)
            {
                foreach (String otherBlock in other.Keys)
                {
                    if (key != otherBlock)
                    {
                        // key から otherBlockへのベクトル
                        Vector3 node2other = new Vector3(float.Parse(other[otherBlock][0]["x"].ToString()) - float.Parse(other[key][0]["x"].ToString()), 0, float.Parse(other[otherBlock][0]["z"].ToString()) - float.Parse(other[key][0]["z"].ToString()));

                        // すべてのノードから受ける斥力
                        velocityDictionary[key] -= k_Repulsion * k_Repulsion / node2other.magnitude * node2other.normalized;

                        // リンクを持つノード同士の引力
                        if (IsLinkedNode(other[key], other[otherBlock]))
                        {
                            velocityDictionary[key] += node2other.magnitude * node2other.magnitude / k_Attraction_Link * node2other.normalized;
                        }

                        // 同じパッケージ内のノード同士の引力
                        if (other[otherBlock][0]["name"].ToString().Substring(0, other[otherBlock][0]["name"].ToString().LastIndexOf("/")) == (other[key][0]["name"].ToString().Substring(0, other[key][0]["name"].ToString().LastIndexOf("/"))))
                        {
                            velocityDictionary[key] += node2other.magnitude * node2other.magnitude / k_Attraction_Package * node2other.normalized;
                        }

                        // 近すぎるノード同士の斥力
                        if (node2other.magnitude < MinDistance)
                        {
                            velocityDictionary[key] -= (MinDistance - node2other.magnitude) * node2other.normalized;
                        }

                    }
                }
                // 温度パラメータにより速度を制限する
                if (tmp < velocityDictionary[key].magnitude)
                {
                    velocityDictionary[key] = velocityDictionary[key].normalized * tmp;
                }
            }

            // すべてのノードを移動する
            foreach (String key in other.Keys)
            {
                other[key][0]["x"] = float.Parse(other[key][0]["x"].ToString()) + velocityDictionary[key].x;
                other[key][0]["z"] = float.Parse(other[key][0]["z"].ToString()) + velocityDictionary[key].z;
                //Debug.Log(key + ", " + other[key][0]["x"] + ", " + other[key][0]["z"]);
            }            
        }
    }

    bool IsLinkedNode(List<Dictionary<String, object>> a, List<Dictionary<String, object>> b)
    {
        if (a[0]["name"].ToString() == b[0]["name"].ToString().Substring(0, b[0]["name"].ToString().LastIndexOf("/")) || b[0]["name"].ToString() == a[0]["name"].ToString().Substring(0, a[0]["name"].ToString().LastIndexOf("/")))
            return true;
        else
            return false;
    }

    // 周りのブロックを大きさ順に並べた時のお名前のリストを返す
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
     * 用途1：ビルを属するブロックごとにまとめる
	 * 用途2：ブロックを名前ごとにまとめる
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


    Dictionary<String, List<Dictionary<String, object>>> ArrangeByKey2(IList target, List<String> key)
    {
        Dictionary<String, List<Dictionary<String, object>>> arrangedTarget = new Dictionary<string, List<Dictionary<String, object>>>();

        foreach (Dictionary<string, object> contents in target)
        {
            Boolean addCheck = false;

            // リストの1番目から見る
            for(int i = 1; i < key.Count; i++)
            {   
                // ビルのpath(ファイルのフルパス)がkeyの1番目以降のいずれかを含んでいる
                if(contents["path"].ToString().Contains(key[i] + "/"))
                {
                    if (arrangedTarget.ContainsKey(key[i]))
                    {
                        // 既に辞書に存在する場合はaddだけする
                        arrangedTarget[key[i]].Add(contents);
                    }
                    else
                    {
                        // 初めて出てきたらnewして辞書にaddする
                        arrangedTarget[key[i]] = new List<Dictionary<String, object>>();
                        arrangedTarget[key[i]].Add(contents);
                    }

                    addCheck = true;
                }
            }

            // ビルのブロック名がkeyの1番目以降のどれも含んでいない = root直下のファイルのビル
            if (!addCheck)
            {
                if (arrangedTarget.ContainsKey(key[0]))
                {
                    // 既に辞書に存在する場合はaddだけする
                    arrangedTarget[key[0]].Add(contents);
                }
                else
                {
                    // 初めて出てきたらnewして辞書にaddする
                    arrangedTarget[key[0]] = new List<Dictionary<String, object>>();
                    arrangedTarget[key[0]].Add(contents);
                }
            }      
        }
        return arrangedTarget;
    }


    Dictionary<String, List<Dictionary<String, object>>> ArrangeByKey3(IList target, String key)
    {
        Dictionary<String, List<Dictionary<String, object>>> arrangedTarget = new Dictionary<string, List<Dictionary<String, object>>>();

        foreach (Dictionary<string, object> contents in target)
        {
            String contentsName = contents[key].ToString();
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

        // ファイルのないディレクトリを追加していく
        foreach (String oneDir in dir)
        {

            if (!arrangedTarget.ContainsKey(oneDir))
            {
                Dictionary<String, object> oneBlock = new Dictionary<string, object>();
                List<Dictionary<String, object>> oneList = new List<Dictionary<string, object>>();

                oneBlock.Add("name", oneDir);
                oneList.Add(oneBlock);

                arrangedTarget.Add(oneDir, oneList);
            }
            
        }

        return arrangedTarget;
    }


    // ビルを正方形っぽく並べる
    int SetBuildingLocation(List<Dictionary<String, object>> target)
    {
        //target.Sort((b, a) => string.Compare(b["block"].ToString(), a["block"].ToString()));
        // ブロック名で比較して同じだったらファイル名で比較してソート
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

        return edgeNum;
    }


    /**
	 *
	 * targetの相対座標を決めるメソッド
	 * ビル間・ブロック間での座標を決める
	 * 
	 */


    void SetLocation (List<Dictionary<String,object>> target)
	{
		// targetをソートする
		//Debug.Log (target [0] ["width"]);
		target.Sort((b,a) => int.Parse(a["width"].ToString()) - int.Parse(b["width"].ToString()));
		
		
		// 0 3 8
		// 1 2 7
		// 4 5 6
		// 
		// 以下のコードで上記の0→8の順番のように配置していく
        // ↑SetLocation2の順の方がイメージ的には正しい？
		
		int count = 0;
		float space = float.Parse(target[0]["width"].ToString()) + 20; // 基準になる間隔
		for (int i = 0; ;i++) {
			int constX = 0;
			int y = i;
			// 1.右に向かって並べていく
			// ex) 0
			// ex) 1→2
            // ex) 4→6
			for(int x = 0; x <= y; x++){
				target[count]["x"] = (space * x) + (space / 2);
				target[count]["y"] = (space * y) + (space / 2);
				count++;
				if(count == target.Count)
				{
					goto Finish; // 全部終わったらFinishへ行く
				}
				constX = x;
			}
			// 2.上に向かって並べていく
			// ex) 3
			// ex) 7→8
			for(y--; y >= 0; y--){
				target[count]["x"] = (space * constX) + (space / 2);
				target[count]["y"] = (space * y) + (space / 2);
				count++;
				if(count == target.Count)
				{
					goto Finish; // 全部終わったらFinishへ行く
				}
			}
		}
		
	Finish:
			return;
	}

    /**
	 *
	 * targetの相対座標を決めるメソッド2
	 * ビル間・ブロック間での座標を決める
     * y座標は実際はz座標だったりする…
	 * 
	 */

    void SetLocation2(List<Dictionary<String, object>> target)
    {
        // targetをソートする
        target.Sort((b, a) => int.Parse(a["widthX"].ToString()) - int.Parse(b["widthX"].ToString()));

        // 0 1 4
        // 3 2 5
        // 8 7 6
        // 
        // 以下のコードで上記の0→8の順番のように配置していく

        int count = 0;  // ビル・ブロックの個数

        int space = 50; // 固定幅

        for (int i = 0; ; i++)
        {
            int y = i;
            // 1.下に向かって並べていく
            // ex) 0
            // ex) 1→2
            // ex) 4→6
            for (int x = 0; x <= y; x++)
            {
                // i^2番目のとき
                if(count == i * i)
                {
                    // 一番最初のとき
                    if(i == 0)
                    {
                        target[count]["x"] = space + int.Parse(target[count]["widthX"].ToString()) / 2;
                        target[count]["y"] = space + int.Parse(target[count]["widthY"].ToString()) / 2;
                    }
                    // それ以外のとき
                    else
                    {
                        target[count]["x"] = space + int.Parse(target[count]["widthX"].ToString()) / 2;
                        target[count]["y"] = space + int.Parse(target[(i - 1) * (i - 1)]["y"].ToString()) + int.Parse(target[(i - 1) * (i - 1)]["widthY"].ToString()) / 2 + int.Parse(target[count]["widthY"].ToString()) / 2;
                    }
                }
                // i^2+i番目の時
                else if(count == i * i + i)
                {
                    target[count]["x"] = space + int.Parse(target[(i - 1) * (i - 1) + (i - 1)]["x"].ToString()) + int.Parse(target[(i - 1) * (i - 1) + (i - 1)]["widthX"].ToString()) / 2 + int.Parse(target[count]["widthX"].ToString()) / 2;
                    target[count]["y"] = int.Parse(target[i * i]["y"].ToString());
                }
                // それ以外
                else
                {
                    target[count]["x"] = int.Parse(target[(i - 1) * (i - 1) + x]["x"].ToString());
                    target[count]["y"] = int.Parse(target[(i - 1) * (i - 1) + x]["y"].ToString()) + int.Parse(target[(i - 1) * (i - 1) + x]["widthY"].ToString()) / 2 + int.Parse(target[count]["widthY"].ToString()) / 2 + space;
                }

                count++;
                if (count == target.Count)
                {
                    goto Finish; // 全部終わったらFinishへ行く
                }
            }
            // 2.左に向かって並べていく
            // ex) 3
            // ex) 7→8
            for (y--; y >= 0; y--)
            {
                target[count]["x"] = int.Parse(target[i * i + i]["x"].ToString());
                target[count]["y"] = int.Parse(target[count - (2 * i + 1)]["y"].ToString());
                count++;
                if (count == target.Count)
                {
                    goto Finish; // 全部終わったらFinishへ行く
                }
            }
        }

    Finish:
        return;
    }



    void SetLocation2_1(List<Dictionary<String, object>> target)
    {
        // targetをソートする
        target.Sort((b, a) => int.Parse(a["widthX"].ToString()) - int.Parse(b["widthX"].ToString()));

        // 0 1 4
        // 3 2 5
        // 8 7 6
        // 
        // 以下のコードで上記の0→8の順番のように配置していく

        int count = 0;  // ビル・ブロックの個数

        int space = 30; // 固定幅

        for (int i = 0; ; i++)
        {
            int y = i;
            // 1.下に向かって並べていく
            // ex) 0
            // ex) 1→2
            // ex) 4→6
            for (int x = 0; x <= y; x++)
            {
                // i^2番目のとき
                if (count == i * i)
                {
                    // 一番最初のとき
                    if (i == 0)
                    {
                        target[count]["x"] = space + 100;
                        target[count]["y"] = space + 100;
                    }
                    // それ以外のとき
                    else
                    {
                        target[count]["x"] = space + 100;
                        target[count]["y"] = space + int.Parse(target[(i - 1) * (i - 1)]["y"].ToString()) + 100 + 100;
                    }
                }
                // i^2+i番目の時
                else if (count == i * i + i)
                {
                    target[count]["x"] = space + int.Parse(target[(i - 1) * (i - 1) + (i - 1)]["x"].ToString()) + 100 + 100;
                    target[count]["y"] = int.Parse(target[i * i]["y"].ToString());
                }
                // それ以外
                else
                {
                    target[count]["x"] = int.Parse(target[(i - 1) * (i - 1) + x]["x"].ToString());
                    target[count]["y"] = int.Parse(target[(i - 1) * (i - 1) + x]["y"].ToString()) + 100 + 100 + space;
                }

                count++;
                if (count == target.Count)
                {
                    goto Finish; // 全部終わったらFinishへ行く
                }
            }
            // 2.左に向かって並べていく
            // ex) 3
            // ex) 7→8
            for (y--; y >= 0; y--)
            {
                target[count]["x"] = int.Parse(target[i * i + i]["x"].ToString());
                target[count]["y"] = int.Parse(target[count - (2 * i + 1)]["y"].ToString());
                count++;
                if (count == target.Count)
                {
                    goto Finish; // 全部終わったらFinishへ行く
                }
            }
        }

    Finish:
        return;
    }





    // ブロックの配置をディレクトリ構造が分かり易いように決めるメソッド
    Dictionary<String, List<Dictionary<String, object>>> SetBlockLocation(List<Dictionary<String, object>> target)
    {
        // 1個前で置いたブロックのX座標とXの幅の情報
        int prePositionX = 0;
        int preWidthX = 0;

        // 階層が1個上のブロックのY座標とYの幅（実際はZ座標とZの幅）の情報
        int prePositionY = 0;
        int preWidthY = 0;

        // 固定値
        int space = 50;

        // ディレクトリがブロックのリストにない場合に設定するXとYの幅
        int fixedX = 100;
        int fixedY = 100;

        // 1個上の階層のディレクトリ名
        string upperDir = "";

        // 返り値の辞書（今回追加したブロックのリストの辞書）
        Dictionary<String, List<Dictionary<String, object>>> addedBlockList = new Dictionary<String, List<Dictionary<String, object>>>();

        // チェック用フラグ
        int targetFound = 0;
        int upperFound = 0;

        // ブロックのリストを名前順にソートする
        target.Sort((b, a) => string.Compare(b["name"].ToString(), a["name"].ToString()));

        /*
        for (int x = 0; x < target.Count; x++)
            Debug.Log("name: " + target[x]["name"]);
        */


        // ディレクトリを順番に見ていく
        for (int i = 0; i < dir.Count; i++)
        {
            //Debug.Log(i + " : "  + dir[i]);

            // まだそのディレクトリの配置が決まっていないとき
            if(addedDir.IndexOf(dir[i]) < 0)
            {
                // 最初以外の時は1個上の階層を見てくる
                if (i != 0)
                {
                    upperDir = dir[i].Substring(0, dir[i].LastIndexOf("/"));

                    // 1個上の階層がtarget内のとき
                    for (int k = 0; k < target.Count; k++)
                    {
                        if (target[k]["name"].ToString() == upperDir)
                        {
                            prePositionY = int.Parse(target[k]["y"].ToString());
                            preWidthY = int.Parse(target[k]["widthY"].ToString());
                            upperFound = 1;
                            break;
                        }
                    }
                    // 1個上の階層がtarget内に存在しない（ビルがないブロック＝このメソッドで新たに追加されたブロックのとき）
                    if(upperFound == 0)
                    {
                        prePositionY = int.Parse(addedBlockList[upperDir][0]["y"].ToString());
                        preWidthY = int.Parse(addedBlockList[upperDir][0]["widthY"].ToString());
                    }

                }

                // チェック用フラグをリセット
                targetFound = 0;
                upperFound = 0;


                // ブロックの辞書を調べる
                for (int j = 0; j < target.Count; j++)
                {
                    // 配置を決めたディレクトリがブロックのリストにあるもの（ビルが存在しているディレクトリ）
                    if (target[j]["name"].ToString() == dir[i])
                    {
                        target[j]["x"] = prePositionX + preWidthX / 2 + int.Parse(target[j]["widthX"].ToString()) / 2 + space;
                        prePositionX = int.Parse(target[j]["x"].ToString());
                        preWidthX = int.Parse(target[j]["widthX"].ToString());

                        target[j]["y"] = prePositionY + preWidthY / 2 + int.Parse(target[j]["widthY"].ToString()) / 2 + space;
                        prePositionY = int.Parse(target[j]["y"].ToString());
                        preWidthY = int.Parse(target[j]["widthY"].ToString());

                        targetFound = 1;
                        break;
                    }
                }

                // 配置を決めたディレクトリがブロックのリストにないもの（ビルが存在しないディレクトリ） 
                if (targetFound == 0)
                {
                    
                    Dictionary<String, object> newDictionary = new Dictionary<string, object>();
                    newDictionary["name"] = dir[i];
                    newDictionary["widthX"] = fixedX;
                    newDictionary["widthY"] = fixedY;
                    newDictionary["x"] = prePositionX + preWidthX / 2 + int.Parse(newDictionary["widthX"].ToString()) / 2 + space;
                    newDictionary["y"] = prePositionY + preWidthY / 2 + int.Parse(newDictionary["widthY"].ToString()) / 2 + space;

                    List<Dictionary<String, object>> addList = new List<Dictionary<string, object>>();
                    addList.Add(newDictionary);

                    // ブロックの一覧に追加する
                    addedBlockList.Add(dir[i], addList);

                    prePositionX = int.Parse(newDictionary["x"].ToString());
                    prePositionY = int.Parse(newDictionary["y"].ToString());

                    preWidthX = int.Parse(newDictionary["widthX"].ToString());
                    preWidthY = int.Parse(newDictionary["widthY"].ToString());

                }

                // 既に置いたディレクトリ一覧に追加する
                addedDir.Add(dir[i]);
            }
        }
        return addedBlockList;
    }

    // ブロックの配置をディレクトリ構造が分かり易いように決めるメソッドVer.2
    // 縦長にならないように調整した感じ
    Dictionary<String, List<Dictionary<String, object>>> SetBlockLocation2(List<Dictionary<String, object>> target)
    {
        // 1個前で置いたブロックのX座標とXの幅の情報
        int prePositionX = 0;
        int preWidthX = 0;

        // 階層が1個上のブロックのY座標とYの幅（実際はZ座標とZの幅）の情報
        int prePositionY = 0;
        int preWidthY = 0;

        // 固定値
        int space = 50;

        // ディレクトリがブロックのリストにない場合に設定するXとYの幅
        int fixedX = 100;
        int fixedY = 100;

        // 1個上の階層のディレクトリ名
        string upperDir = "";

        // 1個上の階層のディレクトリ名を含む最後に置いたブロック（ディレクトリ）の名前
        //string lastDir = "";

        // 返り値の辞書（今回追加したブロックのリストの辞書）
        Dictionary<String, List<Dictionary<String, object>>> addedBlockList = new Dictionary<String, List<Dictionary<String, object>>>();

        // チェック用フラグ
        int targetFound = 0;
        int upperFound = 0;
        int lastFound = 0;

        // ブロックのリストを名前順にソートする
        target.Sort((b, a) => string.Compare(b["name"].ToString(), a["name"].ToString()));

        /*
        for (int x = 0; x < target.Count; x++)
            Debug.Log("name: " + target[x]["name"]);
        */


        // ディレクトリを順番に見ていく
        for (int i = 0; i < dir.Count; i++)
        {
            //Debug.Log(i + " : "  + dir[i]);

            // まだそのディレクトリの配置が決まっていないとき
            if (addedDir.IndexOf(dir[i]) < 0)
            {
                // 最初以外の時は1個上の階層を見てくる
                if (i != 0)
                {
                    upperDir = dir[i].Substring(0, dir[i].LastIndexOf("/"));


                    lastFound = 0;

                    // 既に置いたブロックを調べる
                    foreach(string dire in addedDir)
                    {
                        // 1個上の階層のディレクトリ名を含み、かつ1個上のディレクトリではないものがあるか
                        if(dire.Contains(upperDir) && string.Compare(dire, upperDir) != 0)
                        {
                            //lastDir = dir;
                            lastFound = 1;
                            break;
                        }
                    }

                    // target内（ビルがあるブロック）を見る
                    for (int k = 0; k < target.Count; k++)
                    {
                        // 1個上の階層がtarget内のとき
                        if (target[k]["name"].ToString() == upperDir)
                        {
                            prePositionY = int.Parse(target[k]["y"].ToString());
                            preWidthY = int.Parse(target[k]["widthY"].ToString());
                            upperFound = 1;
                            //break;
                        }

                        // 1個上の階層のディレクトリ名を含み、かつ1個上のディレクトリではないものがあった場合
                        if(lastFound == 1)
                        {
                            // 1個上の階層のディレクトリ名を含む、まだ置いていないtargetを見る
                            if (target[k]["name"].ToString().Contains(upperDir) && addedDir.Contains(target[k]["name"].ToString()) != false)
                            {
                                // X座標＋Xの幅/2が大きかったら更新
                                if (prePositionX + preWidthX / 2 < int.Parse(target[k]["x"].ToString()) + int.Parse(target[k]["widthX"].ToString()) / 2)
                                {
                                    prePositionX = int.Parse(target[k]["x"].ToString());
                                    preWidthX = int.Parse(target[k]["widthX"].ToString());
                                }
                            }
                        }
                    }

                    // 1個上の階層がtarget内に存在しない（ビルがないブロック＝このメソッドで新たに追加されたブロックのとき）
                    if (upperFound == 0)
                    {
                        prePositionY = int.Parse(addedBlockList[upperDir][0]["y"].ToString());
                        preWidthY = int.Parse(addedBlockList[upperDir][0]["widthY"].ToString());


                        // 1個上の階層のディレクトリ名を含み、かつ1個上のディレクトリではないものがあった場合
                        if (lastFound == 1)
                        {
                            // このメソッドで新たに追加されたブロックの辞書のキー（＝ディレクトリ名）の配列を作成
                            string[] keyList = new string[addedBlockList.Keys.Count];
                            addedBlockList.Keys.CopyTo(keyList, 0);
                            foreach (string key in keyList)
                            {
                                // キー（＝ディレクトリ）が1個上の階層のディレクトリ名を含んでいる場合
                                if (key.Contains(upperDir))
                                {
                                    // X座標＋Xの幅/2が大きかったら更新
                                    if (prePositionX + preWidthX / 2 < int.Parse(addedBlockList[key][0]["x"].ToString()) + int.Parse(addedBlockList[key][0]["widthX"].ToString()) / 2)
                                    {
                                        prePositionX = int.Parse(addedBlockList[key][0]["x"].ToString());
                                        preWidthX = int.Parse(addedBlockList[key][0]["widthX"].ToString());
                                    }
                                }
                            }
                        }
                    }
                    // 1個上の階層がtarget内に存在するけどビルがないブロックのリストに1個上の階層の名前を含むブロックがある場合
                    else
                    {
                        // 新たに追加したブロックのリストのキーを順番に見ていく
                        foreach(string key in addedBlockList.Keys)
                        {
                            // キーに1個上の階層のディレクトリ名が含まれているとき
                            if(key.Contains(upperDir))
                            {
                                // X座標＋Xの幅/2が大きかったら更新
                                if (prePositionX + preWidthX / 2 < int.Parse(addedBlockList[key][0]["x"].ToString()) + int.Parse(addedBlockList[key][0]["widthX"].ToString()) / 2)
                                {
                                    prePositionX = int.Parse(addedBlockList[key][0]["x"].ToString());
                                    preWidthX = int.Parse(addedBlockList[key][0]["widthX"].ToString());
                                }
                            }
                        }
                    }
                }

                // チェック用フラグをリセット
                targetFound = 0;
                upperFound = 0;

                // ブロックの辞書を調べる
                for (int j = 0; j < target.Count; j++)
                {
                    // 配置を決めたディレクトリがブロックのリストにあるもの（ビルが存在しているディレクトリ）
                    if (target[j]["name"].ToString() == dir[i])
                    {
                        // 1個上の階層のディレクトリ名を含み、かつ1個上のディレクトリではないものがあった場合（X更新）
                        if (lastFound == 1)
                        {
                            target[j]["x"] = prePositionX + preWidthX / 2 + int.Parse(target[j]["widthX"].ToString()) / 2 + space + 100;
                            prePositionX = int.Parse(target[j]["x"].ToString()) - int.Parse(target[j]["widthX"].ToString()) / 2;

                        }
                        // その他の場合（Xそのまま）
                        else
                        {
                            target[j]["x"] = prePositionX + int.Parse(target[j]["widthX"].ToString()) / 2;
                            //prePositionX = int.Parse(target[j]["x"].ToString()) - int.Parse(target[j]["widthX"].ToString()) / 2;
                        }

                        
                        preWidthX = int.Parse(target[j]["widthX"].ToString());

                        target[j]["y"] = prePositionY + preWidthY / 2 + int.Parse(target[j]["widthY"].ToString()) / 2 + space;
                        prePositionY = int.Parse(target[j]["y"].ToString());
                        preWidthY = int.Parse(target[j]["widthY"].ToString());

                        targetFound = 1;
                        break;
                    }
                }

                // 配置を決めたディレクトリがブロックのリストにないもの（ビルが存在しないディレクトリ） 
                if (targetFound == 0)
                {
                    Dictionary<String, object> newDictionary = new Dictionary<string, object>();
                    newDictionary["name"] = dir[i];
                    newDictionary["widthX"] = fixedX;
                    newDictionary["widthY"] = fixedY;
                    
                    newDictionary["y"] = prePositionY + preWidthY / 2 + int.Parse(newDictionary["widthY"].ToString()) / 2 + space;

                    // 1個上の階層のディレクトリ名を含み、かつ1個上のディレクトリではないものがあった場合（X更新）
                    if (lastFound == 1)
                    {
                        newDictionary["x"] = prePositionX + preWidthX / 2 + int.Parse(newDictionary["widthX"].ToString()) / 2 + space + 100;
                        prePositionX = int.Parse(newDictionary["x"].ToString()) - int.Parse(newDictionary["widthX"].ToString()) / 2;

                    }
                    // その他の場合（Xそのまま）
                    else
                    {
                        newDictionary["x"] = prePositionX + int.Parse(newDictionary["widthX"].ToString()) / 2;
                        //prePositionX = int.Parse(newDictionary["x"].ToString()) - int.Parse(newDictionary["widthX"].ToString()) / 2;
                    }

                    

                    List<Dictionary<String, object>> addList = new List<Dictionary<string, object>>();
                    addList.Add(newDictionary);

                    // ブロックの一覧に追加する
                    addedBlockList.Add(dir[i], addList);

                    
                    prePositionY = int.Parse(newDictionary["y"].ToString());

                    preWidthX = int.Parse(newDictionary["widthX"].ToString());
                    preWidthY = int.Parse(newDictionary["widthY"].ToString());

                }


                // 既に置いたディレクトリ一覧に追加する
                addedDir.Add(dir[i]);
            }
        }
        return addedBlockList;
    }


    // 円上にブロックを置くための座標を決める
    void SetBlockCircleLocation(Dictionary<String, List<Dictionary<String, object>>> root, Dictionary<String, List<Dictionary<String, object>>> first)
    {
        //firstDirNameList

        // 中央にrootを置く
        root[rootDirName][0]["x"] = 0;
        root[rootDirName][0]["z"] = 0;

        
        // 角度
        float deg = 360 / (float)first.Count;
        double rad = deg * Mathf.PI / 180.0;

        float max = 0;

        // 一番大きい半径を取得する
        foreach(String key in first.Keys)
        {
            if (float.Parse(first[key][0]["radius"].ToString()) >= max)
                max = float.Parse(first[key][0]["radius"].ToString());
        }

        // 直径にブロック数/10をかける
        double circle_r = max * 2 * first.Count / 50;

        // 1階層目の座標を決めていく
        int radnumber = 0;
        foreach (String key in first.Keys)
        {
            first[key][0]["x"] = Mathf.Cos((float)rad * radnumber) * (float.Parse(first[key][0]["radius"].ToString()) * 2 * first.Count / 5 + float.Parse(root[rootDirName][0]["radius"].ToString()) * 2);
            first[key][0]["z"] = Mathf.Sin((float)rad * radnumber) * (float.Parse(first[key][0]["radius"].ToString()) * 2 * first.Count / 5 + float.Parse(root[rootDirName][0]["radius"].ToString()) * 2);

            //first[key][0]["x"] = Mathf.Cos((float)rad * radnumber) * (float.Parse(first[key][0]["radius"].ToString()) * float.Parse(first[key][0]["radius"].ToString()) / 1000 + float.Parse(root[rootDirName][0]["radius"].ToString()) * 2 + float.Parse(first[key][0]["radius"].ToString()) * 2);
            //first[key][0]["z"] = Mathf.Sin((float)rad * radnumber) * (float.Parse(first[key][0]["radius"].ToString()) * float.Parse(first[key][0]["radius"].ToString()) / 1000 + float.Parse(root[rootDirName][0]["radius"].ToString()) * 2 + float.Parse(first[key][0]["radius"].ToString()) * 2);

            //Debug.Log(first[key][0]["x"] + "," + first[key][0]["z"]);
            radnumber++;
        }

    }

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


        /*
        //Debug.Log(max);
        // 直径にブロック数/10をかける
        double circle_r = max * 2 * first.Count / 10;

        foreach (String key in first.Keys)
        {
            //first[key][0]["x"] = Mathf.Cos((float)rad * radnumber) * (float.Parse(first[key][0]["radius"].ToString()) * 2 * first.Count / 5 + float.Parse(root[rootName][0]["radius"].ToString()) * 2);
            //first[key][0]["z"] = Mathf.Sin((float)rad * radnumber) * (float.Parse(first[key][0]["radius"].ToString()) * 2 * first.Count / 5 + float.Parse(root[rootName][0]["radius"].ToString()) * 2);

            first[key][0]["x"] = Mathf.Cos((float)rad * radnumber) * (circle_r + float.Parse(root[rootName][0]["radius"].ToString()) + float.Parse(first[key][0]["radius"].ToString()));
            first[key][0]["z"] = Mathf.Sin((float)rad * radnumber) * (circle_r + float.Parse(root[rootName][0]["radius"].ToString()) + float.Parse(first[key][0]["radius"].ToString()));

            //first[key][0]["x"] = Mathf.Cos((float)rad * radnumber) * (float.Parse(first[key][0]["radius"].ToString()) * float.Parse(first[key][0]["radius"].ToString()) / 1000 + float.Parse(root[rootDirName][0]["radius"].ToString()) * 2 + float.Parse(first[key][0]["radius"].ToString()) * 2);
            //first[key][0]["z"] = Mathf.Sin((float)rad * radnumber) * (float.Parse(first[key][0]["radius"].ToString()) * float.Parse(first[key][0]["radius"].ToString()) / 1000 + float.Parse(root[rootDirName][0]["radius"].ToString()) * 2 + float.Parse(first[key][0]["radius"].ToString()) * 2);

            //Debug.Log(first[key][0]["x"] + "," + first[key][0]["z"]);
            radnumber++;
        }
        */


        // 1階層目の座標を決めていく
        int radnumber = 0;

        // 周りのブロックが4つ以下なら各ブロックの直径分離せば重ならない
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
                float distance = (max + (20 + plusMinus * 0) + float.Parse(root[rootName][0]["radius"].ToString()) * 2 + float.Parse(first[keyList[nameOrder[i]]][0]["radius"].ToString()) * (7 + plusMinus * 3 + (max - min) / -80));

                // 差分のマイナスが大きすぎる場合はルートの直径＋そのブロックの直径分離す
                if (distance < minDistance)
                {
                    first[keyList[nameOrder[i]]][0]["x"] = Mathf.Cos((float)rad * radnumber) * minDistance;
                    first[keyList[nameOrder[i]]][0]["z"] = Mathf.Sin((float)rad * radnumber) * minDistance;
                }
                // 最大値*20 + ルートの直径 + そのブロックの半径*7 + 外側ならブロックの半径*3内側ならブロックの半径*-3 + ブロックの半径*最大値と最小値の差分の-1/80
                // ブロックの半径の最大・最小値の差が大きいほど内向きに補正がかかってブロックが遠くなりすぎなくなる…はず
                else
                {
                    first[keyList[nameOrder[i]]][0]["x"] = Mathf.Cos((float)rad * radnumber) * distance;
                    first[keyList[nameOrder[i]]][0]["z"] = Mathf.Sin((float)rad * radnumber) * distance;
                }
                //first[keyList[nameOrder[i]]][0]["x"] = Mathf.Cos((float)rad * radnumber) * (max + (20 + plusMinus * 1) +  float.Parse(root[rootName][0]["radius"].ToString()) * 2 + float.Parse(first[keyList[nameOrder[i]]][0]["radius"].ToString()) * (7 + plusMinus * 3 + (max - min) / -100));
                //first[keyList[nameOrder[i]]][0]["z"] = Mathf.Sin((float)rad * radnumber) * (max + (20 + plusMinus * 1) +  float.Parse(root[rootName][0]["radius"].ToString()) * 2 + float.Parse(first[keyList[nameOrder[i]]][0]["radius"].ToString()) * (7 + plusMinus * 3 + (max - min) / -100));

                plusMinus *= -1;
                radnumber++;
            }
        }
    }



    /**
	 *
	 * ブロックの幅を決めるメソッド
	 * targetの0番目のビルの幅に20を足して
	 * targetの個数以上になる最小の平方根を求める
     * ex) targetが1個（ビルが1個）→1^2 = 1 >= 1 ∴1倍でOK
     * ex) targetが2～4個→2^2 = 4 >= 2～4 ∴0番目の2倍の幅があればOK
     * ex) targetが5～9個→3^2 = 9 >= 5～9 ∴0番目の3倍の幅があればOK 
	 * 
	 */
    void SetWidth (List<Dictionary<String,object>> target, List<Dictionary<String, object>> block)
	{
		float space = float.Parse(target[0]["widthX"].ToString()) + 20;
		for (int i = 0; ; i++) {
			if(i * i > target.Count)
			{
				block[0]["widthX"] = space * i;
                block[0]["widthY"] = space * i;
                break;
			}
		}
	}


    /**
	 *
	 * ブロックの幅を決めるメソッド2
	 * 
	 */
    void SetWidth2(List<Dictionary<String, object>> target, List<Dictionary<String, object>> block)
    {
        int count = 0;  // ビル・ブロックの個数

        int space = 50; // 固定幅

        int i;

        // ビル・ブロックを並べるときと同じ方法でカウントしていく
        for (i = 0; ; i++)
        {
            int y = i;

            for (int x = 0; x <= y; x++)
            {
                count++;
                if (count == target.Count)
                {
                    goto Finish; // 全部終わったらFinishへ行く
                }
            }
            for (y--; y >= 0; y--)
            {
                count++;
                if (count == target.Count)
                {
                    goto Finish; // 全部終わったらFinishへ行く
                }
            }
        }

    Finish:
        // 最後のビルの番号（個数-1）がi^2+i以上の時は角のビルとi^2のビルが基準
        if(count - 1 >= i * i + i)
        {
            
            block[0]["widthX"] = int.Parse(target[i * i + i]["x"].ToString()) + int.Parse(target[i * i + i]["widthX"].ToString()) / 2 + space;
            block[0]["widthY"] = int.Parse(target[i * i]["y"].ToString()) + int.Parse(target[i * i]["widthY"].ToString()) / 2 + space;
        }
        // 最後のビルの番号がi^2+1より小さいときはi^2番目ビルと(i-1)の順の時の角のビルが基準
        else
        {          
            block[0]["widthX"] = int.Parse(target[(i - 1) * (i - 1) + (i - 1)]["x"].ToString()) + int.Parse(target[(i - 1) * (i - 1) + (i - 1)]["widthX"].ToString()) / 2 + space;
            block[0]["widthY"] = int.Parse(target[i * i]["y"].ToString()) + int.Parse(target[i * i]["widthY"].ToString()) / 2 + space;
        }

    }


    void SetWidth2_1(List<Dictionary<String, object>> target, List<Dictionary<String, object>> block)
    {
        int count = 0;  // ビル・ブロックの個数

        int space = 50; // 固定幅

        int i;

        // ビル・ブロックを並べるときと同じ方法でカウントしていく
        for (i = 0; ; i++)
        {
            int y = i;

            for (int x = 0; x <= y; x++)
            {
                count++;
                if (count == target.Count)
                {
                    goto Finish; // 全部終わったらFinishへ行く
                }
            }
            for (y--; y >= 0; y--)
            {
                count++;
                if (count == target.Count)
                {
                    goto Finish; // 全部終わったらFinishへ行く
                }
            }
        }

    Finish:
        // 最後のビルの番号（個数-1）がi^2+i以上の時は角のビルとi^2のビルが基準
        if (count - 1 >= i * i + i)
        {

            block[0]["widthX"] = int.Parse(target[i * i + i]["x"].ToString()) + 100 + space;
            block[0]["widthY"] = int.Parse(target[i * i]["y"].ToString()) + 100 + space;
        }
        // 最後のビルの番号がi^2+1より小さいときはi^2番目ビルと(i-1)の順の時の角のビルが基準
        else
        {
            block[0]["widthX"] = int.Parse(target[(i - 1) * (i - 1) + (i - 1)]["x"].ToString()) + 100 + space;
            block[0]["widthY"] = int.Parse(target[i * i]["y"].ToString()) + 100 + space;
        }

    }

    /**
	 *
	 * ビルの座標を決めていくメソッド
	 * ブロックの座標と相対座標からビルを置く座標を決めていく
	 * 
	 */
    void SetGlobalLocation(Dictionary<String,List<Dictionary<String, object>>> building, Dictionary<String,List<Dictionary<String, object>>> block)
	{
		foreach (String key in building.Keys) {
            // ブロックの座標を取ってくる
            float blockX = float.Parse(block[key][0]["x"].ToString()) - float.Parse(block[key][0]["widthX"].ToString()) / 2;
			float blockY = float.Parse(block[key][0]["y"].ToString()) - float.Parse(block[key][0]["widthY"].ToString()) / 2;
            List<Dictionary<String, object>> buildingList = building[key];

            // ビルの座標を決めていく
			foreach(Dictionary<String, object> oneBuilding in buildingList){
				oneBuilding["globalX"] = float.Parse(blockX.ToString()) + float.Parse(oneBuilding["x"].ToString());
				oneBuilding["globalY"] = float.Parse(blockY.ToString()) + float.Parse(oneBuilding["y"].ToString());
			}
		}
	}


    void SetGlobalCircleLocation(Dictionary<String, List<Dictionary<String, object>>> building, Dictionary<String, List<Dictionary<String, object>>> rootBlock, Dictionary<String, List<Dictionary<String, object>>> firstBlock)
    {
        foreach (String key in building.Keys)
        {
            float blockX;
            float blockZ;
            float radius;

            if (key == rootDirName)
            {
                blockX = float.Parse(rootBlock[rootDirName][0]["x"].ToString());
                blockZ = float.Parse(rootBlock[rootDirName][0]["z"].ToString());
                radius = float.Parse(rootBlock[rootDirName][0]["radius"].ToString());
            }
            else
            {
                blockX = float.Parse(firstBlock[key][0]["x"].ToString());
                blockZ = float.Parse(firstBlock[key][0]["z"].ToString());
                radius = float.Parse(firstBlock[key][0]["radius"].ToString());
            }

            List<Dictionary<String, object>> buildingList = building[key];

            foreach (Dictionary<String, object> oneBuilding in buildingList)
            {
                oneBuilding["globalX"] = float.Parse(blockX.ToString()) - Mathf.Sqrt(2) / 2 * radius + 50 + float.Parse(oneBuilding["x"].ToString());
                oneBuilding["globalZ"] = float.Parse(blockZ.ToString()) - Mathf.Sqrt(2) / 2 * radius + 50 + float.Parse(oneBuilding["z"].ToString());
            }

        }

            if (!noBuildingDirNameList.Contains(rootDirName))
        {
            float rootX = float.Parse(rootBlock[rootDirName][0]["x"].ToString());
            float rootZ = float.Parse(rootBlock[rootDirName][0]["z"].ToString());
        }

        foreach (String key in firstBlock.Keys)
        {
            if (!noBuildingDirNameList.Contains(key))
            {
                
            }
        }
    }


    void SetGlobalCircleLocation2(Dictionary<String, List<Dictionary<String, object>>> building, Dictionary<String, List<Dictionary<String, object>>> rootBlock, Dictionary<String, List<Dictionary<String, object>>> firstBlock, String root)
    {
        foreach (String key in building.Keys)
        {
            float blockX = 0;
            float blockZ = 0;
            float radius = 0;

            if (key == root)
            {
                blockX = float.Parse(rootBlock[root][0]["x"].ToString());
                blockZ = float.Parse(rootBlock[root][0]["z"].ToString());
                radius = float.Parse(rootBlock[root][0]["radius"].ToString());
            }
            else if(firstBlock.ContainsKey(key))
            {
                blockX = float.Parse(firstBlock[key][0]["x"].ToString());
                blockZ = float.Parse(firstBlock[key][0]["z"].ToString());
                radius = float.Parse(firstBlock[key][0]["radius"].ToString());
            }

            List<Dictionary<String, object>> buildingList = building[key];

            foreach (Dictionary<String, object> oneBuilding in buildingList)
            {
                if(firstBlock.ContainsKey(key) || key == root)
                {
                    oneBuilding["globalX"] = float.Parse(blockX.ToString()) - Mathf.Sqrt(2) / 2 * radius + 50 + float.Parse(oneBuilding["x"].ToString());
                    oneBuilding["globalZ"] = float.Parse(blockZ.ToString()) - Mathf.Sqrt(2) / 2 * radius + 50 + float.Parse(oneBuilding["z"].ToString());
                }
                
            }

        }

        if (!noBuildingDirNameList.Contains(root))
        {
            float rootX = float.Parse(rootBlock[root][0]["x"].ToString());
            float rootZ = float.Parse(rootBlock[root][0]["z"].ToString());
        }

        foreach (String key in firstBlock.Keys)
        {
            if (!noBuildingDirNameList.Contains(key))
            {

            }
        }
    }


    /**
	 *
	 * ビルを建てるメソッド
	 * buildingオブジェクトを複製して配置していく
	 * 
	 */
    void BuildBuildings(Dictionary<String,List<Dictionary<String, object>>> building, Dictionary<String,List<Dictionary<String, object>>> block, Dictionary<String, List<Dictionary<String, object>>> block2)
    {
        //Debug.Log("TEST");
        //Debug.Log(this.checktest);
        //Debug.Log(this.building);
        //Debug.Log(this.ground);
        foreach (String key in building.Keys) {
			List<Dictionary<String, object>> buildingList = building [key];
			foreach (Dictionary<String, object> oneBuilding in buildingList) {

                // プレートでビルを作る版
                //PilingPlate(oneBuilding);


                

                // ビルを建てる
                //GameObject clone = Instantiate (this.building, new Vector3 (float.Parse (oneBuilding ["globalX"].ToString ()), (float.Parse (oneBuilding ["height"].ToString ()) / 2) + 3, float.Parse (oneBuilding ["globalY"].ToString ())), transform.rotation) as GameObject;
                GameObject clone = Instantiate(this.building, new Vector3(float.Parse(oneBuilding["globalX"].ToString()), 3, float.Parse(oneBuilding["globalY"].ToString())), Quaternion.Euler(-90, 0, 0)) as GameObject;


                // ビルにおなまえを付ける
                /*
                if(oneBuilding["name"].ToString().Contains("("))
                {
                    clone.name = oneBuilding["name"].ToString().Substring(0, oneBuilding["name"].ToString().IndexOf("(")) + "/";
                }
                else
                {
                    clone.name = oneBuilding["name"].ToString() + "/";
                }
                */

                clone.name = (oneBuilding["path"].ToString() + ":").Substring(1);
                clone.tag = "NormalBuilding";

                var data = clone.GetComponent<BuildingData>();
                data.filename = oneBuilding["name"].ToString();
                data.pathname = oneBuilding["path"].ToString();
                data.loc = int.Parse(oneBuilding["height"].ToString()) - 1;
                data.comment = (int.Parse(oneBuilding["widthX"].ToString()) - 1) / 10;

                // ビルの大きさをいじる
                //clone.transform.localScale = new Vector3 (float.Parse (oneBuilding ["widthX"].ToString ()), float.Parse (oneBuilding ["height"].ToString ()), float.Parse (oneBuilding ["widthY"].ToString ()));
                //clone.transform.localScale = new Vector3(float.Parse(oneBuilding["widthX"].ToString()) * (float)0.1, float.Parse(oneBuilding["widthY"].ToString()) * (float)0.1, float.Parse(oneBuilding["height"].ToString()) * (float)0.1);
                clone.transform.localScale = new Vector3(10, 10, float.Parse(oneBuilding["height"].ToString()) * (float)0.1);

                //clone.GetComponent<Renderer>().material.color = Color.blue;



                //ビルの色を変える
                //clone.GetComponent<Building>().Init(new Color (float.Parse (oneBuilding ["color_r"].ToString ()), float.Parse (oneBuilding ["color_g"].ToString ()), float.Parse (oneBuilding ["color_b"].ToString ())));
                //clone.Gmponent<Building>().Init(new Color((float)0.5, (float)0.8, (float)1.0));


                // SATDがあった時に目印を入れる
                //AddSATD(oneBuilding);


                GameObject filetext = Instantiate(this.FileNamePlate, new Vector3(float.Parse(oneBuilding["globalX"].ToString()), float.Parse(oneBuilding["height"].ToString()) / 2, float.Parse(oneBuilding["globalY"].ToString())), Quaternion.Euler(-90, 0, 0)) as GameObject;
                filetext.GetComponent<FileName>().SetNameText(data.filename);
                filetext.GetComponent<FileName>().SetBackSize();


                IList sList = oneBuilding["SATD"] as IList;
				if(sList.Count != 0){

                    clone.tag = "SATDBuilding";
                    clone.GetComponent<Building>().SetMaterial(Color.blue);

                    // 目印をつくる
                    //GameObject test = Instantiate (this.checkSATD, new Vector3 (1, 1, 1), transform.rotation) as GameObject;
                    //test.name = clone.name;
                    //test.tag = "enemy";

                    for (int i = 0; i < sList.Count; i++)
                    {
                        data.satd.Add(int.Parse(sList[i].ToString()) + 1);

                        //test.name = test.name + (int.Parse(sList[i].ToString()) + 1).ToString() + ",";
                        clone.name = clone.name + (int.Parse(sList[i].ToString()) + 1).ToString() + ",";

                        // パーティクルの目印を作る
                        GameObject particle = Instantiate(this.sense, new Vector3(0, 1, 0), transform.rotation) as GameObject;
                        var r = particle.GetComponent<ParticleSystem>().shape;
                        //r.radius = float.Parse(oneBuilding["widthX"].ToString()) * (float)0.7;
                        r.radius = 70;

                        var s = particle.GetComponent<ParticleSystem>();
                        //s.startSize = float.Parse(oneBuilding["widthX"].ToString()) * (float)0.6 + 5;

                        s.startSize = 40;
                        if (s.startSize < 20)
                        {
                            s.startSize = 20;
                        }
                        else if (s.startSize > 100)
                        {
                            s.startSize = 100;
                        }

                        //s.startSpeed = float.Parse(oneBuilding["widthX"].ToString()) * (float)0.5;
                        s.startSpeed = 50;


                        particle.transform.Rotate(new Vector3((float)270, (float)0, (float)0));
                        particle.transform.position = new Vector3(float.Parse(oneBuilding["globalX"].ToString()), (float.Parse(oneBuilding["height"].ToString()) - float.Parse(sList[i].ToString())) * (float)0.8845 + 3, float.Parse(oneBuilding["globalY"].ToString()));
                        particle.name = "sence:" + oneBuilding["name"] + (int.Parse(sList[i].ToString())).ToString();
                    }
                    //test.name = test.name.Substring(0, test.name.Length - 1);
                    clone.name = clone.name.Substring(0, clone.name.Length - 1);

                    //clone.name = test.name;

                    //test.name = "marker_" + test.name;

                    //test.transform.localScale = new Vector3(70, 70, 70);
                    //test.transform.position = new Vector3(float.Parse(oneBuilding["globalX"].ToString()), (float)(double.Parse(oneBuilding["height"].ToString()) * 1 + 50), float.Parse(oneBuilding["globalY"].ToString()));
                    /*
                    if (float.Parse(oneBuilding["widthX"].ToString()) > 3000)
                    {
                        test.transform.localScale = new Vector3(3000, 3000, 3000);
                        test.transform.position = new Vector3(float.Parse(oneBuilding["globalX"].ToString()), (float)(double.Parse(oneBuilding["height"].ToString()) * 1 + 3000), float.Parse(oneBuilding["globalY"].ToString()));
                    }
                    else if (float.Parse(oneBuilding["widthX"].ToString()) > 500)
                    {
                        test.transform.localScale = new Vector3(500, 500, 500);
                        test.transform.position = new Vector3(float.Parse(oneBuilding["globalX"].ToString()), (float)(double.Parse(oneBuilding["height"].ToString()) * 1 + float.Parse(oneBuilding["widthX"].ToString()) + 100), float.Parse(oneBuilding["globalY"].ToString()));
                    }
                    else if(float.Parse(oneBuilding["widthX"].ToString()) > 50)
                    {
                        test.transform.localScale = new Vector3(float.Parse(oneBuilding["widthX"].ToString()), float.Parse(oneBuilding["widthX"].ToString()), float.Parse(oneBuilding["widthX"].ToString()));
                        test.transform.position = new Vector3(float.Parse(oneBuilding["globalX"].ToString()), (float)(double.Parse(oneBuilding["height"].ToString()) * 1.2 + float.Parse(oneBuilding["widthX"].ToString()) + 50), float.Parse(oneBuilding["globalY"].ToString()));
                    }
                    else
                    {
                        test.transform.localScale = new Vector3(50, 50, 50);
                        test.transform.position = new Vector3(float.Parse(oneBuilding["globalX"].ToString()), (float)(double.Parse(oneBuilding["height"].ToString()) * 1 + float.Parse(oneBuilding["widthX"].ToString()) + 50), float.Parse(oneBuilding["globalY"].ToString()));
                    }
                    */

                    //test.transform.rotation = Quaternion.Euler(45,45,45);

                    /*
                    // パーティクルの目印を作る
                    GameObject particle = Instantiate(this.sense, new Vector3(0, 1, 0), transform.rotation) as GameObject;
                    var r = particle.GetComponent<ParticleSystem>().shape;
                    r.radius = float.Parse(oneBuilding["widthX"].ToString()) * (float)0.85;
                    var s = particle.GetComponent<ParticleSystem>();
                    s.startSize = float.Parse(oneBuilding["widthX"].ToString()) / 4;
                    s.startSpeed = float.Parse(oneBuilding["widthX"].ToString()) * (float)2.5;


                    particle.transform.Rotate(new Vector3((float)270, (float)0, (float)0));
                    particle.transform.position = new Vector3(float.Parse(oneBuilding["globalX"].ToString()), (float)(5), float.Parse(oneBuilding["globalY"].ToString()));
                    particle.name = "sence:" + oneBuilding["name"];
                    */

                    // プリミティブなオブジェクトで仮実装
                    //GameObject check = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //check.transform.Translate(float.Parse (oneBuilding ["globalX"].ToString ()), (float.Parse (oneBuilding ["height"].ToString ())) + float.Parse (oneBuilding ["width"].ToString ()), float.Parse (oneBuilding ["globalY"].ToString ()));
                    // check.transform.localScale = new Vector3(float.Parse(oneBuilding["width"].ToString()), float.Parse(oneBuilding["width"].ToString()), float.Parse(oneBuilding["width"].ToString()));

                    // text = gameObject;
                    // text.transform.parent = transform;
                    // textrender = text.GetComponent<MeshRenderer>();
                    // text.transform.Translate(float.Parse(oneBuilding["globalX"].ToString()), (float)(double.Parse(oneBuilding["height"].ToString()) * 1.3), float.Parse(oneBuilding["globalY"].ToString()));
                    //meshtext.text = "test!";



                }
            }
		}

        // ブロックを下に置いていく

        //Debug.Log("TEST");
        foreach (String key in block.Keys) {
			//Debug.Log(key);
			List<Dictionary<String, object>> blockList = block [key];
			GameObject clone = Instantiate (this.ground, new Vector3(float.Parse(blockList[0]["x"].ToString()), 2, float.Parse(blockList[0]["y"].ToString())), transform.rotation) as GameObject;
			clone.transform.localScale = new Vector3 (float.Parse (blockList [0]["widthX"].ToString ()), 2, float.Parse (blockList [0]["widthY"].ToString ()));
            clone.name = blockList[0]["name"].ToString().Substring(1);

            var data = clone.GetComponent<BlockData>();
            data.pathname = blockList[0]["name"].ToString().Substring(1);

            if (blockList[0]["name"].ToString().IndexOf(".git") + 4 == blockList[0]["name"].ToString().Length)
            {
                data.blockname = "root";
            }
            // その他は最後の/から後ろだけ取る
            else
            {
                data.blockname = blockList[0]["name"].ToString().Substring(blockList[0]["name"].ToString().LastIndexOf("/") + 1);
            }

            GameObject dirtext = Instantiate(this.DirNamePlate, new Vector3(float.Parse(blockList[0]["x"].ToString()), 200 + UnityEngine.Random.Range(0, 50), float.Parse(blockList[0]["y"].ToString())), transform.rotation) as GameObject;
            dirtext.GetComponent<DirName>().SetNameText(data.blockname);
            dirtext.GetComponent<DirName>().SetBackSize();
        }


        // 追加分のブロックを下に置いていく

        //Debug.Log("TEST");
        foreach (String key in block2.Keys)
        {
            //Debug.Log("AddedBlock:"+key);
            List<Dictionary<String, object>> blockList = block2[key];
            GameObject clone = Instantiate(this.ground, new Vector3(float.Parse(blockList[0]["x"].ToString()), 2, float.Parse(blockList[0]["y"].ToString())), transform.rotation) as GameObject;
            clone.transform.localScale = new Vector3(float.Parse(blockList[0]["widthX"].ToString()), 2, float.Parse(blockList[0]["widthY"].ToString()));
            //clone.GetComponent<Renderer>().material.color = Color.green;
            clone.name = blockList[0]["name"].ToString().Substring(1);

            var data = clone.GetComponent<BlockData>();
            data.pathname = blockList[0]["name"].ToString().Substring(1);

            if (blockList[0]["name"].ToString().IndexOf(".git") + 4 == blockList[0]["name"].ToString().Length)
            {
                data.blockname = "root";
            }
            // その他は最後の/から後ろだけ取る
            else
            {
                data.blockname = blockList[0]["name"].ToString().Substring(blockList[0]["name"].ToString().LastIndexOf("/") + 1);
            }

            GameObject dirtext = Instantiate(this.DirNamePlate, new Vector3(float.Parse(blockList[0]["x"].ToString()), 200 + UnityEngine.Random.Range(0,50), float.Parse(blockList[0]["y"].ToString())), transform.rotation) as GameObject;
            dirtext.GetComponent<DirName>().SetNameText(data.blockname);
            dirtext.GetComponent<DirName>().SetBackSize();
        }

    }

    // プレートを積み上げてSATDがあるトコだけ目印がついたビルを作る
    void PilingPlate(Dictionary<String, object> target)
    {
        IList sList = target["SATD"] as IList;  // SATDのリスト
        Material satd = Resources.Load("SATD") as Material; // SATD用のマテリアル

        int check = 0;

        // 一段ずつ積み上げる
        for (int i = 0; i < int.Parse(target["height"].ToString()); i++)
        {
            GameObject clone = Instantiate(this.plate, new Vector3(float.Parse(target["globalX"].ToString()), float.Parse((i * 2).ToString()) + 1f, float.Parse(target["globalY"].ToString())), transform.rotation) as GameObject; // 1段積む
            clone.transform.localScale = new Vector3(float.Parse(target["width"].ToString()), 2, float.Parse(target["width"].ToString()));

            // その段（行）にSATDがあるときはマテリアルを変更する
            if (sList.Count != 0 && sList[check].ToString() == i.ToString())
            //if (sList.Contains((object)i) == true)
            {
                clone.GetComponent<Renderer>().material = satd;

                if(check < sList.Count-1)
                {
                    check++;
                }

                //Debug.Log("SATD!");
            }

            clone.name = target["name"].ToString();
        }
    }

    // SATDがあるところに幅がちょっとだけ大きい目印用のプレートを作る
    void AddSATD(Dictionary<String, object> target)
    {
        IList sList = target["SATD"] as IList;  // SATDのリスト
        int check = 0;

        // 1段ずつ見ていく
        for (int i = 0; i < int.Parse(target["height"].ToString()); i++)
        {
            // その段（行）にSATDがあるときに目印をつける
            if (sList.Count != 0 && sList[check].ToString() == i.ToString())
            {
                GameObject clone = Instantiate(this.plate, new Vector3(float.Parse(target["globalX"].ToString()), float.Parse(i.ToString()) + 1f, float.Parse(target["globalY"].ToString())), transform.rotation) as GameObject;
                clone.transform.localScale = new Vector3(float.Parse(target["widthX"].ToString()) + 0.7f, 1, float.Parse(target["widthY"].ToString()) + 0.7f);

                // 目印を補色にする
                //clone.GetComponent<Renderer>().material.color = CalcComplementaryColor(new Color(float.Parse(target["color_r"].ToString()), float.Parse(target["color_g"].ToString()), float.Parse(target["color_b"].ToString())));
                clone.GetComponent<Renderer>().material.color = CalcComplementaryColor(new Color((float)0.5, (float)0.8, (float)1.0));
              

                // おなまえをつける
                clone.name = "(SATD)" + target["name"].ToString() + "@line" + i.ToString();

                if (check < sList.Count - 1)
                {
                    check++;
                }
            }

        }
    }


    void BuildCircle(Dictionary<String, List<Dictionary<String, object>>> building, Dictionary<String, List<Dictionary<String, object>>> rootBlock, Dictionary<String, List<Dictionary<String, object>>> firstBlock)
    {
        // ルートのブロックを置く
        GameObject rootCircle = Instantiate(this.CircleBlock, new Vector3(float.Parse(rootBlock[rootDirName][0]["x"].ToString()), 3, float.Parse(rootBlock[rootDirName][0]["z"].ToString())), transform.rotation) as GameObject;
        rootCircle.transform.localScale = new Vector3(float.Parse(rootBlock[rootDirName][0]["radius"].ToString()) * 2, (float)2, float.Parse(rootBlock[rootDirName][0]["radius"].ToString()) * 2);
        //rootCircle.name = "Circle " + rootDirName;
        rootCircle.name = rootDirName.Substring(1);

        // メタ情報
        var rootData = rootCircle.GetComponent<BlockData>();
        rootData.pathname = rootDirName.Substring(1);
        rootData.blockname = "root";

        /*
        // お名前カンバン
        GameObject rootDirText = Instantiate(this.DirNamePlate, new Vector3(float.Parse(rootBlock[rootDirName][0]["x"].ToString()), 200, float.Parse(rootBlock[rootDirName][0]["z"].ToString())), transform.rotation) as GameObject;
        rootDirText.GetComponent<DirName>().SetNameText(rootData.blockname);
        rootDirText.GetComponent<DirName>().SetBackSize();
        */

        // ブロックを置く
        foreach (String key in firstBlock.Keys)
        {
            GameObject firstCircle = Instantiate(this.CircleBlock, new Vector3(float.Parse(firstBlock[key][0]["x"].ToString()), 3, float.Parse(firstBlock[key][0]["z"].ToString())), transform.rotation) as GameObject;
            firstCircle.transform.localScale = new Vector3(float.Parse(firstBlock[key][0]["radius"].ToString()) * 2, (float)2, float.Parse(firstBlock[key][0]["radius"].ToString()) * 2);
            
            //firstCircle.name = "Circle " + key;
            firstCircle.name = key.Substring(1);

            // メタ情報
            var circleData = firstCircle.GetComponent<BlockData>();
            circleData.pathname = key.Substring(1);
            circleData.blockname = key.Substring(key.ToString().LastIndexOf("/") + 1);

            // お名前カンバン
            GameObject dirtext = Instantiate(this.DirNamePlate, new Vector3(float.Parse(firstBlock[key][0]["x"].ToString()), 200, float.Parse(firstBlock[key][0]["z"].ToString())), transform.rotation) as GameObject;
            dirtext.GetComponent<DirName>().SetNameText(circleData.blockname);
            dirtext.GetComponent<DirName>().SetBackSize();
        }

        // ビルを建てる
        foreach (String key in building.Keys)
        {
            List<Dictionary<String, object>> buildingList = building[key];
            foreach (Dictionary<String, object> oneBuilding in buildingList)
            {
                //Debug.Log(oneBuilding["name"]);
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

                /*
                // お名前カンバン
                GameObject filetext = Instantiate(this.FileNamePlate, new Vector3(float.Parse(oneBuilding["globalX"].ToString()), float.Parse(oneBuilding["height"].ToString()) / 2, float.Parse(oneBuilding["globalZ"].ToString())), Quaternion.Euler(-90, 0, 0)) as GameObject;
                filetext.GetComponent<FileName>().SetNameText(buildingData.filename);
                filetext.GetComponent<FileName>().SetBackSize();
                */

                IList sList = oneBuilding["SATD"] as IList;
                if (sList.Count != 0)
                {

                    // SATDあるビルだけお名前カンバン
                    GameObject filetext = Instantiate(this.FileNamePlate, new Vector3(float.Parse(oneBuilding["globalX"].ToString()), float.Parse(oneBuilding["height"].ToString()) / 2, float.Parse(oneBuilding["globalZ"].ToString())), Quaternion.Euler(-90, 0, 0)) as GameObject;
                    filetext.GetComponent<FileName>().SetNameText(buildingData.filename);
                    filetext.GetComponent<FileName>().SetBackSize();

                    buildingObj.tag = "SATDBuilding";
                    buildingObj.GetComponent<Building>().SetMaterial(Color.blue);

                    for (int i = 0; i < sList.Count; i++)
                    {
                        buildingData.satd.Add(int.Parse(sList[i].ToString()) + 1);

                        buildingObj.name = buildingObj.name + (int.Parse(sList[i].ToString()) + 1).ToString() + ",";

                        // パーティクルの目印を作る
                        GameObject particle = Instantiate(this.sense, new Vector3(0, 1, 0), transform.rotation) as GameObject;
                        var r = particle.GetComponent<ParticleSystem>().shape;
                        r.radius = 50;

                        var s = particle.GetComponent<ParticleSystem>();

                        s.startSize = 20;

                        s.startSpeed = 50;


                        particle.transform.Rotate(new Vector3((float)270, (float)0, (float)0));
                        particle.transform.position = new Vector3(float.Parse(oneBuilding["globalX"].ToString()), (float.Parse(oneBuilding["height"].ToString()) - float.Parse(sList[i].ToString())) * (float)0.8845 + 3, float.Parse(oneBuilding["globalZ"].ToString()));
                        particle.name = "sence:" + oneBuilding["name"] + (int.Parse(sList[i].ToString())).ToString();
                        particle.layer = LayerMask.NameToLayer("Building");
                    }
                    buildingObj.name = buildingObj.name.Substring(0, buildingObj.name.Length - 1);
                }
            }
        }
    }

    void BuildCircle2(Dictionary<String, List<Dictionary<String, object>>> block)
    {
        foreach(String key in block.Keys)
        {
            GameObject firstCircle = Instantiate(this.CircleBlock, new Vector3(float.Parse(block[key][0]["x"].ToString()), -1000, float.Parse(block[key][0]["z"].ToString())), transform.rotation) as GameObject;
            firstCircle.transform.localScale = new Vector3(float.Parse(block[key][0]["radius"].ToString()) * 2, (float)2, float.Parse(block[key][0]["radius"].ToString()) * 2);

            //firstCircle.name = "Circle " + key;
            firstCircle.name = "Force##" + key.Substring(1);
        }
    }


    // 補色を計算する関数
   Color CalcComplementaryColor(Color original)
    {

        float max = original.r;
        float min = original.r;

        if(original.g > max)
        {
            max = original.g;
        }
        if(original.b > max)
        {
            max = original.b;
        }

        if(original.g < min)
        {
            min = original.g;
        }
        if (original.b < min)
        {
            min = original.b;
        }

        float sum = min + max;


        return new Color(sum - original.r, sum - original.g, sum - original.b);
    }


    // 地面を作る関数
    void SetGround(List<Dictionary<String, object>> target)
    {
        int maxX = 0;
        int maxY = 0;

        int minX = 0;
        int minY = 0;

        float positionX;
        float positionY;
        float widthX;
        float widthY;

        int space = 500;

        // 座標＋幅が一番大きい & 座標―幅が小さい、X座標とY（実際はZ）座標の番号を取ってくる
        if (target.Count >= 2)
        {
            for (int i = 1; i < target.Count; i++)
            {
                if (float.Parse(target[maxX]["x"].ToString()) + float.Parse(target[maxX]["widthX"].ToString()) / 2 < float.Parse(target[i]["x"].ToString()) + float.Parse(target[i]["widthX"].ToString()) / 2)
                {
                    maxX = i;
                }

                if (float.Parse(target[maxY]["y"].ToString()) + float.Parse(target[maxY]["widthY"].ToString()) / 2 < float.Parse(target[i]["y"].ToString()) + float.Parse(target[i]["widthY"].ToString()) / 2)
                {
                    maxY = i;
                }

                if(float.Parse(target[minX]["x"].ToString()) - float.Parse(target[minX]["widthX"].ToString()) / 2 > float.Parse(target[i]["x"].ToString()) - float.Parse(target[i]["widthX"].ToString()) / 2)
                {
                    minX = i;
                }
                if (float.Parse(target[minY]["y"].ToString()) - float.Parse(target[minY]["widthY"].ToString()) / 2 > float.Parse(target[i]["y"].ToString()) - float.Parse(target[i]["widthY"].ToString()) / 2)
                {
                    minY = i;
                }
            }
        }

        // XとY（ホントはZ）の座標と幅を決めていく
        if(float.Parse(target[minX]["x"].ToString()) != float.Parse(target[maxX]["x"].ToString()))
        {
            //positionX = float.Parse(target[minX]["x"].ToString()) / 2 + (float.Parse(target[maxX]["x"].ToString()) - float.Parse(target[minX]["x"].ToString())) / 2;
            positionX = (float.Parse(target[maxX]["x"].ToString()) + float.Parse(target[maxX]["widthX"].ToString()) / 2) - ((float.Parse(target[maxX]["x"].ToString()) + float.Parse(target[maxX]["widthX"].ToString()) / 2) - (float.Parse(target[minX]["x"].ToString()) - float.Parse(target[minX]["widthX"].ToString()) / 2)) / 2;
            widthX = (float.Parse(target[maxX]["x"].ToString()) - float.Parse(target[minX]["x"].ToString())) + float.Parse(target[minX]["widthX"].ToString()) / 2 + float.Parse(target[maxX]["widthX"].ToString()) / 2 + space;
        }
        else
        {
            positionX = float.Parse(target[minX]["x"].ToString());
            widthX = float.Parse(target[0]["widthX"].ToString()) + space;
        }

        if(float.Parse(target[minY]["y"].ToString()) != float.Parse(target[maxY]["y"].ToString()))
        {
            //positionY = float.Parse(target[minY]["y"].ToString()) / 2 + (float.Parse(target[maxY]["y"].ToString()) - float.Parse(target[minY]["y"].ToString())) / 2;
            positionY = (float.Parse(target[maxY]["y"].ToString()) + float.Parse(target[maxY]["widthY"].ToString()) / 2) - ((float.Parse(target[maxY]["y"].ToString()) + float.Parse(target[maxY]["widthY"].ToString()) / 2) - (float.Parse(target[minY]["y"].ToString()) - float.Parse(target[minY]["widthY"].ToString()) / 2)) / 2;
            widthY = (float.Parse(target[maxY]["y"].ToString()) - float.Parse(target[minY]["y"].ToString())) + float.Parse(target[minY]["widthY"].ToString()) / 2 + float.Parse(target[maxY]["widthY"].ToString()) / 2 + space;
        }
        else
        {
            positionY = float.Parse(target[minY]["y"].ToString());
            widthY = float.Parse(target[0]["widthY"].ToString()) + space;
        }


        earth.transform.position = new Vector3(positionX, float.Parse((-0.5).ToString()), positionY);
        earth.transform.localScale = new Vector3(widthX, 1, widthY);
        earth.name = "Ground";

        var a = earth.GetComponent<Renderer>().material;
        a.mainTextureScale = new Vector2(widthX / 100, widthY / 100);
        

        // カメラをスタートする
        GameObject obj = GameObject.Find("Main Camera");
        mainCamera = obj.GetComponent<CameraMove>();
        mainCamera.StartCamera();

    }


    void SetCircleGround(Dictionary<String, List<Dictionary<String, object>>> rootBlock, Dictionary<String, List<Dictionary<String, object>>> firstBlock)
    {
        float maxX = 0;
        float minX = 0;

        float maxZ = 0;
        float minZ = 0;

        string maxXKey = rootDirName;
        string minXKey = rootDirName;

        string maxZKey = rootDirName;
        string minZKey = rootDirName;

        foreach (String key in firstBlock.Keys)
        {
            if(maxX < float.Parse(firstBlock[key][0]["x"].ToString()) + float.Parse(firstBlock[key][0]["radius"].ToString()))
            {
                maxX = float.Parse(firstBlock[key][0]["x"].ToString()) + float.Parse(firstBlock[key][0]["radius"].ToString());
                maxXKey = key;
            }

            if(maxZ < float.Parse(firstBlock[key][0]["z"].ToString()) + float.Parse(firstBlock[key][0]["radius"].ToString()))
            {
                maxZ = float.Parse(firstBlock[key][0]["z"].ToString()) + float.Parse(firstBlock[key][0]["radius"].ToString());
                maxZKey = key;
            }

            if(minX > float.Parse(firstBlock[key][0]["x"].ToString()) - float.Parse(firstBlock[key][0]["radius"].ToString()))
            {
                minX = float.Parse(firstBlock[key][0]["x"].ToString()) - float.Parse(firstBlock[key][0]["radius"].ToString());
                minXKey = key;
            }

            if(minZ > float.Parse(firstBlock[key][0]["z"].ToString()) - float.Parse(firstBlock[key][0]["radius"].ToString()))
            {
                minZ = float.Parse(firstBlock[key][0]["z"].ToString()) - float.Parse(firstBlock[key][0]["radius"].ToString());
                minZKey = key;
            }
        }
        //GameObject ground = Instantiate(this.CircleGround, new Vector3(0,0,0), transform.rotation) as GameObject;

        if (maxXKey == rootDirName)
        {
            maxX = maxX + float.Parse(rootBlock[rootDirName][0]["radius"].ToString()) * 2 + 100;
        }
        else
        {
            maxX = maxX + 50;
        }

        if (maxZKey == rootDirName)
        {
            maxZ = maxZ + float.Parse(rootBlock[rootDirName][0]["radius"].ToString()) * 2 + 100;
        }
        else
        {
            maxZ = maxZ + 50;
        }

        if (minXKey == rootDirName)
        {
            minX = minX - float.Parse(rootBlock[rootDirName][0]["radius"].ToString()) * 2 - 100;
        }
        else
        {
            minX = minX - 50;
        }

        if (minZKey == rootDirName)
        {
            minZ = minZ - float.Parse(rootBlock[rootDirName][0]["radius"].ToString()) * 2 - 100;
        }
        else
        {
            minZ = minZ - 50;
        }

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



    // 道を作っていく関数
    void BuildStreets(List<Dictionary<String, object>> target)
    {
        // ブロックのリストを名前順にソートする
        target.Sort((b, a) => string.Compare(b["name"].ToString(), a["name"].ToString()));

        // 最後に見つかった前方一致するブロックの番号
        int foundLastBlock;

        // 前方一致したところ + 1から後ろのブロックの名前
        string afterName = "";


        // 1個上の階層のお名前
        string upperDir = "";

        int foundUpperDir;

        for (int i = 0; i < target.Count; i++)
        {
            foundLastBlock = -1;
            foundUpperDir = -1;

            upperDir = target[i]["name"].ToString().Substring(0, target[i]["name"].ToString().LastIndexOf("/"));
            //Debug.Log(upperDir);

            for (int j = 0; j < target.Count; j++)
            {
                // i番目のtargetのお名前と前方一致するか
                if (target[j]["name"].ToString().StartsWith(target[i]["name"].ToString()) == true)
                {
                    //Debug.Log("orig: " + target[i]["name"].ToString());
                    //Debug.Log("after: " + target[j]["name"].ToString());

                    if (j > i)
                    {
                        // 前方一致したところ+1から後ろのお名前を取得してみる
                        afterName = target[j]["name"].ToString().Substring(target[i]["name"].ToString().Length + 1);

                        // 前方一致した後のお名前に/がないとき
                        if (afterName.IndexOf("/") < 0)
                        {
                            foundLastBlock = j;
                        }
                    }
                }

                if(target[j]["name"].ToString() == upperDir)
                {
                    foundUpperDir = j;
                }
            }

            if (foundLastBlock >= 0)
            {
                // 前方一致したところ+1から後ろのお名前を取得
                afterName = target[foundLastBlock]["name"].ToString().Substring(target[i]["name"].ToString().Length + 1);
                //Debug.Log("*orig: " + target[i]["name"].ToString());
                //Debug.Log("*after: " + target[foundLastBlock]["name"].ToString());
                //Debug.Log("*cut: " + afterName);

                GameObject clone = Instantiate(this.street, new Vector3(((float.Parse(target[foundLastBlock]["x"].ToString()) + float.Parse(target[i]["x"].ToString())) / 2) + 25, (float)0, float.Parse(target[i]["y"].ToString())), transform.rotation) as GameObject;
                clone.transform.localScale = new Vector3(float.Parse(target[foundLastBlock]["x"].ToString()) - float.Parse(target[i]["x"].ToString()), 2, 50);
                clone.name = "streetX" + i.ToString();

            }

            if(foundUpperDir >= 0)
            {
                GameObject clone = Instantiate(this.street, new Vector3(float.Parse(target[i]["x"].ToString()), (float)0.1, ((float.Parse(target[i]["y"].ToString()) + float.Parse(target[foundUpperDir]["y"].ToString())) / 2) + 25), Quaternion.Euler(0,90,0)) as GameObject;
                clone.transform.localScale = new Vector3(float.Parse(target[i]["y"].ToString()) - float.Parse(target[foundUpperDir]["y"].ToString()) + 0, 2, 50);
                clone.name = "streetY" + i.ToString();
            }
        }
    }


    // 道を作っていく関数2
    // ブロックの中心じゃなくて辺に沿うようにおいていく
    void BuildStreets2(List<Dictionary<String, object>> target)
    {
        // ブロックのリストを名前順にソートする
        target.Sort((b, a) => string.Compare(b["name"].ToString(), a["name"].ToString()));

        // 最後に見つかった前方一致するブロックの番号
        int foundLastBlock;

        // 前方一致したところ + 1から後ろのブロックの名前
        string afterName = "";


        // 1個上の階層のお名前
        string upperDir = "";

        int foundUpperDir;

        for (int i = 0; i < target.Count; i++)
        {
            foundLastBlock = -1;
            foundUpperDir = -1;

            upperDir = target[i]["name"].ToString().Substring(0, target[i]["name"].ToString().LastIndexOf("/"));
            //Debug.Log(upperDir);

            for (int j = 0; j < target.Count; j++)
            {
                // i番目のtargetのお名前と前方一致するか
                if (target[j]["name"].ToString().StartsWith(target[i]["name"].ToString()) == true)
                {
                    //Debug.Log("orig: " + target[i]["name"].ToString());
                    //Debug.Log("after: " + target[j]["name"].ToString());

                    if (j > i)
                    {
                        // 前方一致したところから後ろのお名前を取得してみる
                        afterName = target[j]["name"].ToString().Substring(target[i]["name"].ToString().Length);

                        // 前方一致した後のお名前の先頭が'/'で、それ以外に'/'がないとき
                        if (afterName.Substring(0,1) == "/" && CountChar(afterName, '/') == 1)
                        {
                            foundLastBlock = j;
                        }
                    }
                }

                if (target[j]["name"].ToString() == upperDir)
                {
                    foundUpperDir = j;
                }
            }

            if (foundLastBlock >= 0 && float.Parse(target[foundLastBlock]["x"].ToString()) - float.Parse(target[foundLastBlock]["widthX"].ToString()) / 2 != float.Parse(target[i]["x"].ToString()) - float.Parse(target[i]["widthX"].ToString()) / 2)
            {
                // 前方一致したところ+1から後ろのお名前を取得
                afterName = target[foundLastBlock]["name"].ToString().Substring(target[i]["name"].ToString().Length + 1);
                //Debug.Log("*orig: " + target[i]["name"].ToString());
                //Debug.Log("*after: " + target[foundLastBlock]["name"].ToString());
                //Debug.Log("*cut: " + afterName);

                GameObject clone = Instantiate(this.street, new Vector3(((float.Parse(target[foundLastBlock]["x"].ToString()) - float.Parse(target[foundLastBlock]["widthX"].ToString()) / 2 + float.Parse(target[i]["x"].ToString()) - float.Parse(target[i]["widthX"].ToString()) / 2) / 2) - 25, (float)0.1, float.Parse(target[i]["y"].ToString()) + float.Parse(target[i]["widthY"].ToString()) / 2 + 25), transform.rotation) as GameObject;
                clone.transform.localScale = new Vector3(float.Parse(target[foundLastBlock]["x"].ToString()) - float.Parse(target[foundLastBlock]["widthX"].ToString()) / 2 - float.Parse(target[i]["x"].ToString()) + float.Parse(target[i]["widthX"].ToString()) / 2 + 50, 2, 50);
                //clone.name = "streetX" + i.ToString();
                clone.name = "streetX" + i.ToString() + ":" + target[i]["name"].ToString() + " to " + target[foundLastBlock]["name"].ToString();

                var a = clone.GetComponent<Renderer>().material;
                a.mainTextureScale = new Vector2((float.Parse(target[foundLastBlock]["x"].ToString()) - float.Parse(target[foundLastBlock]["widthX"].ToString()) / 2 - float.Parse(target[i]["x"].ToString()) + float.Parse(target[i]["widthX"].ToString()) / 2 + 50) / 100, 1);

            }

            if (foundUpperDir >= 0)
            {
                //GameObject clone = Instantiate(this.street, new Vector3(float.Parse(target[i]["x"].ToString()) - float.Parse(target[i]["widthX"].ToString()) / 2 - 25, (float)0.1, ((float.Parse(target[i]["y"].ToString()) + float.Parse(target[i]["widthY"].ToString()) / 2 + float.Parse(target[foundUpperDir]["y"].ToString()) - float.Parse(target[foundUpperDir]["widthY"].ToString()) / 2) / 2) - 0), Quaternion.Euler(0, 90, 0)) as GameObject;
                //clone.transform.localScale = new Vector3(float.Parse(target[i]["y"].ToString()) + float.Parse(target[i]["widthY"].ToString()) / 2 - float.Parse(target[foundUpperDir]["y"].ToString()) + float.Parse(target[foundUpperDir]["widthY"].ToString()) / 2 + 0, 2, 50);
                GameObject clone = Instantiate(this.street, new Vector3(float.Parse(target[i]["x"].ToString()) - float.Parse(target[i]["widthX"].ToString()) / 2 - 25, (float)0, float.Parse(target[i]["y"].ToString()) - 25), Quaternion.Euler(0, 90, 0)) as GameObject;
                clone.transform.localScale = new Vector3(float.Parse(target[i]["widthY"].ToString()) + 50, 2, 50);
                clone.name = "streetY" + i.ToString() + ":" + target[i]["name"].ToString() + " to " + target[foundUpperDir]["name"].ToString();

                var a = clone.GetComponent<Renderer>().material;
                a.mainTextureScale = new Vector2((float.Parse(target[i]["widthY"].ToString()) + 50) / 100, 1);

            }
        }
    }

    void BuildCircleStreet(Dictionary<String, List<Dictionary<String, object>>> rootBlock, Dictionary<String, List<Dictionary<String, object>>> firstBlock)
    {
        float centerX = float.Parse(rootBlock[rootDirName][0]["x"].ToString());
        float centerZ = float.Parse(rootBlock[rootDirName][0]["z"].ToString());

        int num = 0;

        foreach(String key in firstBlock.Keys)
        {
            float x = float.Parse(firstBlock[key][0]["x"].ToString());
            float z = float.Parse(firstBlock[key][0]["z"].ToString());

            float midX = (centerX + x) / 2;
            float midZ = (centerZ + z) / 2;

            Vector2 vec = new Vector2(centerX, centerZ) - new Vector2(x, z);
            float distance = vec.magnitude;

            float deg = 360 / (float)firstBlock.Count;
            double rad = deg * Mathf.PI / 180.0;


            GameObject street = Instantiate(this.street, new Vector3(midX, 2, midZ), Quaternion.Euler(0, -(float)rad * num * Mathf.Rad2Deg, 0)) as GameObject;
            street.transform.localScale = new Vector3(distance, 1, 50);
            street.name = "To:" + firstBlock[key][0]["name"].ToString();

            var a = street.GetComponent<Renderer>().material;
            a.mainTextureScale = new Vector2(distance / 100, 1);

            num++;
        }
    }

    void BuildCircleStreet2(Dictionary<String, List<Dictionary<String, object>>> rootBlock, Dictionary<String, List<Dictionary<String, object>>> firstBlock, String root)
    {
        float centerX = float.Parse(rootBlock[root][0]["x"].ToString());
        float centerZ = float.Parse(rootBlock[root][0]["z"].ToString());

        //int num = 0;

        // 0度のベクトル
        Vector2 uVec = new Vector2(1, 0);

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


    void nori_rogic_ver2 (IList blocks, IList buildings)
	{
		Dictionary<string,int> block_ID_Dic = new Dictionary<string,int> ();
		
		Dictionary<int,int> building_step_cnt = new Dictionary<int,int> ();
		
		Dictionary<int,int> building_cnt = new Dictionary<int,int> ();
		Dictionary<int,int> block_cnt = new Dictionary<int,int> ();
		
		Dictionary<int,float> x_cnt = new Dictionary<int,float> ();
		Dictionary<int,float> z_cnt = new Dictionary<int,float> ();
		
		Dictionary<int,float> s_point_x = new Dictionary<int,float> ();
		Dictionary<int,float> s_point_z = new Dictionary<int,float> ();
		Dictionary<int,float> e_point_x = new Dictionary<int,float> ();
		Dictionary<int,float> e_point_z = new Dictionary<int,float> ();
		
		List<sort_block> sorted_block_list = new List<sort_block> ();
		List<sort_block> sorted_block_list_temp = new List<sort_block> ();
		List<sort_building> sorted_building_list = new List<sort_building> ();
		List<x_hold> x_list = new List<x_hold> ();
		List<x_hold> x_list_temp = new List<x_hold> ();
		List<z_hold> z_list = new List<z_hold> ();
		List<z_hold> z_list_temp = new List<z_hold> ();
		
		List<x_hold> b_x_list = new List<x_hold> ();
		List<x_hold> b_x_list_temp = new List<x_hold> ();
		List<z_hold> b_z_list = new List<z_hold> ();
		List<z_hold> b_z_list_temp = new List<z_hold> ();
		
		int cnt = 0;
		int b_cnt = 0;
		int block_step_cnt = 0;
		
		float x_pos = 0;
		float y_pos = 0;
		float z_pos = 0;
		
		float x = 0;
		float z = 0;
		
		float se_x = 0;
		float se_z = 0;
		
		Instantiate(this.ground,new Vector3 (0, 0, 0),transform.rotation);
		/* step.1 */
		
		cnt = 0;
		
		foreach(Dictionary<string,object> block in blocks){
			block_ID_Dic.Add (block["name"].ToString(),cnt);
			building_step_cnt.Add (cnt,0);
			block_cnt.Add (cnt,1);
			building_cnt.Add (cnt,1);
			s_point_x.Add (cnt,0);
			s_point_z.Add (cnt,0);
			e_point_x.Add (cnt,0);
			e_point_z.Add (cnt,0);
			
			x_cnt.Add (cnt,0);
			z_cnt.Add (cnt,0);
			
			sorted_block_list.Add (new sort_block(){block_ID=cnt,block_name=block["name"].ToString (),x_width=0,z_width=0,x_pos=0,y_pos=0,z_pos=0});
			cnt++;
		}
		
		/* step.2 */
		
		foreach (Dictionary<string,object> building in buildings) {	
			float width = float.Parse (building ["width"].ToString ());
			float height = float.Parse (building ["height"].ToString ());
			string block_name = building ["block"].ToString ();
			string building_name = building["name"].ToString ();
			
			float building_color_r = float.Parse (building["color_r"].ToString ());
			float building_color_g = float.Parse (building["color_g"].ToString ());
			float building_color_b = float.Parse (building["color_b"].ToString ());
			
			
			sorted_building_list.Add (new sort_building(){block_ID=block_ID_Dic[block_name],building_name=building_name,width=width,height=height,x_pos=width/2,y_pos=2,z_pos=width/2,color_r=building_color_r,color_g=building_color_g,color_b=building_color_b});
		}
		
		sorted_building_list.Sort ((b,a) => (int)a.width - (int)b.width);
		
		/* step.3 */
		
		cnt = 0;
		
		foreach(sort_building building_pos in sorted_building_list){
			
			/* 1 */
			
			if(building_cnt[building_pos.block_ID]==1){
				
				/* building position */
				building_pos.x_pos=0;
				building_pos.z_pos=0;
				/* block width */
				sorted_block_list[building_pos.block_ID].x_width=building_pos.width+10;
				sorted_block_list[building_pos.block_ID].z_width=building_pos.width+10;
				
				x_list.Add (new x_hold(){block_ID=building_pos.block_ID,building_step_cnt=building_step_cnt[building_pos.block_ID],x=building_pos.x_pos,width=building_pos.width});
				z_list.Add (new z_hold(){block_ID=building_pos.block_ID,building_step_cnt=building_step_cnt[building_pos.block_ID],z=building_pos.z_pos,width=building_pos.width});
				
				e_point_x[building_pos.block_ID]=0;
				e_point_z[building_pos.block_ID]=0;
				
				/* building cnt 1 -> 2 */
				building_cnt[building_pos.block_ID]++;
				/* building step cnt 0 -> 1 */
				building_step_cnt[building_pos.block_ID]++;
			}
			
			/* 5 */
			
			else if(building_cnt[building_pos.block_ID]==Math.Pow(building_step_cnt[building_pos.block_ID],2)+1){
				
				x_list_temp=x_list.FindAll(e => e.block_ID == building_pos.block_ID && e.building_step_cnt == 0);
				z_list_temp=z_list.FindAll(f => f.block_ID == building_pos.block_ID && f.building_step_cnt == building_step_cnt[building_pos.block_ID]-1);
				
				/* building position */
				building_pos.x_pos=x_list_temp[0].x;
				building_pos.z_pos=z_list_temp[0].z+(z_list_temp[0].width/2)+10+(building_pos.width/2);
				
				/* block width */
				sorted_block_list[building_pos.block_ID].z_width += building_pos.width+10;
				
				e_point_z[building_pos.block_ID]+=(building_pos.width+10)/2;
				
				z_list.Add (new z_hold(){block_ID=building_pos.block_ID,building_step_cnt=building_step_cnt[building_pos.block_ID],z=building_pos.z_pos,width=building_pos.width});
				
				x_list_temp.RemoveAll(e => e.block_ID == building_pos.block_ID && e.building_step_cnt == building_step_cnt[building_pos.block_ID]-1);
				z_list_temp.RemoveAll(f => f.block_ID == building_pos.block_ID && f.building_step_cnt == building_step_cnt[building_pos.block_ID]-1);
				
				building_cnt[building_pos.block_ID]++;
			}
			
			/* 6 */
			
			else if(building_cnt[building_pos.block_ID]>Math.Pow(building_step_cnt[building_pos.block_ID],2)+1 && building_cnt[building_pos.block_ID]<Math.Pow(building_step_cnt[building_pos.block_ID],2)+building_step_cnt[building_pos.block_ID]+1){
				x_list_temp=x_list.FindAll(e => e.block_ID == building_pos.block_ID && e.building_step_cnt == building_step_cnt[building_pos.block_ID]-(Math.Pow(building_step_cnt[building_pos.block_ID],2)+building_step_cnt[building_pos.block_ID]+1-building_cnt[building_pos.block_ID]));
				z_list_temp=z_list.FindAll(f => f.block_ID == building_pos.block_ID && f.building_step_cnt == building_step_cnt[building_pos.block_ID]);
				
				building_pos.x_pos=x_list_temp[0].x;
				building_pos.z_pos=z_list_temp[0].z;
				
				x_list_temp.RemoveAll(e => e.block_ID == building_pos.block_ID && e.building_step_cnt == building_step_cnt[building_pos.block_ID]-(Math.Pow(building_step_cnt[building_pos.block_ID],2)+building_step_cnt[building_pos.block_ID]+1-building_cnt[building_pos.block_ID]));
				z_list_temp.RemoveAll(f => f.block_ID == building_pos.block_ID && f.building_step_cnt == building_step_cnt[building_pos.block_ID]);
				
				building_cnt[building_pos.block_ID]++;
			}
			
			/* 7 */
			
			else if(building_cnt[building_pos.block_ID]==Math.Pow(building_step_cnt[building_pos.block_ID],2)+building_step_cnt[building_pos.block_ID]+1){
				x_list_temp=x_list.FindAll(e => e.block_ID == building_pos.block_ID && e.building_step_cnt == building_step_cnt[building_pos.block_ID]-1);
				z_list_temp=z_list.FindAll(f => f.block_ID == building_pos.block_ID && f.building_step_cnt == building_step_cnt[building_pos.block_ID]);
				
				building_pos.x_pos=x_list_temp[0].x+(x_list_temp[0].width/2)+10+(building_pos.width/2);
				building_pos.z_pos=z_list_temp[0].z;
				
				sorted_block_list[building_pos.block_ID].x_width += building_pos.width+10;
				
				e_point_x[building_pos.block_ID]+=(building_pos.width+10)/2;
				
				x_list.Add (new x_hold(){block_ID=building_pos.block_ID,building_step_cnt=building_step_cnt[building_pos.block_ID],x=building_pos.x_pos,width=building_pos.width});
				
				x_list_temp.RemoveAll(e => e.block_ID == building_pos.block_ID && e.building_step_cnt == building_step_cnt[building_pos.block_ID]-1);
				z_list_temp.RemoveAll(f => f.block_ID == building_pos.block_ID && f.building_step_cnt == building_step_cnt[building_pos.block_ID]);
				
				building_cnt[building_pos.block_ID]++;
			}
			
			/* 8 */
			
			else if(building_cnt[building_pos.block_ID]>Math.Pow(building_step_cnt[building_pos.block_ID],2)+building_step_cnt[building_pos.block_ID]+1 && building_cnt[building_pos.block_ID]<Math.Pow(building_step_cnt[building_pos.block_ID]+1,2)){
				x_list_temp=x_list.FindAll(e => e.block_ID == building_pos.block_ID && e.building_step_cnt == building_step_cnt[building_pos.block_ID]);
				z_list_temp=z_list.FindAll(f => f.block_ID == building_pos.block_ID && f.building_step_cnt == building_step_cnt[building_pos.block_ID]-(building_cnt[building_pos.block_ID]-(Math.Pow(building_step_cnt[building_pos.block_ID],2)+building_step_cnt[building_pos.block_ID]+1)));
				
				building_pos.x_pos=x_list_temp[0].x;
				building_pos.z_pos=z_list_temp[0].z;
				
				x_list_temp.RemoveAll(e => e.block_ID == building_pos.block_ID && e.building_step_cnt == building_step_cnt[building_pos.block_ID]);
				z_list_temp.RemoveAll(f => f.block_ID == building_pos.block_ID && f.building_step_cnt == building_step_cnt[building_pos.block_ID]-(building_cnt[building_pos.block_ID]-(Math.Pow(building_step_cnt[building_pos.block_ID],2)+building_step_cnt[building_pos.block_ID]+1)));
				
				building_cnt[building_pos.block_ID]++;
			}
			
			/* 9 */
			
			else if(building_cnt[building_pos.block_ID]==Math.Pow(building_step_cnt[building_pos.block_ID]+1,2)){
				
				x_list_temp=x_list.FindAll(e => e.block_ID == building_pos.block_ID && e.building_step_cnt == building_step_cnt[building_pos.block_ID]);
				z_list_temp=z_list.FindAll(f => f.block_ID == building_pos.block_ID && f.building_step_cnt == 0);
				
				/* building position */
				building_pos.x_pos=x_list_temp[0].x;
				building_pos.z_pos=z_list_temp[0].z;
				
				x_list_temp.RemoveAll(e => e.block_ID == building_pos.block_ID && e.building_step_cnt == building_step_cnt[building_pos.block_ID]);
				z_list_temp.RemoveAll(f => f.block_ID == building_pos.block_ID && f.building_step_cnt == 0);
				
				/* building cnt n -> n+1 */
				building_cnt[building_pos.block_ID]++;
				/* building step cnt m -> m+1 */
				building_step_cnt[building_pos.block_ID]++;
			}
			
			else{
				Debug.Log("aho");
			}
			
			sorted_building_list[cnt].x_pos=building_pos.x_pos;
			sorted_building_list[cnt].z_pos=building_pos.z_pos;
			
			cnt++;
		}
		
		/* step.4 */
		
		sorted_block_list.Sort ((b,a) => (int)a.z_width - (int)b.z_width);
		
		cnt = 0;
		
		Debug.Log(sorted_block_list[0].block_ID);
		
		foreach(sort_block block_pos in sorted_block_list){
			Debug.Log(block_pos.block_ID);
			Debug.Log(block_pos.z_width);
			
			/* 1 */
			
			if(block_cnt[0]==1){
				
				/* building position */
				block_pos.x_pos=0;
				block_pos.z_pos=0;
				/* block width */
				
				b_x_list.Add (new x_hold(){block_ID=0,building_step_cnt=b_cnt,x=block_pos.x_pos,width=block_pos.z_width});
				b_z_list.Add (new z_hold(){block_ID=0,building_step_cnt=b_cnt,z=block_pos.z_pos,width=block_pos.z_width});
				
				/* building cnt 1 -> 2 */
				block_cnt[0]++;
				/* building step cnt 0 -> 1 */
				b_cnt++;
			}
			
			/* 5 */
			
			else if(block_cnt[0]==Math.Pow(b_cnt,2)+1){
				
				b_x_list_temp=b_x_list.FindAll(e => e.building_step_cnt == 0);
				b_z_list_temp=b_z_list.FindAll(f => f.building_step_cnt == b_cnt-1);
				
				/* building position */
				block_pos.x_pos=b_x_list_temp[0].x;
				block_pos.z_pos=b_z_list_temp[0].z+(b_x_list_temp[0].width/2)+10+(block_pos.z_width/2);
				
				b_z_list.Add (new z_hold(){block_ID=0,building_step_cnt=b_cnt,z=block_pos.z_pos,width=block_pos.z_width});
				
				b_x_list_temp.RemoveAll(e => e.building_step_cnt == 0);
				b_z_list_temp.RemoveAll(f => f.building_step_cnt == b_cnt-1);
				
				block_cnt[0]++;
			}
			
			/* 6 */
			
			else if(block_cnt[0]>Math.Pow(b_cnt,2)+1 && block_cnt[0]<Math.Pow(b_cnt,2)+b_cnt+1){
				b_x_list_temp=b_x_list.FindAll(e => e.building_step_cnt == b_cnt-(Math.Pow(b_cnt,2)+b_cnt+1-block_cnt[0]));
				b_z_list_temp=b_z_list.FindAll(f => f.building_step_cnt == b_cnt);
				
				block_pos.x_pos=b_x_list_temp[0].x;
				block_pos.z_pos=b_z_list_temp[0].z;
				
				b_x_list_temp.RemoveAll(e => e.building_step_cnt == b_cnt-(Math.Pow(b_cnt,2)+b_cnt+1-block_cnt[0]));
				b_z_list_temp.RemoveAll(f => f.building_step_cnt == b_cnt);
				
				block_cnt[0]++;
			}
			
			/* 7 */
			
			else if(block_cnt[0]==Math.Pow(b_cnt,2)+b_cnt+1){
				b_x_list_temp=b_x_list.FindAll(e => e.building_step_cnt == b_cnt-1);
				b_z_list_temp=b_z_list.FindAll(f => f.building_step_cnt == b_cnt);
				
				block_pos.x_pos=b_x_list_temp[0].x+(b_z_list_temp[0].width/2)+10+(block_pos.z_width/2);
				block_pos.z_pos=b_z_list_temp[0].z;
				
				b_x_list.Add (new x_hold(){block_ID=0,building_step_cnt=b_cnt,x=block_pos.x_pos,width=block_pos.z_width});
				
				b_x_list_temp.RemoveAll(e => e.building_step_cnt == b_cnt-1);
				b_z_list_temp.RemoveAll(f => f.building_step_cnt == b_cnt);
				
				block_cnt[0]++;
			}
			
			/* 8 */
			
			else if(block_cnt[0]>Math.Pow(b_cnt,2)+b_cnt+1 && block_cnt[0]<Math.Pow(b_cnt+1,2)){
				b_x_list_temp=b_x_list.FindAll(e => e.building_step_cnt == b_cnt);
				b_z_list_temp=b_z_list.FindAll(f => f.building_step_cnt == b_cnt-(block_cnt[0]-(Math.Pow(b_cnt,2)+b_cnt+1)));
				
				block_pos.x_pos=b_x_list_temp[0].x;
				block_pos.z_pos=b_z_list_temp[0].z;
				
				b_x_list_temp.RemoveAll(e => e.building_step_cnt == b_cnt);
				b_z_list_temp.RemoveAll(f => f.building_step_cnt == b_cnt-(block_cnt[0]-(Math.Pow(b_cnt,2)+b_cnt+1)));
				
				block_cnt[0]++;
			}
			
			/* 9 */
			
			else if(block_cnt[0]==Math.Pow(b_cnt+1,2)){
				
				b_x_list_temp=b_x_list.FindAll(e => e.building_step_cnt == b_cnt);
				b_z_list_temp=b_z_list.FindAll(f => f.building_step_cnt == 0);
				
				/* building position */
				block_pos.x_pos=b_x_list_temp[0].x;
				block_pos.z_pos=b_z_list_temp[0].z;
				
				b_x_list_temp.RemoveAll(e => e.building_step_cnt == b_cnt);
				b_z_list_temp.RemoveAll(f => f.building_step_cnt == 0);
				
				/* building cnt n -> n+1 */
				block_cnt[0]++;
				/* building step cnt m -> m+1 */
				b_cnt++;
			}
			
			else{
				Debug.Log("aho");
			}
			
			sorted_block_list[cnt].x_pos=block_pos.x_pos;
			sorted_block_list[cnt].z_pos=block_pos.z_pos;
			
			cnt++;
			
		}
		
		Debug.Log(sorted_block_list[0].block_ID);
		Debug.Log(sorted_block_list[0].x_pos);
		
		cnt = 0;
		
		foreach (sort_block block_pos in sorted_block_list) {
			
			GameObject clone = Instantiate(this.building,new Vector3 (block_pos.x_pos, 1, block_pos.z_pos),transform.rotation) as GameObject;
			clone.name = block_pos.block_name;
			clone.transform.localScale = new Vector3 (block_pos.x_width, 2, block_pos.z_width);
		}
		
		cnt = 0;
		
		/* step.5 */
		
		Debug.Log ("---");
		
		foreach (sort_building building_pos in sorted_building_list) {
			
			//	se_x=(e_point_x[building_pos.block_ID]-s_point_x[building_pos.block_ID])/2;
			//	se_z=(e_point_z[building_pos.block_ID]-s_point_z[building_pos.block_ID])/2;
			
			sorted_block_list_temp=sorted_block_list.FindAll(d => d.block_ID == building_pos.block_ID);
			
			x_list_temp=x_list.FindAll(e => e.block_ID == building_pos.block_ID && e.building_step_cnt == 0);
			z_list_temp=z_list.FindAll(f => f.block_ID == building_pos.block_ID && f.building_step_cnt == 0);
			
			//GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
			GameObject clone = Instantiate(this.building,new Vector3 (building_pos.x_pos+sorted_block_list_temp[0].x_pos-e_point_x[building_pos.block_ID], (building_pos.height/2)+2, building_pos.z_pos+sorted_block_list_temp[0].z_pos-e_point_z[building_pos.block_ID]),transform.rotation) as GameObject;
			clone.name = building_pos.building_name;
			clone.transform.localScale = new Vector3 (building_pos.width, building_pos.height, building_pos.width);
			//clone.GetComponent<Renderer>().material.color = Color.blue;
			clone.GetComponent<Renderer>().material.color = new Color(building_pos.color_r,building_pos.color_g,building_pos.color_b);
			
			sorted_block_list_temp.RemoveAll(d => d.block_ID == building_pos.block_ID);
			
			x_list_temp.RemoveAll(e => e.block_ID == building_pos.block_ID && e.building_step_cnt == 0);
			z_list_temp.RemoveAll(f => f.block_ID == building_pos.block_ID && f.building_step_cnt == 0);
			
		}
		
	}
	
	
	void nori_rogic (IList blocks, IList buildings)
	{
		
		/*
				GameObject plate = GameObject.CreatePrimitive (PrimitiveType.Cube);
				plate.transform.localScale = new Vector3 (10000, 1, 10000);
				plate.transform.position = new Vector3 (0, 0, 0);
*/
		// add
		
		Dictionary<string,int> block_ID_Dic = new Dictionary<string,int> ();
		Dictionary<int,int> building_cnt = new Dictionary<int,int> ();
		Dictionary<int,int> block_cnt = new Dictionary<int,int> ();
		Dictionary<int,float> x_cnt = new Dictionary<int,float> ();
		Dictionary<int,float> z_cnt = new Dictionary<int,float> ();
		Dictionary<int,float> s_point_x = new Dictionary<int,float> ();
		Dictionary<int,float> s_point_z = new Dictionary<int,float> ();
		Dictionary<int,float> e_point_x = new Dictionary<int,float> ();
		Dictionary<int,float> e_point_z = new Dictionary<int,float> ();
		List<sort_block> sorted_block_list = new List<sort_block> ();
		List<sort_building> sorted_building_list = new List<sort_building>();
		
		int cnt = 0;
		float zero = 0;
		float two = 2;
		float x_pos = 0;
		float y_pos = 0;
		float z_pos = 0;
		
		float x = 0;
		float z = 0;
		
		float se_x = 0;
		float se_z = 0;
		
		//
		
		/* sec.1 */
		cnt = 0;
		foreach(Dictionary<string,object> block in blocks){
			block_ID_Dic.Add (block["name"].ToString(),cnt);
			building_cnt.Add (cnt,1);
			block_cnt.Add (cnt,1);
			s_point_x.Add (cnt,0);
			s_point_z.Add (cnt,0);
			e_point_x.Add (cnt,0);
			e_point_z.Add (cnt,0);
			
			x_cnt.Add (cnt,0);
			z_cnt.Add (cnt,0);
			
			sorted_block_list.Add (new sort_block(){block_ID=cnt,block_name=block["name"].ToString (),x_width=zero,z_width=zero,x_pos=zero,y_pos=zero,z_pos=zero});
			cnt++;
		}
		
		/* sec.2 */
		
		foreach (Dictionary<string,object> building in buildings) {	
			float width = float.Parse (building ["width"].ToString ());
			float height = float.Parse (building ["height"].ToString ());
			string block_name = building ["block"].ToString ();
			string building_name = building["name"].ToString ();
			
			sorted_building_list.Add (new sort_building(){block_ID=block_ID_Dic[block_name],building_name=building_name,width=width,height=height,x_pos=width/two,y_pos=two,z_pos=width/two});
		}
		
		sorted_building_list.Sort ((b,a) => (int)a.width - (int)b.width);
		
		cnt = 0;
		
		foreach(sort_building building_pos in sorted_building_list){
			
			if(building_cnt[building_pos.block_ID]==1){
				building_pos.x_pos=(building_pos.width+10)/2;
				building_pos.z_pos=(building_pos.width+10)/2;
				sorted_block_list[building_pos.block_ID].x_width=building_pos.width+10;
				sorted_block_list[building_pos.block_ID].z_width=building_pos.width+10;
				
				s_point_x[building_pos.block_ID]=building_pos.x_pos;
				s_point_z[building_pos.block_ID]=building_pos.z_pos;
				
				e_point_x[building_pos.block_ID]=building_pos.x_pos;
				e_point_z[building_pos.block_ID]=building_pos.z_pos;
				
				building_cnt[building_pos.block_ID]++;
			}
			
			else if(building_cnt[building_pos.block_ID]==2){
				building_pos.x_pos=sorted_block_list[building_pos.block_ID].x_width/2;
				building_pos.z_pos=sorted_block_list[building_pos.block_ID].z_width+(building_pos.width+10)/2;
				sorted_block_list[building_pos.block_ID].z_width += building_pos.width+10;
				
				e_point_x[building_pos.block_ID]=building_pos.x_pos;
				e_point_z[building_pos.block_ID]=building_pos.z_pos;
				
				building_cnt[building_pos.block_ID]++;
				
				z_cnt[building_pos.block_ID]=building_pos.z_pos;
			}
			
			else if(building_cnt[building_pos.block_ID]==3){
				building_pos.x_pos=sorted_block_list[building_pos.block_ID].x_width+(building_pos.width+10)/2;
				building_pos.z_pos=z_cnt[building_pos.block_ID];
				sorted_block_list[building_pos.block_ID].x_width += building_pos.width+10;
				
				e_point_x[building_pos.block_ID]=building_pos.x_pos;
				e_point_z[building_pos.block_ID]=building_pos.z_pos;
				
				building_cnt[building_pos.block_ID]++;
				
				x_cnt[building_pos.block_ID]=building_pos.x_pos;
			}
			
			else if(building_cnt[building_pos.block_ID]==4){
				building_pos.x_pos=x_cnt[building_pos.block_ID];
				building_pos.z_pos=sorted_block_list[building_pos.block_ID].z_width/2;
				building_cnt[building_pos.block_ID]++;
			}
			
			else{
				Debug.Log(building_cnt[building_pos.block_ID]);
			}
			
			sorted_building_list[cnt].x_pos=building_pos.x_pos;
			sorted_building_list[cnt].z_pos=building_pos.z_pos;
			
			cnt++;
		}
		
		cnt = 0;
		
		foreach(sort_block block_pos in sorted_block_list){
			
			if((block_pos.block_ID)+1==1){
				block_pos.x_pos=block_pos.x_width/2;
				block_pos.z_pos=block_pos.z_width/2;
				
				sorted_block_list[cnt].x_pos=block_pos.x_width/2;
				sorted_block_list[cnt].z_pos=block_pos.z_width/2;
				
				x=block_pos.x_pos;
				z=block_pos.z_pos;
				
				block_cnt[block_pos.block_ID]++;
			}
			
			else if((block_pos.block_ID)+1==2){
				block_pos.x_pos=block_pos.x_width/2;
				block_pos.z_pos=block_pos.z_width/2+50+z;
				
				sorted_block_list[cnt].x_pos=block_pos.x_width/2;
				sorted_block_list[cnt].z_pos=block_pos.z_width/2+50+z;
				
				x=block_pos.x_pos;
				z=block_pos.z_width+50+z;
				
				block_cnt[block_pos.block_ID]++;
			}
			
			else if((block_pos.block_ID)+1==3){
				block_pos.x_pos=block_pos.x_width/2;
				block_pos.z_pos=block_pos.z_width/2+50+z;
				
				sorted_block_list[cnt].x_pos=block_pos.x_width/2;
				sorted_block_list[cnt].z_pos=block_pos.z_width/2+50+z;
				
				x=block_pos.x_pos;
				z=block_pos.z_width+50+z;
				
				block_cnt[block_pos.block_ID]++;
			}
			
			else if((block_pos.block_ID)+1==4){
				block_pos.x_pos=block_pos.x_width/2;
				block_pos.z_pos=block_pos.z_width/2+50+z;
				
				sorted_block_list[cnt].x_pos=block_pos.x_width/2;
				sorted_block_list[cnt].z_pos=block_pos.z_width/2+50+z;
				
				x=block_pos.x_pos;
				z=block_pos.z_width+50+z;
				
				block_cnt[block_pos.block_ID]++;
			}
			
			else{
				Debug.Log(building_cnt[block_pos.block_ID]);
			}
			GameObject clone = Instantiate(this.building,new Vector3 (block_pos.x_pos, 1, block_pos.z_pos),transform.rotation) as GameObject;
			clone.transform.localScale = new Vector3 (block_pos.x_width, 2, block_pos.z_width);
			cnt++;
			
		}
		
		
		
		foreach (sort_building building_pos in sorted_building_list) {
			
			se_x=(e_point_x[building_pos.block_ID]-s_point_x[building_pos.block_ID])/2;
			se_z=(e_point_z[building_pos.block_ID]-s_point_z[building_pos.block_ID])/2;
			/*
					GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
					cube.name = building_pos.building_name;
					cube.transform.localScale = new Vector3 (building_pos.width, building_pos.height, building_pos.width);
					cube.transform.position = new Vector3 (building_pos.x_pos-s_point_x[building_pos.block_ID]+sorted_block_list[building_pos.block_ID].x_pos-se_x, (building_pos.height/2)+2, building_pos.z_pos-s_point_z[building_pos.block_ID]+sorted_block_list[building_pos.block_ID].z_pos-se_z);
					*/
		}
		
		/* sec.3 */
		/*				
				foreach (Dictionary<string,object> block in blocks) {
					y +=  maxW[block ["name"].ToString ()]/2 ;
					maxX.Add (block ["name"].ToString (), 0);
					maxY.Add (block ["name"].ToString (), y);
					y +=  maxW[block ["name"].ToString ()]/2+ 20;
				}
*/				
		/* sec.4 */
		/*				
				foreach (Dictionary<string,object> building in buildings) {
					var block = building ["block"].ToString ();
					var width = float.Parse (building ["width"].ToString ());
					var height = float.Parse (building ["height"].ToString ());
					var name = building ["name"].ToString ();
					GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
					cube.name = name;
					cube.transform.localScale = new Vector3 (width, height, width);
					cube.transform.position = new Vector3 (maxX [block]+ width/2, height / 2, maxY [block]);
					maxX [block] += width + 20;
					maxW [block] = System.Math.Max (width, maxW [block]);
				}
*/				
		/* sec.5 */
		/*		
				foreach (Dictionary<string,object> block in blocks) {
					var name = block ["name"].ToString ();
					GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
					cube.transform.localScale = new Vector3 (maxX [name] +10, 2, maxW [name] + 10);
					cube.transform.position = new Vector3 (maxX [name] / 2, 1, maxY [name]);
				}		
*/
	}
	
	void Simple (IList blocks, IList buildings)
	{
		Dictionary<string,float> maxX = new Dictionary<string, float> ();
		Dictionary<string,float> maxY = new Dictionary<string, float> ();
		Dictionary<string,float> maxW = new Dictionary<string, float> ();
		GameObject plate = GameObject.CreatePrimitive (PrimitiveType.Cube);
		plate.transform.localScale = new Vector3 (10000, 1, 10000);
		plate.transform.position = new Vector3 (0, 0, 0);
		float y = 0;
		
		/* sec.1 */
		
		foreach(Dictionary<string,object> block in blocks){
			maxW.Add (block["name"].ToString(),0);
		}
		
		/* sec.2 */
		
		foreach (Dictionary<string,object> building in buildings) {	
			var width = float.Parse (building ["width"].ToString ());
			var name = building ["block"].ToString ();
			Debug.Log(name);
			maxW[name] = System.Math.Max (width, maxW [name]);
			
		}
		
		/* sec.3 */
		
		foreach (Dictionary<string,object> block in blocks) {
			y +=  maxW[block ["name"].ToString ()]/2 ;
			maxX.Add (block ["name"].ToString (), 0);
			maxY.Add (block ["name"].ToString (), y);
			y +=  maxW[block ["name"].ToString ()]/2+ 20;
		}
		
		/* sec.4 */
		
		foreach (Dictionary<string,object> building in buildings) {
			var block = building ["block"].ToString ();
			var width = float.Parse (building ["width"].ToString ());
			var height = float.Parse (building ["height"].ToString ());
			var name = building ["name"].ToString ();
			
			GameObject clone = Instantiate(this.building,new Vector3 (maxX [block]+ width/2, height / 2, maxY [block]),transform.rotation) as GameObject;
			clone.name = name;
			clone.transform.localScale = new Vector3 (width, height, width);
			
			maxX [block] += width + 20;
			maxW [block] = System.Math.Max (width, maxW [block]);
		}
		
		
		/* sec.5 */
		
		foreach (Dictionary<string,object> block in blocks) {
			GameObject clone;
			var name = block ["name"].ToString ();
			if(name.Contains ("test")){
				clone = Instantiate(this.testGround,new Vector3 (maxX [name] / 2, 1, maxY [name]),transform.rotation) as GameObject;
			}else{
				clone = Instantiate(this.ground,new Vector3 (maxX [name] / 2, 1, maxY [name]),transform.rotation) as GameObject;
			}
			clone.name = name;
			clone.transform.localScale =  new Vector3 (maxX [name] +10, 2, maxW [name] + 10);
			/*
						var name = block ["name"].ToString ();
						GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
						cube.transform.localScale = new Vector3 (maxX [name] +10, 2, maxW [name] + 10);
						cube.transform.position = new Vector3 (maxX [name] / 2, 1, maxY [name]);
	*/
		}
	}
	
    /*
	void ReadFile () 
	{
		FileInfo file = new FileInfo (TARGET);
		try {
			using (StreamReader sr = new StreamReader(file.OpenRead (),Encoding.UTF8)) {
				jsonText = sr.ReadToEnd ();
			}
		} catch (Exception e) {
			jsonText += SetDefaultText ();
		}
	}
    */

    // Javascriptから街を作り始めるためのメソッド
    public void StartCityCreater(string id)
    {
        StartCoroutine(ReadFileOnline(id));
    }

    // サーバにあるJsonファイルを読み込むメソッド
    IEnumerator ReadFileOnline(string id)
    {
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
	
	public Dictionary<string,object> GetCity(){
		return this.city;
	}
	
    public GameObject GetGround()
    {
        return this.earth as GameObject;
    }

    public string GetJsonText()
    {
        return this.jsonText;
    }

    public Dictionary<String, List<Dictionary<String, object>>> GetFirstBlockList()
    {
        //return this.firstBlockDictionary;
        return this.firstBlockDictionary2;
    }

    public void RemakeCity(String root, Boolean directFlag)
    {
        // リストで飛ぶときか上へのボタンを押したときは直接行く
        if (directFlag)
        {
            //Debug.Log(root);
            LocateBlockAndBuilding2(allDirectory, arrangedBuildings, root);
        }
        // リストから飛んでいない、かつrootに指定したブロックが現在のrootのときは1個上に行く
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

    public String GetRootName()
    {
        return rootDirName;
    }

    public String GetCurrentDir()
    {
        if (currentRoot == rootDirName)
            return "/";
        else
            return currentRoot.Substring(rootDirName.Length);
    }

    public Dictionary<string, object> GetCityData()
    {
        return this.city;
    }

}
