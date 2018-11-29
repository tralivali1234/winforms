using System.Windows.Forms;

namespace WFCTestLib.Logging
{
    public interface IMethodReturnMessage : IMessage, IMethodMessage
    {
        object ReturnValue { get; set; }
    }
}