namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            int ungroupedCount = 0;

            // Collect all groups first
            List<Group> allGroups = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Group))
                .Cast<Group>()
                .ToList();

            int totalCount = allGroups.Count;
            if (totalCount == 0)
            {
                TaskDialog.Show("Group Cleanup", "No groups found.");
                return Result.Succeeded;
            }

            ProgressBarHelper progressBar = new ProgressBarHelper();

            try
            {
                progressBar.ShowProgress(totalCount);

                using (Transaction trans = new Transaction(doc, "Ungroup All Groups"))
                {
                    trans.Start();

                    int current = 0;
                    foreach (Group group in allGroups)
                    {
                        if (progressBar.IsCancelled())
                        {
                            progressBar.UpdateProgress(current, "Operation cancelled by user.");
                            trans.RollBack();
                            return Result.Cancelled;
                        }

                        group.UngroupMembers();
                        ungroupedCount++;
                        current++;

                        progressBar.UpdateProgress(current, $"Ungrouping {current} of {totalCount} groups");
                    }

                    trans.Commit();
                }

                progressBar.UpdateProgress(totalCount, "Ungrouping complete!");
                System.Threading.Thread.Sleep(1000);
                progressBar.CloseProgress();

                TaskDialog.Show("Group Cleanup", $"{ungroupedCount} groups ungrouped.");
            }
            catch (Exception ex)
            {
                progressBar.CloseProgress();
                TaskDialog.Show("Error", ex.Message);
            }

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCommand2";
            string buttonTitle = "Ungroup All Groups";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Ungroup32x32,
                Properties.Resources.Ungroup16x16,
                "Ungroup all placed model and detail groups in the project.");

            return myButtonData.Data;
        }
    }
}
