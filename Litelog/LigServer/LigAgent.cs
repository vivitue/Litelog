﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行库版本:2.0.50727.3053
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Lig.vivitue.Constract.Data
{
    using System.Runtime.Serialization;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LigLevel", Namespace="http://schemas.datacontract.org/2004/07/Lig.vivitue.Constract.Data")]
    public enum LigLevel : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Info = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Debug = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Warn = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Error = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Fatal = 4,
    }
}


[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(ConfigurationName="ILigAgent")]
public interface ILigAgent
{
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILigAgent/LigMessage", ReplyAction="http://tempuri.org/ILigAgent/LigMessageResponse")]
    void LigMessage(string message, Lig.vivitue.Constract.Data.LigLevel level);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILigAgent/LigInfo", ReplyAction="http://tempuri.org/ILigAgent/LigInfoResponse")]
    void LigInfo(string message);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILigAgent/LigDebug", ReplyAction="http://tempuri.org/ILigAgent/LigDebugResponse")]
    void LigDebug(string message);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILigAgent/LigWarn", ReplyAction="http://tempuri.org/ILigAgent/LigWarnResponse")]
    void LigWarn(string message);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILigAgent/LigError", ReplyAction="http://tempuri.org/ILigAgent/LigErrorResponse")]
    void LigError(string message);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILigAgent/LigFatal", ReplyAction="http://tempuri.org/ILigAgent/LigFatalResponse")]
    void LigFatal(string message);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILigAgent/LigFatalex", ReplyAction="http://tempuri.org/ILigAgent/LigFatalexResponse")]
    void LigFatalex(string message, System.Exception ex);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILigAgent/SayHelloToServer", ReplyAction="http://tempuri.org/ILigAgent/SayHelloToServerResponse")]
    void SayHelloToServer(string message);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILigAgent/Initialize", ReplyAction="http://tempuri.org/ILigAgent/InitializeResponse")]
    void Initialize(string ligPath);
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public interface ILigAgentChannel : ILigAgent, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public partial class LigAgentClient : System.ServiceModel.ClientBase<ILigAgent>, ILigAgent
{
    
    public LigAgentClient()
    {
    }
    
    public LigAgentClient(string endpointConfigurationName) : 
            base(endpointConfigurationName)
    {
    }
    
    public LigAgentClient(string endpointConfigurationName, string remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public LigAgentClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public LigAgentClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(binding, remoteAddress)
    {
    }
    
    public void LigMessage(string message, Lig.vivitue.Constract.Data.LigLevel level)
    {
        base.Channel.LigMessage(message, level);
    }
    
    public void LigInfo(string message)
    {
        base.Channel.LigInfo(message);
    }
    
    public void LigDebug(string message)
    {
        base.Channel.LigDebug(message);
    }
    
    public void LigWarn(string message)
    {
        base.Channel.LigWarn(message);
    }
    
    public void LigError(string message)
    {
        base.Channel.LigError(message);
    }
    
    public void LigFatal(string message)
    {
        base.Channel.LigFatal(message);
    }
    
    public void LigFatalex(string message, System.Exception ex)
    {
        base.Channel.LigFatalex(message, ex);
    }
    
    public void SayHelloToServer(string message)
    {
        base.Channel.SayHelloToServer(message);
    }
    
    public void Initialize(string ligPath)
    {
        base.Channel.Initialize(ligPath);
    }
}