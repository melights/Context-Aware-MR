using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class DataManager : MonoBehaviour
{
    private MaterialStruct[] m_materialData;

	// Use this for initialization
	void Start () {
        LoadMaterialData();
    }
	
    private void LoadMaterialData()
    {
        var materialDataParse = JSON.ParseFile("material_definitions");
        var materialDataArray = materialDataParse["MATERIAL_DEFINITIONS"].AsArray;
        var materialDataCount = materialDataArray.Count;

        m_materialData = new MaterialStruct[materialDataCount];
  
        for (int i = 0; i < materialDataCount; i++)
        {
            var thisMaterial = materialDataArray[i];
        
            string name = thisMaterial["NAME"];
            string colHex = thisMaterial["HEX_COL"];
            Color col = HexUtility.hexToColor(colHex);
            m_materialData[i] = new MaterialStruct(name, col);
        }
    }
}
