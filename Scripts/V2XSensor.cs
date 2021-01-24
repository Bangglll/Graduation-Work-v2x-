using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
Edit by HyeWon
*/
public class V2XSensor : MonoBehaviour, Comm.BridgeClient {
    [System.NonSerialized]
    public List<IntersectionComponent> intersections = new List<IntersectionComponent>();
    [System.NonSerialized]
    public List<IntersectionLightComponent> IntersectionlightC = new List<IntersectionLightComponent>();
    [System.NonSerialized]
    public List<IntersectionTrafficLightSetComponent> lightGroups = new List<IntersectionTrafficLightSetComponent>();
    [System.NonSerialized]
    public List<uint> single_group = new List<uint>();
    [System.NonSerialized]
    public List<string> single_movement = new List<string>();
    public List<uint> single_event_state = new List<uint>();
    public List<uint> single_min_endtime = new List<uint>();
    [System.NonSerialized]
    public uint id_region;
    public uint movement_cnt;
    public string SensorName = "V2XDECODER";
    public string Spat_Topic = "/simulator/sensor/v2x/spat";
    public string Map_Topic = "/simulator/sensor/v2x/map";
    public string Tim_Topic = "/simulator/sensor/v2x/tim";
    public string Bsm_Topic = "/simulator/sensor/v2x/bsm";
    public string V2xFrameId = "/v2x";
    public ROSTargetEnvironment TargetRosEnv;
    Vector3 pos;

    public Transform intersectionsHolder;
    public Transform TFHolder;
    public sim_map tf;
    public float lon;
    public float lat;
    private float tf_x = 636.8605168f;
    private float tf_y = -1026.726107f;

    Comm.Bridge Bridge;

    Comm.Writer<Ros.V2x_Spat> SV2x;
    Comm.Writer<Ros.V2x_BSM> BV2x;
    Comm.Writer<Ros.V2x_MAP> MV2x;
    Comm.Writer<Ros.V2x_TIM> TV2x;

    private GameObject Agent;
    public bool PublishMessage = false;

    [HideInInspector]
    public bool IsEnabled;
    uint Sequence;

    private void Awake()
    {
        AddUIElement();
    }
    void Start(){
        intersectionsHolder = GameObject.Find("TrafficLight").GetComponent<Transform>();
        //TFHolder = GameObject.Find("TF").GetComponent<Transform>();
        //Menu Scene에서 intersectionsHolder가 비어있는 오류가 발생하여 TrafficLight GameObject를 찾아서 넣어줌.
    }

    void Update()
    {
        TFHolder = GameObject.Find("XE_Rigged-autoware").GetComponent<Transform>();
        pos = TFHolder.position;
        Receive();
    }

    int cnt = 0;
        
    void SendV_x(){
        if (Bridge == null || Bridge.Status != Comm.BridgeStatus.Connected || !PublishMessage || !IsEnabled)
        {
            return;
        }
    
        var smsg = new Ros.V2x_Spat()//Spat message에 관한 pub
        {
            header = new Ros.Header()
            { 
                stamp = Ros.Time.Now(),
                seq = Sequence++,
                frame_id = V2xFrameId,
            },
            msg_type = "SPAT_MSG_TYPE",
            spat_id_region = id_region,
            spat_movement_cnt = movement_cnt,
            //v2x메세지 발행시 필요한 토픽
            spat_signalgroup = single_group,
			spat_movement_name = single_movement, // assume that movement state contains only one movement event
			spat_eventstate=single_event_state, //0 : unavaliable/ 3: stop and remain/ 5 : permissive_movement_allowed
			spat_minendtime=single_min_endtime,
        };
        SV2x.Publish(smsg);

        var mmsg = new Ros.V2x_MAP()//MAP message에 관한 pub
        {
            header = new Ros.Header()
            { 
                stamp = Ros.Time.Now(),
                seq = Sequence++,
                frame_id = V2xFrameId,
            },
            msg_type = "MAP_MSG_TYPE",
        };
        MV2x.Publish(mmsg);

        var bmsg = new Ros.V2x_BSM()//BSM message에 관한 pub
        {
            header = new Ros.Header()
            { 
                stamp = Ros.Time.Now(),
                seq = Sequence++,
                frame_id = V2xFrameId,
            },
            msg_type = "BSM_MSG_TYPE",
            bsm_lat = lat,
            bsm_lon = lon,
        };
        BV2x.Publish(bmsg);

        var tmsg = new Ros.V2x_TIM()//TIM message에 관한 pub
        {
            header = new Ros.Header()
            { 
                stamp = Ros.Time.Now(),
                seq = Sequence++,
                frame_id = V2xFrameId,
            },
            msg_type = "TIM_MSG_TYPE",
        };
        TV2x.Publish(tmsg);
        
    }
    void Receive(){
        intersections.AddRange(intersectionsHolder.GetComponentsInChildren<IntersectionComponent>());
        //tf = TFHolder.GetComponent<sim_map>();
        //lat = tf.lat;
        //lon = tf.lon;
        lat = pos.x-tf_x;
        lon = pos.z-tf_y;
        foreach(var item in intersections)
        {
            if(item.intersection_region > 0){
                var msg  = new V2x.single_spat_msg();
                msg = item.Traffic_Spat_intersection_info();
                id_region = msg.single_id_region;
                single_group = msg.single_signal_group;
                single_movement = msg.single_movement_name;
                movement_cnt = msg.movement_count;
                single_event_state = msg.single_eventstate;
                single_min_endtime = msg.single_minendtime;
                SendV_x();
            }
            else{
                continue;
            }
            single_group.Clear();
            single_movement.Clear();
            single_event_state.Clear();
            single_min_endtime.Clear();
        }
        intersections.Clear();
    }

    public void GetSensors(List<Component> sensors)
    {
        sensors.Add(this);
    }

    public void OnBridgeAvailable(Comm.Bridge bridge)
    {
        Debug.Log("Init V2XBridge");//HW_start
        Bridge = bridge;
        Bridge.OnConnected += () =>
        {
            if (TargetRosEnv == ROSTargetEnvironment.AUTOWARE || TargetRosEnv == ROSTargetEnvironment.DUCKIETOWN_ROS1)
            {
                SV2x = Bridge.AddWriter<Ros.V2x_Spat>(Spat_Topic);
                BV2x = Bridge.AddWriter<Ros.V2x_BSM>(Bsm_Topic);
                MV2x = Bridge.AddWriter<Ros.V2x_MAP>(Map_Topic);
                TV2x = Bridge.AddWriter<Ros.V2x_TIM>(Tim_Topic);
            }
        };
    }
		
    private void AddUIElement()
    {
        if (Agent == null)
            Agent = transform.root.gameObject;
        var v2xCheckbox = Agent.GetComponent<UserInterfaceTweakables>().AddCheckbox("ToggleV2X", "Enable V2X:", IsEnabled);
        v2xCheckbox.onValueChanged.AddListener(x => IsEnabled = x);
    }
}
