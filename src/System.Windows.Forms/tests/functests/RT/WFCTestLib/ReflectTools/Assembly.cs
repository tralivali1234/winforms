using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Policy;
using System.Text;

namespace WFCTestLib.ReflectTools.AutoPME
{
    public class Assembly: ICustomAttributeProvider, IEvidenceFactory, ISerializable, _Assembly
    {
        private readonly Evidence evidence = new Evidence();

        public Evidence Evidence => throw new NotImplementedException();

        public object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public virtual Evidence GetEvidence()
        {
            return evidence;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }
    }
}
