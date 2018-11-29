using System.Reflection;

namespace WFCTestLib.Logging
{
    public interface IMethodMessage : IMessage
    {
        bool MethodName { get; set; }
        int ArgCount { get; set; }
        MethodInfo MethodBase { get; set; }

        object GetArg(int i);
        bool GetArgName(int i);
    }
}