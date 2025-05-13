namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command2B : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            int explodedDetailCount = 0;
            int explodedModelCount = 0;
            int deletedDetailCount = 0;
            int deletedModelCount = 0;

            // Collect all groups and group types up front
            var detailGroups = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_IOSDetailGroups)
                .WhereElementIsNotElementType()
                .Cast<Group>()
                .ToList();

            var modelGroups = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                .WhereElementIsNotElementType()
                .Cast<Group>()
                .ToList();

            var detailGroupTypes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_IOSDetailGroups)
                .WhereElementIsElementType()
                .ToList();

            var modelGroupTypes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                .WhereElementIsElementType()
                .ToList();

            int totalSteps = detailGroups.Count + modelGroups.Count + detailGroupTypes.Count + modelGroupTypes.Count;

            if (totalSteps == 0)
            {
                TaskDialog.Show("Group Cleanup", "No groups or group types found.");
                return Result.Succeeded;
            }

            var progressBar = new ProgressBarHelper();
            progressBar.ShowProgress(totalSteps);

            try
            {
                int current = 0;

                using (Transaction trans = new Transaction(doc, "Ungroup All Groups"))
                {
                    trans.Start();

                    foreach (Group group in detailGroups)
                    {
                        if (progressBar.IsCancelled())
                        {
                            trans.RollBack();
                            progressBar.UpdateProgress(current, "Operation cancelled.");
                            progressBar.CloseProgress();
                            return Result.Cancelled;
                        }

                        group.UngroupMembers();
                        explodedDetailCount++;
                        current++;
                        progressBar.UpdateProgress(current, $"Ungrouped {current} of {totalSteps}");
                    }

                    foreach (Group group in modelGroups)
                    {
                        if (progressBar.IsCancelled())
                        {
                            trans.RollBack();
                            progressBar.UpdateProgress(current, "Operation cancelled.");
                            progressBar.CloseProgress();
                            return Result.Cancelled;
                        }

                        group.UngroupMembers();
                        explodedModelCount++;
                        current++;
                        progressBar.UpdateProgress(current, $"Ungrouped {current} of {totalSteps}");
                    }

                    trans.Commit();
                }

                using (Transaction trans = new Transaction(doc, "Delete All Group Types"))
                {
                    trans.Start();

                    foreach (Element groupType in detailGroupTypes)
                    {
                        if (progressBar.IsCancelled())
                        {
                            trans.RollBack();
                            progressBar.UpdateProgress(current, "Operation cancelled.");
                            progressBar.CloseProgress();
                            return Result.Cancelled;
                        }

                        doc.Delete(groupType.Id);
                        deletedDetailCount++;
                        current++;
                        progressBar.UpdateProgress(current, $"Deleted {current} of {totalSteps}");
                    }

                    foreach (Element groupType in modelGroupTypes)
                    {
                        if (progressBar.IsCancelled())
                        {
                            trans.RollBack();
                            progressBar.UpdateProgress(current, "Operation cancelled.");
                            progressBar.CloseProgress();
                            return Result.Cancelled;
                        }

                        doc.Delete(groupType.Id);
                        deletedModelCount++;
                        current++;
                        progressBar.UpdateProgress(current, $"Deleted {current} of {totalSteps}");
                    }

                    trans.Commit();
                }

                progressBar.UpdateProgress(totalSteps, "Cleanup completed.");
                System.Threading.Thread.Sleep(1000);
                progressBar.CloseProgress();

                TaskDialog.Show("Group Cleanup",
                    $"{explodedDetailCount} placed detail groups ungrouped.\n" +
                    $"{explodedModelCount} placed model groups ungrouped.\n\n" +
                    $"{deletedDetailCount} unplaced detail group types deleted.\n" +
                    $"{deletedModelCount} unplaced model group types deleted.");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                progressBar.CloseProgress();
                TaskDialog.Show("Error", ex.Message);
                return Result.Failed;
            }
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCommand2B";
            string buttonTitle = "Ungroup and \nDelete All Groups";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.UngroupAndDelete32x32,
                Properties.Resources.UngroupAndDelete16x16,
                "Ungroup and then delete all detail and model groups in the project.");

            return myButtonData.Data;
        }
    }

}