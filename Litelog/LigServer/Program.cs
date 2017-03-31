
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
using System.Windows.Forms;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Linq;
using System.Text;

using L.vivitue;
using L.vivitue.Common;
using L.vivitue.Common.WCF;
namespace Lig.vivitue.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\r\n-------------------------------------------------------------");
            Console.WriteLine(SysInfo.CurrentTime + "LigServer is starting.........");
            int beginTime = SysInfo.TickCount;
            LServiceHost.ConfigPath = RunTime.DicLFiles[FileID.LServer];
            using (LServiceHost serviceHost = new LServiceHost(typeof(Lig.vivitue.Contract.Services.LigAgent)))
            {

                serviceHost.Opened += (sender, e) =>
                {
                    Console.WriteLine(SysInfo.CurrentTime + "LigServer resources are opening.........");

                    foreach (ChannelDispatcherBase channelDiapacher in serviceHost.ChannelDispatchers)
                    {
                        Console.WriteLine(SysInfo.CurrentTime + "LigServer Listen url:" + channelDiapacher.Listener.Uri.ToString());
                    }
                };

                serviceHost.Open();
                Console.WriteLine(SysInfo.CurrentTime + "LigServer resources are opened! LigServer has been started!");

                int endTime = SysInfo.TickCount - beginTime;

                Console.WriteLine("\r\n-------------------------------------------------------------");
                Console.WriteLine(SysInfo.CurrentTime + "Attention : AlarmServer is running.........");
                Console.WriteLine(SysInfo.CurrentTime + "TotalElapsedTime : " + endTime.ToString() + "ms");
                Console.WriteLine("-------------------------------------------------------------");
                Application.Run();
            }
        }
    }
}
