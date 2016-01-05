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
	public GameObject check;
	public Dictionary<string,object> city;
	
	public string jsonText = "";
	// Use this for initialization
	void Start ()
	{ 
		//TARGET = Application.dataPath + "/target/redmine_path.json";
		TARGET = Application.dataPath + "/target/test.json";
		ReadFile ();
		CreateCity ();
	} 
	
	
	
	void CreateCity ()
	{
		this.city = Json.Deserialize (jsonText) as Dictionary<string,object>;
		var blocks = this.city ["blocks"] as IList;
		var buildings = this.city ["buildings"] as IList;
		LocateBlockAndBuilding(blocks, buildings);
		//nori_rogic_ver2 (blocks, buildings);
		/*
			Simple (blocks, buildings);
			 */
	}
	
	/**
	 *
     * ブロックの位置を決めるメソッド
	 *
	 */
	void LocateBlockAndBuilding (IList blocks, IList buildings)
	{
		Dictionary<String,List<Dictionary<String, object>>> arrangedBlock = ArrangeByKey (buildings, "block"); // ブロックごとにビルをまとめる
		Dictionary<String,List<Dictionary<String, object>>> blockDictionary = ArrangeByKey (blocks, "name"); // 名前ごとにブロックをまとめる
		
		List<Dictionary<String, object>> blockList = new List<Dictionary<string, object>> ();
		//Debug.Log (new HashSet<String>(arrangedBlock.Keys).Equals(new HashSet<String>(blockDictionary.Keys)));
		
		foreach (String key in blockDictionary.Keys) { // ブロックのkeyごとに実行する
			
			SetLocation (arrangedBlock[key]); // 1.ビルの座標を決める
			SetWidth (arrangedBlock[key], blockDictionary[key]); // 2.ブロックの幅を決める
			blockList.Add(blockDictionary[key][0]);
			Debug.Log(int.Parse(blockDictionary[key][0]["width"].ToString()));
			
		}
		Debug.Log("#####");
		SetLocation (blockList);
		
		Debug.Log("TEST");
		SetGlobalLocation (arrangedBlock, blockDictionary);
		Debug.Log("TEST");
		BuildBuildings (arrangedBlock, blockDictionary);
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
		float space = float.Parse(target[0]["width"].ToString()) + 20;
		for (int i = 0; ; i++) {
			if(i * i > target.Count)
			{
				block[0]["width"] = space * i;
				break;
			}
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
            float blockX = float.Parse(block[key][0]["x"].ToString()) - float.Parse(block[key][0]["width"].ToString()) / 2;
			float blockY = float.Parse(block[key][0]["y"].ToString()) - float.Parse(block[key][0]["width"].ToString()) / 2;
			
			List<Dictionary<String, object>> buildingList = building[key];

            // ビルの座標を決めていく
			foreach(Dictionary<String, object> oneBuilding in buildingList){
				oneBuilding["globalX"] = float.Parse(blockX.ToString()) + float.Parse(oneBuilding["x"].ToString());
				oneBuilding["globalY"] = float.Parse(blockY.ToString()) + float.Parse(oneBuilding["y"].ToString());
			}
		}
	}

    /**
	 *
	 * ビルを建てるメソッド
	 * buildingオブジェクトを複製して配置していく
	 * 
	 */
    void BuildBuildings(Dictionary<String,List<Dictionary<String, object>>> building, Dictionary<String,List<Dictionary<String, object>>> block){
		Debug.Log("TEST");
		foreach (String key in building.Keys) {
			List<Dictionary<String, object>> buildingList = building [key];
			foreach (Dictionary<String, object> oneBuilding in buildingList) {
				GameObject clone = Instantiate (this.building, new Vector3 (float.Parse (oneBuilding ["globalX"].ToString ()), (float.Parse (oneBuilding ["height"].ToString ()) / 2) + 2, float.Parse (oneBuilding ["globalY"].ToString ())), transform.rotation) as GameObject;
				clone.name = oneBuilding ["name"].ToString ();
				clone.transform.localScale = new Vector3 (float.Parse (oneBuilding ["width"].ToString ()), float.Parse (oneBuilding ["height"].ToString ()), float.Parse (oneBuilding ["width"].ToString ()));
				//clone.GetComponent<Renderer>().material.color = Color.blue;
				
				clone.GetComponent<Building>().Init(new Color (float.Parse (oneBuilding ["color_r"].ToString ()), float.Parse (oneBuilding ["color_g"].ToString ()), float.Parse (oneBuilding ["color_b"].ToString ())));
				
                // SATDがあった時に仮に目印を入れておく
				IList sList = oneBuilding["SATD"] as IList;
				Debug.Log(this.check);
				Debug.Log(this.building);
				if(sList.Count != 0){
					//GameObject test = Instantiate (this.check, new Vector3 (float.Parse (oneBuilding ["globalX"].ToString ()), (float.Parse (oneBuilding ["height"].ToString ())) + 10, float.Parse (oneBuilding ["globalY"].ToString ())), transform.rotation) as GameObject;

					GameObject check = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					check.transform.Translate(float.Parse (oneBuilding ["globalX"].ToString ()), (float.Parse (oneBuilding ["height"].ToString ())) + float.Parse (oneBuilding ["width"].ToString ()), float.Parse (oneBuilding ["globalY"].ToString ()));
					//test.transform.localScale = new Vector3 (float.Parse (oneBuilding ["width"].ToString ()), float.Parse (oneBuilding ["width"].ToString ()), float.Parse (oneBuilding ["width"].ToString ()));
					//GameObject check = Instantiate (this.building, new Vector3 (float.Parse (oneBuilding ["globalX"].ToString ()), (float.Parse (oneBuilding ["height"].ToString ())) + 10, float.Parse (oneBuilding ["globalY"].ToString ())), transform.rotation) as GameObject;
					check.transform.localScale = new Vector3 (float.Parse (oneBuilding ["width"].ToString ()), float.Parse (oneBuilding ["width"].ToString ()), float.Parse (oneBuilding ["width"].ToString ()));
				}
			}
		}
		
		Debug.Log("TEST");
		foreach (String key in block.Keys) {
			Debug.Log(key);
			List<Dictionary<String, object>> blockList = block [key];
			GameObject clone = Instantiate (this.ground, new Vector3(float.Parse(blockList[0]["x"].ToString()), 1, float.Parse(blockList[0]["y"].ToString())), transform.rotation) as GameObject;
			clone.transform.localScale = new Vector3 (float.Parse (blockList [0]["width"].ToString ()), 2, float.Parse (blockList [0]["width"].ToString ()));
		}
		
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
	
	string SetDefaultText ()
	{
		return "cant read\n";
	}
	
	public Dictionary<string,object> GetCity(){
		return this.city;
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
