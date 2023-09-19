namespace MetaRelay.API;

public class InterlockedStatistic
{
    private long value;
    public long Value => Interlocked.Read(ref value);

    public InterlockedStatistic(long value = 0)
    {
        this.value = value;
    }

    public void Set(long value = 0) => Interlocked.Exchange(ref this.value, value);
    public void Increment() => Interlocked.Increment(ref value);
    public void Add(long value) => Interlocked.Add(ref this.value, value);
}