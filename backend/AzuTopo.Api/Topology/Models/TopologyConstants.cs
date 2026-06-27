namespace AzuTopo.Api.Topology.Models;

public static class TopologyNodeTypes
{
    public const string Function = "function";
    public const string ServiceBusQueue = "serviceBusQueue";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Function,
        ServiceBusQueue,
    };
}

public static class TopologyEdgeTypes
{
    public const string Trigger = "trigger";
    public const string Output = "output";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Trigger,
        Output,
    };
}

public static class ServiceBusQueueStatuses
{
    public const string Active = "Active";
    public const string Disabled = "Disabled";
    public const string SendDisabled = "SendDisabled";
    public const string ReceiveDisabled = "ReceiveDisabled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Active,
        Disabled,
        SendDisabled,
        ReceiveDisabled,
    };
}

public static class TopologyWarningCodes
{
    public const string ConnectionNotFound = "connectionNotFound";
    public const string UnsupportedBinding = "unsupportedBinding";
}
