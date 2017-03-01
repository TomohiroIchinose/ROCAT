using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO; //System.IO.FileInfo, System.IO.StreamReader, System.IO.StreamWriter
using System; //Exception
using System.Text;
using MiniJSON;
//using CityCreater;

public class CameraMove : MonoBehaviour {

	const float SPEED = 0.1f;

	public Canvas canvas;   // 画面下に表示されるファイル名用Canvas
    public Canvas canvas2;  // 画面下に表示されるディレクトリ名用Canvas

    // 画面下に表示されるファイル名orディレクトリ名用のTextとImage
    public Text file_name;
    public Text block_name;
    public Image file_back;
    public Image block_back;

    // CityCreaterクラス
	public CityCreater cc;


    public bool isControlAvailable = false;     // カメラを操作できるかどうかの判定
    private bool isMouseAvailable = true;       // 常にマウスに追従するかどうかの判定
	private Building selectedBuilding;          // 現在マウスカーソルが乗っているビル
    private Block selectedBlock;                // 現在マウスカーソルが乗っている土台

    // カメラ制御用
	private float rotationY = 0f;
	private const float CAMERA_SPEED = 2000f;
	private const float CAMERA_CONTROL_SENSITIVITY = 3F;
	private const float MIN_ROTATION_Y = -90F;
	private const float MAX_ROTATION_Y = 90F;

    private GameObject ground;      // 地面
    private float mostheight;       // 最も高いビルの高さ

    public Camera mapCamera;        // マップのカメラ
    public Camera sensorCamera;     // レーダーのカメラ
    public MapCameraMove rcm;       // マップのカメラの操作用

    // SATDのあるファイルのリスト用のCanvasとImage
    public Canvas list;
    public Image satdlist;

    // ファイルの情報のウィンドウ用のCanvasとText
    public Canvas info;
    public Text infoText;
    public Text nameText;

    // 周囲の土台の辞書とキーのリストと個数
    public Dictionary<String, List<Dictionary<String, object>>> firstBlockDictionary;
    public List<String> firstBlockDicitonalyKeys;
    public int keyNum;

    // 現在中心にあるディレクトリの表示用のCanvasとText
    public Canvas currentDir;
    public Text dirName;



    // Use this for initialization
    void Start () {

        this.enabled = false;

        mapCamera.enabled = false;
        sensorCamera.enabled = false;

        //viewMaterial = Resources.Load("red Material", typeof(Material)) as Material;
        //defaultBuildingMaterial = Resources.Load("Building", typeof(Material)) as Material;
        //defaultBlockingMaterial = Resources.Load("Block", typeof(Material)) as Material;

        cc = GameObject.Find ("CityCreater").GetComponent<CityCreater> ();
        list = GameObject.Find("List").GetComponent<Canvas>();
        rcm = GameObject.Find("MapCamera").GetComponent<MapCameraMove>();

        file_name = canvas.transform.GetComponentInChildren<Text>();
        file_back = canvas.transform.GetComponentInChildren<Image>();
        file_name.text = "";
        file_back.color = new Color(file_back.color.r, file_back.color.g, file_back.color.b, 0);

        block_name = canvas2.transform.GetComponentInChildren<Text>();
        block_back = canvas2.transform.GetComponentInChildren<Image>();
        block_name.text = "";
        block_back.color = new Color(block_back.color.r, block_back.color.g, block_back.color.b, 0);

        satdlist = list.transform.GetComponentInChildren<Image>();

        list.enabled = false;

        info.enabled = false;
        //infoText = info.transform.GetComponentInChildren<Text>();
        infoText = info.transform.FindChild("Image/InfoText").gameObject.GetComponentInChildren<Text>();
        nameText = info.transform.FindChild("Image/NameText").gameObject.GetComponentInChildren<Text>();

        dirName = currentDir.transform.GetComponentInChildren<Text>();

        ground = cc.GetGround();

        mapCamera.enabled = true;

    }

    void Update()
    {
        // 常にキーボードとマウス操作を受け付ける
        if (!isControlAvailable) { return; }
        ControlByKeyboard();
		ControlByMouse();

        // 地面からメインカメラがはみ出ないように座標を調整
        if (ground != null)
        {
            this.transform.position = (new Vector3(Mathf.Clamp(this.transform.position.x, (ground.transform.position.x - ground.transform.localScale.x / 2), (ground.transform.position.x + ground.transform.localScale.x / 2)),
                                                   this.transform.position.y,
                                                   Mathf.Clamp(this.transform.position.z, (ground.transform.position.z - ground.transform.localScale.z / 2), (ground.transform.position.z + ground.transform.localScale.z / 2))));
        }
        if (this.transform.position.y <= 0)
            this.transform.position = new Vector3(this.transform.position.x, 10, this.transform.position.z);

        if (this.transform.position.y >= mostheight + 1000)
            this.transform.position = new Vector3(this.transform.position.x, mostheight + 1000, this.transform.position.z);
    }

    // カメラをスタートさせる
    public void StartCamera()
    {
        isControlAvailable = false;

        // 土台を更新
        ground = cc.GetGround();
       
        // 一番高いビルの高さを調べてメインカメラが動ける範囲を決めてからカメラを配置
        mostheight = MostHeighestBuilding();
        this.enabled = true;

        this.transform.localPosition = new Vector3(ground.transform.localPosition.x - ground.transform.localScale.x / 2, mostheight / 2 + 500, ground.transform.localPosition.z - ground.transform.localScale.x / 2);
        this.transform.LookAt(new Vector3(ground.transform.localPosition.x + ground.transform.localScale.x / 2, 0, ground.transform.localPosition.z + ground.transform.localScale.x / 2));

        // 周りにある土台の一覧を作成する
        //firstBlockDictionary.Clear();
        firstBlockDictionary = cc.GetFirstBlockList();
        firstBlockDicitonalyKeys.Clear();
        foreach (String key in firstBlockDictionary.Keys)
        {  
            firstBlockDicitonalyKeys.Add(key);
        }

        keyNum = 0;

        dirName.text = cc.GetCurrentDir();

        // マップ用カメラの位置を調整
        float height = (ground.transform.localScale.x > ground.transform.localScale.z ? ground.transform.localScale.x : ground.transform.localScale.z) * 0.5f / Mathf.Tan(rcm.GetComponent<Camera>().fieldOfView * 0.5f * Mathf.Deg2Rad);
        rcm.transform.position = (new Vector3(ground.transform.position.x, height, ground.transform.position.z));

        isControlAvailable = true;

    }

    // キーボード操作関連の関数
    private void ControlByKeyboard()
	{
        Building building = GetRaycastHitBuilding();
        Block block = GetRaycastHitBlock();

        Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
		Vector3 velocity = Vector3.zero;

        float camera = CAMERA_SPEED;

        // 左シフトで加速
        if(Input.GetKey(KeyCode.LeftControl))
        {
            camera = camera * 5;
        }

        // 矢印キーまたはWASDキーで移動
		if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
		{
			velocity += transform.forward * camera;
		}
		if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
		{
			velocity += transform.forward * camera * -1;
		}

		if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
		{
			velocity += transform.right * camera * -1;
		}
		if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
		{
			velocity += transform.right * camera;
		}

		rigidBody.velocity = velocity;

        // スペースキーで上昇or下降
		if (Input.GetKey(KeyCode.Space))
		{
            if (this.transform.position.y > 10 && Input.GetKey(KeyCode.LeftShift))
                rigidBody.velocity = transform.up * camera * -1;
            else if (this.transform.position.y <= 0 && Input.GetKey(KeyCode.LeftShift))
            {
                rigidBody.velocity = transform.up * 0;
                this.transform.position = new Vector3(this.transform.position.x, 10, this.transform.position.z);
            }
            else
                rigidBody.velocity = transform.up * camera;
        }
        
        // カメラが常にマウスに追従するかどうか切り替え
		if (Input.GetKeyDown(KeyCode.E))
		{
			isMouseAvailable = !isMouseAvailable;
		}
        
        // 中心の土台に移動
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            this.transform.localPosition = new Vector3(0, 100, 0);
            this.transform.localRotation = new Quaternion(0,0,0,0);
            keyNum = 0;

            // 一番最初に置かれている土台の方（＝中央）を見る
            this.transform.LookAt(new Vector3(float.Parse(firstBlockDictionary[firstBlockDicitonalyKeys[0]][0]["x"].ToString()), 5, float.Parse(firstBlockDictionary[firstBlockDicitonalyKeys[0]][0]["z"].ToString())));
        }

        // マップとレーダー切り替え
        if (Input.GetKeyDown(KeyCode.C))
        {
            mapCamera.enabled = !mapCamera.enabled;
            sensorCamera.enabled = !sensorCamera.enabled;
        }

        // 後ろに大きく移動
        if (Input.GetKeyDown(KeyCode.Z))
        {
            this.transform.position += this.transform.forward * -3000;
        }

        // 前に大きく移動
        if (Input.GetKeyDown(KeyCode.Q))
        {
            this.transform.position += this.transform.forward * 3000;
        }

        // 真後ろを向く
        if (Input.GetKeyDown(KeyCode.X))
        {
            this.transform.rotation = this.transform.rotation * Quaternion.Euler(0, 180, 0);
        }

        // SATDのあるファイルのリスト表示
        if (Input.GetKeyDown(KeyCode.F))
        {
            list.enabled = !list.enabled;
        }

        // JKLIキーでカメラ操作
        if (Input.GetKeyDown(KeyCode.J))
        {
            this.transform.rotation = this.transform.rotation * Quaternion.Euler(0, -10, 0);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            this.transform.rotation = this.transform.rotation * Quaternion.Euler(0, 10, 0);
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            this.transform.rotation = this.transform.rotation * Quaternion.Euler(-10, 0, 0);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            this.transform.rotation = this.transform.rotation * Quaternion.Euler(10, 0, 0);
        }

        // ,キーでカメラの角度をリセット
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            this.transform.rotation = this.transform.rotation * Quaternion.Euler(-this.transform.localEulerAngles.x, 0, -this.transform.localEulerAngles.z);
        }

        // Enterキーでマウスクリックと同じ動作
        if (Input.GetKeyDown(KeyCode.Return))
        {
            MouseClicked(building, block);
        }
        
        // Tabキーで周囲の土台へワープ        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // ABC順
            if (Input.GetKey(KeyCode.LeftShift))
            {
                keyNum--;
                if (keyNum == -1)
                    keyNum = firstBlockDictionary.Count - 1;
            }
            // ABC順の逆
            else
            {
                keyNum++;
                if (keyNum == firstBlockDictionary.Count)
                    keyNum = 0;
            }
            this.transform.localPosition = new Vector3(float.Parse(firstBlockDictionary[firstBlockDicitonalyKeys[keyNum]][0]["x"].ToString()), 200, float.Parse(firstBlockDictionary[firstBlockDicitonalyKeys[keyNum]][0]["z"].ToString()) - float.Parse(firstBlockDictionary[firstBlockDicitonalyKeys[keyNum]][0]["radius"].ToString()) - 300);
            this.transform.LookAt(new Vector3(float.Parse(firstBlockDictionary[firstBlockDicitonalyKeys[keyNum]][0]["x"].ToString()), 0, float.Parse(firstBlockDictionary[firstBlockDicitonalyKeys[keyNum]][0]["z"].ToString())));
        }
        
    }

    // マウス操作関連の関数
	private void ControlByMouse()
	{
        float wheel = Input.GetAxis("Mouse ScrollWheel");

        Block block = GetRaycastHitBlock();
        Building building = GetRaycastHitBuilding();
        
        HighlighMouseOverBuilding(building);
        HighlighMouseOverBlock(block);

        // 右クリックしたとき
        if (Input.GetMouseButtonDown(1))
		{
            //RightMouseClicked(building);
		}

        // マウスホイールが動かされたとき
        if(wheel != 0)
        {
            this.transform.position += this.transform.forward * wheel * ground.transform.localScale.x / 20;
        }
        
        // 左クリックしたとき
        if (Input.GetMouseButtonDown(0))
        {
            MouseClicked(building, block);
            BuildingInfo(building);

            // 土台がクリックされた場合は都市を再構成
            if (block != null)
            {
                cc.RemakeCity("/" + block.name, false);
            }

        }

        // マウスカーソルの位置にメインカメラを向ける
        if (Input.GetMouseButton(0) && isMouseAvailable || !isMouseAvailable)
        {
            float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * CAMERA_CONTROL_SENSITIVITY;
            rotationY += Input.GetAxis("Mouse Y") * CAMERA_CONTROL_SENSITIVITY;
            rotationY = Mathf.Clamp(rotationY, MIN_ROTATION_Y, MAX_ROTATION_Y);
            transform.localEulerAngles = new Vector3(rotationY * -1, rotationX, 0);    
        }

    }

    // ビル（＝ファイル）の情報を表示する
    private void BuildingInfo(Building building)
    {
        if(building != null)
        {
            nameText.text = building.GetComponent<BuildingData>().pathname;

            infoText.text = "LOC:" + building.GetComponent<BuildingData>().loc.ToString() +
                            "\n#Comment:" + building.GetComponent<BuildingData>().comment.ToString() +
                            "\n\n#SATD:" + building.GetComponent<BuildingData>().satd.Count.ToString() +
                            "\nSATD lines:\n";

            for (int i = 0; i < building.GetComponent<BuildingData>().satd.Count; i++)
                infoText.text = infoText.text + building.GetComponent<BuildingData>().satd[i] + ", ";

            infoText.text = infoText.text.Substring(0, infoText.text.Length - 2) + "\n";

            // ウィンドウを表示する
            if (!info.enabled)
                info.enabled = true;
        }
        // ビル以外のところをクリックしていた時はウィンドウを消す
        else
        {
            infoText.text = "null";
            if (info.enabled)
                info.enabled = false;
        }
    }

    // マウスをクリックしたときにJavascriptを呼び出す
	private void MouseClicked(Building building, Block block)
	{
        string path;            // ファイルorディレクトリのパス
        string filename;        // ファイル名
        string fileFullPath;    // サーバにクローンしてあるファイルにアクセスするためのパス
        string type;            // ファイルか土台か
        string satd = "";       // SATDの一覧

        if (building == null)
        {
            filename = "";
            fileFullPath = null;

            // ブロックをクリック
            if (block != null)
            {
                type = "block";
                //path = SearchPathFromFileNameforBlock("/" + block.transform.name);              
                path = "/" + block.GetComponent<BlockData>().pathname;

                // -------------for Normal repository---------------

                // ディレクトリがrootだったらrootに書き換える
                if (path.IndexOf(".git") + 4 == path.Length)
                {
                    path = "root";
                }
                // その他は最初の.gitから後ろだけ取る
                else
                {
                    path = path.Substring(path.IndexOf(".git") + 5);
                }
                
                // -------------------------------------------------


                // ---------------for Histrage---------------------
                /*
                path = path.Substring(23);
                int firsts = path.IndexOf("/");
                path = path.Substring(firsts + 1);
                path = path.Replace('_', '/');
                */
                // -------------------------------------------------

            }
            // 何もないトコをクリック
            else
            {
                type = "nothing";
                path = "";
            }
        }
        // ビルをクリック
        else
        {

            type = "building";

            filename = building.GetComponent<BuildingData>().filename;

            if (building.GetComponent<BuildingData>().satd.Count > 0)
            {
                for (int i = 0; i < building.GetComponent<BuildingData>().satd.Count; i++)
                {
                    satd = satd + building.GetComponent<BuildingData>().satd[i] + ",";
                }
                satd = satd.Substring(0, satd.Length - 1);
            }
            

            // --------------for Normal repository--------------

            fileFullPath = building.GetComponent<BuildingData>().fullpath;
            path = building.GetComponent<BuildingData>().pathname;
            fileFullPath = "../" + fileFullPath.Substring(fileFullPath.IndexOf("repository"));

            // 同名ファイルが複数あるタイプのおなまえを修正する
            if (filename.Contains("("))
            {
                filename = filename.Substring(0, filename.IndexOf("("));
            }

            // -------------------------------------------------


            // ---------for Histrage repository-----------------
            /*
            fileFullPath = ".." + path.Substring(13);

            int cn = path.IndexOf("[CN]");
            path = path.Substring(0, cn - 1);
            path = path.Substring(23);
            int firsts = path.IndexOf("/");
            path = path.Substring(firsts + 1);
            path = path.Replace('_', '/');
            path = path + "---" + filename;
            */
            // -------------------------------------------------


            
        }

// Unity以外のトコで実行したときにHTMLファイルのJavascriptの関数を呼び出す
#if UNITY_EDITOR

#else
			        Application.ExternalCall("OnBuildingClick", path , filename, fileFullPath, type, satd);
#endif
    }

    // ビルにマウスを合わせたときの動作
    private void HighlighMouseOverBuilding(Building building)
	{
		if (building != null)
		{
			if (selectedBuilding == null || selectedBuilding != building){
				file_name.text = building.transform.name.Substring(building.transform.name.LastIndexOf("/") + 1);
                file_back.color = new Color(file_back.color.r, file_back.color.g, file_back.color.b, 0.7f);     // ファイル名を出す部分を表示する

                // ビルからビルにマウスが動いたときに先にマウスが乗っていた方のビルの選択を解除
                if (selectedBuilding)
				{
                    selectedBuilding.Deselected();
                }

				selectedBuilding = building;
				selectedBuilding.Selected();
			}
		}
        // ビル以外にマウスがあるとき
		else
		{
			if (selectedBuilding)
			{
                selectedBuilding.Deselected();
                selectedBuilding = null;
            }
			file_name.text = "";
            file_back.color = new Color(file_back.color.r, file_back.color.g, file_back.color.b, 0);            // ファイル名を出す部分を非表示にする
        }
	}

    // 土台にマウスを合わせたときの動作
    private void HighlighMouseOverBlock(Block block)
    {
        if (block != null)
        {
            if (selectedBlock == null || selectedBlock != block)
            {
                // マウスが乗っている土台がrootディレクトリの場合
                if (block.transform.name.IndexOf(".git") + 4 == block.transform.name.Length)
                {
                    block_name.text = "(root)";
                }
                // rootディレクトリ以外のディレクトリの場合
                else
                {
                    block_name.text = "/" + block.transform.name.Substring(block.transform.name.IndexOf(".git") + 5);
                }
                block_back.color = new Color(block_back.color.r, block_back.color.g, block_back.color.b, 0.7f);     // ディレクトリ名を出す部分を表示する

                // 土台から土台にマウスが移る場合、先に乗っていた方の土台からマウスが離れたことにするよう処理
                if (selectedBlock)
                {
                    selectedBlock.Deselected();
                }

                selectedBlock = block;
                selectedBlock.Selected();
            }
        }
        // 土台以外のところにマウスがあるとき
        else
        {
            if (selectedBlock)
            {
                selectedBlock.Deselected();
                selectedBlock = null;
            }
            block_name.text = "";
            block_back.color = new Color(block_back.color.r, block_back.color.g, block_back.color.b, 0);            // ディレクトリ名を出す部分を非表示にする
        }
    }

    // ビルにマウスを合わせたかどうか調べる
    private Building GetRaycastHitBuilding()
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hit, 10000)) {
			Building hitBuilding = hit.transform.GetComponent<Building>();
            //MouseClicked(hitBuilding);
			return hitBuilding;
		}

		return null;
	}

    // 土台にマウスを合わせたかどうか調べる
    private Block GetRaycastHitBlock()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 10000))
        {
            Block hitBlock = hit.transform.GetComponent<Block>();
            return hitBlock;
        }

        return null;
    }


    // SATDのリストをクリックしたときの動作
    public void SATDListClick(string fileFullPath)
    {
        // ルートディレクトリを中心とすれば良い場合＝クリックした項目のビルがあるディレクトリがルートディレクトリorルートディレクトリのサブディレクトリの場合
        // ルートディレクトリを中心として都市を再構成
        if (fileFullPath.Substring(0, fileFullPath.LastIndexOf("/")).Substring(0, fileFullPath.Substring(0, fileFullPath.LastIndexOf("/")).LastIndexOf("/")).Length < cc.GetRootName().Length)
        {
            cc.RemakeCity(cc.GetRootName(), true);
        }
        // その他の場合＝クリックした項目のビルがあるディレクトリが周囲に来るように都市を再構成
        else
        {
            cc.RemakeCity("/" + fileFullPath.Substring(0, fileFullPath.LastIndexOf("/")).Substring(0, fileFullPath.Substring(0, fileFullPath.LastIndexOf("/")).LastIndexOf("/")), true);
        }

        // クリックした項目のビルのオブジェクトを取ってくる
        GameObject search_building = GameObject.Find(fileFullPath);
        if (search_building != null)
        {
            // メインカメラをそのビルの近くに移動させる
            this.transform.position = new Vector3(search_building.transform.position.x - search_building.transform.localScale.x * 15, search_building.transform.localScale.y * 10 + 200, search_building.transform.position.z);
            this.transform.LookAt(search_building.transform);

            // そのビルをクリックしたときと同じ動作をするようにする
            MouseClicked(search_building.GetComponent<Building>(), null);
            BuildingInfo(search_building.GetComponent<Building>());

            // リストを閉じる
            list.enabled = !list.enabled;
        }
    }

    // 一番高いビルの高さを求める
    float MostHeighestBuilding()
    {
        float max = 0;

        GameObject[] normalBuildingList = GameObject.FindGameObjectsWithTag("NormalBuilding");
        GameObject[] satdBuildingList = GameObject.FindGameObjectsWithTag("SATDBuilding");

        foreach(GameObject building in normalBuildingList)
        {
            if (building.transform.localScale.z * 10 >= max)
                max = building.transform.localScale.z * 10;
        }

        foreach (GameObject building in satdBuildingList)
        {
            if (building.transform.localScale.z * 10 >= max)
                max = building.transform.localScale.z * 10;
        }
        return max;
    }
}
