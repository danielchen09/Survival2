using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour {
    public float ttl;
    public Color32 critColor;

    private void Start() {
        Destroy(gameObject, ttl);
    }

    public void Init(float damage, bool isCrit) {
        TMP_Text text = GetComponent<TMP_Text>();
        text.SetText(damage.ToString());
        if (isCrit)
            text.color = critColor;
    }
}
