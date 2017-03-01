using UnityEngine;
using System.Collections;

public class FireBehaviour : MonoBehaviour {

    private Camera mainCamera;

    // Use this for initialization
    void Start () {
        GameObject obj = GameObject.Find("Main Camera");
        mainCamera = obj.GetComponent<Camera>();
    }
	
	// Update is called once per frame
	void Update () {

        //Debug.Log(this.name + ":" + Vector2.Distance(new Vector2(this.transform.position.x, this.transform.position.z), new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.z)));

        // 炎がメインカメラに近いと見えるようになって遠いと見えなくなるようにする
        if (Vector2.Distance(new Vector2(this.transform.position.x, this.transform.position.z), new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.z)) < 2500)
        {
            //Debug.Log(this.name + ":ちかい");
            this.GetComponent<Renderer>().enabled = true;
        }
        else
        {
            //Debug.Log(this.name + ":とおい");
            this.GetComponent<Renderer>().enabled = false;
        }

    }
}
