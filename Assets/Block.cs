using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Selected()
    {
        //SetMaterial(Building.SELECTED_COLOR);
    }

    public void Deselected()
    {
        //SetMaterial(originalColor);
    }
}
