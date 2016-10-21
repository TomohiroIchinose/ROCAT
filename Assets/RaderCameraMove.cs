using UnityEngine;
using System.Collections;

public class RaderCameraMove : MonoBehaviour {

    public GameObject mainCamera;

	// Use this for initialization
	void Start () {
	
	}

    // Update is called once per frame
    void Update()
    {
        this.transform.position = (new Vector3(mainCamera.transform.position.x, (mainCamera.transform.position.y) + 1800, mainCamera.transform.position.z));
    }
}
