/**
 * Copyright (c) 2018 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionComponent : MonoBehaviour
{
    [System.NonSerialized]
    public List<IntersectionTrafficLightSetComponent> lightGroups = new List<IntersectionTrafficLightSetComponent>();
    [System.NonSerialized]
    public List<IntersectionTrafficLightSetComponent> facingGroup = new List<IntersectionTrafficLightSetComponent>();
    [System.NonSerialized]
    public List<IntersectionTrafficLightSetComponent> oppFacingGroup = new List<IntersectionTrafficLightSetComponent>();
    [System.NonSerialized]
    private List<IntersectionTrafficLightSetComponent> currentTrafficLightSet = new List<IntersectionTrafficLightSetComponent>();
    [System.NonSerialized]//HW_Edit
    public List<IntersectionLightComponent> currentIntersectionC = new List<IntersectionLightComponent>();//HW_Edit
    [System.NonSerialized]//HW_Edit
    public List<IntersectionTrafficLightSetComponent> currentlightGroups = new List<IntersectionTrafficLightSetComponent>();//HW_Edit
    //[System.NonSerialized]
    //public V2x.single_spat_msg Spat_msg = new V2x.single_spat_msg();//HW_edit
    private bool isFacing = false;
    public uint intersection_region;//HW_Edit
    [System.NonSerialized]//HW_Edit
    public uint eventstate;//HW_Edit
    [System.NonSerialized]//HW_Edit
    public float min_endtime;//HW_Edit
    public List<uint> single_Sig_group = new List<uint>();//HW_Edit
    public List<string> single_Move_name = new List<string>();//HW_Edit
    public List<uint> single_Event_state = new List<uint>();//HW_Edit
    public List<uint> single_Min_end_time = new List<uint>();//HW_Edit
    private float m_yellowTime = 0f;
    private float m_allRedTime = 0f;
    private float m_activeTime = 0f;

    // TODO need to refactor for one material
    private Material m_yellowMat;
    private Material m_redMat;
    private Material m_greenMat;

    public SphereCollider yieldTrigger { get; set; }
    public float yieldTriggerRadius = 10f; // match to size of intersection so all stop sign queue goes in and out
    [System.NonSerialized]
    public List<Transform> npcsInIntersection = new List<Transform>();
    
    public void SetLightGroupData(float yellowTime, float allRedTime, float activeTime, Material yellow, Material red, Material green)
    {
        m_yellowTime = yellowTime;
        m_allRedTime = allRedTime;
        m_activeTime = activeTime;
        m_yellowMat = yellow;
        m_redMat = red;
        m_greenMat = green;
        isFacing = false;
        lightGroups.AddRange(transform.GetComponentsInChildren<IntersectionTrafficLightSetComponent>());

        foreach (var item in lightGroups)
        {
            foreach (var group in lightGroups)
            {
                float dot = Vector3.Dot(group.transform.TransformDirection(Vector3.forward), item.transform.TransformDirection(Vector3.forward)); // TODO not vector right usually

                if (dot < -0.7f) // facing
                {
                    if (!facingGroup.Contains(item) && !oppFacingGroup.Contains(item))
                        facingGroup.Add(item);
                    if (!facingGroup.Contains(group) && !oppFacingGroup.Contains(group))
                        facingGroup.Add(group);
                }
                else if (dot > -0.5f && dot < 0.5f) // perpendicular
                {
                    if (!facingGroup.Contains(item) && !oppFacingGroup.Contains(item))
                        facingGroup.Add(item);
                    if (!oppFacingGroup.Contains(group) && !facingGroup.Contains(group))
                        oppFacingGroup.Add(group);
                }
                else if (lightGroups.Count == 1) // same direction
                {
                    if (!facingGroup.Contains(item))
                        facingGroup.Add(item);
                }
            }
        }
        if (lightGroups.Count != facingGroup.Count + oppFacingGroup.Count)
            Debug.LogError("Error finding facing light sets, please check light set parent rotation");

        // trigger
        yieldTrigger = null;
        List<SphereCollider> oldTriggers = new List<SphereCollider>();
        oldTriggers.AddRange(GetComponents<SphereCollider>());
        for (int i = 0; i < oldTriggers.Count; i++)
            Destroy(oldTriggers[i]);

        yieldTrigger = this.gameObject.AddComponent<SphereCollider>();
        yieldTrigger.isTrigger = true;
        yieldTrigger.radius = yieldTriggerRadius;
    }

    public void StartTrafficLightLoop()
    {
        if (m_greenMat == null || m_yellowMat == null || m_redMat == null)
        {
            Debug.Log("Please add traffic light materials to MapManager");
        }
        StartCoroutine(TrafficLightLoop());
    }

    private IEnumerator TrafficLightLoop()
    {
        //Spat_msg.single_id_region = intersection_region;//HW_Edit
        yield return new WaitForSeconds(Random.Range(0, 5f));
        while (true)
        {
            yield return null;

            currentTrafficLightSet = isFacing ? facingGroup : oppFacingGroup;

            foreach (var state in currentTrafficLightSet)
            {
                state.SetLightColor(TrafficLightSetState.Green, m_greenMat);
                state.SetMinTime(m_activeTime);
                //lightGroups.event_state = 7;//HW_Edit
                //min_endtime = m_activeTime;//HW_Edit
            }
            //lightGroups.min_endtime = (uint)m_activeTime;//HW_Edit

            yield return new WaitForSeconds(m_activeTime);

            foreach (var state in currentTrafficLightSet)
            {
                state.SetLightColor(TrafficLightSetState.Yellow, m_yellowMat);
                state.SetMinTime(m_yellowTime);
                //lightGroups.event_state = 5;//HW_Edit
                //min_endtime = m_yellowTime;//HW_Edit
            }
            //lightGroups.min_endtime = (uint)m_yellowTime;//HW_Edit

            yield return new WaitForSeconds(m_yellowTime);

            foreach (var state in currentTrafficLightSet)
            {
                state.SetLightColor(TrafficLightSetState.Red, m_redMat);
                state.SetMinTime(m_allRedTime);
                //lightGroups.event_state = 3;//HW_Edit
                //min_endtime = m_allRedTime;//HW_Edit
            }
            //lightGroups.min_endtime = (uint)m_allRedTime;//HW_Edit

            yield return new WaitForSeconds(m_allRedTime);

            isFacing = !isFacing;
        }
    }

    private void Update()
    {
        for (int i = 0; i < npcsInIntersection.Count; i++)
        {
            if (Vector3.Distance(npcsInIntersection[i].position, transform.position) > yieldTrigger.radius * 2f)
            {
                if (npcsInIntersection.Contains(npcsInIntersection[i]))
                    npcsInIntersection.Remove(npcsInIntersection[i]);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        npcsInIntersection.Add(other.transform);
        NPCControllerComponent npcControllerComponent = other.GetComponent<NPCControllerComponent>();
        if (npcControllerComponent != null && npcControllerComponent.currentIntersectionComponent == null)
            npcControllerComponent.currentIntersectionComponent = this;
    }

    private void OnTriggerExit(Collider other)
    {
        npcsInIntersection.Remove(other.transform);
        NPCControllerComponent npcControllerComponent = other.GetComponent<NPCControllerComponent>();
        if (npcControllerComponent != null)
        {
            npcControllerComponent.RemoveFromStopSignQueue();
            npcControllerComponent.currentIntersectionComponent = null;
        }
    }
    //HW_start
    public V2x.single_spat_msg Traffic_Spat_intersection_info()
    { 
        var msg = new V2x.single_spat_msg();
        msg.single_id_region = intersection_region; 
        single_Sig_group.Clear();
        single_Move_name.Clear();
        single_Event_state.Clear();
        single_Min_end_time.Clear();
        //currentlightGroups.AddRange(transform.GetComponentsInChildren<IntersectionTrafficLightSetComponent>());
        foreach(var item1 in lightGroups)
        {
            //currentIntersectionC.AddRange(transform.GetComponentsInChildren<IntersectionLightComponent>());
            //currentIntersectionC.Lenght;
            //single_Event_state.Add(item1.event_state);
            //single_Min_end_time.Add(item1.min_endtime);
            foreach(var item2 in item1.intersectionLightC)
            {
                single_Sig_group.Add(item2.signal_group);
                single_Move_name.Add(item2.movement_name);
            }
        }
        foreach(var item in currentTrafficLightSet)
        {
            single_Event_state.Add(item.event_state);
            single_Min_end_time.Add(item.min_endtime);
        }
        msg.movement_count = (uint)single_Move_name.Count;
        msg.single_signal_group = single_Sig_group;
        msg.single_movement_name = single_Move_name;
        msg.single_eventstate = single_Event_state;
        msg.single_minendtime = single_Min_end_time;
        return msg;
    }//HW_end

    //public bool IsOnComing(Transform checkNPC)
    //{
    //    bool isOnComing = false;

    //    foreach (var npc in npcsInIntersection)
    //    {
    //        if (npc == checkNPC)
    //            continue;

    //        if (Vector3.Dot(checkNPC.TransformDirection(Vector3.forward), (npc.position - checkNPC.position).normalized) > 0.7f )
    //        {
    //            if (!npc.GetComponent<NPCControllerComponent>().isLeftTurn)
    //            {
    //                isOnComing = true;
    //                break;
    //            }
    //        }
    //    }

    //    return isOnComing;
    //}
}
