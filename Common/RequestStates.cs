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
    public static RequestState ToEnum(this string status)
    {
        return RequestStatusDictionary[status];
    }
    private static readonly Dictionary<RequestState, string> RequestStatesDictionary = new(){
        {RequestState.IN_PROGRESS, "IN_PROGRESS"},
        {RequestState.READY, "READY"},
        {RequestState.ERROR, "ERROR"}
    };
    private static readonly Dictionary<string, RequestState> RequestStatusDictionary = new(){
        {"IN_PROGRESS", RequestState.IN_PROGRESS},
        {"READY", RequestState.READY},
        {"ERROR", RequestState.ERROR}
    };
}
