using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidationBehavior : MonoBehaviour
{
    [SerializeField] private Sprite correctSprite, wrongSprite;
    [SerializeField] private Sprite[] rankingSprites;

    [SerializeField] private SpriteRenderer sr, srHightlight;
    [SerializeField] private Animator an;

    private bool isRanking = false;

    void Start() {
        an.Play("PopUp");
        if (!isRanking)
            Invoke("PopDown", CrowdGameManager.inst.GetRemainingPhaseTime() - .1f);
    }

    void PopDown() {
        an.Play("PopDown");
        Destroy(gameObject, 1.5f);
    }

    public void SetVisuals(bool isSuccess, Color playerColor) {
        srHightlight.color = playerColor;
        sr.sprite = isSuccess ? correctSprite : wrongSprite;
    }

    public void UseRank(int ranking, Color playerColor) {
        srHightlight.enabled = false;
        sr.color = playerColor;
        sr.sprite = rankingSprites[ranking - 1];
        isRanking = true;
    }
}
