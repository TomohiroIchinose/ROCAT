using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MapCameraMove : MonoBehaviour {

    public GameObject mainCamera;
    private Block selectedBlock;

    public Canvas canvas2;
    public Text block_name;
    public Image block_back;

    public CityCreater cc;

    // Use this for initialization
    void Start () {
        cc = GameObject.Find("CityCreater").GetComponent<CityCreater>();

        block_name = canvas2.transform.GetComponentInChildren<Text>();
        block_back = canvas2.transform.GetComponentInChildren<Image>();
        block_name.text = "";
        block_back.color = new Color(block_back.color.r, block_back.color.g, block_back.color.b, 0);
    }

    // Update is called once per frame
    void Update()
    {
        ControlByMouse();
    }

    // マウス操作に関する関数
    void ControlByMouse()
    {
        Block block = GetRaycastHitBlock();

        HighlighMouseOverBlock(block);

        // クリックしたときの動作
        if (Input.GetMouseButtonDown(0))
        {
            // 土台をクリックしていたらメインカメラで映っている画面で土台をクリックした時と同じ動作をする
            if (block != null)
            {
                //Debug.Log("###Remake###");
                cc.RemakeCity("/" + block.name, false);
            }

        }
    }

    // マウスカーソル上にあるオブジェクトを返す（何もないor土台以外の場合はnull）
    private Block GetRaycastHitBlock()
    {
        RaycastHit hit;
        Ray ray = this.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 10000))
        {
            Block hitBlock = hit.transform.GetComponent<Block>();
            return hitBlock;
        }

        return null;
    }

    // マップ上の土台の上にマウスを乗せたときの動作（メインカメラで映っている画面で土台にマウスを載せた時と同じ動作をするようにしている）
    private void HighlighMouseOverBlock(Block block)
    {
        if (block != null)
        {
            if (selectedBlock == null || selectedBlock != block)
            {
                // マウスが乗っている土台がrootディレクトリの場合
                if (block.transform.name.IndexOf(".git") + 4 == block.transform.name.Length)
                {
                    block_name.text = "(root)";
                }
                // rootディレクトリ以外のディレクトリの場合
                else
                {
                    block_name.text = block.transform.name.Substring(block.transform.name.IndexOf(".git") + 5);
                }
                block_back.color = new Color(block_back.color.r, block_back.color.g, block_back.color.b, 0.7f); // ディレクトリ名を出す部分を表示する

                // 土台から土台にマウスが移る場合、先に乗っていた方の土台からマウスが離れたことにするよう処理
                if (selectedBlock)
                {
                    selectedBlock.Deselected();
                }

                selectedBlock = block;
                selectedBlock.Selected();
            }
        }
        // 土台以外のところにマウスがあるとき
        else
        {
            if (selectedBlock)
            {
                selectedBlock.Deselected();
                selectedBlock = null;
            }
            block_name.text = "";   // 
            block_back.color = new Color(block_back.color.r, block_back.color.g, block_back.color.b, 0);        // ディレクトリ名を出す部分を非表示にする
        }
    }
}
