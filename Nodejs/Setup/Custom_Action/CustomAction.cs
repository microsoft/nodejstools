using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using System.Diagnostics;
using System.Net;
using System.Collections.Specialized;
using System.Web.Script.Serialization;

namespace Custom_Action
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult RecordInstallStartTime(Session session)
        {
            session["InstallStartTime"] = DateTime.Now.ToString();
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
            session.Log("Is Aready Installed: "+session.EvaluateCondition("Installed").ToString());

            FeatureInfoCollection fc = session.Features;
            foreach (FeatureInfo f in fc)
              {
                 currentState = f.CurrentState.ToString();
                 requestState = f.RequestState.ToString();
                 break; // we just want the current and requested state of A feature to understand if its a new user, upgrade, reinstall or remove.
              }

            session.Log("Starting POST");
            Dictionary<string, object> data = new Dictionary<string,object>()
            {
                { "iKey", "377a3718-78a7-49df-abcc-1001317db729" },
                 { "name", "Microsoft.ApplicationInsights.Event"},
                {"time", DateTime.Now.ToUniversalTime().ToString()},
                  {"data",new Dictionary<string,object>()
                  {
                      {"baseType","EventData"},
                      {"baseData",new Dictionary<string,object>()
                      {
                          {"ver","2"},
                          {"name","NTVSInstallerTel"},
                          {"properties",new Dictionary<string,object>()
                          {
                               {"InstallStatus","Success"},
                               {"IsNTVSInstalled",isInstalled.ToString()},
                               {"CurrentState",currentState},
                               {"RequestState",requestState},
                               {"NTVSVersion",session["NTVSVersion"]},
                               {"VSVersion",session["VSVersion"]},
                               {"MSIVersion",session["MSIVersion"]},
                               {"TimeTakenInSeconds",installTime.TotalSeconds.ToString()}
                          }}
                      }}
                  }}
            };

            string jsonString = (new JavaScriptSerializer()).Serialize(data);          
            using (WebClient client = new WebClient())
            {
                string response =     client.UploadString("https://dc.services.visualstudio.com/v2/track", jsonString);
                session.Log(response);
            }
            session.Log("End Telemetry Log");
            return ActionResult.Success;
        }

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
                break; // we just want the current and requested state of A feature to understand if its a new user, upgrade, reinstall or remove.
            }

            session.Log("Starting POST");
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                { "iKey", "377a3718-78a7-49df-abcc-1001317db729" },
                 { "name", "Microsoft.ApplicationInsights.Event"},
                {"time", DateTime.Now.ToUniversalTime().ToString()},
                  {"data",new Dictionary<string,object>()
                  {
                      {"baseType","EventData"},
                      {"baseData",new Dictionary<string,object>()
                      {
                          {"ver","2"},
                          {"name","NTVSInstallerTel"},
                          {"properties",new Dictionary<string,object>()
                          {
                               {"InstallStatus","Error"},
                               {"IsNTVSInstalled",isInstalled.ToString()},
                               {"CurrentState",currentState},
                               {"RequestState",requestState},
                               {"NTVSVersion",session["NTVSVersion"]},
                               {"VSVersion",session["VSVersion"]},
                               {"MSIVersion",session["MSIVersion"]},
                               {"TimeTakenInSeconds",installTime.TotalSeconds.ToString()}
                          }}
                      }}
                  }}
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

        public static ActionResult LogInstallCancelResult(Session session)
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
                break; // we just want the current and requested state of A feature to understand if its a new user, upgrade, reinstall or remove.
            }

            session.Log("Starting POST");
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                { "iKey", "377a3718-78a7-49df-abcc-1001317db729" },
                 { "name", "Microsoft.ApplicationInsights.Event"},
                {"time", DateTime.Now.ToUniversalTime().ToString()},
                  {"data",new Dictionary<string,object>()
                  {
                      {"baseType","EventData"},
                      {"baseData",new Dictionary<string,object>()
                      {
                          {"ver","2"},
                          {"name","NTVSInstallerTel"},
                          {"properties",new Dictionary<string,object>()
                          {
                               {"InstallStatus","Cancel"},
                               {"IsNTVSInstalled",isInstalled.ToString()},
                               {"CurrentState",currentState},
                               {"RequestState",requestState},
                               {"NTVSVersion",session["NTVSVersion"]},
                               {"VSVersion",session["VSVersion"]},
                               {"MSIVersion",session["MSIVersion"]},
                               {"TimeTakenInSeconds",installTime.TotalSeconds.ToString()}
                          }}
                      }}
                  }}
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
