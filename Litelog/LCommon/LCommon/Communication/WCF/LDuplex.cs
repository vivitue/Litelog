
#region Apache License
//
// Copyright(c)2017 vivitue

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.ServiceModel;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Security.Cryptography.X509Certificates;

namespace L.vivitue.Common.WCF
{
    public class LDuplex<T> : DuplexChannelFactory<T>
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configpath"></param>
        public LDuplex(InstanceContext callbackInstance, string configpath)
            : base(typeof(T))
        {
            this.configpath = configpath;
            base.InitializeEndpoint((string)null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binding"></param>
        /// <param name="configpath"></param>
        public LDuplex(InstanceContext callbackInstance, Binding binding, string configpath)
            : this(callbackInstance, binding, (EndpointAddress)null, configpath)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="configpath"></param>
        public LDuplex(InstanceContext callbackInstance, ServiceEndpoint endpoint, string configpath)
            : base(typeof(T))
        {
            this.configpath = configpath;
            base.InitializeEndpoint(endpoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpointCfgname"></param>
        /// <param name="configpath"></param>
        public LDuplex(InstanceContext callbackInstance, string endpointCfgname, string configpath)
            : this(callbackInstance, endpointCfgname, null, configpath)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binding"></param>
        /// <param name="epAddr"></param>
        /// <param name="configpath"></param>
        public LDuplex(InstanceContext callbackInstance, Binding binding, EndpointAddress epAddr, string configpath)
            : base(typeof(T))
        {
            this.configpath = configpath;
            base.InitializeEndpoint(binding, epAddr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binding"></param>
        /// <param name="remoteAddr"></param>
        /// <param name="configpath"></param>
        public LDuplex(InstanceContext callbackInstance, Binding binding, string remoteAddr, string configpath)
            : this(callbackInstance, binding, new EndpointAddress(remoteAddr), configpath)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callbackInstance"></param>
        /// <param name="endpointCfgname"></param>
        /// <param name="epAddr"></param>
        /// <param name="configpath"></param>
 
        public LDuplex(InstanceContext callbackInstance, string endpointCfgname, EndpointAddress epAddr, string configpath)
            : base(callbackInstance, endpointCfgname)
        {
            this.configpath = configpath;
            this.endpointCfgname = endpointCfgname;

        }
        
        #endregion

        #region OverrideMethods & InternalMethods

        /// <summary>
        /// Override CreateDescription and applyConfiguration
        /// </summary>
        /// <returns></returns>
        protected override ServiceEndpoint CreateDescription()
        {
            // Set path!!!!!!!!!!!!!!!!
            this.configpath = ConfigInfo;
            this.endpointCfgname = ServiceInfo;

            if (!CheckConfigExist(this.configpath))
            {
                return base.CreateDescription();
            }
            if (string.IsNullOrEmpty(this.endpointCfgname))
            {
                return base.CreateDescription();
            }
            lock (lockObj)
            {
                try
                {
                    // Build service endpoint info
                    ServiceEndpoint servicep = new ServiceEndpoint(ContractDescription.GetContract(typeof(T)));
                    // Set serviceEndpoint name as config
                    //servicep.Name = this.endpointCfgname;

                    // Set user config
                    ExeConfigurationFileMap execfgMap = new ExeConfigurationFileMap();
                    execfgMap.ExeConfigFilename = this.configpath;

                    // Get config infomation
                    Configuration cfg = ConfigurationManager.OpenMappedExeConfiguration(execfgMap, ConfigurationUserLevel.None);

                    // Get all service model sections info
                    ServiceModelSectionGroup servicemodelSections = ServiceModelSectionGroup.GetSectionGroup(cfg);

                    // tmp servicep.Name
                    string tmpnameforep = null;

                    // Get Endpoint element info
                    ChannelEndpointElement selectedep = GetEndpointElement(servicemodelSections,
                        servicep.Contract.ConfigurationName, out tmpnameforep);

                    // Get completed servicep info
                    bool isAddAll = GetServicepCompletedInfo(servicep, servicemodelSections, selectedep);

                    // Set servicep.Name the right info
                    if (!string.IsNullOrEmpty(tmpnameforep)) servicep.Name = tmpnameforep;

                    this.epr = servicep.Address;
                    return servicep;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error : CreateDescription exception ocurred! Apply user config failed!" + ex.ToString());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="servicep"></param>
        /// <param name="servicemodelSections"></param>
        /// <param name="eplement"></param>
        /// <returns></returns>
        private bool GetServicepCompletedInfo(ServiceEndpoint servicep,
            ServiceModelSectionGroup servicemodelSections, ChannelEndpointElement eplement)
        {
            if (eplement != null)
            {
                if (servicep.Binding == null)
                {
                    servicep.Binding = CreateBinding(eplement.Binding, servicemodelSections);
                    if (servicep.Binding != null) this.isAddBinding = true;
                }
                if (servicep.Address == null)
                {
                    servicep.Address = new EndpointAddress(eplement.Address, GetIdentityForVivitue(eplement.Identity), eplement.Headers.Headers);
                    if (servicep.Address != null) this.isAddAddr = true;
                }
                if (servicep.Behaviors.Count == 0 && eplement.BehaviorConfiguration != null)
                {
                    //
                    bool isAdd = false;
                    isAdd = AddBehaviors(eplement.BehaviorConfiguration, servicep, servicemodelSections);
                    if (isAdd) this.isAddBehaviors = true;
                }
            }
            if (this.isAddBinding && this.isAddAddr && this.isAddBehaviors) return true;
            return false;
        }

        /// <summary>
        /// Get EndpointElement info
        /// </summary>
        /// <param name="servicemodelSections"></param>
        /// <param name="contractName"></param>
        /// <param name="epName"></param>
        /// <returns></returns>
        private ChannelEndpointElement GetEndpointElement(ServiceModelSectionGroup servicemodelSections,
            string contractName, out string epName)
        {
            epName = null;
            if (servicemodelSections == null) return null;
            if (string.IsNullOrEmpty(contractName)) return null;

            ChannelEndpointElement epElement = null;
            // Element flag
            bool isExisted = false;
            foreach (ChannelEndpointElement element in servicemodelSections.Client.Endpoints)
            {
                if (element.Contract == contractName &&
                    this.endpointCfgname == element.Name)
                {
                    // Get element info
                    epElement = element;
                    // Get service name
                    epName = element.Name;
                    // Find successfully
                    isExisted = true;
                    break;
                }
            }
            if (!isExisted) return null;
            return epElement;
        }

        /// <summary>
        /// Override ApplyConfiguration  - Current doing nothing here
        /// </summary>
        /// <param name="configurationName"></param>
        protected override void ApplyConfiguration(string configurationName)
        {
            //base.ApplyConfiguration
            // nop
        }

        /// <summary>
        /// Get identity
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private EndpointIdentity GetIdentityForVivitue(IdentityElement element)
        {
            if (element == null) return null;
            EndpointIdentity itentity = null;

            //PropertyInformationCollection properties = element.ElementInformation.Properties;
            if (!string.IsNullOrEmpty(element.UserPrincipalName.Value))
            {
                return EndpointIdentity.CreateUpnIdentity(element.UserPrincipalName.Value);
            }
            else if (!string.IsNullOrEmpty(element.ServicePrincipalName.Value))
            {
                return EndpointIdentity.CreateSpnIdentity(element.ServicePrincipalName.Value);
            }
            else if (!string.IsNullOrEmpty(element.Dns.Value))
            {
                return EndpointIdentity.CreateDnsIdentity(element.Dns.Value);
            }
            else if (!string.IsNullOrEmpty(element.Rsa.Value))
            {
                return EndpointIdentity.CreateRsaIdentity(element.Rsa.Value);
            }
            else if (!string.IsNullOrEmpty(element.Certificate.EncodedValue))
            {
                X509Certificate2Collection supportCerfificates = new X509Certificate2Collection();
                supportCerfificates.Import(Convert.FromBase64String(element.Certificate.EncodedValue));
                if (supportCerfificates.Count == 0)
                {
                    throw new InvalidOperationException("Error : Unable to load certificate identity!");
                }
                X509Certificate2 primaryCertificate = supportCerfificates[0];
                supportCerfificates.RemoveAt(0);
                return EndpointIdentity.CreateX509CertificateIdentity(primaryCertificate, supportCerfificates);
            }
            return itentity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [Obsolete("Warning : Instead of using this method, use GetIdentityForVivitue!")]
        private EndpointIdentity GetIdentity(IdentityElement element)
        {
            if (element == null) return null;
            EndpointIdentity itentity = null;
            PropertyInformationCollection properties = element.ElementInformation.Properties;
            if (properties["userPrincipalName"].ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateUpnIdentity(element.UserPrincipalName.Value);
            }
            if (properties["servicePrincipalName"].ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateSpnIdentity(element.ServicePrincipalName.Value);
            }
            if (properties["dns"].ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateDnsIdentity(element.Dns.Value);
            }
            if (properties["rsa"].ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateRsaIdentity(element.Rsa.Value);
            }
            if (properties["certificate"].ValueOrigin != PropertyValueOrigin.Default)
            {
                X509Certificate2Collection supportCerfificates = new X509Certificate2Collection();
                supportCerfificates.Import(Convert.FromBase64String(element.Certificate.EncodedValue));
                if (supportCerfificates.Count == 0)
                {
                    throw new InvalidOperationException("Error : Unable to load certificate identity!");
                }
                X509Certificate2 primaryCertificate = supportCerfificates[0];
                supportCerfificates.RemoveAt(0);
                return EndpointIdentity.CreateX509CertificateIdentity(primaryCertificate, supportCerfificates);
            }
            return itentity;
        }

        /// <summary>
        /// Get binding info
        /// </summary>
        /// <param name="bindingName"></param>
        /// <param name="modelGroup"></param>
        /// <returns></returns>
        private Binding CreateBinding(string bindingName, ServiceModelSectionGroup modelGroup)
        {
            if (string.IsNullOrEmpty(bindingName)) return null;
            if (modelGroup == null) return null;

            try
            {
                BindingCollectionElement bingdingElement = modelGroup.Bindings[bindingName];
                if (bingdingElement.ConfiguredBindings.Count > 0)
                {
                    IBindingConfigurationElement element = bingdingElement.ConfiguredBindings[0];
                    Binding binding = GetBindingInfo(element);
                    if (binding != null) element.ApplyConfiguration(binding);
                    return binding;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Attention : CreateBinding failed! " + ex.ToString());
            }
            return null;
        }

        /// <summary>
        /// Add behaviors info
        /// </summary>
        /// <param name="behaviorCfg"></param>
        /// <param name="servicep"></param>
        /// <param name="modelGroup"></param>
        /// <returns></returns>
        private bool AddBehaviors(string behaviorCfg, ServiceEndpoint servicep, ServiceModelSectionGroup modelGroup)
        {
            if (string.IsNullOrEmpty(behaviorCfg)) return false;
            if (servicep == null || modelGroup == null) return false;
            try
            {
                EndpointBehaviorElement behaviorElement = modelGroup.Behaviors.EndpointBehaviors[behaviorCfg];
                for (int i = 0; i < behaviorElement.Count; i++)
                {
                    BehaviorExtensionElement behaviorEx = behaviorElement[i];
                    object extention = behaviorEx.GetType().InvokeMember("CreateBehavior",
                        BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                        null,
                        behaviorEx,
                        null);
                    if (extention == null) continue;
                    servicep.Behaviors.Add((IEndpointBehavior)extention);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Warning : Exception ocurred! Behaviors info added failed!  " + ex.ToString());
                return false;
            }
            if (servicep.Behaviors.Count == 0) return false;
            return true;
        }

        /// <summary>
        /// Get Binding info
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private Binding GetBindingInfo(IBindingConfigurationElement element)
        {
            if (element == null) return null;

            if (element is CustomBindingElement) return new CustomBinding();
            else if (element is BasicHttpBindingElement) return new BasicHttpBinding();
            else if (element is NetMsmqBindingElement) return new NetMsmqBinding();
            else if (element is NetNamedPipeBindingElement) return new NetMsmqBinding();
            else if (element is NetTcpBindingElement) return new NetTcpBinding();
            else if (element is WSDualHttpBindingElement) return new WSDualHttpBinding();
            else if (element is WSHttpBindingElement) return new WSHttpBinding();
            else if (element is WSFederationHttpBindingElement) return new WSFederationHttpBinding();
            else return null;
        }

        /// <summary>
        /// Check config file existed
        /// </summary>
        /// <param name="configpath"></param>
        /// <returns></returns>
        private bool CheckConfigExist(string configpath)
        {
            if (string.IsNullOrEmpty(configpath)) return false;
            if (!File.Exists(configpath)) return false;
            return true;
        }
        #endregion

        #region Fileds & Properties
        private object lockObj = new object();
        private string configpath = null;
        private string endpointCfgname = null;
        private bool isAddBinding = false;
        private bool isAddAddr = false;
        private bool isAddBehaviors = false;
        public bool IsAddBinding
        {
            get { return isAddBinding; }
        }
        public bool IsAddAddr
        {
            get { return isAddAddr; }
        }
        public bool IsAddBehaviors
        {
            get { return isAddBehaviors; }
        }
        private EndpointAddress epr = null;
        /// <summary>
        /// For config and service name setting
        /// </summary>
        public static string ConfigInfo { get; set; }
        public static string ServiceInfo { get; set; }
        #endregion
    }
}

