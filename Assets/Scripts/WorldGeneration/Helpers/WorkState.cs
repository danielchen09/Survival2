public class WorkState {
    public const int
        FILL = 0,
        MESH = 1,
        BAKE = 2,
        DONE = 3;

    public int workState;

    public WorkState() {
        workState = 0;
    }

    public void Set(int val) {
        workState = val;
    }

    public void Next() {
        if (workState < 3)
            workState = workState + 1;
    }

    public void NextInLoop() {
        workState = (workState + 1) % 3;
    }

    public void Restart() {
        workState = 0;
    }

    public static bool operator ==(WorkState ws1, WorkState ws2) {
        return ws1.workState == ws2.workState;
    }

    public static bool operator !=(WorkState ws1, WorkState ws2) {
        return ws1.workState != ws2.workState;
    }


    public override bool Equals(object obj) {
        return workState == ((WorkState)obj).workState;
    }

    public override int GetHashCode() {
        return workState.GetHashCode();
    }
}