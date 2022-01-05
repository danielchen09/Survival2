using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct BakeColliderJob : IJobParallelFor {
    public NativeArray<int> meshIds;

    public void Execute(int index) {
        Physics.BakeMesh(meshIds[index], false);
    }

    public void Dispose() {
        this.meshIds.Dispose();
    }
}
