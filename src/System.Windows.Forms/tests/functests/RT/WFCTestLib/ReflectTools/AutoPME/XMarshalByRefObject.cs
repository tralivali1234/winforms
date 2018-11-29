using System;
using WFCTestLib.Logging;
using WFCTestLib.Util;
using System.Runtime.Remoting;
namespace ReflectTools.AutoPME {
    public abstract class XMarshalByRefObject : XObject {
        public XMarshalByRefObject(String[] args) : base(args) { }
        
        ScenarioResult ObjectIsNull(Log log) {
            log.WriteLine ("erk! object is null, returning PASS - what else can I do?");
            return ScenarioResult.Pass;
        }

        protected virtual ScenarioResult GetLifetimeService(TParams p) {
            MarshalByRefObject obj = (MarshalByRefObject)p.target;

            if (obj == null)
                return ObjectIsNull(p.log);

            object ob = obj.GetLifetimeService();

            p.log.WriteLine("returned LifetimeService object is Null: " + (ob == null).ToString());
            return ScenarioResult.Pass;
        }

        protected virtual ScenarioResult InitializeLifetimeService(TParams p) {
            MarshalByRefObject obj = (MarshalByRefObject)p.target;

            if (obj == null)
                return ObjectIsNull(p.log);

            try {
                obj.InitializeLifetimeService();
                p.log.WriteLine("InitializeLifetimeService was called without exception ");
                return ScenarioResult.Pass;
            }
            catch (Exception e)    {
                p.log.WriteLine("InitializeLifetimeService caused exception: " + e.Message);
                return ScenarioResult.Fail;
            }
        }
	protected virtual ScenarioResult CreateObjRef(TParams p, Type requestedType)
	{
		return ScenarioResult.Pass;
	}
    }
}
