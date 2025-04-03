public enum RequestState
{
    IN_PROGRESS,
    READY,
    ERROR
}

public static class RequestStates
{
    public static string ToString(this RequestState state)
    {
        return RequestStatesDictionary[state];
    }
    private static readonly Dictionary<RequestState, string> RequestStatesDictionary = new(){
        {RequestState.IN_PROGRESS, "IN_PROGRESS"},
        {RequestState.READY, "READY"},
        {RequestState.ERROR, "ERROR"}
    };
}
