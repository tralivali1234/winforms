using System.Reflection;

namespace WFCTestLib.Logging
{
    public interface IMethodCallMessage : IMessage, IMethodMessage
    {
        new MethodInfo MethodBase { get; set; }
        object Args { get; }
        object LogicalCallContext { get; }
    }
}