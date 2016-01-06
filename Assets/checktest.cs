using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class checktest : MonoBehaviour
{
    private Color originalColor;

    public void Init(Color color)
    {
        this.originalColor = color;

        SetMaterial(originalColor);
    }

    public void Selected()
    {
        SetMaterial(checktest.SELECTED_COLOR);
    }

    public void Deselected()
    {
        SetMaterial(originalColor);
    }

    public void SetMaterial(Color color)
    {
        Material material = null;
        if (checktest.HasMaterial(color))
        {
            material = checktest.GetMaterial(color);
        }
        else
        {
            material = new Material(GetComponent<Renderer>().material);
            material.color = color;
            checktest.AddMaterial(color, material);
        }

        GetComponent<Renderer>().sharedMaterial = material;
    }

    #region Static
    private static Dictionary<Color, Material> materialDict = new Dictionary<Color, Material>();
    private static readonly Color SELECTED_COLOR = Color.green;

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
