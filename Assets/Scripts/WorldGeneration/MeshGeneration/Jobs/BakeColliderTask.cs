using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BakeColliderTask : ThreadedTask {
    public Chunk chunk;
    private int instanceID;

    public BakeColliderTask(Chunk chunk) {
        this.chunk = chunk;
        this.instanceID = chunk.mesh.GetInstanceID();
    }

    public override void ThreadFunction() {
        Physics.BakeMesh(instanceID, false);
    }

    public override void OnFinish() {
        chunk.hasColliderBaked = true;
        chunk.SetCollider();
        chunk.chunkGameObject.SetActive(true);
    }

    public override int GetHashCode() {
        return chunk.GetHashCode();
    }

    public override bool Equals(object obj) {
        if (obj.GetType() != this.GetType())
            return false;
        return ((BakeColliderTask)obj).chunk.Equals(chunk);
    }
}
