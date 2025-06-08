namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command2A : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            int ungroupedCount = 0;

            List<Group> modelGroups = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Group))
                .Cast<Group>()
                .Where(g => g.GroupType?.Category?.Id.IntegerValue == (int)BuiltInCategory.OST_IOSModelGroups)
                .ToList();

            if (!modelGroups.Any())
            {
                TaskDialog.Show("Ungroup 3D Groups", "No model groups found.");
                return Result.Succeeded;
            }

            ProgressBarHelper progressBar = new ProgressBarHelper();

            try
            {
                progressBar.ShowProgress(modelGroups.Count);

                using (Transaction trans = new Transaction(doc, "Ungroup Model Groups"))
                {
                    trans.Start();

                    for (int i = 0; i < modelGroups.Count; i++)
                    {
                        if (progressBar.IsCancelled())
                        {
                            progressBar.UpdateProgress(i, "Operation cancelled.");
                            trans.RollBack();
                            return Result.Cancelled;
                        }

                        modelGroups[i].UngroupMembers();
                        ungroupedCount++;
                        progressBar.UpdateProgress(i + 1, $"Ungrouping {i + 1} of {modelGroups.Count} model groups");
                    }

                    trans.Commit();
                }

                progressBar.UpdateProgress(modelGroups.Count, "Model group ungrouping complete!");
                System.Threading.Thread.Sleep(1000);
                progressBar.CloseProgress();

                TaskDialog.Show("Ungroup 3D Groups", $"{ungroupedCount} model groups ungrouped.");
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
            return new Common.ButtonDataClass(
                "btnUngroupModelGroups",
                "Ungroup All Model Groups",
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.UngroupMGroups32x32,
                Properties.Resources.UngroupMGroups16x16,
                "Ungroup all placed model groups in the project.").Data;
        }
    }
}
