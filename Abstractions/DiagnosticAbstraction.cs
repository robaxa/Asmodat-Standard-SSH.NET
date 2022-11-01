// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Abstractions.DiagnosticAbstraction
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System.Diagnostics;
using System.Threading;

namespace Renci.SshNet.Abstractions
{
  internal static class DiagnosticAbstraction
  {
    private static readonly SourceSwitch SourceSwitch = new SourceSwitch("SshNetSwitch");
    private static readonly TraceSource Loggging = new TraceSource("SshNet.Logging", SourceLevels.All);

    public static bool IsEnabled(TraceEventType traceEventType) => DiagnosticAbstraction.SourceSwitch.ShouldTrace(traceEventType);

    [Conditional("DEBUG")]
    public static void Log(string text) => DiagnosticAbstraction.Loggging.TraceEvent(TraceEventType.Verbose, Thread.CurrentThread.ManagedThreadId, text);
  }
}
