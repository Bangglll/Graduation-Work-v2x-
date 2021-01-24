using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.IO;
using V2X.Header;
using System.Linq;

public class Network_Rsu: MonoBehaviour {
    private Text guiConsole;
    private Socket sock = null;
    private float timer = 0.0 f;
    private int counter = 0;
    private byte[] recvBuffer = new byte[1024];
    private byte[] sendBuffer = new byte[2048];
    private GameObject Agent;

    [HideInInspector]
    public bool IsEnabled;
    public bool done = false;
    int sz = 0;

    [System.NonSerialized]
    public List<uint> single_group = new List<uint>();
    public List<uint> single_event_state = new List<uint>();
    public List<uint> single_min_endtime = new List<uint>();

    [System.NonSerialized]
    public int id_region;
    public int movement_cnt;

    [System.NonSerialized]
    public List<IntersectionComponent> intersections = new List<IntersectionComponent>();
    [System.NonSerialized]
    public List<IntersectionLightComponent> IntersectionlightC = new List<IntersectionLightComponent>();
    [System.NonSerialized]
    public List<IntersectionTrafficLightSetComponent> lightGroups = new List<IntersectionTrafficLightSetComponent>();
    public Transform intersectionsHolder;

    [DllImport("v2x12", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr return_spat(
        ref int size,
        int id_region,
        int movement_count,
        uint[] single_signal_group,
        uint[] single_event_state,
        uint[] single_min_endtime
    );

    private string build;
    private string pay;
    public string dt;
    public obu_tcp_header[] _face;

    TcpListener Server;
    TcpClient Client;

    //set port
    private const int m_port = 15111;

    //for state
    private State m_state;

    //socket!!!
    private Socket m_socket;
    private enum State {
        SelectHost = 0,
        Listener,
        AcceptClient,
        ServerCommunication,
        StopListener,
        ClientCommunication,
        EndCommunication
    }

    void Start() {
        guiConsole = GameObject
            .Find("Console")
            .GetComponent<Text>();
        intersectionsHolder = GameObject
            .Find("TrafficLight")
            .GetComponent<Transform>();
        Debug.Log("in start [TCP]");
        m_state = State.Listener;
    }

    void Update() {
        switch (m_state) {
            case State.Listener:
                Debug.Log("in Listener [TCP]");
                Listener();
                break;
            case State.AcceptClient:
                AcceptClient();
                break;
            case State.ServerCommunication:
                ServerCommunication();
                break;
            case State.StopListener:
                StopListener();
                break;
            case State.ClientCommunication:
                ClientProcess();
                break;
            default:
                break;
        }
    }

    public static byte[] StructureToByte(object obj) {
        int datasize = Marshal.SizeOf(obj);
        IntPtr buff = Marshal.AllocHGlobal(datasize);
        Marshal.StructureToPtr(obj, buff, false);
        byte[] data = new byte[datasize];
        Marshal.Copy(buff, data, 0, datasize);
        Marshal.FreeHGlobal(buff);
        return data;
    }

    private T ByteToStruct<T>(byte[] buffer)where T : struct {
        int size = Marshal.SizeOf(typeof(T));
        if (size > buffer.Length) {
            throw new Exception();
        }
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(buffer, 0, ptr, size);
        T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
        Marshal.FreeHGlobal(ptr);
        return obj;
    }

    void AcceptClient() {
        Debug.Log("in AcceptClient");
        Client = Server.AcceptTcpClient();
        m_state = State.ServerCommunication;
    }

    void ServerCommunication() {
        intersections.AddRange(
            intersectionsHolder.GetComponentsInChildren < Inters ectionComponent > ()
        );
        foreach(var item in intersections) {
            if (item.intersection_region > 0) {
                var msg = new V2x.single_spat_msg();
                msg = item.Traffic_Spat_intersection_info();
                id_region = (int)msg.single_id_region;
                single_group = msg.single_signal_group;
                movement_cnt = (int)msg.movement_count;
                single_event_state = msg.single_eventstate;
                single_min_endtime = msg.single_minendtime;
                timer += Time.deltaTime;
                if (timer > 1.0 f) {
                    String tmp = String.Format("{0} times try: ", counter);
                    NetworkStream stream = Client.GetStream();
                    if (Client != null) {
                        try {
                            uint[] signal_group = single_group.ToArray();
                            uint[] event_state = single_event_state.ToArray();
                            uint[] min_endtime = single_min_endtime.ToArray();
                            sz = 0;
                            IntPtr ptr = return_spat(
                                ref sz,
                                id_region,
                                movement_cnt,
                                signal_group,
                                event_state,
                                min_endtime
                            ); //, single_move);
                            _face = new obu_tcp_header[1];
                            _face[0].packet_type = 0x5432;
                            _face[0].current_sequence = 99;
                            _face[0].payload_size = 11;
                            _face[0].device_type = 0x01;
                            _face[0].device_id = new byte[3];
                            _face[0].device_id[0] = 0x11;
                            _face[0].device_id[1] = 0x11;
                            _face[0].device_id[2] = 0x11;
                            byte[] header = new byte[sz];
                            Debug.Log("hello" + header.Length);
                            Marshal.Copy(ptr, header, 0, header.Length);
                            Marshal.FreeHGlobal(ptr);
                            Debug.Log("sz");
                            Array.Copy(header, 0, sendBuffer, 0, sz);
                            obu_tcp_header o_tmp = ByteToStruct<obu_tcp_header>(sendBuffer);
                            stream.Write(sendBuffer, 0, sz);
                            tmp += "Success \n" + ptr + "\n" + _face[0].packet_type + "\n" + face[0].current_sequence +
                                    "\n" + _face[0].payload_size + "\n" + _face[0].device_type + "\n" + _face[0].device_id[0] +
                                    "\n" + _face[0].device_id[1] + "\n" + _face[0].device_id[2]; //+"\n"+bsm;
                            ConsoleMessage(tmp);
                        } catch (Exception e) {
                            tmp += "Exception: " + e.Message;
                            ConsoleMessage(tmp);
                            Shutdown();
                        }
                    }
                    counter++;
                    timer = 0.0 f;
                }
            } else {
                continue;
            }
            single_group.Clear();
            single_event_state.Clear();
            single_min_endtime.Clear();
        }
        intersections.Clear();
    }
    
    void StopListener() {
        Debug.Log("Stop Listen");
        Client.Close();
        Server.Stop();
    }

    void ClientProcess() {}

    void Listener() {
        Server = new TcpListener(IPAddress.Any, m_port);
        try {
            Server.Start();
            Debug.Log("Start Server [TCP]");
        } catch (Exception ex) {
            Debug.Log("ex [TCP]");
        }
        m_state = State.AcceptClient;
    }

    void OnApplicationQuit() {
        Shutdown();
    }

    private void ReceiveComplete(IAsyncResult ar) {
        try {
            if (null == sock) 
                return;
            int len = sock.EndReceive(ar);
            if (len == 0) {
                Shutdown();
            } else {
                ConsoleMessage(String.Format("{0} received", recvBuffer[0]));
                sock.BeginReceive(
                    recvBuffer,
                    0,
                    recvBuffer.Length,
                    SocketFlags.None,
                    new AsyncCallback(ReceiveComplete),
                    null
                );
            }
        } catch (Exception e) {
            ConsoleMessage("Exception: " + e.Message);
            Shutdown();
        }
    }

    private void SendComplete(IAsyncResult ar) {
        try {
            if (null == sock) 
                return;
            int len = sock.EndSend(ar);
            if (len == 1) {
                ConsoleMessage("Send success");
            }
        } catch (Exception e) {
            ConsoleMessage("Exception: " + e.Message);
            Shutdown();
        }
    }

    private void ConsoleMessage(string msg) {
        guiConsole.text = msg;
    }
    
    private void Shutdown() {
        if (sock != null) {
            sock.Shutdown(SocketShutdown.Both);
            sock = null;
        }
    }
}