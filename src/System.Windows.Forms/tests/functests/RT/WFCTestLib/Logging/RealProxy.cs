
using System;

namespace WFCTestLib.Logging
{
    public abstract class RealProxy
    {
        private Type type;

        protected RealProxy(Type type)
        {
            this.type = type;
        }

        public Type ProxiedType { get; }

        public abstract IMessage Invoke(IMessage msg);
        public virtual object TransparentProxy { get; }
    }
}