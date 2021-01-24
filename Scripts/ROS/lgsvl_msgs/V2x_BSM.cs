using System.Collections.Generic;
/*
Edit by HyeWon
*/
namespace Ros
{
    [MessageType("lgsvl_msgs/V2x_BSM")]
    public struct V2x_BSM
    {
        public Header header;
				public string msg_type;
				//public uint BSM_MSG_TYPE = 1;
				//public uint SPAT_MSG_TYPE = 2;
				//public uint TIM_MSG_TYPE = 3;
				//public uint MAP_MSG_TYPE = 4;
				

				public List<uint> bsm_id;
        public float bsm_lat;
        public float bsm_lon;
        public int bsm_angle;
        public int bsm_size_width;
        public int bsm_size_length;
        public int bsm_classification;
    }
}