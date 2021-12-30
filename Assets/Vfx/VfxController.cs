using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VfxController : MonoBehaviour {

    public VisualEffect vfx;
    public float timer;

    private void Start() {
        timer = Time.time;
    }

    private void Update() {
        if (vfx.aliveParticleCount == 0 && Time.time - timer > 5)
            Destroy(gameObject);
    }
}
