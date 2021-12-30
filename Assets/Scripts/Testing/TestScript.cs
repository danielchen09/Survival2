using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TestScript : MonoBehaviour {
    public NativeArray<int> a;

    void Start() {
    }
}

public struct TestJob : IJob {
    public NativeArray<int> a;

    public void Execute() {
        a[0] = 2;
    }
}
