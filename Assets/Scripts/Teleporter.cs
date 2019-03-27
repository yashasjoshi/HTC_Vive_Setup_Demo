using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Teleporter : MonoBehaviour {

    public GameObject m_Pointer;
    public SteamVR_Action_Boolean m_TeleportAction;

    private SteamVR_Behaviour_Pose m_Pose = null;
    private bool m_HasPosition = false;
    private bool m_IsTeleporting = false;
    private float m_FadeTime = 0.5f;

    private void Awake()
    {
        m_Pose = GetComponent<SteamVR_Behaviour_Pose>();
    }

    private void Update () {
        //Pointer
        m_HasPosition = UpdatePointer();
        m_Pointer.SetActive(m_HasPosition);

        //Teleport
        if (m_TeleportAction.GetStateUp(m_Pose.inputSource))
            TryTeleport();
	}

    private void TryTeleport()
    {
        //Check for valid position, and if already teleporting
        if (!m_HasPosition || m_IsTeleporting)
            return;

        //Get CameraRig and Head Position
        Transform cameraRig = SteamVR_Render.Top().origin;    //Basically the cameraRig Transform
        Vector3 headPosition = SteamVR_Render.Top().head.position;
        //Now we know where the cameraRig is, where the head is and where our pointer is pointing. So with all the information, we have to figure 
        //out the Translation of how we are going to move the cameraRig as well as keeping the relationship between the cameraRig and the head, same.
        //If you simple place the cameraRig to where we are pointing, the cameraRig will translate to that position but the player's head will not. (Theres gonna be a little bit of offset)

        //Figure out the translation 
        Vector3 GroundPosition = new Vector3(headPosition.x, cameraRig.position.y, headPosition.z);
        Vector3 TranslationVector = m_Pointer.transform.position - GroundPosition;

        //Move
        StartCoroutine(MoveRig(cameraRig, TranslationVector));
    }

    private IEnumerator MoveRig(Transform cameraRig, Vector3 translation)
    {
        //Flagging the IsTeleporting boolean
        m_IsTeleporting = true;

        //Fade to Black
        SteamVR_Fade.Start(Color.black, m_FadeTime, true);

        //Apply Translation
        yield return new WaitForSeconds(m_FadeTime);
        cameraRig.position += translation;

        //Clear Fade
        SteamVR_Fade.Start(Color.clear, m_FadeTime, true);

        //Toggle the Flag
        m_IsTeleporting = false;
    }

    private bool UpdatePointer()
    {
        //Ray Coming from the controller
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        //If it's a hit
        if(Physics.Raycast(ray, out hit))
        {
            m_Pointer.transform.position = hit.point;
            return true;
        }

        //if not a hit

        return false;
    }
}
