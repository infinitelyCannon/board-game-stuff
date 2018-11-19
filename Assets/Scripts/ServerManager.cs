using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class ServerManager : MonoBehaviour {

    public InputField serverEntry;
    public TriggerField[] triggerFields;
    //public int maxPlayers;

    private bool isConnected = false;
    private WebSocket ws;
    private List<BoxCollider> bColliders;
    private bool shouldCheck = false;
    private const string GAME_TAG = "UNITY_GAME";
    private List<NetPlayer> players;
    private bool inbox = false;
    private string dataBuffer;
    private GameObject dropBox;
    private GameObject notification;

    string JSONIFY(string[] values)
    {
        string result = "{";

        for (int i = 0; i < values.Length; i += 2)
        {
            result += "\"" + values[i] + "\":" + "\"" + (i + 1 >= values.Length ? "null" : values[i + 1]) + "\",";
        }

        result = result.Substring(0, result.Length - 1);

        return result += "}";
    }

	// Use this for initialization
	void Start () {
        serverEntry.onEndEdit.AddListener(HandleInput);
        bColliders = new List<BoxCollider>();
        players = new List<NetPlayer>();
        dropBox = GameObject.Find("Delivered");
        notification = GameObject.Find("Notify");
        for(int i = 0; i < triggerFields.Length; i++)
        {
            triggerFields[i].setName("");
            BoxCollider bcol = gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;
            bcol.isTrigger = true;
            bcol.center = triggerFields[i].center;
            bcol.size = triggerFields[i].size;
            bColliders.Add(bcol);
        }
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyUp(KeyCode.X) && isConnected)
        {
            ws.Close();
            notification.GetComponent<Text>().text = "Connection Closed";
        }

        if (inbox)
        {
            inbox = false;
            OnMessage(dataBuffer);
        }
	}

    void AssignPlayer(string name)
    {
        for(int i = 0; i < triggerFields.Length; i++)
        {
            if(triggerFields[i].getName() == "")
            {
                triggerFields[i].setName(name);
                GameObject.Find("PlayerSpot" + (i + 1)).GetComponent<Text>().text = name + " (" + 0 + ")";
                Debug.Log("Added Player: " + name + " to Field: " + i.ToString());
                ws.Send(JSONIFY(new string[] {"type", "result", "data", "ADD_PLAYER_SUCESS", "player", name}));
                return;
            }
        }

        Debug.Log("Warning: No player slots left.");
    }

    void SubtractCard(string card, string player)
    {
        Transform outbox = GameObject.Find("Delivered").transform;
        Transform bObjects = GameObject.Find("Interactive").transform;

        for (int i = 0; i < players.Count; i++)
        {
            if(players[i].name == player)
            {
                GameObject.Find("PlayerSpot" + (i + 1)).GetComponent<Text>().text = players[i].name + " (" + --players[i].cardNum + ")";
            }
        }

        for (int j = 0; j < outbox.childCount; j++)
        {
            Transform temp = outbox.GetChild(j);

            if (temp.GetComponent<CardBack>().frontSprite.name == card)
            {
                temp.GetComponent<CardBack>().backUp = false;
                temp.GetComponent<SpriteRenderer>().sprite = temp.GetComponent<CardBack>().frontSprite;
                temp.parent = bObjects;
                temp.position = (char.IsDigit(temp.gameObject.name[0]) ? new Vector3(-1.54f, 0.75f, -1f) : new Vector3(0.12f, 0.98f, -1f));
                for (int k = bObjects.childCount - 1; k > -1; k--)
                {
                    if(bObjects.GetChild(k).GetComponent<SpriteRenderer>() != null)
                    {
                        bObjects.GetChild(k).GetComponent<SpriteRenderer>().sortingOrder = k + 1;
                    }
                }
            }
        }
    }

    void OnMessage(string str)
    {
        string[] command = str.Split(':');

        switch (command[0])
        {
            case "ADD_PLAYER":
                players.Add(new NetPlayer(command[1]));
                AssignPlayer(command[1]);
                break;
            case "GET_CARD":
                SubtractCard(command[1], command[2]);
                break;
            default:
                break;
        }
    }

    public void DropEvent()
    {
        Debug.Log("Card(s) Droped");
        shouldCheck = true;
    }

    private void FixedUpdate()
    {
        if (shouldCheck)
        {
            shouldCheck = false;

            for (int i = 0; i < bColliders.Capacity; i++)
            {
                Collider[] check = Physics.OverlapBox(bColliders[i].center, bColliders[i].size / 2f);
                for (int j = 0; j < check.Length; j++)
                {
                    if (check[j].tag.Contains("Card") && triggerFields[i].getName() != "")
                    {
                        Debug.Log("Found '" + check[j].name + "' in Box: " + triggerFields[i].getName());
                        ws.Send(JSONIFY(new string[] {"type", "give", "name", triggerFields[i].getName(), "card", check[j].gameObject.GetComponent<CardBack>().frontSprite.name}));
                        GameObject.Find("PlayerSpot" + (i + 1)).GetComponent<Text>().text = triggerFields[i].getName() + " (" + ++players[i].cardNum + ")";
                        check[j].gameObject.transform.parent = dropBox.transform;
                        check[j].gameObject.transform.localPosition = new Vector3(0, 0, -1f);
                    }
                }
            }
        }
    }

    private void HandleInput(string data)
    {
        //Debug.Log(data);
        if (!isConnected)
        {
            ws = new WebSocket(data);
            ws.OnMessage += (sender, e) =>
            {
                Debug.Log("Got: " + e.Data);
                dataBuffer = e.Data;
                inbox = true;
            };
            ws.OnOpen += (sender, e) =>
            {
                Debug.Log("Connection Opened");
                isConnected = true;
                ws.Send("{\"type\":\"register\", \"name\":\"" + GAME_TAG + "\"}");
                notification.GetComponent<Text>().text = "Connected!";
            };

            ws.OnClose += (sender, e) =>
            {
                Debug.Log("Connection Closed");
                serverEntry.transform.GetChild(0).GetComponent<Text>().text = "Connection Closed";
                isConnected = false;
            };
                

            ws.Connect();
        }
        else
        {
            ws.Send(data);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0, 0, 0.4f);
        for(int g = 0; g < triggerFields.Length; g++)
            Gizmos.DrawCube(triggerFields[g].center, triggerFields[g].size);
    }
}
