using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RaderCameraMove : MonoBehaviour {

    public GameObject mainCamera;
    private Block selectedBlock;

    public Canvas canvas2;
    public Text block_name;
    public Image block_back;

    public CityCreater cc;
    private GameObject ground;

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
        
        ground = cc.GetGround();

        float height = ground.transform.localScale.x * 0.5f / Mathf.Tan(this.GetComponent<Camera>().fieldOfView * 0.5f * Mathf.Deg2Rad);

        this.transform.position = (new Vector3(this.transform.position.x, height, this.transform.position.z));
        ControlByMouse();
    }

    void ControlByMouse()
    {
        Block block = GetRaycastHitBlock();

        HighlighMouseOverBlock(block);


        if (Input.GetMouseButtonDown(0))
        {
            if (block != null)
            {
                //Debug.Log("###Remake###");
                cc.RemakeCity("/" + block.name, false);
            }

        }
    }

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

    private void HighlighMouseOverBlock(Block block)
    {
        if (block != null)
        {
            if (selectedBlock == null || selectedBlock != block)
            {
                if (block.transform.name.IndexOf(".git") + 4 == block.transform.name.Length)
                {
                    block_name.text = "(root)";
                }
                else
                {
                    block_name.text = block.transform.name.Substring(block.transform.name.IndexOf(".git") + 5);
                }
                block_back.color = new Color(block_back.color.r, block_back.color.g, block_back.color.b, 0.7f);

                if (selectedBlock)
                {
                    selectedBlock.Deselected();
                }

                selectedBlock = block;
                selectedBlock.Selected();
            }
        }
        else
        {
            if (selectedBlock)
            {
                selectedBlock.Deselected();
                selectedBlock = null;
            }
            block_name.text = "";
            block_back.color = new Color(block_back.color.r, block_back.color.g, block_back.color.b, 0);
        }
    }
}
