using UnityEngine;

public class playerAnimationEvents : MonoBehaviour
{
    //animation events only happen on the same gameobject as the animator component.
    //this sends them up, useless ik but only way around I can find.

    [SerializeField]
    private playerInteractionController controller;


    public void onInteractStart()
    {
        controller.onInteractionAnimation(true);
    }

    public void onInteractMidPoint()
    {
        controller.onInteractionAnimationMidPoint();
    }

    public void onInteractEnd()
    {
        controller.onInteractionAnimation(false);
    }

}
