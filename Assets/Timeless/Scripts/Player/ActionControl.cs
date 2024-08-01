﻿using UnityEngine;
using System.Collections;
using MassiveNet;

public class ActionControl : MonoBehaviour {
	
    public bool testing = false;
    private NetView view;
    private Character character;
    //private CameraFollow limbsCam;

    void Awake(){
        view = GetComponent<NetView>();
        character = GetComponent<Character>();
        //limbsCam = GameObject.Find("LimbsCamera").GetComponent<CameraFollow>();
    }
    void Update(){
        if ( Input.GetMouseButton(0) ){
            if ( testing ){
                character.Fire();
            } else
                view.SendReliable("AttackInput", RpcTarget.Server);
        }

        //if ( Input.GetMouseButton(1) ){
        //    limbsCam.Aim();
        //}
        //if ( Input.GetMouseButtonUp(1) ){
        //    limbsCam.Reset();
        //}
    }

}
