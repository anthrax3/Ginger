#region License
/*
Copyright © 2014-2019 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using GingerCore.Platforms;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GingerCore.Actions
{
    public class ActBrowserElement : Act, IActPluginExecution
    {
        public new static partial class Fields
        {
            public static string ControlAction = "ControlAction";
            public static string ValueUC = "ValueUC";
            public static string ElementLocateValue = "ElementLocateValue";
            public static string ElementLocateBy = "ElementLocateBy";
            public static string GotoURLType = "GotoURLType";
            public static string ImplicitWait = "ImplicitWait";
            public static string URLSrc = "URLSrc";
            public static string PomGUID = "PomGUID";
        }

        public override string ActionDescription { get { return "Browser Action"; } }
        public override string ActionUserDescription { get { return string.Empty; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Action to handle Browser and Widgets");
            TBH.AddLineBreak();
            TBH.AddText("Widget are Internet Explorer browser inside Java form, they contain their own HTML and can have many HTML controls like regular Web Page");
            TBH.AddLineBreak();
            TBH.AddText("For Java- This action enable you to set the current active browser control and then locate a specific HTML element and run action on this element");
        }

        public override string ActionEditPage { get { return "ActBrowserElementEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Java);
                    mPlatforms.Add(ePlatformType.Web);
                    mPlatforms.Add(ePlatformType.Mobile);
                    mPlatforms.Add(ePlatformType.PowerBuilder);
                    mPlatforms.Add(ePlatformType.Windows);
                }
                return mPlatforms;
            }
        }

        public enum eGotoURLType
        {
            [EnumValueDescription("Current")]
            Current,
            [EnumValueDescription("New Tab")]
            NewTab,
            [EnumValueDescription("New Window")]
            NewWindow,
        }

        public enum eURLSrc
        {
            [EnumValueDescription("Static")]
            Static,
            [EnumValueDescription("Page Objects Model URL")]
            UrlPOM,
        }


        public eGotoURLType GotoURLRadioButton
        {
            get {

                return GetOrCreateInputParam<eGotoURLType>(Fields.GotoURLType);
            }
            set
            {
                GetOrCreateInputParam(Fields.GotoURLType).Value = value.ToString();
            }
        }
        private eGotoURLType gotoURLRadioButton = eGotoURLType.Current;

        private int mImplicitWait = 60;
        [IsSerializedForLocalRepository]
        public int ImplicitWait
        {
            get { return mImplicitWait; }
            set
            {
                mImplicitWait = value;
                OnPropertyChanged(Fields.ImplicitWait);
            }
        }

        public enum eControlAction
        {
            [EnumValueDescription("Initialize Browser")]
            InitializeBrowser,
            [EnumValueDescription("Get Page Source")]
            GetPageSource,
            [EnumValueDescription("Get Page URL")]
            GetPageURL,
            [EnumValueDescription("Switch Frame")]
            SwitchFrame,
            [EnumValueDescription("Switch to Default Frame")]
            SwitchToDefaultFrame,
            [EnumValueDescription("Switch to Parent Frame")]
            SwitchToParentFrame,
            [EnumValueDescription("Maximize Window")]
            Maximize,
            [EnumValueDescription("Close Tab")]
            Close,
            [EnumValueDescription("Switch Window")]
            SwitchWindow,
            [EnumValueDescription("Switch to Default Window")]
            SwitchToDefaultWindow,
            [EnumValueDescription("Inject Java Script")]
            InjectJS,
            [EnumValueDescription("Wait till page loaded")]
            CheckPageLoaded,
            [EnumValueDescription("Open URL in New Tab")]
            OpenURLNewTab,
            [EnumValueDescription("Goto URL")]
            GotoURL,
            [EnumValueDescription("Close Tab Except")]
            CloseTabExcept,
            [EnumValueDescription("Close All")]
            CloseAll,
            [EnumValueDescription("Refresh")]
            Refresh,
            [EnumValueDescription("Navigate Back")]
            NavigateBack,
            [EnumValueDescription("Dismiss Message Box")]
            DismissMessageBox,
            [EnumValueDescription("Delete All Cookies")]
            DeleteAllCookies,
            [EnumValueDescription("Accept Message Box")]
            AcceptMessageBox,
            [EnumValueDescription("Get Window Title")]
            GetWindowTitle,
            [EnumValueDescription("Get MessageBox Text")]
            GetMessageBoxText,
            [EnumValueDescription("Set AlertBox Text")]
            SetAlertBoxText,
            [EnumValueDescription("Run Java Script")]
            RunJavaScript
        }



        public eControlAction ControlAction
        {
            get
            {
                return GetOrCreateInputParam<eControlAction>(Fields.ControlAction);
            }
            set
            {
                 AddOrUpdateInputParamValue(Fields.ControlAction, value.ToString());
     
                OnPropertyChanged(nameof(ControlAction));
            }
        }

        public override String ToString()
        {
            return "BrowserControl - " + ControlAction;
        }

        public NewPayLoad GetActionPayload()
        {
            NewPayLoad PL = new NewPayLoad("RunPlatformAction");
            PL.AddValue("BrowserAction");
            List<NewPayLoad> PLParams = new List<NewPayLoad>();

            foreach (FieldInfo FI in typeof(ActBrowserElement.Fields).GetFields())
            {
                string Name = FI.Name;
                string Value = GetOrCreateInputParam(Name).ValueForDriver;

                if (string.IsNullOrEmpty(Value))
                {
                    object Output = this.GetType().GetProperty(Name) != null ? this.GetType().GetProperty(Name).GetValue(this, null) : string.Empty;

                    if (Output != null)
                    {
                        Value = Output.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(Value))
                {
                    NewPayLoad FieldPL = new NewPayLoad("Field", Name, Value);
                    PLParams.Add(FieldPL);
                }
            }

            foreach (FieldInfo FI in typeof(Act.Fields).GetFields())
            {
                string Name = FI.Name;
                string Value = GetOrCreateInputParam(Name).ValueForDriver;

                if (string.IsNullOrEmpty(Value))
                {
                    object Output = this.GetType().GetProperty(Name) != null ? this.GetType().GetProperty(Name).GetValue(this, null) : string.Empty;

                    if (Output != null)
                    {
                        Value = Output.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(Value))
                {
                    NewPayLoad FieldPL = new NewPayLoad("Field", Name, Value);
                    PLParams.Add(FieldPL);
                }
            }


            foreach (ActInputValue AIV in this.InputValues)
            {
                if (!string.IsNullOrEmpty(AIV.ValueForDriver))
                {
                    NewPayLoad AIVPL = new NewPayLoad("AIV", AIV.Param, AIV.ValueForDriver);
                    PLParams.Add(AIVPL);
                }
            }




            PL.AddListPayLoad(PLParams);
            PL.ClosePackage();

            return PL;
        }

        public string GetName()
        {
            return "BrowserAction";
        }
        
        public override String ActionType
        {
            get
            {
                return "BrowserControl: " + ControlAction.ToString();
            }
        }

        public string PomGUID
        {
            get
            {
                return GetOrCreateInputParam(Fields.PomGUID).Value;
            }
            set
            {
                GetOrCreateInputParam(Fields.PomGUID).Value = value;
                OnPropertyChanged(nameof(PomGUID));
            }
        }



        public PlatformAction GetAsPlatformAction()
        {
            PlatformAction platformAction = new PlatformAction(platform: "Web", actionHandler: "BrowserActions", action: "GotoURL" );


            Dictionary<string, string> InputValueCalculated = new Dictionary<string, string>();

            foreach(ActInputValue aiv in this.InputValues)
            {
                if(!InputValueCalculated.ContainsKey(aiv.Param))
                InputValueCalculated.Add(aiv.Param, aiv.ValueForDriver);
            }
            platformAction.InputParams.Add("InputValues", InputValueCalculated);
            //platformAction.InputParams.Add("GotoURLType", GotoURLRadioButton);            
            //platformAction.InputParams.Add("URL", Value);
            return platformAction;            
        }


    }
}