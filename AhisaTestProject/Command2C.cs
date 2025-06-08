namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command2C : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var modelGroups = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                .WhereElementIsNotElementType()
                .Cast<Group>()
                .ToList();

            var modelGroupTypes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                .WhereElementIsElementType()
                .ToList();

            int explodedCount = 0;
            int deletedCount = 0;
            int totalSteps = modelGroups.Count + modelGroupTypes.Count;

            if (totalSteps == 0)
            {
                TaskDialog.Show("Model Group Cleanup", "No model groups or group types found.");
                return Result.Succeeded;
            }

            var progressBar = new ProgressBarHelper();
            progressBar.ShowProgress(totalSteps);
            int current = 0;

            try
            {
                using (Transaction trans = new Transaction(doc, "Ungroup Model Groups"))
                {
                    trans.Start();
                    foreach (Group group in modelGroups)
                    {
                        if (progressBar.IsCancelled())
                        {
                            trans.RollBack();
                            progressBar.CloseProgress();
                            return Result.Cancelled;
                        }

                        group.UngroupMembers();
                        explodedCount++;
                        progressBar.UpdateProgress(++current, $"Ungrouped {current} of {totalSteps}");
                    }
                    trans.Commit();
                }

                using (Transaction trans = new Transaction(doc, "Delete Model Group Types"))
                {
                    trans.Start();
                    foreach (Element groupType in modelGroupTypes)
                    {
                        if (progressBar.IsCancelled())
                        {
                            trans.RollBack();
                            progressBar.CloseProgress();
                            return Result.Cancelled;
                        }

                        doc.Delete(groupType.Id);
                        deletedCount++;
                        progressBar.UpdateProgress(++current, $"Deleted {current} of {totalSteps}");
                    }
                    trans.Commit();
                }

                progressBar.UpdateProgress(totalSteps, "Model group cleanup complete.");
                System.Threading.Thread.Sleep(1000);
                progressBar.CloseProgress();

                TaskDialog.Show("Model Group Cleanup",
                    $"{explodedCount} model groups ungrouped.\n{deletedCount} unplaced model group types deleted.");

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
            return new Common.ButtonDataClass(
                "btnUngroupDeleteModelGroups",
                "Ungroup and Delete Model Groups",
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.UngroupDeleteMGroups32x32,
                Properties.Resources.UngroupDeleteMGroups16x16,
                "Ungroup and then delete all model groups in the project.").Data;
        }
    }

}