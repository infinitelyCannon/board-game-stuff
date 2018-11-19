using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TriggerField
{
    public Vector3 center;
    public Vector3 size;

    private string name;

    public void setName(string str)
    {
        name = str;
    }

    public string getName()
    {
        return name;
    }
}
