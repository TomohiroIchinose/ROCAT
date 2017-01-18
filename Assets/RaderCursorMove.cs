using UnityEngine;
using System.Collections;

public class RaderCursorMove : MonoBehaviour {

    public GameObject mainCamera;
    public CityCreater cc;
    private GameObject ground;

    // Use this for initialization
    void Start () {
        cc = GameObject.Find("CityCreater").GetComponent<CityCreater>();
    }
	
	// Update is called once per frame
	void Update () {
        ground = cc.GetGround();

        this.transform.localScale = new Vector3(ground.transform.localScale.x / 40, ground.transform.localScale.x / 40, ground.transform.localScale.x / 40);
        
        this.transform.position = (new Vector3((mainCamera.transform.position.x), 10, mainCamera.transform.position.z));
        transform.rotation = new Quaternion(0, mainCamera.transform.rotation.y, 0, mainCamera.transform.rotation.w);

    }
}
