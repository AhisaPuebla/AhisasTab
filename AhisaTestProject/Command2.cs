﻿namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            int ungroupedCount = 0;

            List<Group> detailGroups = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Group))
                .Cast<Group>()
                .Where(g => g.GroupType?.Category?.Id.IntegerValue == (int)BuiltInCategory.OST_IOSDetailGroups)
                .ToList();

            if (!detailGroups.Any())
            {
                TaskDialog.Show("Ungroup 2D Groups", "No detail groups found.");
                return Result.Succeeded;
            }

            ProgressBarHelper progressBar = new ProgressBarHelper();

            try
            {
                progressBar.ShowProgress(detailGroups.Count);

                using (Transaction trans = new Transaction(doc, "Ungroup Detail Groups"))
                {
                    trans.Start();

                    for (int i = 0; i < detailGroups.Count; i++)
                    {
                        if (progressBar.IsCancelled())
                        {
                            progressBar.UpdateProgress(i, "Operation cancelled.");
                            trans.RollBack();
                            return Result.Cancelled;
                        }

                        detailGroups[i].UngroupMembers();
                        ungroupedCount++;
                        progressBar.UpdateProgress(i + 1, $"Ungrouping {i + 1} of {detailGroups.Count} detail groups");
                    }

                    trans.Commit();
                }

                progressBar.UpdateProgress(detailGroups.Count, "Detail group ungrouping complete!");
                System.Threading.Thread.Sleep(1000);
                progressBar.CloseProgress();

                TaskDialog.Show("Ungroup 2D Groups", $"{ungroupedCount} detail groups ungrouped.");
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
                "btnUngroupDetailGroups",
                "Ungroup Detail Groups",
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.UngroupDGroups32x32,
                Properties.Resources.UngroupDGroups16x16,
                "Ungroup all placed detail groups in the project.").Data;
        }
    }
}
