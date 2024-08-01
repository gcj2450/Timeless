﻿using UnityEngine;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using MassiveNet;

public class Client : MonoBehaviour {

    public List<string> serverIps = new List<string>(){
        "127.0.0.1",
        "47.153.55.146"
    };
    public int serverPort = 17000;

    private NetConnection server;

    private NetSocket socket;
    private NetViewManager viewManager;
    private NetZoneClient zoneClient;
    private int serverIpIndex = 0;

    private static Client _instance;
    public static Client instance {
        get {
            if ( _instance == null ){
                _instance = GameObject.FindObjectOfType<Client>();
            }
            return _instance;
        }
    }

    public NetSocket Socket {
        get {
            return socket ?? GetComponent<NetSocket>();
        }
    }
    public NetViewManager NetViewManager {
        get {
            return viewManager ?? GetComponent<NetViewManager>();
        }
    }
    public NetConnection Server {
        get {
            return server;
        }
    }
    public string ServerIp {
        get {
            return serverIps[serverIpIndex];
        }
    }

    void Awake(){
        if ( GameObject.FindObjectsOfType<Client>().Length > 1 )
            Destroy(gameObject);

        DontDestroyOnLoad(this);
    }
    void Start() {
        socket = GetComponent<NetSocket>();
        viewManager = GetComponent<NetViewManager>();
        zoneClient = GetComponent<NetZoneClient>();

        zoneClient.OnZoneSetupSuccess += ZoneSetupSuccessful;
        socket.Events.OnConnectedToServer += ConnectedToServer;
        socket.Events.OnDisconnectedFromServer += DisconnectedFromServer;
        viewManager.OnNetViewCreated += OnNetViewCreated;

        socket.StartSocket();
        socket.RegisterRpcListener(this);

        NetSerializer.Add<Item>(Item.Serialize,Item.Deserialize);
        NetSerializer.Add<Equip>(Item.Serialize,Item.Deserialize);

        socket.Connect(serverIps[serverIpIndex] + ":" + serverPort);
    }
    
    private void ZoneSetupSuccessful(NetConnection conn) {
        
    }
    private void ConnectedToServer(NetConnection conn){
        if ( conn.Endpoint.ToString() == serverIps[serverIpIndex] + ":" + serverPort ){
            Debug.Log("Connected to server");
            server = conn;
        }
    }
    private void DisconnectedFromServer(NetConnection serv) {
        viewManager.DestroyViewsServing(serv);
    }
    private void OnNetViewCreated(NetView view){
        Debug.Log(view.name + " created");
    }
}
