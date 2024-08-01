﻿using UnityEngine;
using System.Collections;

public class FPSCameraControl : MonoBehaviour {
	
    public Transform follow;
    public float sensitivity = 5f;
    public float minimumY = -30F;
    public float maximumY = 60F;

    private float rotationY = 0F;
    private float rotationX = 0F;
    public Vector3 offset=new Vector3(0.5f,2,-4);

    void Awake(){
        //if ( follow != null ) offset = transform.position - follow.transform.position;

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }
    void Update(){
        if ( follow == null ) return;

        transform.position = follow.transform.position + offset;
        if ( !Cursor.visible ){
            rotationX += Input.GetAxis("Mouse X") * sensitivity;
         
            rotationY += Input.GetAxis("Mouse Y") * sensitivity;
            rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
        }

        transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);

        if ( Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.Escape) ){
            Cursor.visible = !Cursor.visible;
            if ( !Cursor.visible ) Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Initialize(Transform follow){
        this.follow = follow;
        //offset = transform.position - follow.transform.position;
    }
}