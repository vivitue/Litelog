
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
using System.ServiceModel.Description;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel.Dispatcher;

namespace L.vivitue.Common.WCF
{
    /// <summary>
    /// 
    /// </summary>
    public class LServiceHost : ServiceHost
    {
        #region Constructors
        public LServiceHost(object singletonInstance, params Uri[] baseAddresses)
            : base(singletonInstance, baseAddresses)
        {

        }

        public LServiceHost(Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {

        }
        #endregion

        #region Rewrite ApplyConfiguration

        /// <summary>
        /// Override ApplyConfiguration to load custom config file
        /// Attention : ConfigPath must be static variable according to the follow rules:
        /// 1. First run ApplyConfiguration of base class of ServiceHost
        /// 2. Second run constructor of EIPServiceHost itself
        /// </summary>
        protected override void ApplyConfiguration()
        {
            // Check user config invalidation
            if (!CheckConfigExist(ConfigPath))
            {
                // Use default config
                base.ApplyConfiguration();
                return;
            }
            base.ApplyConfiguration();
            // Use user config
            ExeConfigurationFileMap execfgMap = new ExeConfigurationFileMap();
            // Set user config FilePath
            execfgMap.ExeConfigFilename = ConfigPath;
            // Config info
            Configuration cfg = ConfigurationManager.OpenMappedExeConfiguration(execfgMap,ConfigurationUserLevel.None);
            // Gets all service model config sections
            ServiceModelSectionGroup servicemodelSections = ServiceModelSectionGroup.GetSectionGroup(cfg);
            
            // Find serivce section matched with the name "this.Description.ServiceType.FullName" 
            if (!ApplySectionInfo(this.Description.ServiceType.FullName,servicemodelSections))
            {
                throw new Exception("ConfigApply Error : There is no endpoint existed in your config!! Please check your config file!");
            }
            this.ApplyMultiBehaviors(servicemodelSections);
        }

        /// <summary>
        /// Apply section info
        /// </summary>
        /// <param name="serviceFullName"></param>
        /// <param name="servicemodelSections"></param>
        /// <returns></returns>
        private bool ApplySectionInfo(string serviceFullName,ServiceModelSectionGroup servicemodelSections)
        {
            // Check config sections (!including one section at least!)
            if (servicemodelSections == null) return false;
            // Service name can't be none!
            if (string.IsNullOrEmpty(serviceFullName)) return false;
            bool isElementExist = false;
            foreach (ServiceElement element in servicemodelSections.Services.Services)
            {
                if (element.Name == serviceFullName)
                {
                    // Find successfully & apply section info of config file
                    
                    base.LoadConfigurationSection(element);
                    // Find service element successfully
                    isElementExist = true;
                    break;
                }
            } 
            return isElementExist;
        }

        /// <summary>
        /// Add behaviors
        /// </summary>
        /// <param name="servicemodelSections"></param>
        /// <returns></returns>
        private bool ApplyMultiBehaviors(ServiceModelSectionGroup servicemodelSections)
        {
            if (servicemodelSections == null) return false;
            foreach (ServiceBehaviorElement element in servicemodelSections.Behaviors.ServiceBehaviors)
            {
                foreach (BehaviorExtensionElement behavior in element)
                {
                    BehaviorExtensionElement behaviorEx = behavior;
                    object extention = behaviorEx.GetType().InvokeMember("CreateBehavior",
                        BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                        null,
                        behaviorEx,
                        null);
                    if (extention == null) continue;
                    IServiceBehavior isb = (IServiceBehavior)extention;
                    //if (base.Description.Behaviors.Contains(isb)) break;
                    bool isbehaviorExisted = false;
                    foreach (IServiceBehavior i in base.Description.Behaviors)
                    {
                        if (i.GetType().Name == isb.GetType().Name)
                        {
                            isbehaviorExisted = true;
                            break;
                        }
                    }
                    if (isbehaviorExisted) break;
                    base.Description.Behaviors.Add((IServiceBehavior)extention);
                }
            }
            return true;
        }

        /// <summary>
        /// Check config file! 
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

        #region Static Fields & properties
        private const string THROTTLINGBEHAVIOR = "throttlingBehavior";
        private static string configpath = null;

        /// <summary>
        /// Config file full path of user defined!
        /// </summary>
        public static string ConfigPath
        {
            get { return configpath; }
            set { configpath = value; }
        }
        #endregion
    }

    /// <summary>
    /// For web or IIS application
    /// </summary>
    public class LServiceHostFactory : ServiceHostFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="baseAddresses"></param>
        /// <returns></returns>
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new LServiceHost(serviceType, baseAddresses);
        }
    }
}
