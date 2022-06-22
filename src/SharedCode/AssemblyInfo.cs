using System.Resources;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

[assembly: ComVisible(false)]
[assembly: CompilationRelaxations(CompilationRelaxations.NoStringInterning)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: NeutralResourcesLanguage("en-GB")]

[assembly: AssemblyTitle("Custom Illusion Game Launcher")]
[assembly: AssemblyDescription("Custom launcher used as a more powerful alternative to the original launcher made by Illusion")]
[assembly: AssemblyCompany("https://github.com/IllusionMods/IllusionLaunchers")]
[assembly: AssemblyProduct("IllusionLaunchers")]
[assembly: AssemblyCopyright("GPL-3.0")]
[assembly: AssemblyTrademark("")]

[assembly: AssemblyVersion("3.3.0"
#if DEBUG
    + ".*")]
#else
    )]
#endif