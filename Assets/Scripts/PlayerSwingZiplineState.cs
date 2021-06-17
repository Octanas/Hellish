using UnityEngine;

public class PlayerSwingZiplineState : StateMachineBehaviour
{
    private FMOD.Studio.EventInstance eventInstance;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        eventInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Zipline/zipline_start");
        eventInstance.setParameterByName("Volumee", 0.07f);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(eventInstance, animator.transform, animator.gameObject.GetComponent<Rigidbody>());
        eventInstance.start();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        eventInstance.release();
    }
}
