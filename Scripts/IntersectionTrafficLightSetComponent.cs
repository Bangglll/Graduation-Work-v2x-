/**
 * Copyright (c) 2018 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//신호등 한 라인의 정보
public class IntersectionTrafficLightSetComponent : MonoBehaviour
{
    public List<Renderer> lightRenderers = new List<Renderer>();
    //private List<IntersectionLightComponent> intersectionLightC = new List<IntersectionLightComponent>();
    public List<IntersectionLightComponent> intersectionLightC = new List<IntersectionLightComponent>(); //HW_Edit
    private List<IntersectionLightComponent> intersectionLightCurrent = new List<IntersectionLightComponent>(); //HW_edit
    public uint event_state;//HW_Edit
    public uint min_endtime;//HW_Edit
    public TrafficLightSetState currentState = TrafficLightSetState.None;
    public MapStopLineSegmentBuilder stopline;

    public void SetLightRendererData()
    {
        intersectionLightC.AddRange(transform.GetComponentsInChildren<IntersectionLightComponent>());
        foreach (var item in intersectionLightC)
        {
            lightRenderers.Add(item.GetComponent<Renderer>());
        }
    }

    public void SetLightColor(TrafficLightSetState state, Material mat)
    {
        currentState = state;
        if(currentState==TrafficLightSetState.Red){
            event_state = 3;
        }
        else if(currentState==TrafficLightSetState.Yellow){
            event_state = 5;
        }
        else if(currentState==TrafficLightSetState.Green){
            event_state=7;
        }

        if (stopline != null)
            stopline.currentState = state;
        foreach (var item in lightRenderers)
        {
            item.material = mat;
        }
    }
    //HW_start
    public void SetMinTime(float time){
        min_endtime = (uint)time;
    }
    //HW_end

    /*public V2x.single_spat_msg intersection_Light_Component(){//HW_start
        var msg = new V2x.single_spat_msg();
        
        
    }//HW_end*/

}
