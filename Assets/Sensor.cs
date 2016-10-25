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

    public string tagFilter = "enemy";

    // Use this for initialization
    void Start () {
        sensorObjects = new List<GameObject>();
        borderObjects = new List<GameObject>();
        
    }

    public void MakeSensorList()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag(tagFilter);
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
}
