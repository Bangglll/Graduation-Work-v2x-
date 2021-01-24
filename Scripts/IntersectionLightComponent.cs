using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//신호등 한 개의 정보
/*
signal_group 정보를 넣어주자!
Edit by HyeWon
*/
//Start HW
namespace V2x{
	public struct single_spat_msg{
		public uint single_id_region;
		public uint single_signal_group;
		public string single_movement_name;
		public uint single_eventstate;
		public uint single_minendtime;
	}
}//End HW
public class IntersectionLightComponent : MonoBehaviour 
{
	public uint signal_group;
	public string movement_name="STR";

	public V2x.single_spat_msg Traffic_light_Info(){
		var msg = new V2x.single_spat_msg(){
			single_signal_group = signal_group,
			single_movement_name = movement_name,
		};
		return msg;
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}


/*
public class IntersectionLightComponent : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
*/