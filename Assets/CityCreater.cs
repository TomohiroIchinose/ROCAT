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

    public GameObject earth;    // �y��

    public GameObject sense;    // �p�[�e�B�N��

    public GameObject street;   // ��

    private CameraMove mainCamera;  // ���C���J����

    public string jsonText = "";
    // Use this for initialization

    public List<String> dir = new List<String>(); // �f�B���N�g���ꗗ
    public List<String> addedDir = new List<String>();  // ���ɒu���ꂽ�f�B���N�g���̈ꗗ

    public Sensor sensor;

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
        StartCityCreater("cdec");
        //StartCityCreater("tensorflow");
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

        // �f�B���N�g���ꗗ������ă\�[�g���Ă���
        for(int i=0; i< directories.Count; i++)
        {
            dir.Add(directories[i].ToString());
        }

        dir.Sort();

        LocateBlockAndBuilding(blocks, buildings);
		//nori_rogic_ver2 (blocks, buildings);
		/*
			Simple (blocks, buildings);
			 */
	}
	
	/**
	 *
     * �u���b�N�̈ʒu�����߂郁�\�b�h
	 *
	 */
	void LocateBlockAndBuilding (IList blocks, IList buildings)
	{
		Dictionary<String,List<Dictionary<String, object>>> arrangedBlock = ArrangeByKey (buildings, "block"); // �u���b�N���ƂɃr�����܂Ƃ߂�
		Dictionary<String,List<Dictionary<String, object>>> blockDictionary = ArrangeByKey (blocks, "name"); // ���O���ƂɃu���b�N���܂Ƃ߂�
		
		List<Dictionary<String, object>> blockList = new List<Dictionary<string, object>> ();
        //Debug.Log (new HashSet<String>(arrangedBlock.Keys).Equals(new HashSet<String>(blockDictionary.Keys)));

        // �u���b�N�̍��W�����߂�Ƃ��ɏo�Ă���ǉ����̃u���b�N�̎���
        Dictionary<String, List<Dictionary<String, object>>> notBuildingBlockList = new Dictionary<String, List<Dictionary<String, object>>>();

        foreach (String key in blockDictionary.Keys) { // �u���b�N��key���ƂɎ��s����

            //SetLocation (arrangedBlock[key]); // 1.�r���̍��W�����߂�

            SetLocation2(arrangedBlock[key]); // 1.�r���̍��W�����߂�

            //SetWidth (arrangedBlock[key], blockDictionary[key]); // 2.�u���b�N�̕������߂�

            SetWidth2(arrangedBlock[key], blockDictionary[key]); // 2.�u���b�N�̕������߂�

            blockList.Add(blockDictionary[key][0]);
            //Debug.Log(int.Parse(blockDictionary[key][0]["width"].ToString()));
        }

        // �u���b�N�̍��W�����߂�
        //SetLocation (blockList);
        SetLocation2(blockList);

        //notBuildingBlockList = SetBlockLocation(blockList);
        notBuildingBlockList = SetBlockLocation2(blockList);

        // �r���̃��X�g�ɒǉ����Ă���
        foreach (String key in notBuildingBlockList.Keys)
        {
            blockList.Add(notBuildingBlockList[key][0]);
        }

        // �r���̎��ۂ̍��W�����߂�
        SetGlobalLocation (arrangedBlock, blockDictionary);


		// �r���ƃu���b�N�����ĂĂ���
		BuildBuildings (arrangedBlock, blockDictionary, notBuildingBlockList);


        // �n�ʂ�ݒ�
        SetGround(blockList);


        // �������
        //BuildStreets(blockList);
        BuildStreets2(blockList);

        sensor.MakeSensorList();

    }
	
	/**
	 * 
     * key�̒l���Ƃ�target���܂Ƃ߂郁�\�b�h
     * �p�r1�F�r���𑮂���u���b�N���Ƃɂ܂Ƃ߂�
	 * �p�r2�F�u���b�N�𖼑O���Ƃɂ܂Ƃ߂�
	 *
	 */
	Dictionary<String,List<Dictionary<String, object>>> ArrangeByKey (IList target, String key)
	{
		Dictionary<String,List<Dictionary<String, object>>> arrangedTarget = new Dictionary<string, List<Dictionary<String, object>>>();
		
		foreach (Dictionary<string,object> contents in target) {
			String contentsName = contents[key].ToString();
			if(arrangedTarget.ContainsKey(contentsName))
			{
				// ���Ɏ����ɑ��݂���ꍇ��add��������
				arrangedTarget[contentsName].Add(contents);
			}
			else
			{
				// ���߂ďo�Ă�����new���Ď�����add����
				arrangedTarget[contentsName] = new List<Dictionary<String, object>>();
				arrangedTarget[contentsName].Add(contents);
			}
		}
		return arrangedTarget;
	}
	
	/**
	 *
	 * target�̑��΍��W�����߂郁�\�b�h
	 * �r���ԁE�u���b�N�Ԃł̍��W�����߂�
	 * 
	 */
	
	
	void SetLocation (List<Dictionary<String,object>> target)
	{
		// target���\�[�g����
		//Debug.Log (target [0] ["width"]);
		target.Sort((b,a) => int.Parse(a["width"].ToString()) - int.Parse(b["width"].ToString()));
		
		
		// 0 3 8
		// 1 2 7
		// 4 5 6
		// 
		// �ȉ��̃R�[�h�ŏ�L��0��8�̏��Ԃ̂悤�ɔz�u���Ă���
        // ��SetLocation2�̏��̕����C���[�W�I�ɂ͐������H
		
		int count = 0;
		float space = float.Parse(target[0]["width"].ToString()) + 20; // ��ɂȂ�Ԋu
		for (int i = 0; ;i++) {
			int constX = 0;
			int y = i;
			// 1.�E�Ɍ������ĕ��ׂĂ���
			// ex) 0
			// ex) 1��2
            // ex) 4��6
			for(int x = 0; x <= y; x++){
				target[count]["x"] = (space * x) + (space / 2);
				target[count]["y"] = (space * y) + (space / 2);
				count++;
				if(count == target.Count)
				{
					goto Finish; // �S���I�������Finish�֍s��
				}
				constX = x;
			}
			// 2.��Ɍ������ĕ��ׂĂ���
			// ex) 3
			// ex) 7��8
			for(y--; y >= 0; y--){
				target[count]["x"] = (space * constX) + (space / 2);
				target[count]["y"] = (space * y) + (space / 2);
				count++;
				if(count == target.Count)
				{
					goto Finish; // �S���I�������Finish�֍s��
				}
			}
		}
		
	Finish:
			return;
	}

    /**
	 *
	 * target�̑��΍��W�����߂郁�\�b�h2
	 * �r���ԁE�u���b�N�Ԃł̍��W�����߂�
     * y���W�͎��ۂ�z���W�������肷��c
	 * 
	 */

    void SetLocation2(List<Dictionary<String, object>> target)
    {
        // target���\�[�g����
        target.Sort((b, a) => int.Parse(a["widthX"].ToString()) - int.Parse(b["widthX"].ToString()));

        // 0 1 4
        // 3 2 5
        // 8 7 6
        // 
        // �ȉ��̃R�[�h�ŏ�L��0��8�̏��Ԃ̂悤�ɔz�u���Ă���

        int count = 0;  // �r���E�u���b�N�̌�

        int space = 50; // �Œ蕝

        for (int i = 0; ; i++)
        {
            int y = i;
            // 1.���Ɍ������ĕ��ׂĂ���
            // ex) 0
            // ex) 1��2
            // ex) 4��6
            for (int x = 0; x <= y; x++)
            {
                // i^2�Ԗڂ̂Ƃ�
                if(count == i * i)
                {
                    // ��ԍŏ��̂Ƃ�
                    if(i == 0)
                    {
                        target[count]["x"] = space + int.Parse(target[count]["widthX"].ToString()) / 2;
                        target[count]["y"] = space + int.Parse(target[count]["widthY"].ToString()) / 2;
                    }
                    // ����ȊO�̂Ƃ�
                    else
                    {
                        target[count]["x"] = space + int.Parse(target[count]["widthX"].ToString()) / 2;
                        target[count]["y"] = space + int.Parse(target[(i - 1) * (i - 1)]["y"].ToString()) + int.Parse(target[(i - 1) * (i - 1)]["widthY"].ToString()) / 2 + int.Parse(target[count]["widthY"].ToString()) / 2;
                    }
                }
                // i^2+i�Ԗڂ̎�
                else if(count == i * i + i)
                {
                    target[count]["x"] = space + int.Parse(target[(i - 1) * (i - 1) + (i - 1)]["x"].ToString()) + int.Parse(target[(i - 1) * (i - 1) + (i - 1)]["widthX"].ToString()) / 2 + int.Parse(target[count]["widthX"].ToString()) / 2;
                    target[count]["y"] = int.Parse(target[i * i]["y"].ToString());
                }
                // ����ȊO
                else
                {
                    target[count]["x"] = int.Parse(target[(i - 1) * (i - 1) + x]["x"].ToString());
                    target[count]["y"] = int.Parse(target[(i - 1) * (i - 1) + x]["y"].ToString()) + int.Parse(target[(i - 1) * (i - 1) + x]["widthY"].ToString()) / 2 + int.Parse(target[count]["widthY"].ToString()) / 2 + space;
                }

                count++;
                if (count == target.Count)
                {
                    goto Finish; // �S���I�������Finish�֍s��
                }
            }
            // 2.���Ɍ������ĕ��ׂĂ���
            // ex) 3
            // ex) 7��8
            for (y--; y >= 0; y--)
            {
                target[count]["x"] = int.Parse(target[i * i + i]["x"].ToString());
                target[count]["y"] = int.Parse(target[count - (2 * i + 1)]["y"].ToString());
                count++;
                if (count == target.Count)
                {
                    goto Finish; // �S���I�������Finish�֍s��
                }
            }
        }

    Finish:
        return;
    }



    // �u���b�N�̔z�u���f�B���N�g���\����������Ղ��悤�Ɍ��߂郁�\�b�h
    Dictionary<String, List<Dictionary<String, object>>> SetBlockLocation(List<Dictionary<String, object>> target)
    {
        // 1�O�Œu�����u���b�N��X���W��X�̕��̏��
        int prePositionX = 0;
        int preWidthX = 0;

        // �K�w��1��̃u���b�N��Y���W��Y�̕��i���ۂ�Z���W��Z�̕��j�̏��
        int prePositionY = 0;
        int preWidthY = 0;

        // �Œ�l
        int space = 50;

        // �f�B���N�g�����u���b�N�̃��X�g�ɂȂ��ꍇ�ɐݒ肷��X��Y�̕�
        int fixedX = 100;
        int fixedY = 100;

        // 1��̊K�w�̃f�B���N�g����
        string upperDir = "";

        // �Ԃ�l�̎����i����ǉ������u���b�N�̃��X�g�̎����j
        Dictionary<String, List<Dictionary<String, object>>> addedBlockList = new Dictionary<String, List<Dictionary<String, object>>>();

        // �`�F�b�N�p�t���O
        int targetFound = 0;
        int upperFound = 0;

        // �u���b�N�̃��X�g�𖼑O���Ƀ\�[�g����
        target.Sort((b, a) => string.Compare(b["name"].ToString(), a["name"].ToString()));

        /*
        for (int x = 0; x < target.Count; x++)
            Debug.Log("name: " + target[x]["name"]);
        */


        // �f�B���N�g�������ԂɌ��Ă���
        for (int i = 0; i < dir.Count; i++)
        {
            //Debug.Log(i + " : "  + dir[i]);

            // �܂����̃f�B���N�g���̔z�u�����܂��Ă��Ȃ��Ƃ�
            if(addedDir.IndexOf(dir[i]) < 0)
            {
                // �ŏ��ȊO�̎���1��̊K�w�����Ă���
                if (i != 0)
                {
                    upperDir = dir[i].Substring(0, dir[i].LastIndexOf("/"));

                    // 1��̊K�w��target���̂Ƃ�
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
                    // 1��̊K�w��target���ɑ��݂��Ȃ��i�r�����Ȃ��u���b�N�����̃��\�b�h�ŐV���ɒǉ����ꂽ�u���b�N�̂Ƃ��j
                    if(upperFound == 0)
                    {
                        prePositionY = int.Parse(addedBlockList[upperDir][0]["y"].ToString());
                        preWidthY = int.Parse(addedBlockList[upperDir][0]["widthY"].ToString());
                    }

                }

                // �`�F�b�N�p�t���O�����Z�b�g
                targetFound = 0;
                upperFound = 0;


                // �u���b�N�̎����𒲂ׂ�
                for (int j = 0; j < target.Count; j++)
                {
                    // �z�u�����߂��f�B���N�g�����u���b�N�̃��X�g�ɂ�����́i�r�������݂��Ă���f�B���N�g���j
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

                // �z�u�����߂��f�B���N�g�����u���b�N�̃��X�g�ɂȂ����́i�r�������݂��Ȃ��f�B���N�g���j 
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

                    // �u���b�N�̈ꗗ�ɒǉ�����
                    addedBlockList.Add(dir[i], addList);

                    prePositionX = int.Parse(newDictionary["x"].ToString());
                    prePositionY = int.Parse(newDictionary["y"].ToString());

                    preWidthX = int.Parse(newDictionary["widthX"].ToString());
                    preWidthY = int.Parse(newDictionary["widthY"].ToString());

                }

                // ���ɒu�����f�B���N�g���ꗗ�ɒǉ�����
                addedDir.Add(dir[i]);
            }
        }
        return addedBlockList;
    }

    // �u���b�N�̔z�u���f�B���N�g���\����������Ղ��悤�Ɍ��߂郁�\�b�hVer.2
    // �c���ɂȂ�Ȃ��悤�ɒ�����������
    Dictionary<String, List<Dictionary<String, object>>> SetBlockLocation2(List<Dictionary<String, object>> target)
    {
        // 1�O�Œu�����u���b�N��X���W��X�̕��̏��
        int prePositionX = 0;
        int preWidthX = 0;

        // �K�w��1��̃u���b�N��Y���W��Y�̕��i���ۂ�Z���W��Z�̕��j�̏��
        int prePositionY = 0;
        int preWidthY = 0;

        // �Œ�l
        int space = 50;

        // �f�B���N�g�����u���b�N�̃��X�g�ɂȂ��ꍇ�ɐݒ肷��X��Y�̕�
        int fixedX = 100;
        int fixedY = 100;

        // 1��̊K�w�̃f�B���N�g����
        string upperDir = "";

        // 1��̊K�w�̃f�B���N�g�������܂ލŌ�ɒu�����u���b�N�i�f�B���N�g���j�̖��O
        //string lastDir = "";

        // �Ԃ�l�̎����i����ǉ������u���b�N�̃��X�g�̎����j
        Dictionary<String, List<Dictionary<String, object>>> addedBlockList = new Dictionary<String, List<Dictionary<String, object>>>();

        // �`�F�b�N�p�t���O
        int targetFound = 0;
        int upperFound = 0;
        int lastFound = 0;

        // �u���b�N�̃��X�g�𖼑O���Ƀ\�[�g����
        target.Sort((b, a) => string.Compare(b["name"].ToString(), a["name"].ToString()));

        /*
        for (int x = 0; x < target.Count; x++)
            Debug.Log("name: " + target[x]["name"]);
        */


        // �f�B���N�g�������ԂɌ��Ă���
        for (int i = 0; i < dir.Count; i++)
        {
            //Debug.Log(i + " : "  + dir[i]);

            // �܂����̃f�B���N�g���̔z�u�����܂��Ă��Ȃ��Ƃ�
            if (addedDir.IndexOf(dir[i]) < 0)
            {
                // �ŏ��ȊO�̎���1��̊K�w�����Ă���
                if (i != 0)
                {
                    upperDir = dir[i].Substring(0, dir[i].LastIndexOf("/"));


                    lastFound = 0;

                    // ���ɒu�����u���b�N�𒲂ׂ�
                    foreach(string dire in addedDir)
                    {
                        // 1��̊K�w�̃f�B���N�g�������܂݁A����1��̃f�B���N�g���ł͂Ȃ����̂����邩
                        if(dire.Contains(upperDir) && string.Compare(dire, upperDir) != 0)
                        {
                            //lastDir = dir;
                            lastFound = 1;
                            break;
                        }
                    }

                    // target���i�r��������u���b�N�j������
                    for (int k = 0; k < target.Count; k++)
                    {
                        // 1��̊K�w��target���̂Ƃ�
                        if (target[k]["name"].ToString() == upperDir)
                        {
                            prePositionY = int.Parse(target[k]["y"].ToString());
                            preWidthY = int.Parse(target[k]["widthY"].ToString());
                            upperFound = 1;
                            //break;
                        }

                        // 1��̊K�w�̃f�B���N�g�������܂݁A����1��̃f�B���N�g���ł͂Ȃ����̂��������ꍇ
                        if(lastFound == 1)
                        {
                            // 1��̊K�w�̃f�B���N�g�������܂ށA�܂��u���Ă��Ȃ�target������
                            if (target[k]["name"].ToString().Contains(upperDir) && addedDir.Contains(target[k]["name"].ToString()) != false)
                            {
                                // X���W�{X�̕�/2���傫��������X�V
                                if (prePositionX + preWidthX / 2 < int.Parse(target[k]["x"].ToString()) + int.Parse(target[k]["widthX"].ToString()) / 2)
                                {
                                    prePositionX = int.Parse(target[k]["x"].ToString());
                                    preWidthX = int.Parse(target[k]["widthX"].ToString());
                                }
                            }
                        }
                    }

                    // 1��̊K�w��target���ɑ��݂��Ȃ��i�r�����Ȃ��u���b�N�����̃��\�b�h�ŐV���ɒǉ����ꂽ�u���b�N�̂Ƃ��j
                    if (upperFound == 0)
                    {
                        prePositionY = int.Parse(addedBlockList[upperDir][0]["y"].ToString());
                        preWidthY = int.Parse(addedBlockList[upperDir][0]["widthY"].ToString());


                        // 1��̊K�w�̃f�B���N�g�������܂݁A����1��̃f�B���N�g���ł͂Ȃ����̂��������ꍇ
                        if (lastFound == 1)
                        {
                            // ���̃��\�b�h�ŐV���ɒǉ����ꂽ�u���b�N�̎����̃L�[�i���f�B���N�g�����j�̔z����쐬
                            string[] keyList = new string[addedBlockList.Keys.Count];
                            addedBlockList.Keys.CopyTo(keyList, 0);
                            foreach (string key in keyList)
                            {
                                // �L�[�i���f�B���N�g���j��1��̊K�w�̃f�B���N�g�������܂�ł���ꍇ
                                if (key.Contains(upperDir))
                                {
                                    // X���W�{X�̕�/2���傫��������X�V
                                    if (prePositionX + preWidthX / 2 < int.Parse(addedBlockList[key][0]["x"].ToString()) + int.Parse(addedBlockList[key][0]["widthX"].ToString()) / 2)
                                    {
                                        prePositionX = int.Parse(addedBlockList[key][0]["x"].ToString());
                                        preWidthX = int.Parse(addedBlockList[key][0]["widthX"].ToString());
                                    }
                                }
                            }
                        }
                    }
                    // 1��̊K�w��target���ɑ��݂��邯�ǃr�����Ȃ��u���b�N�̃��X�g��1��̊K�w�̖��O���܂ރu���b�N������ꍇ
                    else
                    {
                        // �V���ɒǉ������u���b�N�̃��X�g�̃L�[�����ԂɌ��Ă���
                        foreach(string key in addedBlockList.Keys)
                        {
                            // �L�[��1��̊K�w�̃f�B���N�g�������܂܂�Ă���Ƃ�
                            if(key.Contains(upperDir))
                            {
                                // X���W�{X�̕�/2���傫��������X�V
                                if (prePositionX + preWidthX / 2 < int.Parse(addedBlockList[key][0]["x"].ToString()) + int.Parse(addedBlockList[key][0]["widthX"].ToString()) / 2)
                                {
                                    prePositionX = int.Parse(addedBlockList[key][0]["x"].ToString());
                                    preWidthX = int.Parse(addedBlockList[key][0]["widthX"].ToString());
                                }
                            }
                        }
                    }
                }

                // �`�F�b�N�p�t���O�����Z�b�g
                targetFound = 0;
                upperFound = 0;

                // �u���b�N�̎����𒲂ׂ�
                for (int j = 0; j < target.Count; j++)
                {
                    // �z�u�����߂��f�B���N�g�����u���b�N�̃��X�g�ɂ�����́i�r�������݂��Ă���f�B���N�g���j
                    if (target[j]["name"].ToString() == dir[i])
                    {
                        // 1��̊K�w�̃f�B���N�g�������܂݁A����1��̃f�B���N�g���ł͂Ȃ����̂��������ꍇ�iX�X�V�j
                        if (lastFound == 1)
                        {
                            target[j]["x"] = prePositionX + preWidthX / 2 + int.Parse(target[j]["widthX"].ToString()) / 2 + space + 100;
                            prePositionX = int.Parse(target[j]["x"].ToString()) - int.Parse(target[j]["widthX"].ToString()) / 2;

                        }
                        // ���̑��̏ꍇ�iX���̂܂܁j
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

                // �z�u�����߂��f�B���N�g�����u���b�N�̃��X�g�ɂȂ����́i�r�������݂��Ȃ��f�B���N�g���j 
                if (targetFound == 0)
                {
                    Dictionary<String, object> newDictionary = new Dictionary<string, object>();
                    newDictionary["name"] = dir[i];
                    newDictionary["widthX"] = fixedX;
                    newDictionary["widthY"] = fixedY;
                    
                    newDictionary["y"] = prePositionY + preWidthY / 2 + int.Parse(newDictionary["widthY"].ToString()) / 2 + space;

                    // 1��̊K�w�̃f�B���N�g�������܂݁A����1��̃f�B���N�g���ł͂Ȃ����̂��������ꍇ�iX�X�V�j
                    if (lastFound == 1)
                    {
                        newDictionary["x"] = prePositionX + preWidthX / 2 + int.Parse(newDictionary["widthX"].ToString()) / 2 + space + 100;
                        prePositionX = int.Parse(newDictionary["x"].ToString()) - int.Parse(newDictionary["widthX"].ToString()) / 2;

                    }
                    // ���̑��̏ꍇ�iX���̂܂܁j
                    else
                    {
                        newDictionary["x"] = prePositionX + int.Parse(newDictionary["widthX"].ToString()) / 2;
                        //prePositionX = int.Parse(newDictionary["x"].ToString()) - int.Parse(newDictionary["widthX"].ToString()) / 2;
                    }

                    

                    List<Dictionary<String, object>> addList = new List<Dictionary<string, object>>();
                    addList.Add(newDictionary);

                    // �u���b�N�̈ꗗ�ɒǉ�����
                    addedBlockList.Add(dir[i], addList);

                    
                    prePositionY = int.Parse(newDictionary["y"].ToString());

                    preWidthX = int.Parse(newDictionary["widthX"].ToString());
                    preWidthY = int.Parse(newDictionary["widthY"].ToString());

                }


                // ���ɒu�����f�B���N�g���ꗗ�ɒǉ�����
                addedDir.Add(dir[i]);
            }
        }
        return addedBlockList;
    }


    /**
	 *
	 * �u���b�N�̕������߂郁�\�b�h
	 * target��0�Ԗڂ̃r���̕���20�𑫂���
	 * target�̌��ȏ�ɂȂ�ŏ��̕����������߂�
     * ex) target��1�i�r����1�j��1^2 = 1 >= 1 ��1�{��OK
     * ex) target��2�`4��2^2 = 4 >= 2�`4 ��0�Ԗڂ�2�{�̕��������OK
     * ex) target��5�`9��3^2 = 9 >= 5�`9 ��0�Ԗڂ�3�{�̕��������OK 
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
	 * �u���b�N�̕������߂郁�\�b�h2
	 * 
	 */
    void SetWidth2(List<Dictionary<String, object>> target, List<Dictionary<String, object>> block)
    {
        int count = 0;  // �r���E�u���b�N�̌�

        int space = 50; // �Œ蕝

        int i;

        // �r���E�u���b�N����ׂ�Ƃ��Ɠ������@�ŃJ�E���g���Ă���
        for (i = 0; ; i++)
        {
            int y = i;

            for (int x = 0; x <= y; x++)
            {
                count++;
                if (count == target.Count)
                {
                    goto Finish; // �S���I�������Finish�֍s��
                }
            }
            for (y--; y >= 0; y--)
            {
                count++;
                if (count == target.Count)
                {
                    goto Finish; // �S���I�������Finish�֍s��
                }
            }
        }

    Finish:
        // �Ō�̃r���̔ԍ��i��-1�j��i^2+i�ȏ�̎��͊p�̃r����i^2�̃r�����
        if(count - 1 >= i * i + i)
        {
            
            block[0]["widthX"] = int.Parse(target[i * i + i]["x"].ToString()) + int.Parse(target[i * i + i]["widthX"].ToString()) / 2 + space;
            block[0]["widthY"] = int.Parse(target[i * i]["y"].ToString()) + int.Parse(target[i * i]["widthY"].ToString()) / 2 + space;
        }
        // �Ō�̃r���̔ԍ���i^2+1��菬�����Ƃ���i^2�Ԗڃr����(i-1)�̏��̎��̊p�̃r�����
        else
        {          
            block[0]["widthX"] = int.Parse(target[(i - 1) * (i - 1) + (i - 1)]["x"].ToString()) + int.Parse(target[(i - 1) * (i - 1) + (i - 1)]["widthX"].ToString()) / 2 + space;
            block[0]["widthY"] = int.Parse(target[i * i]["y"].ToString()) + int.Parse(target[i * i]["widthY"].ToString()) / 2 + space;
        }

    }


    /**
	 *
	 * �r���̍��W�����߂Ă������\�b�h
	 * �u���b�N�̍��W�Ƒ��΍��W����r����u�����W�����߂Ă���
	 * 
	 */
    void SetGlobalLocation(Dictionary<String,List<Dictionary<String, object>>> building, Dictionary<String,List<Dictionary<String, object>>> block)
	{
		foreach (String key in building.Keys) {
            // �u���b�N�̍��W������Ă���
            float blockX = float.Parse(block[key][0]["x"].ToString()) - float.Parse(block[key][0]["widthX"].ToString()) / 2;
			float blockY = float.Parse(block[key][0]["y"].ToString()) - float.Parse(block[key][0]["widthY"].ToString()) / 2;
            List<Dictionary<String, object>> buildingList = building[key];

            // �r���̍��W�����߂Ă���
			foreach(Dictionary<String, object> oneBuilding in buildingList){
				oneBuilding["globalX"] = float.Parse(blockX.ToString()) + float.Parse(oneBuilding["x"].ToString());
				oneBuilding["globalY"] = float.Parse(blockY.ToString()) + float.Parse(oneBuilding["y"].ToString());
			}
		}
	}

    /**
	 *
	 * �r�������Ă郁�\�b�h
	 * building�I�u�W�F�N�g�𕡐����Ĕz�u���Ă���
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

                // �v���[�g�Ńr��������
                //PilingPlate(oneBuilding);


                

                // �r�������Ă�
                //GameObject clone = Instantiate (this.building, new Vector3 (float.Parse (oneBuilding ["globalX"].ToString ()), (float.Parse (oneBuilding ["height"].ToString ()) / 2) + 3, float.Parse (oneBuilding ["globalY"].ToString ())), transform.rotation) as GameObject;
                GameObject clone = Instantiate(this.building, new Vector3(float.Parse(oneBuilding["globalX"].ToString()), 3, float.Parse(oneBuilding["globalY"].ToString())), Quaternion.Euler(-90, 0, 0)) as GameObject;


                // �r���ɂ��Ȃ܂���t����
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


                // �r���̑傫����������
                //clone.transform.localScale = new Vector3 (float.Parse (oneBuilding ["widthX"].ToString ()), float.Parse (oneBuilding ["height"].ToString ()), float.Parse (oneBuilding ["widthY"].ToString ()));
                clone.transform.localScale = new Vector3(float.Parse(oneBuilding["widthX"].ToString()) * (float)0.1, float.Parse(oneBuilding["widthY"].ToString()) * (float)0.1, float.Parse(oneBuilding["height"].ToString()) * (float)0.1);
                //clone.GetComponent<Renderer>().material.color = Color.blue;



                //�r���̐F��ς���
                //clone.GetComponent<Building>().Init(new Color (float.Parse (oneBuilding ["color_r"].ToString ()), float.Parse (oneBuilding ["color_g"].ToString ()), float.Parse (oneBuilding ["color_b"].ToString ())));
                //clone.Gmponent<Building>().Init(new Color((float)0.5, (float)0.8, (float)1.0));


                // SATD�����������ɖڈ������
                //AddSATD(oneBuilding);

                IList sList = oneBuilding["SATD"] as IList;
				if(sList.Count != 0){

                    // �ڈ������
                    GameObject test = Instantiate (this.checkSATD, new Vector3 (1, 1, 1), transform.rotation) as GameObject;
                    test.name = clone.name;
                    test.tag = "enemy";

                    for (int i = 0; i < sList.Count; i++)
                    {
                        test.name = test.name + (int.Parse(sList[i].ToString()) + 1).ToString() + ",";

                        // �p�[�e�B�N���̖ڈ�����
                        GameObject particle = Instantiate(this.sense, new Vector3(0, 1, 0), transform.rotation) as GameObject;
                        var r = particle.GetComponent<ParticleSystem>().shape;
                        r.radius = float.Parse(oneBuilding["widthX"].ToString()) * (float)0.7;

                        var s = particle.GetComponent<ParticleSystem>();
                        s.startSize = float.Parse(oneBuilding["widthX"].ToString()) * (float)0.6 + 5;
                        if(s.startSize < 20)
                        {
                            s.startSize = 20;
                        }

                        s.startSpeed = float.Parse(oneBuilding["widthX"].ToString()) * (float)0.5;


                        particle.transform.Rotate(new Vector3((float)270, (float)0, (float)0));
                        particle.transform.position = new Vector3(float.Parse(oneBuilding["globalX"].ToString()), (float.Parse(oneBuilding["height"].ToString()) - float.Parse(sList[i].ToString())) * (float)0.8845 + 3, float.Parse(oneBuilding["globalY"].ToString()));
                        particle.name = "sence:" + oneBuilding["name"] + (int.Parse(sList[i].ToString())).ToString();
                    }
                    test.name = test.name.Substring(0, test.name.Length - 1);

                    clone.name = test.name;

                    test.name = "marker_" + test.name;

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

                    test.transform.rotation = Quaternion.Euler(45,45,45);

                    /*
                    // �p�[�e�B�N���̖ڈ�����
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

                    // �v���~�e�B�u�ȃI�u�W�F�N�g�ŉ�����
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

        // �u���b�N�����ɒu���Ă���

        //Debug.Log("TEST");
        foreach (String key in block.Keys) {
			//Debug.Log(key);
			List<Dictionary<String, object>> blockList = block [key];
			GameObject clone = Instantiate (this.ground, new Vector3(float.Parse(blockList[0]["x"].ToString()), 2, float.Parse(blockList[0]["y"].ToString())), transform.rotation) as GameObject;
			clone.transform.localScale = new Vector3 (float.Parse (blockList [0]["widthX"].ToString ()), 2, float.Parse (blockList [0]["widthY"].ToString ()));
            clone.name = blockList[0]["name"].ToString().Substring(1);
        }


        // �ǉ����̃u���b�N�����ɒu���Ă���

        //Debug.Log("TEST");
        foreach (String key in block2.Keys)
        {
            //Debug.Log("AddedBlock:"+key);
            List<Dictionary<String, object>> blockList = block2[key];
            GameObject clone = Instantiate(this.ground, new Vector3(float.Parse(blockList[0]["x"].ToString()), 2, float.Parse(blockList[0]["y"].ToString())), transform.rotation) as GameObject;
            clone.transform.localScale = new Vector3(float.Parse(blockList[0]["widthX"].ToString()), 2, float.Parse(blockList[0]["widthY"].ToString()));
            //clone.GetComponent<Renderer>().material.color = Color.green;
            clone.name = blockList[0]["name"].ToString().Substring(1);
        }

    }

    // �v���[�g��ςݏグ��SATD������g�R�����ڈ󂪂����r�������
    void PilingPlate(Dictionary<String, object> target)
    {
        IList sList = target["SATD"] as IList;  // SATD�̃��X�g
        Material satd = Resources.Load("SATD") as Material; // SATD�p�̃}�e���A��

        int check = 0;

        // ��i���ςݏグ��
        for (int i = 0; i < int.Parse(target["height"].ToString()); i++)
        {
            GameObject clone = Instantiate(this.plate, new Vector3(float.Parse(target["globalX"].ToString()), float.Parse((i * 2).ToString()) + 1f, float.Parse(target["globalY"].ToString())), transform.rotation) as GameObject; // 1�i�ς�
            clone.transform.localScale = new Vector3(float.Parse(target["width"].ToString()), 2, float.Parse(target["width"].ToString()));

            // ���̒i�i�s�j��SATD������Ƃ��̓}�e���A����ύX����
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

    // SATD������Ƃ���ɕ���������Ƃ����傫���ڈ�p�̃v���[�g�����
    void AddSATD(Dictionary<String, object> target)
    {
        IList sList = target["SATD"] as IList;  // SATD�̃��X�g
        int check = 0;

        // 1�i�����Ă���
        for (int i = 0; i < int.Parse(target["height"].ToString()); i++)
        {
            // ���̒i�i�s�j��SATD������Ƃ��ɖڈ������
            if (sList.Count != 0 && sList[check].ToString() == i.ToString())
            {
                GameObject clone = Instantiate(this.plate, new Vector3(float.Parse(target["globalX"].ToString()), float.Parse(i.ToString()) + 1f, float.Parse(target["globalY"].ToString())), transform.rotation) as GameObject;
                clone.transform.localScale = new Vector3(float.Parse(target["widthX"].ToString()) + 0.7f, 1, float.Parse(target["widthY"].ToString()) + 0.7f);

                // �ڈ���F�ɂ���
                //clone.GetComponent<Renderer>().material.color = CalcComplementaryColor(new Color(float.Parse(target["color_r"].ToString()), float.Parse(target["color_g"].ToString()), float.Parse(target["color_b"].ToString())));
                clone.GetComponent<Renderer>().material.color = CalcComplementaryColor(new Color((float)0.5, (float)0.8, (float)1.0));
              

                // ���Ȃ܂�������
                clone.name = "(SATD)" + target["name"].ToString() + "@line" + i.ToString();

                if (check < sList.Count - 1)
                {
                    check++;
                }
            }

        }
    }
	

    // ��F���v�Z����֐�
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


    // �n�ʂ����֐�
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

        // ���W�{������ԑ傫�� & ���W�\�����������AX���W��Y�i���ۂ�Z�j���W�̔ԍ�������Ă���
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

        // X��Y�i�z���g��Z�j�̍��W�ƕ������߂Ă���
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
        

        // �J�������X�^�[�g����
        GameObject obj = GameObject.Find("Main Camera");
        mainCamera = obj.GetComponent<CameraMove>();
        mainCamera.StartCamera();

    }
	
    // ��������Ă����֐�
    void BuildStreets(List<Dictionary<String, object>> target)
    {
        // �u���b�N�̃��X�g�𖼑O���Ƀ\�[�g����
        target.Sort((b, a) => string.Compare(b["name"].ToString(), a["name"].ToString()));

        // �Ō�Ɍ��������O����v����u���b�N�̔ԍ�
        int foundLastBlock;

        // �O����v�����Ƃ��� + 1������̃u���b�N�̖��O
        string afterName = "";


        // 1��̊K�w�̂����O
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
                // i�Ԗڂ�target�̂����O�ƑO����v���邩
                if (target[j]["name"].ToString().StartsWith(target[i]["name"].ToString()) == true)
                {
                    //Debug.Log("orig: " + target[i]["name"].ToString());
                    //Debug.Log("after: " + target[j]["name"].ToString());

                    if (j > i)
                    {
                        // �O����v�����Ƃ���+1������̂����O���擾���Ă݂�
                        afterName = target[j]["name"].ToString().Substring(target[i]["name"].ToString().Length + 1);

                        // �O����v������̂����O��/���Ȃ��Ƃ�
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
                // �O����v�����Ƃ���+1������̂����O���擾
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


    // ��������Ă����֐�2
    // �u���b�N�̒��S����Ȃ��ĕӂɉ����悤�ɂ����Ă���
    void BuildStreets2(List<Dictionary<String, object>> target)
    {
        // �u���b�N�̃��X�g�𖼑O���Ƀ\�[�g����
        target.Sort((b, a) => string.Compare(b["name"].ToString(), a["name"].ToString()));

        // �Ō�Ɍ��������O����v����u���b�N�̔ԍ�
        int foundLastBlock;

        // �O����v�����Ƃ��� + 1������̃u���b�N�̖��O
        string afterName = "";


        // 1��̊K�w�̂����O
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
                // i�Ԗڂ�target�̂����O�ƑO����v���邩
                if (target[j]["name"].ToString().StartsWith(target[i]["name"].ToString()) == true)
                {
                    //Debug.Log("orig: " + target[i]["name"].ToString());
                    //Debug.Log("after: " + target[j]["name"].ToString());

                    if (j > i)
                    {
                        // �O����v�����Ƃ��납����̂����O���擾���Ă݂�
                        afterName = target[j]["name"].ToString().Substring(target[i]["name"].ToString().Length);

                        // �O����v������̂����O�̐擪��'/'�ŁA����ȊO��'/'���Ȃ��Ƃ�
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
                // �O����v�����Ƃ���+1������̂����O���擾
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


    // �����̏o���񐔂��J�E���g
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

    // Javascript����X�����n�߂邽�߂̃��\�b�h
    public void StartCityCreater(string id)
    {
        StartCoroutine(ReadFileOnline(id));
    }

    // �T�[�o�ɂ���Json�t�@�C����ǂݍ��ރ��\�b�h
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

}
