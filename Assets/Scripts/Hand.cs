using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Hand : MonoBehaviour {

    public SteamVR_Action_Boolean m_GrabAction = null;

    private SteamVR_Behaviour_Pose m_Pose = null;
    private FixedJoint m_Joint = null;

    private Interactable m_CurrentInteractable = null;
    public List<Interactable> m_InteractablesInContact = new List<Interactable>();

	// Use this for initialization
	private void Awake () {
        m_Pose = GetComponent<SteamVR_Behaviour_Pose>();
        m_Joint = GetComponent<FixedJoint>();
	}
	
	// Update is called once per frame
	private void Update () {
        //Down
        if (m_GrabAction.GetStateDown(m_Pose.inputSource))
        {
            print(m_Pose.inputSource + " Trigger Down");
            PickUp();
        }

        //Up
        if (m_GrabAction.GetStateUp(m_Pose.inputSource))
        {
            print(m_Pose.inputSource + " Trigger Up");
            Drop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("interactable"))
            return;

        m_InteractablesInContact.Add(other.gameObject.GetComponent<Interactable>());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("interactable"))
            return;

        m_InteractablesInContact.Remove(other.gameObject.GetComponent<Interactable>());
    }

    public void PickUp()
    {
        //Get the nearest interactable
        m_CurrentInteractable = GetNearestInteractable();

        //null check to see if we actually have something to pick up
        if (!m_CurrentInteractable)
            return;

        //Check if already heald 
        if (m_CurrentInteractable.activeHand != null)
            m_CurrentInteractable.activeHand.Drop();

        //Position it to our controller
        m_CurrentInteractable.transform.position = transform.position;

        //Attach to the fixed joint of our hands
        Rigidbody targetBody = m_CurrentInteractable.GetComponent<Rigidbody>();
        m_Joint.connectedBody = targetBody;

        //Set Active hand
        m_CurrentInteractable.activeHand = this;
    }

    public void Drop()
    {
        //Null Check
        if (!m_CurrentInteractable)
            return;

        //Apply Velocity
        Rigidbody targetBody = m_CurrentInteractable.GetComponent<Rigidbody>();
        targetBody.velocity = m_Pose.GetVelocity();
        targetBody.angularVelocity = m_Pose.GetAngularVelocity();

        //Dettach the object from our hand
        m_Joint.connectedBody = null;

        //Clear
        m_CurrentInteractable.activeHand = null;
        m_CurrentInteractable = null;
    }

    private Interactable GetNearestInteractable()
    {
        Interactable nearest = null;
        float minDistance = float.MaxValue;
        float distance = 0.0f;

        foreach(Interactable interactable in m_InteractablesInContact)
        {
            distance = (interactable.transform.position - transform.position).sqrMagnitude;
            if(distance < minDistance)
            {
                minDistance = distance;
                nearest = interactable;
            }
        }

        return nearest;
    }
}
