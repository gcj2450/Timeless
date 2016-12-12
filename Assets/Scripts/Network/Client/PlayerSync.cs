﻿using MassiveNet;
using UnityEngine;

public class PlayerSync : MonoBehaviour {

    private float inputX;
    private float inputZ;
    private Vector3 lastPos;
    private NetView netView;

    void Start() {
        netView = GetComponent<NetView>();
        netView.OnWriteSync += WriteSync;
    }

    RpcTarget WriteSync(NetStream syncStream) {
        Vector3 velocity = transform.position - lastPos;
        syncStream.WriteVector3(transform.position);
        syncStream.WriteQuaternion(transform.rotation);
        syncStream.WriteVector2(new Vector2(velocity.x, velocity.z));
        lastPos = transform.position;
        return RpcTarget.Server;
    }

}
