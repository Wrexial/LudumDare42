using MEC;

public class TimingHelpers
{
    public static void CleanlyKillCoroutine(ref CoroutineHandle? coroutine)
    {
        if(coroutine.HasValue)
        {
            Timing.KillCoroutines(coroutine.Value);
            coroutine = null;
        }
    }
}