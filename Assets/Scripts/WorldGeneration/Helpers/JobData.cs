using Unity.Jobs;

public class JobData<T> {
    public T job;
    public JobHandle handle;
    public bool noop = false;
    public static JobData<T> NO_OP {
        get => new JobData<T>(true);
    }

    public JobData(T job, JobHandle handle) {
        this.job = job;
        this.handle = handle;
    }

    public JobData(bool noop) {
        this.noop = noop;
    }
}