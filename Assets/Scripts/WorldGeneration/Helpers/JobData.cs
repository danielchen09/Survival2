using Unity.Jobs;

public class JobData<T> {
    public T job;
    public JobHandle handle;
    public bool noop = false;

    public JobData(T job, JobHandle handle) {
        this.job = job;
        this.handle = handle;
    }

    public JobData(bool noop) {
        this.noop = noop;
    }
}