using UnityEngine;
using System.Collections;

public class MoveDistance : MonoBehaviour {

    private Camera mainCamera;

	// Use this for insitialization
	void Start () {
        GameObject obj = GameObject.Find("Main Camera");
        mainCamera = obj.GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        MoveBehaviour move = GetComponent<MoveBehaviour>();
        // 近いと生えてくる
        if (Vector2.Distance(new Vector2(this.transform.position.x,this.transform.position.z), new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.z)) < 2500)
        {
            move.UpBuilding();
        }
        // ちょっと近いと少しだけ見える
        else if(Vector2.Distance(new Vector2(this.transform.position.x, this.transform.position.z), new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.z)) < 8000)
        {
            move.DownBuilding();
        }
        // 遠いと見えない
        else
        {
            move.ElaseBuilding();
        }
    }
}
