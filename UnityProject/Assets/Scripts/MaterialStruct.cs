using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialStruct {

    public string m_name;
    public string m_hexCol;
    public int m_breakOnImpact;
    public Color m_colour;
    public int m_index;


    // Use this for initialization
    public MaterialStruct(
        string name,
        string hexCol,
        int breakOnImpact,
        Color colour,
        int index          
        )
    {
        m_name = name;
        m_hexCol = hexCol;
        m_breakOnImpact = breakOnImpact;
        m_colour = colour;
        m_index = index;
    }
}
