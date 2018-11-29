using System;
using System.Reflection;
using System.Runtime.Remoting;

namespace WFCTestLib.Logging
{
    public class ReturnMessage:IMessage, IMethodMessage, IMethodReturnMessage
    {
        private object callRet;
        private object args;
        private object length;
        private object logicalCallContext;
        private IMethodCallMessage call;

        public virtual object ReturenValue { get; }
        public object[] Args { get; }
        public object[] OutArgs { get; }
        public object[] OutArgCount { get; }
        public int ArgCount { get; }
        //public object GetArg(int argNum);
        //public object GetOutArg(int argNum);
        //public string GetArgName(int index);
        //public string GetOutArgName(int index);

        bool IMethodMessage.GetArgName(int i)
        {
            throw new NotImplementedException();
        }

        public object GetArg(int i)
        {
            throw new NotImplementedException();
        }

        public string MethodName { get; }
        public LogicalCallContext LogicalCallContext { get;}
        public MethodBase MethodBase { get; }
        bool IMethodMessage.MethodName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        int IMethodMessage.ArgCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        MethodInfo IMethodMessage.MethodBase { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public object ReturnValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        //public ReturnMessage(object callRet, object args, object length, Exception e, IMethodCallMessage methodCallMessage);
        //public ReturnMessage(object ret, object[] outArgs, int outArgsCount, LogicalCallContext callContext, IMethodCallMessage methodCallMessage);

        public ReturnMessage(object callRet, object args, object length, object logicalCallContext, IMethodCallMessage call)
        {
            this.callRet = callRet;
            this.args = args;
            this.length = length;
            this.logicalCallContext = logicalCallContext;
            this.call = call;
        }

        public ReturnMessage(Exception innerException, IMethodCallMessage call)
        {
            this.call = call;
        }
    }
}