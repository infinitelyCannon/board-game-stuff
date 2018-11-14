﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DragScript : MonoBehaviour {

    Camera gameCam;
    Transform bObjects;

    int dragType = 0;
    // 0 = 
    int boardObjectCount = 0;
    float scaleTween = 0f;
    public float scaleTweenMax = 0.25f;
    public float scaleOffset = 0.15f;
    public float stackOffset = 0.15f;
    float stackOffsetMult = 1f;
    public float stackOffsetSpeed = 0.15f;
    public Vector3 grabOffset = Vector3.zero;
    string currentTag = "Untagged";

    void addDraggedObject(Transform addObject) {
        if (transform.childCount == 0)
            gameObject.GetComponents<AudioSource>()[0].Play();
        else
            gameObject.GetComponents<AudioSource>()[1].Play();
        Vector3 originalScale = addObject.localScale;
        addObject.parent = transform;
        addObject.localScale = originalScale;
        addObject.GetComponent<BoxCollider>().enabled = false;
        addObject.gameObject.GetComponent<SpriteRenderer>().sortingOrder = boardObjectCount + 1 + transform.childCount;
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).localPosition = grabOffset + new Vector3(transform.childCount * (stackOffset * stackOffsetMult) / 2f - (stackOffset * stackOffsetMult) * i, 0f, 0f);
    }


	// Use this for initialization
	void Start () {
        gameCam = GameObject.Find("Main Camera").GetComponent<Camera>();
        bObjects = GameObject.Find("Interactive").transform;
        boardObjectCount = bObjects.childCount;
        for (int i = boardObjectCount - 1; i > -1; i--) {
            if (bObjects.GetChild(i).GetComponent<SpriteRenderer>() != null) {
                bObjects.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = i + 1;
                bObjects.GetChild(i).transform.position = new Vector3(
                    bObjects.GetChild(i).transform.position.x,
                    bObjects.GetChild(i).transform.position.y,
                    -1f - i * 0.01f);
                bObjects.GetChild(i).gameObject.AddComponent<BoxCollider>();
            }
        }
	}

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Vector3 mousePos = gameCam.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
        if (dragType == 0) {
            if (Input.GetKeyDown(KeyCode.Mouse1)) {
                Ray ray = gameCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit)) {
                    hit.transform.SetAsFirstSibling();
                    gameObject.GetComponents<AudioSource>()[4].Play();
                    for (int i = boardObjectCount - 1; i > -1; i--) {
                        if (bObjects.GetChild(i).GetComponent<SpriteRenderer>() != null) {
                            bObjects.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = i + 1;
                            bObjects.GetChild(i).transform.position = new Vector3(
                                bObjects.GetChild(i).transform.position.x,
                                bObjects.GetChild(i).transform.position.y,
                                -1f - i * 0.01f);
                        }
                    }
                }
            } else if (Input.GetKey(KeyCode.Mouse0)) {
                Ray ray = gameCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit)) {
                    dragType = 1;
                    addDraggedObject(hit.transform);
                }
            } else if (Input.GetKey(KeyCode.LeftShift)) {
                dragType = 2;
            }
            else if (Input.GetKey(KeyCode.LeftControl)) {
                dragType = 3;
            }
        }

        if (dragType > 0) {
            if (scaleTween < scaleTweenMax)
                scaleTween = Mathf.Min(scaleTweenMax, scaleTween + Time.deltaTime);
            transform.localScale = Vector3.one * (1f + scaleOffset * Mathf.Sin(Mathf.Min(Mathf.Max(scaleTween/scaleTweenMax * Mathf.PI / 2f, 0f), Mathf.PI / 2f)));
            transform.position = new Vector3(mousePos.x, mousePos.y, -2f);
            if (Input.GetAxis("Mouse ScrollWheel") > 0 || (Input.GetAxis("Mouse ScrollWheel") < 0 && stackOffsetMult > 0f)) {
                if (Input.GetAxis("Mouse ScrollWheel") > 0)
                    stackOffsetMult += stackOffsetSpeed;
                else
                    stackOffsetMult = Mathf.Max(stackOffsetMult - stackOffsetSpeed, 0f);
                for (int i = 0; i < transform.childCount; i++)
                    transform.GetChild(i).localPosition = grabOffset + new Vector3(transform.childCount * (stackOffset * stackOffsetMult) / 2f - (stackOffset * stackOffsetMult) * i, 0f, 0f);
            }
            if (dragType == 1 && !Input.GetKey(KeyCode.Mouse0) ||
                dragType == 2 && !Input.GetKey(KeyCode.LeftShift) ||
                dragType == 3 && !Input.GetKey(KeyCode.LeftControl)) {
                dragType = 0;
                scaleTween = 0f;
                if (transform.childCount > 0) {
                    if (transform.childCount > 2)
                        gameObject.GetComponents<AudioSource>()[3].Play();
                    else
                        gameObject.GetComponents<AudioSource>()[2].Play();
                }
                while (transform.childCount > 0) {
                    Transform changeChild = transform.GetChild(0);
                    Vector3 originalScale = changeChild.localScale;
                    changeChild.GetComponent<BoxCollider>().enabled = true;
                    changeChild.parent = bObjects;
                    changeChild.localScale = originalScale;
                    changeChild.position = new Vector3(changeChild.position.x, changeChild.position.y, -1f);
                }
                transform.localScale = Vector3.one;

                for (int i = bObjects.childCount - 1; i > -1; i--) {
                    if (bObjects.GetChild(i).GetComponent<SpriteRenderer>() != null) {
                        bObjects.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = i + 1;
                        bObjects.GetChild(i).transform.position = new Vector3(
                            bObjects.GetChild(i).transform.position.x,
                            bObjects.GetChild(i).transform.position.y,
                            -1f - i * 0.01f);
                    }
                }
            }
            else if (dragType == 2 || dragType == 3) {
                Ray ray = gameCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit)) {
                    if ((hit.transform.gameObject.CompareTag(currentTag) || transform.childCount == 0) && dragType == 2 || dragType == 3) {
                        addDraggedObject(hit.transform);
                        if (transform.childCount == 1)
                            currentTag = transform.GetChild(0).gameObject.tag;
                    }
                }
                if (Input.GetKeyDown(KeyCode.Mouse0) && transform.childCount > 1) {
                    gameObject.GetComponents<AudioSource>()[4].Play();
                    transform.GetChild(0).SetAsLastSibling();
                    for (int i = 0; i < transform.childCount; i++) {
                        transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = boardObjectCount + 2 + i;
                        transform.GetChild(i).localPosition = grabOffset + new Vector3(transform.childCount * (stackOffset * stackOffsetMult) / 2f - (stackOffset * stackOffsetMult) * i, 0f, 0f);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Mouse1) && transform.childCount > 1) {
                    gameObject.GetComponents<AudioSource>()[4].Play();
                    transform.GetChild(transform.childCount - 1).SetAsFirstSibling();
                    for (int i = 0; i < transform.childCount; i++) {
                        transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = boardObjectCount + 2 + i;
                        transform.GetChild(i).localPosition = grabOffset + new Vector3(transform.childCount * (stackOffset * stackOffsetMult) / 2f - (stackOffset * stackOffsetMult) * i, 0f, 0f);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Space) && transform.childCount > 1) {
                    gameObject.GetComponents<AudioSource>()[5].Play();
                    for (int i = 0; i < transform.childCount; i++)
                        transform.GetChild(Random.Range(0, transform.childCount)).SetAsFirstSibling();
                    for (int i = 0; i < transform.childCount; i++) {
                        transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = boardObjectCount + 2 + i;
                        transform.GetChild(i).localPosition = grabOffset + new Vector3(transform.childCount * (stackOffset * stackOffsetMult) / 2f - (stackOffset * stackOffsetMult) * i, 0f, 0f);
                    }
                }
            }
        }
	}
}
