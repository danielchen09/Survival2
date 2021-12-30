using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public abstract class ThreadedTask {
    private bool isDone = false;
    private object handle = new object();

    public bool IsDone {
        get {
            bool tmp;
            lock (handle) {
                tmp = isDone;
            }
            return tmp;
        }
        set {
            lock (handle) {
                isDone = value;
            }
        }
    }

    public abstract void ThreadFunction();
    public void Run() {
        ThreadFunction();
        isDone = true;
    }
    public abstract void OnFinish();
}
