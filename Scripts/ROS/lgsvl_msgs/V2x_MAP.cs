using System.Collections.Generic;
/*
Edit by HyeWon
*/
namespace Ros
{
    [MessageType("lgsvl_msgs/V2x_MAP")]
    public struct V2x_MAP
    {
        public Header header;
				public string msg_type;
				//public uint BSM_MSG_TYPE = 1;
				//public uint SPAT_MSG_TYPE = 2;
				//public uint TIM_MSG_TYPE = 3;
				//public uint MAP_MSG_TYPE = 4;
				

			  public int map_id_region;
        public int map_refpoint_lat;
        public int map_refpoint_lon;
        public int map_speed_limit;
        public int map_g_cnt;

        public List<int> map_g_id;
        public List<int> map_g_nodelist_cnt;
        public List<int> map_g_nodelist_xy;
        public List<int> map_g_connectsto_cnt;
        public List<int> map_g_connectsto_lane; 
    }
}