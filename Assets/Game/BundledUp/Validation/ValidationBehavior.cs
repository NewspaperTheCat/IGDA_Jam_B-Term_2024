using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidationBehavior : MonoBehaviour
{
    [SerializeField] private Sprite correctSprite, wrongSprite;
    [SerializeField] private Sprite[] rankingSprites;

    [SerializeField] private SpriteRenderer sr;
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

    public void SetVisuals(bool isSuccess) {
        // swap out for the comments when implemented
        if (isSuccess) {
            sr.color = Color.blue;
            // sr.sprite = correctSprite;
        } else {
            sr.color = Color.red;
            // sr.sprite = wrongSprite;
        }
    }

    public void UseRank(int ranking) {
        sr.sprite = rankingSprites[ranking - 1];
        transform.localScale = Vector3.one * 1.5f;
        isRanking = true;
    }
}
