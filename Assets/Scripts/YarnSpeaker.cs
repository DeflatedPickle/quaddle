using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class YarnSpeaker : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool isInteractable = true;
    [SerializeField] private string openingNode;
    [SerializeField] private float distance;

    private DialogueRunner dialogue;
    private Player player;

    void Start()
    {
        dialogue = FindObjectOfType<Yarn.Unity.DialogueRunner>();
        player = FindObjectOfType<Player>();

        dialogue.onNodeComplete.AddListener(EndConversation);
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, target.position) < distance)
        {
            if (isInteractable && !dialogue.IsDialogueRunning)
            {
                StartConversation();
            }
        }
    }

    private void StartConversation()
    {
        dialogue.StartDialogue(openingNode);
        player.canMove = false;
    }

    public void EndConversation(string str)
    {
        player.canMove = true;
        StartCoroutine(player.Squeeze(
            1f,
            0.6f,
            0.1f
        ));
        isInteractable = false;
    }
}
