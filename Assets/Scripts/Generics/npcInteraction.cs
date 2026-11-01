using UnityEngine;

public class npcInteraction

{
    public npc target; //the npc being talked to
    public Transform interactor; //who's talking to the player, so the npc can turn to face them

    public npcInteraction(npc target, Transform interactor)
    {
        this.target = target;
        this.interactor = interactor;
    }
}
