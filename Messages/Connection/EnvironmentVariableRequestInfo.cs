// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.EnvironmentVariableRequestInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Connection
{
  internal class EnvironmentVariableRequestInfo : RequestInfo
  {
    private byte[] _variableName;
    private byte[] _variableValue;
    public const string Name = "env";

    public override string RequestName => "env";

    public string VariableName => SshData.Utf8.GetString(this._variableName, 0, this._variableName.Length);

    public string VariableValue => SshData.Utf8.GetString(this._variableValue, 0, this._variableValue.Length);

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._variableName.Length + 4 + this._variableValue.Length;

    public EnvironmentVariableRequestInfo() => this.WantReply = true;

    public EnvironmentVariableRequestInfo(string variableName, string variableValue)
      : this()
    {
      this._variableName = SshData.Utf8.GetBytes(variableName);
      this._variableValue = SshData.Utf8.GetBytes(variableValue);
    }

    protected override void LoadData()
    {
      base.LoadData();
      this._variableName = this.ReadBinary();
      this._variableValue = this.ReadBinary();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._variableName);
      this.WriteBinaryString(this._variableValue);
    }
  }
}
