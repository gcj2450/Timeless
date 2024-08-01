﻿using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {

    public List<string> visibleIds = new List<string>(){
        "ChatUI"
    };

    private List<UI> uiElements = new List<UI>();

    private static UIManager _instance;
    public static UIManager instance {
        get {
            if ( _instance == null ){
                _instance = GameObject.FindObjectOfType<UIManager>();
            }
            return _instance;
        }
    }

    void Awake(){
        if ( GameObject.FindObjectsOfType<UIManager>().Length > 1 ){
            Destroy(this);
        }

        DontDestroyOnLoad(this);
    }
    void Start(){
        Debug.Log(transform.GetComponentsInChildren<UI>().Length);
        foreach (UI ui in transform.GetComponentsInChildren<UI>()){
            uiElements.Add(ui);
            if ( !visibleIds.Contains(ui.Id) ) ui.SetDisplay(false);
        }
    }
    void Update(){
        if ( Input.GetKeyDown(KeyCode.I) ){
            UI ui = GetUI("InventoryUI");
            if ( ui != null ) ui.SetDisplay(!ui.Script.gameObject.activeSelf);
        }
    }

    public UI GetUI(string id){
        return uiElements.Where<UI>(ui => ui.Id == id).FirstOrDefault<UI>();
    }
    public bool InDeadZone(Vector2 screenPos){
        foreach (UI ui in uiElements){
            if ( ui.Script.gameObject.activeSelf ){
                if ( RectTransformUtility.RectangleContainsScreenPoint( (RectTransform)ui.Script.transform, screenPos, null) ){
                    return true;
                }
            }
        }

        return false;
    }
}
