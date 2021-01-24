using System.Collections.Generic;
/*
Edit by HyeWon
*/
namespace Ros
{
    [MessageType("lgsvl_msgs/V2x_TIM")]
    public struct V2x_TIM
    {
        public Header header;
				public string msg_type;
				//public uint BSM_MSG_TYPE = 1;
				//public uint SPAT_MSG_TYPE = 2;
				//public uint TIM_MSG_TYPE = 3;
				//public uint MAP_MSG_TYPE = 4;
				public uint tim_dataframe_cnt;
				public List<uint> tim_starttime;
        public List<uint> tim_durationtime;
        public List<float> tim_anchor_lat;
        public List<float> tim_anchor_lon;
        public List<int> tim_lanewidth;
        public List<int> tim_direction;
        public List<int> tim_nodelist_xy_cnt;
        public List<int> tim_nodelist_xy_latlon; //lat, lon...
        public List<int> tim_content;
        public List<int> tim_speedlimit;
    }
}