using TMPro;
using UnityEngine;

[System.Serializable] //lets unity show it in the inspector as group inside the Lines list
public class npcLine //doing this was impossible with just a dictionary ugh
{
    [TextArea]
    public string text;
    public intEvent onSaid; // raised with the npc as the value when this line is said. leave empty for none
}

public class npc : MonoBehaviour
{
    [Header("NPC Settings")]
    [SerializeField]
    string npcName = "";
    [SerializeField]
    npcLine[] lines; //one of these is picked at random each time

    [Header("Dialogue UI")]
    [SerializeField]
    GameObject dialoguePanel; //the panel on this npc's own canvas. hidden when it isn't talking
    [SerializeField]
    TMP_Text dialogueText; //where the line goes
    [SerializeField]
    TMP_Text nameText; //optional. if it's empty, the name goes in front of the line instead
    [SerializeField]
    float showSeconds = 3f; //how long a line stays up
    float timer = 0f;

    void Start()
    {

        dialoguePanel.SetActive(false);

        nameText.text = npcName;

    }

    // NpcInteracted 
    public void onNpcInteracted(npcInteraction interaction)
    {
        if (interaction.target != this || lines.Length == 0)
        {
            return; //meant for a different npc or there's nothing to say

        }

        // pick a random line.
        int index = Random.Range(0, lines.Length);

        npcLine line = lines[index];

        // this line has something to trigger, so let whatever's listening know
        if (line.onSaid != null)
        {
            line.onSaid.Invoke(1);
        }


        Debug.Log($"NPC Said: {npcName}: {line}");


        // with a separate name box the line goes on its own, otherwise the name goes in front of it
        dialogueText.text = line.text;


        dialoguePanel.SetActive(true);


        timer = showSeconds; //a new line restarts the timer, so it always gets its full time on screen
    }



    void Update()
    {
        if (timer <= 0f)
        {
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f && dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
}