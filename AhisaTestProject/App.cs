using Autodesk.Revit.UI;

namespace AhisaTestProject
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            // 1. Set up ribbon tab and panel names
            string tabName = "Ahisa's Tab";
            string panelName = "Project Cleaner";

            // 1. Create ribbon tab
            try { app.CreateRibbonTab(tabName); }
            catch { } // Ignore if the tab already exists

            // 2. Create ribbon panel 
            RibbonPanel panel = Common.Utils.CreateRibbonPanel(app, tabName, panelName);



            // 3. Create button data instances
            PushButtonData btnData1 = Command1.GetButtonData(); //Delete All Non-Placed Groups
            PushButtonData btnData2 = Command2.GetButtonData(); //Ungroup All Groups
            PushButtonData btnData2B = Command2B.GetButtonData(); //Ungroup and Delete All Groups
            PushButtonData btnData3 = Command3.GetButtonData(); //Find and delete all unused scope boxes in the project
            PushButtonData btnData3B = Command3B.GetButtonData(); //Find and delete all scope boxes in the project
            PushButtonData btnData4 = Command4.GetButtonData(); //Delete imported DWGs - DOESNT WORK YET
            PushButtonData btnData5 = Command5.GetButtonData(); //Delete Unplaced Legends
            PushButtonData btnData5B = Command5B.GetButtonData(); //Delete All Legends
            PushButtonData btnData6 = Command6.GetButtonData(); //Delete Unused Filters
            PushButtonData btnData7 = Command7.GetButtonData(); //Delete UnusedLineTypes

            // Create the pull-down button GROUPS
            PulldownButtonData pullDownDataGroup = new PulldownButtonData("Group Cleanup Actions", "Group \nCleanup");
            //pullDownDataGroup.Image = btnData1.Image; // Set initial image (optional)
            //pullDownDataGroup.LargeImage = btnData1.LargeImage;

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("AhisaTestProject.Resources.GroupActions16x16.png")) // use actual resource path
            {
                byte[] imageData = new byte[stream.Length];
                stream.Read(imageData, 0, imageData.Length);

                BitmapImage image = ConvertToImageSource(imageData);
                pullDownDataGroup.Image = image;
                pullDownDataGroup.LargeImage = image;
            }

            PulldownButton pullDownButtonGroups = panel.AddItem(pullDownDataGroup) as PulldownButton;

            pullDownButtonGroups.AddPushButton(btnData2);
            pullDownButtonGroups.AddPushButton(btnData1);
            pullDownButtonGroups.AddPushButton(btnData2B);


            // Create the pull-down button SCOPE BOXES
            PulldownButtonData pullDownDataSBoxes = new PulldownButtonData("Scope Boxes cleanup actions", "Scope Box \nCleanup");
            //pullDownDataSBoxes.Image = btnData3.Image; // Set initial image (optional)
            //pullDownDataSBoxes.LargeImage = btnData3.LargeImage;

            Assembly assembly2 = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("AhisaTestProject.Resources.ScopeBoxActions16x16.png")) // use actual resource path
            {
                byte[] imageData = new byte[stream.Length];
                stream.Read(imageData, 0, imageData.Length);

                BitmapImage image = ConvertToImageSource(imageData);
                pullDownDataSBoxes.Image = image;
                pullDownDataSBoxes.LargeImage = image;
            }

            PulldownButton pullDownButtonSBoxes = panel.AddItem(pullDownDataSBoxes) as PulldownButton;

            pullDownButtonSBoxes.AddPushButton(btnData3);
            pullDownButtonSBoxes.AddPushButton(btnData3B);


            // Create the pull-down button LEGENDS
            PulldownButtonData pullDownDataLegends = new PulldownButtonData("Legends cleanup actions", "Legend \nCleanup");
            pullDownDataLegends.Image = btnData5.Image; // Set initial image (optional)
            pullDownDataLegends.LargeImage = btnData5.LargeImage;

            PulldownButton pullDownButtonLegends = panel.AddItem(pullDownDataLegends) as PulldownButton;
            Assembly assembly3 = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("AhisaTestProject.Resources.LegendActions16x16.png")) // use actual resource path
            {
                byte[] imageData = new byte[stream.Length];
                stream.Read(imageData, 0, imageData.Length);

                BitmapImage image = ConvertToImageSource(imageData);
                pullDownButtonLegends.Image = image;
                pullDownButtonLegends.LargeImage = image;
            }



            pullDownButtonLegends.AddPushButton(btnData5);
            pullDownButtonLegends.AddPushButton(btnData5B);

            PushButton myButton6 = panel.AddItem(btnData6) as PushButton;
            PushButton myButton7 = panel.AddItem(btnData7) as PushButton;

            //pullDownButton.AddSeparator(); // <-- Adds a separator line
            //pullDownButton.AddPushButton(btnData1);
            //


            //PushButton myButton4 = panel.AddItem(btnData4) as PushButton;


            //panel.AddStackedItems(btnData1, btnData2);





            //PushButtonData btnData3 = cmdChallenge03.GetButtonData();
            //PushButtonData btnData4 = cmdChallenge04.GetButtonData();

            ////3b. Create button data
            //PushButtonData btnData1 = new PushButtonData(
            //    "Exlode and Delete Groups",
            //    "Explode and then delete all groups in the project.",
            //    Assembly.GetExecutingAssembly().Location,
            //    "RevitAddinBootcamp.Command1");

            //// 4. Create buttons
            //PushButton myButton1 = panel.AddItem(btnData1) as PushButton;
            //PushButton myButton2 = panel.AddItem(btnData2) as PushButton;

            //PushButton myButton4 = panel.AddItem(btnData4) as PushButton;

            //// 4. Add Tooltips
            //btnData1.ToolTip = "Explode and delete all groups in the project";
            ////buttonData2.ToolTip = "This is Command 2";

            return Result.Succeeded;
        }

        private RibbonPanel CreateGetPanel(UIControlledApplication app, string tabName, string panelName1)
        {
            //look for panel in tab
            foreach (RibbonPanel panel in app.GetRibbonPanels(tabName))
            {
                if (panel.Name == panelName1)
                {
                    return panel;
                }

            }
            // panel not found, create it
            RibbonPanel returnPanel = app.CreateRibbonPanel(tabName, panelName1);
            //return returnPanel;

            return app.CreateRibbonPanel(tabName, panelName1);

        }
        public BitmapImage ConvertToImageSource(byte[] imageData)
        {
            using (MemoryStream mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                BitmapImage bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.StreamSource = mem;
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.EndInit();

                return bmi;
            }
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }

}
