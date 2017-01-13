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

    public string tagFilter = "SATDBuilding";

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
        gos = GameObject.FindGameObjectsWithTag(tagFilter);
        cpGos.AddRange(gos);
        cpGos.Sort((b, a) => (int)b.transform.position.x - (int)a.transform.position.x);
        //Debug.Log(gos.Length);
        createSensorObjects(gos);
    }
	
	// Update is called once per frame
	void Update () {
        for (int i = 0; i < sensorObjects.Count; i++)
        {
            if (Vector3.Distance(sensorObjects[i].transform.position, transform.position) > switchDistance)
            {
                helpTransform.LookAt(sensorObjects[i].transform);
                borderObjects[i].transform.position = transform.position + switchDistance * helpTransform.forward;
                borderObjects[i].layer = LayerMask.NameToLayer("Sensor");
                sensorObjects[i].layer = LayerMask.NameToLayer("Invisible");
            }
            else
            {
                borderObjects[i].layer = LayerMask.NameToLayer("Invisible");
                sensorObjects[i].layer = LayerMask.NameToLayer("Sensor");
            }
        }
        /*
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                Vector2 warp = calcDistance(new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.z), false);
                mainCamera.transform.position = new Vector3(warp.x - 100, 50, warp.y);
                mainCamera.transform.rotation = Quaternion.Euler(0, 90, 0);
            }
            else
            {
                Vector2 warp = calcDistance(new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.z), true);
                mainCamera.transform.position = new Vector3(warp.x - 100, 100, warp.y);
                mainCamera.transform.rotation = Quaternion.Euler(0, 90, 0);
            }
        }
        */

    }

    void createSensorObjects(GameObject[] target)
    {


        foreach (GameObject o in target)
        {
            //Debug.Log(o.name);

            GameObject k = Instantiate(sensorPrefab, o.transform.position, Quaternion.identity) as GameObject;
            sensorObjects.Add(k);

            GameObject j = Instantiate(sensorPrefab, o.transform.position, Quaternion.identity) as GameObject;
            borderObjects.Add(j);
        }

        /*
        foreach(GameObject o in trackedObjects)
        {
            GameObject k = Instantiate(sensorPrefab, o.transform.position, Quaternion.identity) as GameObject;
            sensorObjects.Add(k);

            GameObject j = Instantiate(sensorPrefab, o.transform.position, Quaternion.identity) as GameObject;
            borderObjects.Add(j);
        }
        */
    }

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
        if (distance == 100)
        {
            if (direction == true)
            {
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
            else
            {
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
