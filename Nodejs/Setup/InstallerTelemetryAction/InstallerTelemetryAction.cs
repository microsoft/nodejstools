//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using System.Diagnostics;
using System.Net;
using System.Collections.Specialized;
using System.Web.Script.Serialization;
using System.Globalization;

namespace Microsoft.NodejsTools
{
    public class InstallerTelemetryActions
    {
        private static string AIVersion = "2";
        [CustomAction]
        public static ActionResult RecordInstallStartTime(Session session)
        {
            session["InstallStartTime"] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult LogInstallSuccessResult(Session session)
        {
            TimeSpan installTime = DateTime.Now - DateTime.Parse(session["InstallStartTime"]);
            session.Log("Begin Telemetry Log");
            bool isInstalled = session.EvaluateCondition("Installed");
            string currentState = "";
            string requestState = "";

            FeatureInfoCollection fc = session.Features;
            foreach (FeatureInfo f in fc)
            {
                currentState = f.CurrentState.ToString();
                requestState = f.RequestState.ToString();
                // we just want the current and requested state of A feature to understand if its a new user, upgrade, reinstall or remove.
                break; 
            }

            session.Log("Starting POST");
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                {
                    "iKey", "377a3718-78a7-49df-abcc-1001317db729"
                },
                {
                    "name", "Microsoft.ApplicationInsights.Event"
                },
                {
                    "time", DateTime.Now.ToUniversalTime().ToString(CultureInfo.InvariantCulture)
                },
                {
                    "data", new Dictionary < string, object > ()
                    {
                        {
                            "baseType", "EventData"
                        },
                        {
                            "baseData", new Dictionary < string, object > ()
                            {
                                {
                                    "ver", AIVersion
                                },
                                {
                                    "name", "NtvsInstallerTelemetry"
                                },
                                {
                                    "properties", new Dictionary < string, string > ()
                                    {
                                        {
                                            "InstallStatus", "Success"
                                        },
                                        {
                                            "IsNtvsInstalled", isInstalled.ToString()
                                        },
                                        {
                                            "CurrentState", currentState
                                        },
                                        {
                                            "RequestState", requestState
                                        },
                                        {
                                            "NtvsVersion", session["NtvsVersion"]
                                        },
                                        {
                                            "VSVersion", session["VSVersion"]
                                        },
                                        {
                                            "MsiVersion", session["MsiVersion"]
                                        },
                                        {
                                            "TimeTakenInSeconds", installTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            string jsonString = (new JavaScriptSerializer()).Serialize(data);
            using (WebClient client = new WebClient())
            {
                string response = client.UploadString("https://dc.services.visualstudio.com/v2/track", jsonString);
                session.Log(response);
            }
            session.Log("End Telemetry Log");
            return ActionResult.Success;
        }

        [CustomAction] 
        public static ActionResult LogInstallErrorResult(Session session)
        {
            TimeSpan installTime = DateTime.Now - DateTime.Parse(session["InstallStartTime"]);

            session.Log("Begin Telemetry Log");
            bool isInstalled = session.EvaluateCondition("Installed");
            string currentState = "";
            string requestState = "";
            session.Log("Is Aready Installed: " + session.EvaluateCondition("Installed").ToString());

            FeatureInfoCollection fc = session.Features;
            foreach (FeatureInfo f in fc)
            {
                currentState = f.CurrentState.ToString();
                requestState = f.RequestState.ToString();
                // we just want the current and requested state of A feature to understand if its a new user, upgrade, reinstall or remove.
                break; 
            }

            session.Log("Starting POST");
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                {
                    "iKey", "377a3718-78a7-49df-abcc-1001317db729"
                },
                {
                    "name", "Microsoft.ApplicationInsights.Event"
                },
                {
                    "time", DateTime.Now.ToUniversalTime().ToString(CultureInfo.InvariantCulture)
                },
                {
                    "data", new Dictionary < string, object > ()
                    {
                        {
                            "baseType", "EventData"
                        },
                        {
                            "baseData", new Dictionary < string, object > ()
                            {
                                {
                                    "ver", AIVersion
                                },
                                {
                                    "name", "NtvsInstallerTelemetry"
                                },
                                {
                                    "properties", new Dictionary < string, string > ()
                                    {
                                        {
                                            "InstallStatus", "Error"
                                        },
                                        {
                                            "IsNtvsInstalled", isInstalled.ToString()
                                        },
                                        {
                                            "CurrentState", currentState
                                        },
                                        {
                                            "RequestState", requestState
                                        },
                                        {
                                            "NtvsVersion", session["NtvsVersion"]
                                        },
                                        {
                                            "VSVersion", session["VSVersion"]
                                        },
                                        {
                                            "MsiVersion", session["MsiVersion"]
                                        },
                                        {
                                            "TimeTakenInSeconds", installTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            string jsonString = (new JavaScriptSerializer()).Serialize(data);
            using (WebClient client = new WebClient())
            {
                string response = client.UploadString("https://dc.services.visualstudio.com/v2/track", jsonString);
                session.Log(response);
            }
            session.Log("End Telemetry Log");
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult LogInstallCancelResult(Session session)
        {
            TimeSpan installTime = DateTime.Now - DateTime.Parse(session["InstallStartTime"]);
            string AIVersion = "2";

            session.Log("Begin Telemetry Log");
            bool isInstalled = session.EvaluateCondition("Installed");
            string currentState = "";
            string requestState = "";
            FeatureInfoCollection fc = session.Features;

            foreach (FeatureInfo f in fc)
            {
                currentState = f.CurrentState.ToString();
                requestState = f.RequestState.ToString();
                // we just want the current and requested state of A feature to understand if its a new user, upgrade, reinstall or remove.
                break;
            }

            session.Log("Starting POST");
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                {
                    "iKey", "377a3718-78a7-49df-abcc-1001317db729"
                },
                {
                    "name", "Microsoft.ApplicationInsights.Event"
                },
                {
                    "time", DateTime.Now.ToUniversalTime().ToString(CultureInfo.InvariantCulture)
                },
                {
                    "data", new Dictionary < string, object > ()
                    {
                        {
                            "baseType", "EventData"
                        },
                        {
                            "baseData", new Dictionary < string, object > ()
                            {
                                {
                                    "ver", AIVersion
                                },
                                {
                                    "name", "NtvsInstallerTelemetry"
                                },
                                {
                                    "properties", new Dictionary < string, string > ()
                                    {
                                        {
                                            "InstallStatus", "Cancel"
                                        },
                                        {
                                            "IsNtvsInstalled", isInstalled.ToString()
                                        },
                                        {
                                            "CurrentState", currentState
                                        },
                                        {
                                            "RequestState", requestState
                                        },
                                        {
                                            "NtvsVersion", session["NtvsVersion"]
                                        },
                                        {
                                            "VSVersion", session["VSVersion"]
                                        },
                                        {
                                            "MsiVersion", session["MsiVersion"]
                                        },
                                        {
                                            "TimeTakenInSeconds", installTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            string jsonString = (new JavaScriptSerializer()).Serialize(data);

            using (WebClient client = new WebClient())
            {
                string response = client.UploadString("https://dc.services.visualstudio.com/v2/track", jsonString);
                session.Log(response);
            }
            session.Log("End Telemetry Log");
            return ActionResult.Success;
        }
    }
}