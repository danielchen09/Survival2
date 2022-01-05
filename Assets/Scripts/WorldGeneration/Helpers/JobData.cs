using Unity.Jobs;

public class JobData {
    public JobHandle handle;
    public bool noop = false;
    public delegate void FinishCallback();
    public FinishCallback finishCallback;

    public JobData(JobHandle handle, FinishCallback finishCallback) {
        this.handle = handle;
        this.finishCallback = finishCallback;
    }

    public JobData(bool noop) {
        this.noop = noop;
    }

    public void Complete() {
        handle.Complete();
        finishCallback();
    }
}