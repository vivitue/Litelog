
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
using System.Threading;
using System.Windows.Forms;
using Lig.vivitue;
namespace Lig.vivitue.Client
{
    class Program
    {
        static void Main(string[] args)
        {

            ILigger ilg = new Ligger("Ligv","Ligv");
            ilg.LigInfo("Your info lig here...");
            ilg.SubscribeOnlineEvent(new EventHandler(OnLine));
            while(true)
            {
                Thread.Sleep(100);
                if (ilg.ConnectStatus) break;
            }
            int j = 0;
            for (j = 0; j < 3000; j++)
            {
                ilg.LigInfo("j = "+ j.ToString());
            }
            Console.WriteLine(TimeStamp + "Ligger test is completed! j = {0}",j);
            (ilg as Ligger).Dispose();
            ilg = null;
            Application.Run();
        }

        static void OnLine(object sender, EventArgs e)
        {
            LiggerEventArgs args = e as LiggerEventArgs;
            Console.WriteLine("\r\n--------------------------------------------");
            Console.WriteLine(TimeStamp + "Message Received from LigServer");
            Console.WriteLine(TimeStamp + "ClientID   = {0}",args.ClientID);
            Console.WriteLine(TimeStamp + "LinkStatus = {0}", args.ConnectStatus);
            Console.WriteLine("--------------------------------------------\r\n");
        }

        static string TimeStamp
        {
            get { return DateTime.Now.ToString() + " : "; }
        }
    }
}
