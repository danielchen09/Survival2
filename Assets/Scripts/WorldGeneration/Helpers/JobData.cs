using Unity.Jobs;

public class JobData<T> {
    public T job;
    public JobHandle handle;

    public JobData(T job, JobHandle handle) {
        this.job = job;
        this.handle = handle;
    }
}