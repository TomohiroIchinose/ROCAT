using UnityEngine;
using System.Collections;

public class RaderCursorMove : MonoBehaviour {

    public GameObject mainCamera;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.position = (new Vector3((mainCamera.transform.position.x), (mainCamera.transform.position.y) + 100, mainCamera.transform.position.z));
        transform.rotation = new Quaternion(0, mainCamera.transform.rotation.y, 0, mainCamera.transform.rotation.w);
    }
}
