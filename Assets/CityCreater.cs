using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using MiniJSON;


public class CityCreater : MonoBehaviour
{

	public GameObject building;     // �r��
	public Dictionary<string,object> city;  // Json�t�@�C���̒��g�̎���

    public GameObject CircleBlock;   // �y��

    public GameObject earth;         // �n��

    public GameObject fire;          // ��

    public GameObject street;        // ��

    private CameraMove mainCamera;  // ���C���J����

    public GameObject DirNamePlate;     // �y��̂����O�J���o��
    public GameObject FileNamePlate;    // �r���̂����O�J���o��

    public string jsonText = "";        // Json�t�@�C���̒��g
    // Use this for initialization

    public List<String> dir = new List<String>(); // �f�B���N�g���ꗗ

    public Sensor sensor;               // ���[�_�[

    public String rootDirName;          // ���[�g�f�B���N�g���̂����O
    public List<String> firstDirNameList = new List<String>();      // ���͂ɒu�����f�B���N�g���̂����O�̃��X�g

    

    public Dictionary<String, List<Dictionary<String, object>>> firstBlockDictionary2;  // ���͂ɒu�����y��̎���

    public Dictionary<String, List<Dictionary<String, object>>> allDirectory = new Dictionary<string, List<Dictionary<string, object>>>();  // �S�Ẵf�B���N�g���̎���

    Dictionary<String, List<Dictionary<String, object>>> arrangedBuildings;
    public String currentRoot;          // ���ݒ��S�ɂ���f�B���N�g���̂����O


    public List<String> satdFilesList = new List<String>(); // SATD�̂���t�@�C���̃��X�g

    void Start()
    {
        sensor = GameObject.Find("Main Camera").GetComponent<Sensor>();
        earth = Instantiate(this.earth, new Vector3(0, 0, 0), transform.rotation) as GameObject;

// Unity�ȊO�œ��������Ƃ���HTML�t�@�C����Javascript���Ăяo���悤�ɂ���

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

    // �s�s������Ă���
    void CreateCity ()
	{
        // �ŏ���Json�t�@�C����ǂݍ���
		this.city = Json.Deserialize (jsonText) as Dictionary<string,object>;
		var blocks = this.city ["blocks"] as IList;
		var buildings = this.city ["buildings"] as IList;
        var directories = this.city["directories"] as IList;

        var rootDir = this.city["root_depth"] as IList;
        var firstDir = this.city["first_depth"] as IList;

        var satdfiles = this.city["satdfiles"] as IList;

        // �f�B���N�g���ꗗ������ă\�[�g���Ă���
        for(int i=0; i< directories.Count; i++)
        {
            dir.Add(directories[i].ToString());
        }

        dir.Sort();

        // �f�B���N�g���ꗗ��������
        foreach(String name in dir)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("name", name);
            allDirectory[name] = new List<Dictionary<String, object>>();
            allDirectory[name].Add(data);
        }

        // root�f�B���N�g���̖��O���擾���Ă���
        foreach (Dictionary<string, object> content in rootDir)
        {
            rootDirName = content["name"].ToString();
        }

        // SATD�̂���t�@�C���̃��X�g������Ă���
        foreach(Dictionary<string, object> content in satdfiles)
        {
            satdFilesList.Add(content["name"].ToString());
            //Debug.Log(content["name"].ToString());
        }

        // �y�䂲�ƂɃr�����܂Ƃ߂�
        arrangedBuildings = ArrangeByKey(buildings, "block");

        // �y���r�������ĂĂ���
        LocateBlockAndBuilding2(allDirectory, arrangedBuildings, rootDirName);
	}

	
    // �y���r���̃I�u�W�F�N�g�𗧂ĂĂ����֐�
    // �č\�������̊֐����g��
    void LocateBlockAndBuilding2(Dictionary<string, List<Dictionary<string, object>>> blocks, Dictionary<String, List<Dictionary<String, object>>> buildings, String root)
    {
        // ���݂�root�f�B���N�g���i���S�ɂ���f�B���N�g���j���X�V
        currentRoot = root;
        
        // root�Ɏw�肵���f�B���N�g�����ɂ���f�B���N�g���̈ꗗ���쐬����
        List<String> firstDirList = SearchFirstDirectory(root, dir);

        // root�Ɏw�肵���f�B���N�g���p�̎��������
        List<String> tempRootList = new List<string>();
        tempRootList.Add(root);
        Dictionary<String, List<Dictionary<String, object>>> rootBlockDictionary = SearchDirectoryDictionary(blocks, tempRootList);

        // root�Ɏw�肵���f�B���N�g�������̃f�B���N�g���̎��������
        firstBlockDictionary2 = SearchDirectoryDictionary(blocks, firstDirList);


        // �r������ׂēy��̃T�C�Y�����߂�
        int edge = 0;
        // root�Ɏw�肵���y��̏���
        if (buildings.ContainsKey(root))
        {
            edge = SetBuildingLocation(buildings[root]);
        }

        // �~�̔��a�����߂�
        if (edge != 0)
            rootBlockDictionary[root][0]["radius"] = (edge) * Mathf.Sqrt(2) / 2 * 100;  // 1�ӂɒu�����r���̌�����y��̔��a�����߂�
        else
            rootBlockDictionary[root][0]["radius"] = 100;                               // �r�����u����Ȃ��ꍇ�͌Œ�l


        // ���͂̓y��̏���
        foreach (String key in firstBlockDictionary2.Keys)
        {
            //Debug.Log("***"+key);
            edge = 0;

            // �r����z�u����1�ӂ̌������߂�
            if (buildings.ContainsKey(key))
            {
                edge = SetBuildingLocation(buildings[key]);
            }
            //Debug.Log(edge);

            // �~�̔��a�����߂�
            if (edge != 0)
                firstBlockDictionary2[key][0]["radius"] = (edge) * Mathf.Sqrt(2) / 2 * 100;
            else
                firstBlockDictionary2[key][0]["radius"] = 100;
        }

        // �y��̔z�u�����߂�
        SetBlockCircleLocation2(rootBlockDictionary, firstBlockDictionary2, root);

        // ������I�u�W�F�N�g��destroy���Ă���
        ObjectDestroyer("Block");
        ObjectDestroyer("SATDBuilding");
        ObjectDestroyer("NormalBuilding");
        ObjectDestroyer("Fire");
        ObjectDestroyer("Street");
        //ObjectDestroyer("Ground");
        ObjectDestroyer("enemy");
        ObjectDestroyer("Plate");

        // �y��̏�Ƀr�����u����悤�Ƀr���̍��W�����肷��
        SetGlobalCircleLocation2(buildings, rootBlockDictionary, firstBlockDictionary2, root);


        // ��������r���Ɠy������Ă�i�I�u�W�F�N�g�𐶐����Ă����j

        // ���S�̃f�B���N�g���̓y��
        GameObject rootCircle = Instantiate(this.CircleBlock, new Vector3(float.Parse(rootBlockDictionary[root][0]["x"].ToString()), 3, float.Parse(rootBlockDictionary[root][0]["z"].ToString())), transform.rotation) as GameObject;
        rootCircle.transform.localScale = new Vector3(float.Parse(rootBlockDictionary[root][0]["radius"].ToString()) * 2, (float)2, float.Parse(rootBlockDictionary[root][0]["radius"].ToString()) * 2);
        //rootCircle.name = "Circle " + rootDirName;
        rootCircle.name = root.Substring(1);
        rootCircle.tag = "Block";

        rootCircle.GetComponent<Block>().SetMaterial(Color.cyan);

        // ���S�̃f�B���N�g���̓y��̃��^���
        var rootData = rootCircle.GetComponent<BlockData>();
        rootData.pathname = root.Substring(1);
        rootData.center = true;

        if (root == rootDirName)
            rootData.blockname = "root";
        else
            rootData.blockname = root.Substring(rootDirName.Length);

        rootData.end = false;


        // ���͂̓y���u��
        foreach (String key in firstBlockDictionary2.Keys)
        {
            GameObject firstCircle = Instantiate(this.CircleBlock, new Vector3(float.Parse(firstBlockDictionary2[key][0]["x"].ToString()), 3, float.Parse(firstBlockDictionary2[key][0]["z"].ToString())), transform.rotation) as GameObject;
            firstCircle.transform.localScale = new Vector3(float.Parse(firstBlockDictionary2[key][0]["radius"].ToString()) * 2, (float)2, float.Parse(firstBlockDictionary2[key][0]["radius"].ToString()) * 2);

            //firstCircle.name = "Circle " + key;
            firstCircle.name = key.Substring(1);
            firstCircle.tag = "Block";

            // ���^���
            var circleData = firstCircle.GetComponent<BlockData>();
            circleData.pathname = key.Substring(1);
            circleData.blockname = key.Substring(key.ToString().LastIndexOf("/") + 1);
            circleData.center = false;
            

            // ���̐�̃f�B���N�g���i�T�u�f�B���N�g���j���Ȃ������ׂ�
            bool end = true;
            foreach(String name in dir)
            {
                if (name.Contains("/" + circleData.pathname + "/"))
                    end = false;
            }
            circleData.end = end;

            // ���̐�Ƀf�B���N�g�����Ȃ��ꍇ�͐F���D�F�ɕς���
            if (end)
                firstCircle.GetComponent<Block>().SetMaterial(Color.gray);

            // �y��̐��SATD������Ȃ�y������F�ɂ���
            foreach (String name in satdFilesList)
            {
                
                if (name.Contains("/" + circleData.pathname + "/") && !end && "/" + circleData.pathname != name.Substring(0, name.LastIndexOf(name.Substring(name.LastIndexOf("/")))))
                {
                    circleData.insideSATD = true;
                    firstCircle.GetComponent<Block>().SetMaterial(Color.yellow);

                    break;
                }
            }


            // �����O�J���o����t����
            GameObject dirtext = Instantiate(this.DirNamePlate, new Vector3(float.Parse(firstBlockDictionary2[key][0]["x"].ToString()), 200, float.Parse(firstBlockDictionary2[key][0]["z"].ToString())), transform.rotation) as GameObject;
            dirtext.GetComponent<DirName>().SetNameText(circleData.blockname);
            dirtext.GetComponent<DirName>().SetBackSize();
            dirtext.tag = "Plate";
        }


        // �r�������Ă�
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

                    // ���^���
                    var buildingData = buildingObj.GetComponent<BuildingData>();
                    buildingData.filename = oneBuilding["name"].ToString();
                    buildingData.fullpath = oneBuilding["path"].ToString();
                    buildingData.pathname = buildingData.fullpath.Substring(rootDirName.Length);
                    buildingData.loc = int.Parse(oneBuilding["height"].ToString()) - 1;
                    buildingData.comment = (int.Parse(oneBuilding["widthX"].ToString()) - 1) / 10;
                   

                    // SATD������ꍇ���������𑝂₷
                    IList sList = oneBuilding["SATD"] as IList;
                    if (sList.Count != 0)
                    {

                        // SATD����r�����������O�J���o��
                        GameObject filetext = Instantiate(this.FileNamePlate, new Vector3(float.Parse(oneBuilding["globalX"].ToString()), float.Parse(oneBuilding["height"].ToString()) / 2, float.Parse(oneBuilding["globalZ"].ToString())), Quaternion.Euler(-90, 0, 0)) as GameObject;
                        filetext.GetComponent<FileName>().SetNameText(buildingData.filename);
                        filetext.GetComponent<FileName>().SetBackSize();
                        filetext.tag = "Plate";

                        buildingObj.tag = "SATDBuilding";
                        buildingObj.GetComponent<Building>().SetMaterial(Color.blue);

                        for (int i = 0; i < sList.Count; i++)
                        {
                            buildingData.satd.Add(int.Parse(sList[i].ToString()) + 1);

                            // �r���̃I�u�W�F�N�g�̖��O���C������
                            buildingObj.name = buildingObj.name + (int.Parse(sList[i].ToString()) + 1).ToString() + ",";


                            // ���̖ڈ�����
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



        // �������
        BuildCircleStreet2(rootBlockDictionary, firstBlockDictionary2, root);

        // �n�ʂ�����ăJ���������X�^�[�g
        SetCircleGround2(rootBlockDictionary, firstBlockDictionary2, root);

        // ���[�_�[�X�V
        sensor.MakeSensorList();
    }


    // �w�肵���^�O�̃I�u�W�F�N�g��j�󂵂Ă������\�b�h
    void ObjectDestroyer(String tagName)
    {
        GameObject[] tagobjs = GameObject.FindGameObjectsWithTag(tagName);

        foreach(GameObject obj in tagobjs)
        {
            GameObject.DestroyImmediate(obj);
        }
    }


    
    // root�Ɏw�肵���f�B���N�g�����ɂ���f�B���N�g����T��
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


    // ���X�g�ɂ��閼�O�ƈ�v����f�B���N�g���̎�����Ԃ�
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
                        // ���Ɏ����ɑ��݂���ꍇ��add��������
                        result[name].Add(target[key][0]);
                    }
                    else
                    {
                        // ���߂ďo�Ă�����new���Ď�����add����
                        result[name] = new List<Dictionary<String, object>>();
                        result[name].Add(target[key][0]);
                    }
                }
            }
        }
        return result;
    }
    

    // ����̓y���傫�����ɕ��ׂ����̂����O�̃��X�g��Ԃ�
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
     * key�̒l���Ƃ�target���܂Ƃ߂郁�\�b�h
     * �p�r1�F�r���𑮂���y�䂲�Ƃɂ܂Ƃ߂�
	 * �p�r2�F�y��𖼑O���Ƃɂ܂Ƃ߂�
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



    // �r���𐳕��`���ۂ����ׂ�
    int SetBuildingLocation(List<Dictionary<String, object>> target)
    {
        //target.Sort((b, a) => string.Compare(b["block"].ToString(), a["block"].ToString()));
        // �y�䖼�Ŕ�r���ē�����������t�@�C�����Ŕ�r���ă\�[�g
        target.Sort((b, a) => string.Compare(b["block"].ToString(), a["block"].ToString()) != 0 ? string.Compare(b["block"].ToString(), a["block"].ToString()) : string.Compare(b["name"].ToString(), a["name"].ToString()));

        int edgeNum = 0;
        for (int i = 0; ; i++)
        {
            // �r�����̕��������傫�������l����ӂ̌��ɂ���
            if (i * i > target.Count)
            {
                edgeNum = i;
                break;
            }
        }

        int z = 0;
        int x = 0;

        // �r���̍��W�����߂Ă���
        for(int i = 0; i < target.Count; i++)
        {
            target[i]["x"] = 100 * x;
            target[i]["z"] = 100 * z;

            //Debug.Log(target[i]["x"] + "," + target[i]["z"]);

            // �E��1�i��
            x++;

            // �E�[�܂ōs������0�ɖ߂�
            if (x >= edgeNum)
            {
                x = 0;

                // ����1�i��
                z++;
            }       
        }

        // 1�ӂ̒����i�����`�̕ӂɒu���r���̌��j
        return edgeNum;
    }


    // �y����~�`�ɔz�u���Ă����i�œK���ł͂Ȃ��̂ł����ӂ��j
    void SetBlockCircleLocation2(Dictionary<String, List<Dictionary<String, object>>> root, Dictionary<String, List<Dictionary<String, object>>> first, String rootName)
    {
        //firstDirNameList

        // ������root��u��
        root[rootName][0]["x"] = 0;
        root[rootName][0]["z"] = 0;


        // �p�x
        float deg = 360 / (float)first.Count;
        double rad = deg * Mathf.PI / 180.0;

        float max = 0;
        float min = 0;

        Boolean start = true;

        List<String> keyList = new List<string>();

        // ��ԑ傫���E���������a���擾����
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

        
        // 1�K�w�ڂ̍��W�����߂Ă���
        int radnumber = 0;

        // ����̓y�䂪4�ȉ��Ȃ�e�y��̒��a�������Ώd�Ȃ�Ȃ�
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
            // �����O�̃��X�g�𔼌a���ɂ���
            keyList = SortFirstBlockName(keyList, first);

            // �����O�̃��X�g�̗v�f��
            int listSize = keyList.Count;
            int[] nameOrder = new int[listSize];


            // �召�召�c�ƂȂ�悤�ɂ����O�̃��X�g�̓Y������ׂĂ���
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

                // �����̃}�C�i�X���傫������ꍇ�̓��[�g�̒��a�{���̓y��̒��a������
                if (distance < minDistance)
                {
                    first[keyList[nameOrder[i]]][0]["x"] = Mathf.Cos((float)rad * radnumber) * minDistance;
                    first[keyList[nameOrder[i]]][0]["z"] = Mathf.Sin((float)rad * radnumber) * minDistance;
                }

                // �y��̔��a�̍ő�E�ŏ��l�̍����傫���قǁE�y��̌����傫���قǊO�Ɍ����� �����傫���قǓ����ƊO���̍����L����
                // �ł��������\�e�L�g�[�B�̂����w�̐搶�ɍœK���𕷂��������悳����
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


    // ���߂Ă������r���̔z�u�����ۂ̓y��̏�ɍ��킹�Ă���
    void SetGlobalCircleLocation2(Dictionary<String, List<Dictionary<String, object>>> building, Dictionary<String, List<Dictionary<String, object>>> rootBlock, Dictionary<String, List<Dictionary<String, object>>> firstBlock, String root)
    {
        // �y�䂲�ƂɌ��߂Ă���
        foreach (String key in building.Keys)
        {
            float blockX = 0;
            float blockZ = 0;
            float radius = 0;

            // ���S�̓y��̏ꍇ
            if (key == root)
            {
                blockX = float.Parse(rootBlock[root][0]["x"].ToString());
                blockZ = float.Parse(rootBlock[root][0]["z"].ToString());
                radius = float.Parse(rootBlock[root][0]["radius"].ToString());
            }
            // ���͂̓y��̏ꍇ
            else if(firstBlock.ContainsKey(key))
            {
                blockX = float.Parse(firstBlock[key][0]["x"].ToString());
                blockZ = float.Parse(firstBlock[key][0]["z"].ToString());
                radius = float.Parse(firstBlock[key][0]["radius"].ToString());
            }

            List<Dictionary<String, object>> buildingList = building[key];

            // �r���̍��W�������Ɍ��߂Ă���
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
            
   
    // �n�ʂ�����ă��C���J�������X�^�[�g������
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

        // X����Z������ԑ傫������ԏ������y��𒲂ׂĒn�ʂ�������O���ɑ��݂���悤�ɂ���
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

        // �J�������X�^�[�g����
        GameObject obj = GameObject.Find("Main Camera");
        mainCamera = obj.GetComponent<CameraMove>();
        mainCamera.StartCamera();
    }
    

    // ���S����̂т铹�H�����֐�
    void BuildCircleStreet2(Dictionary<String, List<Dictionary<String, object>>> rootBlock, Dictionary<String, List<Dictionary<String, object>>> firstBlock, String root)
    {
        float centerX = float.Parse(rootBlock[root][0]["x"].ToString());
        float centerZ = float.Parse(rootBlock[root][0]["z"].ToString());

        //int num = 0;

        // 0�x�̃x�N�g��
        Vector2 uVec = new Vector2(1, 0);

        // ����ɏ��Ԃɓ���L�΂��Ă���
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

            // arcCos(�x�N�g���̓��� / �x�N�g���̑傫���̐�)�Ŋp�x���o��
            float newDeg = Mathf.Acos(Vector2.Dot(vec, uVec) / (vec.magnitude * uVec.magnitude));

            // arcCos��0�`�΂܂ł����o���Ȃ��̂�z�����}�C�i�X�Ȃ畉�̒l�ɂ���
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


    // �����̏o���񐔂��J�E���g
    public static int CountChar(string s, char c)
    {
        return s.Length - s.Replace(c.ToString(), "").Length;
    }


    // Javascript����X�����n�߂邽�߂̃��\�b�h
    // �R�R��HTML������Ăяo�����
    public void StartCityCreater(string id)
    {
        StartCoroutine(ReadFileOnline(id));
    }

    // �T�[�o�ɂ���Json�t�@�C����ǂݍ��ރ��\�b�h
    IEnumerator ReadFileOnline(string id)
    {
        // �ȉ��̏ꍇ�A163.221.29.171�ŃA�N�Z�X�����Javascript�̃G���[���N����̂Œ��ӁI
        // rocat.naist.jp�ŃA�N�Z�X�����OK

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
	
    // �s�s�̏��iJson�t�@�C���̏��j��Ԃ�
	public Dictionary<string,object> GetCity(){
		return this.city;
	}
	
    // �n�ʂ̃I�u�W�F�N�g��Ԃ�
    public GameObject GetGround()
    {
        return this.earth as GameObject;
    }

    // �ǂݍ���Json�t�@�C���̏���Ԃ�
    public string GetJsonText()
    {
        return this.jsonText;
    }

    // ���͂ɔz�u�����y��i�f�B���N�g���j�̎�����Ԃ�
    public Dictionary<String, List<Dictionary<String, object>>> GetFirstBlockList()
    {
        //return this.firstBlockDictionary;
        return this.firstBlockDictionary2;
    }


    // �s�s���č\������֐�
    public void RemakeCity(String root, Boolean directFlag)
    {
        // ���X�g�Ŕ�ԂƂ�����ւ̃{�^�����������Ƃ��͒��ڍs��
        if (directFlag)
        {
            //Debug.Log(root);
            LocateBlockAndBuilding2(allDirectory, arrangedBuildings, root);
        }
        // ���X�g������ł��Ȃ��A����root�Ɏw�肵���y�䂪���݂�root�̂Ƃ���1��ɍs��
        else if(root == currentRoot)
        {
            // �v���W�F�N�g�̃��[�g�f�B���N�g���ɓ�����ꍇ�͏オ�Ȃ��̂ōs���Ȃ�
            if(root != rootDirName)
            {
                LocateBlockAndBuilding2(allDirectory, arrangedBuildings, root.Substring(0, root.LastIndexOf("/")));
            }
        }
        // ����ȊO�̏ꍇ�͉��ɊK�w������ꍇ�̂ݍs��
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

    // ���[�g�f�B���N�g���̖��O��Ԃ�
    public String GetRootName()
    {
        return rootDirName;
    }

    // ���ݒ��S�ɂ���f�B���N�g���̖��O��Ԃ�
    public String GetCurrentDir()
    {
        if (currentRoot == rootDirName)
            return "/";
        else
            return currentRoot.Substring(rootDirName.Length);
    }
}
