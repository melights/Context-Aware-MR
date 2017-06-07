using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialStruct {

    private string m_name;
    private Color m_colour;

    // Use this for initialization
    public MaterialStruct(
        string name,
        Color colour              
        )
    {
        m_name = name;
        m_colour = colour;		
	}
}
