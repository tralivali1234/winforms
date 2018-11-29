using System;
using System.Diagnostics;
using System.Security.Permissions;
using System.Reflection;
using System.Runtime;
using System.Runtime.Remoting;
using WFCTestLib.Logging;

namespace WFCTestLib.Logging
{
	/// <summary>
	/// Flags which determine the verbosity of logging that an AutoLogger performs.
	/// </summary>
	[Flags]
	public enum LogLevel
	{ 
		MethodCall = 0x0001,
		MethodCallParameters = 0x0002,
		MethodCallVerbose = MethodCall|MethodCallParameters,
		MethodReturn = 0x0004,
		MethodReturnValue = 0x0008,
		MethodReturnVerbose = MethodReturn|MethodReturnValue,
		ObjectStateBeforeCall = 0x0010,
		ObjectStateAfterCall = 0x0020,
		ObjectStateVerbose = ObjectStateBeforeCall|ObjectStateAfterCall,
		Exception = 0x0040,
		ExceptionStackTrace = 0x0080,
		ExceptionVerbose = Exception | ExceptionStackTrace,
		PermissionElevation = 0x0100,
		CallingMethod = 0x0200,
		FullCallStack = 0x0400,
		Verbose = MethodCallVerbose | MethodReturnVerbose | ObjectStateVerbose | ExceptionVerbose | PermissionElevation | CallingMethod,
		VerboseWithStack = Verbose | FullCallStack,
		Simple = MethodCallVerbose | MethodReturnVerbose,
		None = 0,
		Default=Simple

	}
	/// <summary>
	/// AutoLogger provides functionality to allow all method invocations to the encapsulated
	/// object to be intercepted, and written to the specified log.
	/// </summary>
	public class AutoLogger : RealProxy
	{
		/// <summary>
		/// The default loglevel that any AutoLoggers created without an explicit LogLevel will acquire
		/// </summary>
		public static LogLevel DefaultLogLevel = LogLevel.Default;
		/// <summary>
		/// The object which is the target of all method invocations coming through this proxy.
		/// </summary>
		private object target;

		/// <summary>
		/// The log to write method call output to.
		/// </summary>
		private Log log;

		/// <summary>
		/// The type of the target object.  This is cached here for performance reasons.
		/// (.GetType()) is slow
		/// </summary>
		private Type type;

		/// <summary>
		/// The Log level for this AutoLogger.  This flag is used to determine what information
		/// is written to the log.
		/// </summary>
		private LogLevel level;

		/// <summary>
		/// Private constructor, to use externally, call <see cref="CreateLoggerProxy"/>
		/// </summary>
		/// <param name="target">The target this proxy should wrap.</param>
		/// <param name="log">The logfile output should be written to</param>
		/// <param name="level">Specifies which information should be logged</param>
		private AutoLogger(object target, Log log, LogLevel level):base(target.GetType())
		{
			this.target = target;
			this.log = log;
            //this.type = GetProxiedType();
            this.type = ProxiedType();
            this.level = level;
		}

        private new Type ProxiedType()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new proxy to the specified object (target) and returns a TransparentProxy
        /// which can be cast as the original type.  For example:
        /// <code>
        /// ToolStrip ts = (ToolStrip)AutoLogger.CreateLoggerProxy(new ToolStrip(), p.log);
        /// </code>
        /// will create a proxy bound to a new ToolStrip.  The return value can then be used
        /// as if it were the encapsulated object itself, but all calls will be intercepted.
        /// and optionally logged.
        /// 
        /// This overload uses the DefaultLogLevel
        /// </summary>
        /// <param name="target">The target this proxy should wrap.</param>
        /// <param name="log">The logfile output should be written to.</param>
        /// <returns>A transparent proxy to the target.</returns>
        public static object CreateLoggerProxy(object target, Log log)
		{ return CreateLoggerProxy(target, log, DefaultLogLevel); }

		/// <summary>
		/// Creates a new proxy to the specified object (target) and returns a TransparentProxy
		/// which can be cast as the original type.  For example:
		/// <code>
		/// ToolStrip ts = (ToolStrip)AutoLogger.CreateLoggerProxy(new ToolStrip(), p.log);
		/// </code>
		/// will create a proxy bound to a new ToolStrip.  The return value can then be used
		/// as if it were the encapsulated object itself, but all calls will be intercepted.
		/// and optionally logged
		/// </summary>
		/// <param name="target">The target this proxy should wrap.</param>
		/// <param name="log">The logfile output should be written to.</param>
		/// <param name="level">Specifies which information should be logged.</param>
		/// <returns>A transparent proxy to the target.</returns>
		public static object CreateLoggerProxy(object target, Log log,LogLevel level)
		{
			//If there is not going to be any logging done, skip the overhead
			if (level == LogLevel.None) { return target; }

			return new AutoLogger(target, log,level).TransparentProxy(); }

        private new object TransparentProxy()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Format an object for output in the logfile.
        /// [TypeName] Value
        /// If the type cannot be determined (null parameter), then the string "[Unknown] null" will be returned.
        /// When possible, use the overload that takes a type parameter so that the type of null parameters
        /// can be displayed.
        /// </summary>
        /// <param name="o">The object to format</param>
        /// <returns>The formatted object</returns>
        private string FormatObject(object o)
		{
			if (o == null)
			{ return "[Unknown] null"; }
			else
			{ return FormatObject(o, o.GetType()); }
		}

		/// <summary>
		/// Format an object for output in the logfile.
		/// [TypeName] Value
		/// </summary>
		/// <param name="o">The object to format.</param>
		/// <param name="t">The type of the object.</param>
		/// <returns>The formatted object</returns>
		private string FormatObject(object o, Type t)
		{return string.Join("", new string[] { "[", FormatType(t), "] ", (o == null) ? "null" : o.ToString() });}
		/// <summary>
		/// Formats the given type for logfile output.  (strips off unnecissary prefixes)
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		private string FormatType(Type t)
		{
			string name = t.Name;

			if (name.ToLower().StartsWith("system.windows.forms."))
			{ return name.Substring("system.windows.forms.".Length); }

			return t.Name;
		}

		/// <summary>
		/// Logs return information
		/// </summary>
		/// <param name="ret">The return information to log</param>
		private void LogReturn(IMethodReturnMessage ret)
		{
			if (0!=(level & LogLevel.MethodReturn))
			{ log.WriteLine("Returning from method: " + GetCallSignature(ret)); }

			if (0!=(level & LogLevel.MethodReturnValue))
			{ 
				MethodInfo inf = (MethodInfo)ret.MethodBase;

				if (inf.ReturnType != typeof(void))
				{ 
					log.WriteLine("\tReturned: " + FormatObject(ret.ReturnValue, inf.ReturnType));
				}
			}
		}

		/// <summary>
		/// Logs call invocation information
		/// </summary>
		/// <param name="call">The information to log</param>
		private void LogCall(IMethodMessage call)
		{
			if (0 != (level & LogLevel.MethodCall))
			{
				log.WriteLine("Calling method: " + GetCallSignature(call));
			}

			MethodInfo inf = (MethodInfo)call.MethodBase;

			if (0 != (level & LogLevel.MethodCallParameters))
			{
				if (inf.GetParameters().Length > 0)
				{
					log.WriteLine("Values: ");
					for (int i = 0; i < call.ArgCount; i++)
					{
						object arg = call.GetArg(i);

						log.WriteLine("\t"+ FormatObject(arg));
					}
				}
			}

			if (0 != (level&LogLevel.FullCallStack))
			{
				StackTrace t = new StackTrace();

				log.WriteLine("Stack trace: ");
				log.WriteLine(new StackTrace(3).ToString());
			}
			else if (0 != (level&LogLevel.CallingMethod))
			{
				//Frame 0 LogCall
				//Frame 1 Invoke
				//Frame 2 PrivateInvoke
				//Frame 3 immediate caller
				log.WriteLine("Called By: " + new StackFrame(3).ToString());
			}
		}
		/// <summary>
		/// Gets a string identifying the type of the specified object
		/// </summary>
		private static string GetArgType(object arg)
		{ return (arg == null) ? "Unknown" : arg.GetType().Name; }

		/// <summary>
		/// Logs an exception
		/// </summary>
		/// <param name="ex">the exception to log</param>
		private void LogException(Exception ex)
		{
			if (0 != (level & LogLevel.Exception))
			{ log.WriteLine("Method call excepted: " + ex.Message); }

			if (0 != (level & LogLevel.ExceptionStackTrace))
			{ 
				log.LogException(ex);
			}
		}

		/// <summary>
		/// Logs the current state of the object
		/// Right now this is just a ToString, but it could be expanded to include reflected information
		/// //TODO: LogObjectState Verbose
		/// </summary>
		private void LogObjectState()
		{ log.WriteLine("Object State: " + FormatObject(target, type)); }
		/// <summary>
		/// Returns a string with the call signature of the method called
		/// </summary>
		/// <param name="call">The call to get the signature of</param>
		/// <returns>A string "string GetCallSignature(IMethodMessage call)"</returns>
		private string GetCallSignature(IMethodMessage call)
		{
			System.Text.StringBuilder bldr = new System.Text.StringBuilder();

			bldr.Append(type.Name);
			bldr.Append(".");
			bldr.Append(call.MethodName);
			bldr.Append("(");

			int argCount = call.ArgCount;

			for (int i = 0; i < argCount; i++)
			{
				object arg = call.GetArg(i);

				bldr.Append(GetArgType(arg));
				bldr.Append(" ");
				bldr.Append(call.GetArgName(i));
				if (i < argCount - 1)
				{ bldr.Append(", "); }
			}

			bldr.Append(")");
			return bldr.ToString();
		}

		private static ReflectionPermission _MethodAccess;

		private static ReflectionPermission MethodAccess
		{
			get
			{
				if (null == _MethodAccess) 
				{ _MethodAccess = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess); }

				return _MethodAccess;
			}
		}

        public IMethodReturnMessage retMsg { get; private set; }


        /// <summary>
        /// Actually invoke the appropriate method on the target
        /// </summary>
        /// <param name="msg">the message to invoke</param>
        /// <returns>the result of the invocation</returns>
        public override IMessage Invoke(IMessage msg)
		{
			IMethodCallMessage call = (IMethodCallMessage)msg;

			LogCall(call);

			MethodInfo callMethod =(MethodInfo) call.MethodBase;

			if (0 != (level & LogLevel.ObjectStateBeforeCall))
			{ LogObjectState(); }

			if (0 != (level & LogLevel.MethodCall))
			{ log.WriteLine("*Actual Method call"); }

			try
			{
				bool bAssert = false;

				try
				{
					StackTrace t = new StackTrace();
					MethodBase method = t.GetFrame(2).GetMethod();
					if (!(callMethod.IsPrivate || callMethod.IsAssembly) && method.DeclaringType.Assembly != callMethod.DeclaringType.Assembly)
					{
						throw new ApplicationException("Permission elevation denied, calling code does not have permission to access the internal member");
					}


					//Frame 0 Invoke
					//Frame 1 PrivateInvoke
					//Frame 2 Actual calling method

					if (0 != (level & LogLevel.PermissionElevation))
					{
						log.WriteLine("--Elevating permission to allow internal method call");
						log.WriteLine("Caller and target are both in assembly: " + method.DeclaringType.Assembly.FullName);
					}

					bAssert = true;
					MethodAccess.Assert();
					//callRet = callMethod.Invoke(target, call.Args);
				}
				finally
				{
					if (bAssert)
					{ SecurityPermission.RevertAssert(); }
				}
			}
			catch (TargetInvocationException tio)
			{
				LogException(tio.InnerException);
				return new ReturnMessage(tio.InnerException, call);
			}
			catch (MethodAccessException)
			{
				log.WriteLine(callMethod.ToString());
				log.WriteLine(callMethod.Attributes.ToString());
				log.WriteLine("IsPublic: " + callMethod.IsAbstract.ToString());
				log.WriteLine("IsAssembly: " + callMethod.IsAssembly.ToString());
				log.WriteLine("IsConstructor: " + callMethod.IsConstructor.ToString());
				log.WriteLine("IsFamily: " + callMethod.IsFamily.ToString());
				log.WriteLine("IsFamilyAndAssembly: " + callMethod.IsFamilyAndAssembly.ToString());
				log.WriteLine("IsFamilyOrAssembly: " + callMethod.IsFamilyOrAssembly.ToString());
				log.WriteLine("IsFinal: " + callMethod.IsFinal.ToString());
				log.WriteLine("IsGenericMethodDefinition: " + callMethod.IsGenericMethodDefinition.ToString());
				log.WriteLine("IsHideBySig: " + callMethod.IsHideBySig.ToString());
				log.WriteLine("IsPrivate: " + callMethod.IsPrivate.ToString());
				log.WriteLine("IsPublic: " + callMethod.IsPublic.ToString());
				log.WriteLine("IsSpecialName: " + callMethod.IsSpecialName.ToString());
				log.WriteLine("IsStatic: " + callMethod.IsStatic.ToString());
				log.WriteLine("IsVirtual: " + callMethod.IsVirtual.ToString());
				throw;
			}
			if (0 != (level & LogLevel.ObjectStateBeforeCall))
			{ LogObjectState(); }

			//ReturnMessage retMsg = new ReturnMessage(callRet, call.Args, call.Args.Length, call.LogicalCallContext, call);

			LogReturn(retMsg);
			if (0 != level)
			{ log.WriteLine(); }

			return retMsg;
		}
	}
}
