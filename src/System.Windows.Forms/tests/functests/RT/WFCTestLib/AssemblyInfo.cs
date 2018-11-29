using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
#pragma warning disable 1699
// HACK: We need this stupid hack because VS looks for the SNK file relative to the output directory,
//       whereas when built from the command-line it looks in the library root directory.  We set the
//       BUILD_IN_VS flag in the VS project file so it's only defined when building in VS.
#if BUILD_IN_VS
[assembly: AssemblyKeyFile(@"..\..\WinFormsTestLibs.snk"),
		   AllowPartiallyTrustedCallers()]
#else
//[assembly: AssemblyKeyFile("WinFormsTestLibs.snk"),
//           AllowPartiallyTrustedCallers()]
#endif
//[assembly: SecurityRules(SecurityRuleSet.Level1)]
#pragma warning restore 1699