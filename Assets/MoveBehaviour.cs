using UnityEngine;
using System.Collections;

public class MoveBehaviour : MonoBehaviour {

    private const int maxFrame = 40;    // 移動にかかる時間（F)
    private int _frame = 0;
    private Vector3 _move;
    private float startY;

	// Use this for initialization
	void Start () {
        //_move = new Vector3(0, 0, 0);
        startY = this.transform.position.y;
        //_move = GetDisplacement(new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z), new Vector3(this.transform.position.x, startY - this.transform.localScale.y - 10, this.transform.position.z), maxFrame);
        _move = GetDisplacement(new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z), new Vector3(this.transform.position.x, this.transform.localScale.z * -10, this.transform.position.z), maxFrame);

    }

    // Update is called once per frame
    void Update () {
        // maxFrameの時間の間動かす
	    if(_frame < maxFrame)
        {
                this.transform.position = this.transform.position + _move;
                _frame++;
        }
	}

    // ビルを上に動かす
    public void UpBuilding()
    {
        _frame = 0;
        _move = GetDisplacement(new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z), new Vector3(this.transform.position.x, startY, this.transform.position.z), maxFrame);
    }

    // ビルを下に動かす
    public void DownBuilding()
    {
        _frame = 0;
        //_move = GetDisplacement(new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z), new Vector3(this.transform.position.x, startY - this.transform.localScale.y + this.transform.localScale.y / 20, this.transform.position.z), maxFrame);
        _move = GetDisplacement(new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z), new Vector3(this.transform.position.x, this.transform.localScale.z * -7, this.transform.position.z), maxFrame);
    }

    // ビルを隠す（地面より下に下げて見えなくしている）
    public void ElaseBuilding()
    {
        _frame = 0;
        //_move = GetDisplacement(new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z), new Vector3(this.transform.position.x, startY - this.transform.localScale.y - 10, this.transform.position.z), maxFrame);
        _move = GetDisplacement(new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z), new Vector3(this.transform.position.x, this.transform.localScale.z * -10, this.transform.position.z), maxFrame);
    }

    // 1Fごとに動かす大きさを決める
    Vector3 GetDisplacement(Vector3 start, Vector3 end, uint frame)
    {
        return new Vector3((end.x - start.x) / (float)frame, (end.y - start.y) / (float)frame, (end.z - start.z) / (float)frame);
    }
}
