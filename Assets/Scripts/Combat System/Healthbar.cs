using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour {
    public Image healthAmount;
    public float fullWidth;

    private void Awake() {
        fullWidth = healthAmount.rectTransform.sizeDelta.x;
    }

    public void SetHealth(float healthNormalized) {
        healthAmount.rectTransform.sizeDelta = new Vector2(fullWidth * healthNormalized, healthAmount.rectTransform.sizeDelta.y);
    }
}
