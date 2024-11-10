using System.Collections;
using System.Collections.Generic;
using Game.MinigameFramework.Scripts.Framework.Input;
using Game.MinigameFramework.Scripts.Tags;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrowdPlayerPawn : Pawn
{
    public int playerPawnIndex;
    [SerializeField] private Color playerColor;
    [SerializeField] private float pedestrianSpeed = 3.25f;
    [SerializeField] private float pointerSpeed = 10f;

    private Vector2 _moveInput = Vector2.zero;
    private bool _select = false;

    private PedestrianBehavior controlledPedestrian, selectedPedestrian, highlightedPedestrian;
    [SerializeField] private GameObject pointer;
    private float selectionStartTime, highlightTime;
    
    private void Update() {
        if (CrowdGameManager.inst.currentGamePhase == CrowdGameManager.gamePhase.Search) {
            controlledPedestrian.SetVelocity(_moveInput * pedestrianSpeed);
        } else if (CrowdGameManager.inst.currentGamePhase == CrowdGameManager.gamePhase.Select) {
            Vector2 delta = _moveInput * pointerSpeed * Time.deltaTime;
            pointer.transform.position += new Vector3(delta.x, delta.y, 0);
        }
    }

    private void FixedUpdate() {
        if (CrowdGameManager.inst.currentGamePhase == CrowdGameManager.gamePhase.Select) {
            Collider2D col = Physics2D.OverlapCircle(pointer.transform.position, .01f);
            if (col != null) {
                PedestrianBehavior newPedestrian = col.transform.GetComponent<PedestrianBehavior>();
                if (selectedPedestrian != newPedestrian) {
                    if (selectedPedestrian != null) selectedPedestrian.Select(false);
                    selectedPedestrian = newPedestrian;
                    selectedPedestrian.Select(true);
                }
            } else if (selectedPedestrian != null) {
                selectedPedestrian.Select(false);
                selectedPedestrian = null;
            }

            if (_select && selectedPedestrian != null && selectedPedestrian != highlightedPedestrian && !selectedPedestrian.IsHighlighted()) {
                if (highlightedPedestrian != null) highlightedPedestrian.Highlight(Color.white);
                highlightedPedestrian = selectedPedestrian;
                highlightedPedestrian.Highlight(playerColor);
                highlightTime = Time.time;
            }
        }
    }

    public void SceneChanged() {
        if (CrowdGameManager.inst.currentGamePhase == CrowdGameManager.gamePhase.Search) {
            pointer.SetActive(false);
            if (highlightedPedestrian != null) { 
                highlightedPedestrian.Highlight(Color.white);
                highlightedPedestrian = null;
            }
            if (controlledPedestrian != null) controlledPedestrian.CeasePlayer();
            controlledPedestrian = CrowdGameManager.inst.ChooseRandomPlayer(playerPawnIndex);
        } else if (CrowdGameManager.inst.currentGamePhase == CrowdGameManager.gamePhase.Select) {
            selectionStartTime = Time.time;
            pointer.SetActive(true);
            pointer.transform.position = Vector3.zero;
        } else if (CrowdGameManager.inst.currentGamePhase == CrowdGameManager.gamePhase.Score) {
            if (selectedPedestrian != null) selectedPedestrian.Select(false);
        }
    }

    // Handle input
    protected override void OnActionPressed(InputAction.CallbackContext context) {
        if (context.action.name == "Move") _moveInput = context.ReadValue<Vector2>();
        if (context.action.name == "ButtonA") _select = true;
    }

    protected override void OnActionReleased(InputAction.CallbackContext context) {
        if (context.action.name == "ButtonA") _select = false;
    }

    public bool HighlightedAny() {
        return highlightedPedestrian != null;
    }

    public bool HighlightedCorrectPedestrian() {
        return HighlightedAny() ? highlightedPedestrian.controlledByIndex == playerPawnIndex : false;
    }

    public float GetTimeToHighlight() {
        return highlightTime - selectionStartTime;
    }

    public int CompareTo(object obj)
    {
        var a = this;
        var b = obj as CrowdPlayerPawn;
     
        if (a.GetTimeToHighlight() < b.GetTimeToHighlight())
            return -1;
     
        if (a.GetTimeToHighlight() > b.GetTimeToHighlight())
            return 1;

        return 0;
    }
}
