using System.Collections.Generic;
/*
Edit by HyeWon
*/
namespace Ros
{
    [MessageType("lgsvl_msgs/V2x_Spat")]
    public struct V2x_Spat
    {
        public Header header;
				public string msg_type;
				//public uint BSM_MSG_TYPE = 1;
				//public uint SPAT_MSG_TYPE = 2;
				//public uint TIM_MSG_TYPE = 3;
				//public uint MAP_MSG_TYPE = 4;
				public uint spat_id_region;
				public uint spat_movement_cnt;

				public List<uint> spat_signalgroup;
				public List<string> spat_movement_name; // assume that movement state contains only one movement event
				public List<uint> spat_eventstate; //0 : unavaliable/ 3: stop and remain/ 5 : permissive_movement_allowed
				public List<uint> spat_minendtime;
    }
}