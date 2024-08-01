﻿using MassiveNet;
using UnityEngine;

public class PlayerPeer : MonoBehaviour {

    private NetView view;

    private void Awake() {
        view = GetComponent<NetView>();
        view.OnReadInstantiateData += ReadInstantiateData;
        view.OnReadSync += ReadSync;
    }

    private void ReadSync(NetStream stream) {
        transform.position = stream.ReadVector3();
    }

    private void ReadInstantiateData(NetStream stream) {
        transform.position = stream.ReadVector3();
    }
}
