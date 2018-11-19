using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPlayer{

    public string name;
    public int cardNum;

    public NetPlayer(string str)
    {
        name = str;
        cardNum = 0;
    }
}
