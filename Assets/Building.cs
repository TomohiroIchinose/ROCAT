using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Building : MonoBehaviour
{
	private Color originalColor;

    private Material originalMaterial;

	public void Init(Color color)
	{
        //this.originalColor = color;
        //this.originalColor = (Resources.Load("Building", typeof(Material)) as Material).color;
        this.originalColor = (Resources.Load("Building_001_5", typeof(Material)) as Material).color;
        SetMaterial(originalColor);
	}

    // オンマウスのとき
	public void Selected()
	{
		SetMaterial(Building.SELECTED_COLOR);
	}

    // オンマウスの状態が解除されたとき
	public void Deselected()
	{
        //SetMaterial(originalColor);
        //SetMaterial((Resources.Load("Building", typeof(Material)) as Material).color);
        if (this.tag == "NormalBuilding")
            SetMaterial((Resources.Load("Building_001_5", typeof(Material)) as Material).color);    // 普通のビルの場合
        else
            SetMaterial(Building.SATD_COLOR);                                                       // SATDがあるビルの場合
    }

    // マテリアルを設定する
	public void SetMaterial(Color color)
	{
		Material material = null;
		if (Building.HasMaterial(color))
		{
			material = Building.GetMaterial(color);
		}
		else
		{
			material = new Material(GetComponent<Renderer>().material);
			material.color = color;
			Building.AddMaterial(color, material);
		}

		GetComponent<Renderer>().sharedMaterial = material;
	}

#region Static
	private static Dictionary<Color, Material> materialDict = new Dictionary<Color, Material>();
	private static readonly Color SELECTED_COLOR = Color.red;
    private static readonly Color SATD_COLOR = Color.blue;

    public static bool HasMaterial(Color color)
	{
		return materialDict.ContainsKey(color);
	}

	public static Material GetMaterial(Color color)
	{
		return materialDict[color];
	}

	public static void AddMaterial(Color color, Material material)
	{
		materialDict.Add(color, material);
	}
#endregion
}
