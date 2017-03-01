using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sensor : MonoBehaviour {

    public GameObject[] trackedObjects;
    List<GameObject> sensorObjects;
    public GameObject sensorPrefab;
    List<GameObject> borderObjects;
    public float switchDistance;
    public Transform helpTransform;

    public string tagFilter = "SATDBuilding";   // レーダーに映す対象

    GameObject[] gos;
    List<GameObject> cpGos = new List<GameObject>();
    public GameObject mainCamera;

    public float sizeX = 0;

    // Use this for initialization
    void Start () {
        sensorObjects = new List<GameObject>();
        borderObjects = new List<GameObject>();
        
    }

    public void MakeSensorList()
    {
        cpGos = new List<GameObject>();
        gos = GameObject.FindGameObjectsWithTag(tagFilter);     // 対象のオブジェクトを全部取得
        cpGos.AddRange(gos);                                    // 配列の中身をリストに入れる
        cpGos.Sort((b, a) => (int)b.transform.position.x - (int)a.transform.position.x);    // X座標の大きさでソート（今は使ってないけど）
        //Debug.Log(gos.Length);
        createSensorObjects(gos);
    }
	
	// Update is called once per frame
	void Update () {
        for (int i = 0; i < sensorObjects.Count; i++)
        {
            if (sensorObjects[i] != null)
            {
                // メインカメラとレーダーの点（対象のビル）の距離が一定値より大きいとき
                if (Vector3.Distance(sensorObjects[i].transform.position, transform.position) > switchDistance)
                {
                    helpTransform.LookAt(sensorObjects[i].transform);               // 対象のビルの方向を求める
                    borderObjects[i].transform.position = transform.position + switchDistance * helpTransform.forward;  // 縁の方の点を動かす
                    borderObjects[i].layer = LayerMask.NameToLayer("Sensor");       // 縁の方の点を見えるようにする
                    sensorObjects[i].layer = LayerMask.NameToLayer("Invisible");    // 内側の方の点を見えないようにする
                }
                // カメラと点の距離が一定値以下のとき
                else
                {
                    borderObjects[i].layer = LayerMask.NameToLayer("Invisible");    // 縁の方の点を見えないようにする
                    sensorObjects[i].layer = LayerMask.NameToLayer("Sensor");       // 内側の方の点を見えるようにする
                }
            }
        }

    }

    // レーダーに映る点を生成する
    void createSensorObjects(GameObject[] target)
    {
        foreach (GameObject o in target)
        {
            //Debug.Log(o.name);

            // レーダーの円の内側に映る点を生成
            GameObject k = Instantiate(sensorPrefab, o.transform.position, Quaternion.identity) as GameObject;
            sensorObjects.Add(k);
            k.name = "Radar:" + o.name;
            k.tag = "enemy";

            // レーダーの円の縁に映る点を生成
            GameObject j = Instantiate(sensorPrefab, o.transform.position, Quaternion.identity) as GameObject;
            borderObjects.Add(j);
            j.name = "Border:" + o.name;
            j.tag = "enemy";
        }
    }

    // カメラから一番近いSATDがあるビルの座標を返す（今は使っていません）
    Vector2 calcDistance(Vector2 camera, bool direction)
    {
        Vector2 targetEnemy = new Vector2();


        int targetNum = 0;

        float distance = -1;

        // 一番近いEnemy = SATDを探す
        for(int i = 0; i < cpGos.Count; i++)
        {
            if(Vector2.Distance(camera, new Vector2(cpGos[i].transform.position.x, cpGos[i].transform.position.z)) < distance || distance == -1)
            {
                distance = Vector2.Distance(camera, new Vector2(cpGos[i].transform.position.x, cpGos[i].transform.position.z));
                targetEnemy = new Vector2(cpGos[i].transform.position.x, cpGos[i].transform.position.z);
                targetNum = i;
            }
        }
        // Debug.Log(distance);

        // 距離が一定値のときに処理
        if (distance == 100)
        {
            // 移動方向がtrueのとき
            if (direction == true)
            {
                // SATDのあるビルの個数の次を参照しようとしたときは0番目にする
                if (targetNum + 1 >= gos.Length)
                {
                    targetEnemy = new Vector2(cpGos[0].transform.position.x, cpGos[0].transform.position.z);
                    sizeX = cpGos[0].transform.localScale.x;
                }
                else
                {
                    targetEnemy = new Vector2(cpGos[targetNum + 1].transform.position.x, cpGos[targetNum + 1].transform.position.z);
                    sizeX = cpGos[targetNum + 1].transform.localScale.x;
                }
            }
            // 移動方向がfalseのとき
            else
            {
                // 0番目を参照しようとしたときは最後のものにする
                if (targetNum - 1 < 0)
                {
                    targetEnemy = new Vector2(cpGos[cpGos.Count - 1].transform.position.x, cpGos[cpGos.Count - 1].transform.position.z);
                    sizeX = cpGos[cpGos.Count - 1].transform.localScale.x;
                }
                else
                {
                    targetEnemy = new Vector2(cpGos[targetNum - 1].transform.position.x, cpGos[targetNum - 1].transform.position.z);
                    sizeX = cpGos[targetNum - 1].transform.localScale.x;
                }
            }
        }

        return targetEnemy;
    }
}
