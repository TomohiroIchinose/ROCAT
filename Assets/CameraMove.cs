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
	public Canvas canvas;
    public Canvas canvas2;
	public Text file_name;
    public Text block_name;
	private string src_txt = "";
	public CityCreater cc;
	public string ta;
	public Color preColor;
	public Material viewMaterial;
    public Material defaultBuildingMaterial;
    public Material defaultBlockingMaterial;
    public Material defaultMarkeringMaterial;
    private bool view_src;

    public bool isControlAvailable = false;
    private bool isMouseAvailable = true;
	private Building selectedBuilding;
    private Block selectedBlock;
    private Marker selectedMarker;

	private float rotationY = 0f;
	private const float CAMERA_SPEED = 500f;
	private const float CAMERA_CONTROL_SENSITIVITY = 3F;
	private const float MIN_ROTATION_Y = -90F;
	private const float MAX_ROTATION_Y = 90F;

    private GameObject ground;


    public Camera mapCamera;
    public Camera sensorCamera;

	// Use this for initialization
	void Start () {
		view_src = false;

        this.enabled = false;

        mapCamera.enabled = true;
        sensorCamera.enabled = false;

        //viewMaterial = Resources.Load("red Material", typeof(Material)) as Material;
        //defaultBuildingMaterial = Resources.Load("Building", typeof(Material)) as Material;
        //defaultBlockingMaterial = Resources.Load("Block", typeof(Material)) as Material;

        cc = GameObject.Find ("CityCreater").GetComponent<CityCreater> ();
		foreach( Transform child in canvas.transform){
			file_name = child.gameObject.GetComponent<Text>();
            //block_name = child.gameObject.GetComponent<Text>();
            file_name.text = "";
            //block_name.text = "";
		}
        foreach (Transform child in canvas2.transform)
        {
            block_name = child.gameObject.GetComponent<Text>();
            block_name.text = "";
        }
        
        ground = cc.GetGround();      

    }

	void Update ()
	{
        if (!isControlAvailable) { return; }
        ControlByKeyboard();
		ControlByMouse();

        // 土台？からはみ出ないように調整
        this.transform.position = (new Vector3(Mathf.Clamp(this.transform.position.x, (ground.transform.position.x - ground.transform.localScale.x / 2), (ground.transform.position.x + ground.transform.localScale.x / 2)),
                                               this.transform.position.y,
                                               Mathf.Clamp(this.transform.position.z, (ground.transform.position.z - ground.transform.localScale.z / 2), (ground.transform.position.z + ground.transform.localScale.z / 2))));
        if(this.transform.position.y <= 0)
            this.transform.position = new Vector3(this.transform.position.x, 10, this.transform.position.z);
    }

    // カメラをスタートさせる
    public void StartCamera()
    {
        this.transform.position = (new Vector3(ground.transform.position.x - ground.transform.localScale.x / 2 + 10, (float)100, ground.transform.position.z - ground.transform.localScale.z / 2 + 10));
        this.transform.LookAt(ground.transform);
        this.enabled = true;
    }

	private void ControlByKeyboard()
	{
		Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
		Vector3 velocity = Vector3.zero;

        float camera = CAMERA_SPEED;

        if(Input.GetKey(KeyCode.LeftControl))
        {
            camera = camera * 5;
        }

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
        
		if (Input.GetKeyDown(KeyCode.E))
		{
			isMouseAvailable = !isMouseAvailable;
		}
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                this.transform.position = (new Vector3(ground.transform.position.x + ground.transform.localScale.x / 2 - 10, (float)100, ground.transform.position.z + ground.transform.localScale.z / 2 - 10));
            }
            else
            {
                this.transform.position = (new Vector3(ground.transform.position.x - ground.transform.localScale.x / 2 + 10, (float)100, ground.transform.position.z - ground.transform.localScale.z / 2 + 10));
            }
                
            this.transform.LookAt(ground.transform);
        }

            if (Input.GetKeyDown(KeyCode.V))
		{
			//view_src = !view_src;
		}

        if (Input.GetKeyDown(KeyCode.C))
        {
            mapCamera.enabled = !mapCamera.enabled;
            sensorCamera.enabled = !sensorCamera.enabled;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            this.transform.position += this.transform.forward * -3000;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            this.transform.position += this.transform.forward * 3000;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            this.transform.rotation = this.transform.rotation * Quaternion.Euler(0, 180, 0);
        }

        /*
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // left
            if (Input.GetKey(KeyCode.LeftShift))
            {
                //this.transform.position = (new Vector3(this.transform.position.x - 5000, this.transform.position.y, this.transform.position.z));
                this.transform.position += this.transform.forward * -2500;
            }
            // right
            else
            {
                //this.transform.position = (new Vector3(this.transform.position.x + 5000, this.transform.position.y, this.transform.position.z));
                this.transform.position += this.transform.forward * 2500;
            }

        }
        */
    }

	private void ControlByMouse()
	{
        float wheel = Input.GetAxis("Mouse ScrollWheel");

		Building building = GetRaycastHitBuilding();
        Block block = GetRaycastHitBlock();
        Marker marker = GetRaycastHitMarker();
        HighlighMouseOverBuilding(building);
        HighlighMouseOverBlock(block);

        if (Input.GetMouseButtonDown(0))
		{
			MouseClicked(building, block, marker);
		}

        // if (!isMouseAvailable) {return;}

        if(wheel != 0)
        {
            this.transform.position += this.transform.forward * wheel * 1500;
        }

        if ((isMouseAvailable && Input.GetMouseButton(2)) || !isMouseAvailable)
        {
            float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * CAMERA_CONTROL_SENSITIVITY;
            rotationY += Input.GetAxis("Mouse Y") * CAMERA_CONTROL_SENSITIVITY;
            rotationY = Mathf.Clamp(rotationY, MIN_ROTATION_Y, MAX_ROTATION_Y);
            transform.localEulerAngles = new Vector3(rotationY * -1, rotationX, 0);
        }

        /*
        if(Input.GetMouseButtonDown(2))
        {
            this.transform.position += this.transform.forward * 2000;
        }
        */
	}

	private void MouseClicked(Building building, Block block, Marker marker)
	{
        string path;
        string filename;
        string fileFullPath;
        string type;
        string satd = "";

        if (building == null && marker == null)
        {
            filename = "";
            fileFullPath = null;

            // ブロックをクリック
            if (block != null && marker == null)
            {
                type = "block";
                path = SearchPathFromFileNameforBlock(block.transform.name);


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
        // ビルかマーカーををクリック
        else
        {
            // ビルをクリック
            if (building != null)
            {
                type = "building";
                filename = building.transform.name;
                //Debug.Log(filename);

            }
            // マーカーをクリック
            else
            {
                type = "marker";
                int slashnum = marker.transform.name.IndexOf("/");
                satd = marker.transform.name.Substring(slashnum + 1, marker.transform.name.Length - slashnum - 1);
                filename = marker.transform.name.Substring(0, slashnum);    
            }
            path = SearchPathFromFileName(filename);
            //Debug.Log(path);

            // --------------for Normal repository--------------
            
            fileFullPath = path;
            path = path.Substring(path.IndexOf(".git") + 5);
            fileFullPath = "../" + fileFullPath.Substring(fileFullPath.IndexOf("repository"));
            
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
#if UNITY_EDITOR
        //Debug.Log(path);
        //Debug.Log(filename);
        //Debug.Log(fileFullPath);
#else
			        Application.ExternalCall("OnBuildingClick", path , filename, fileFullPath, type, satd);
#endif



        /*
		if (building == null) {return;}
		string path = SearchPathFromFileName(building.transform.name);
		src_txt = ReadFile(path);
        */
    }

    private void HighlighMouseOverBuilding(Building building)
	{
		if (building != null)
		{
			if (selectedBuilding == null || selectedBuilding != building){
				file_name.text = building.transform.name;

				if (selectedBuilding)
				{
                    //selectedBuilding.GetComponent<Renderer>().material = defaultBuildingMaterial;
                    //selectedBuilding.GetComponent<Renderer>().material.color = preColor;
                    selectedBuilding.Deselected();
                }

				selectedBuilding = building;
				selectedBuilding.Selected();
                //preColor = selectedBuilding.GetComponent<Renderer>().material.color;
                //selectedBuilding.GetComponent<Renderer>().material = viewMaterial;
			}
		}
		else
		{
			if (selectedBuilding)
			{
                //selectedBuilding.GetComponent<Renderer>().material = defaultBuildingMaterial;
                //selectedBuilding.GetComponent<Renderer>().material.color = preColor;
                selectedBuilding.Deselected();
                selectedBuilding = null;
            }
			file_name.text = "";
		}
	}

    private void HighlighMouseOverBlock(Block block)
    {
        if (block != null)
        {
            if (selectedBlock == null || selectedBlock != block)
            {
                block_name.text = block.transform.name;

                if (selectedBlock)
                {
                    selectedBlock.Deselected();
                }

                selectedBlock = block;
                selectedBlock.Selected();
            }
        }
        else
        {
            if (selectedBlock)
            {
                selectedBlock.Deselected();
                selectedBlock = null;
            }
            block_name.text = "";
        }
    }

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


    private Marker GetRaycastHitMarker()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 10000))
        {
            Marker hitMarker = hit.transform.GetComponent<Marker>();
            return hitMarker;
        }

        return null;
    }


    void OnGUI()
	{
		if(view_src)
			//src_txt = GUI.TextArea (new Rect (5, 5, Screen.width-10, Screen.height-100), src_txt);
			src_txt = GUI.TextArea (new Rect (5, 5, Screen.width-10, Screen.height-100), src_txt.Substring(0,Math.Min(10000,src_txt.Length)));

	}

    /*
	string ReadFile(string path){
		string st = "";
		try {
		// FileReadTest.txtファイルを読み込む
		FileInfo fi = new FileInfo(path);


			// 一行毎読み込み
			using (StreamReader sr = new StreamReader(fi.OpenRead(), Encoding.UTF8)){
				st = sr.ReadToEnd();
			}
		} catch (Exception e){
			// 改行コード
			st += SetDefaultText();
		}

		return st;
	}
    */

	string SearchPathFromFileName(string file_name){
		string path = "";
		IList buildings = cc.GetCity()["buildings"] as IList;
		foreach (Dictionary<string,object> building in buildings) {
			if(building["name"].ToString() == file_name){
				path = building["path"].ToString();
			}
		}
		return path;

	}

    string SearchPathFromFileNameforBlock(string block_name)
    {
        string path = "";
        IList blocks = cc.GetCity()["blocks"] as IList;
        IList dirs = cc.GetCity()["directories"] as IList;
        foreach (Dictionary<string, object> block in blocks)
        {
            if (block["name"].ToString() == block_name)
            {
                path = block["name"].ToString();
            }
        }

        if (path == "")
        {
            for(int i = 0; i < dirs.Count; i++)
            {
                if ((string)dirs[i] == block_name)
                    path = (string)dirs[i];
            }
        }

        return path;

    }

    // 改行コード処理
    string SetDefaultText(){
		return "cant read\n";
	}
}
