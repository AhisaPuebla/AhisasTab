﻿namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command2B : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var detailGroups = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_IOSDetailGroups)
                .WhereElementIsNotElementType()
                .Cast<Group>()
                .ToList();

            var detailGroupTypes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_IOSDetailGroups)
                .WhereElementIsElementType()
                .ToList();

            int explodedCount = 0;
            int deletedCount = 0;
            int totalSteps = detailGroups.Count + detailGroupTypes.Count;

            if (totalSteps == 0)
            {
                TaskDialog.Show("Detail Group Cleanup", "No detail groups or group types found.");
                return Result.Succeeded;
            }

            var progressBar = new ProgressBarHelper();
            progressBar.ShowProgress(totalSteps);
            int current = 0;

            try
            {
                using (Transaction trans = new Transaction(doc, "Ungroup Detail Groups"))
                {
                    trans.Start();
                    foreach (Group group in detailGroups)
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

                using (Transaction trans = new Transaction(doc, "Delete Detail Group Types"))
                {
                    trans.Start();
                    foreach (Element groupType in detailGroupTypes)
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

                progressBar.UpdateProgress(totalSteps, "Detail group cleanup complete.");
                System.Threading.Thread.Sleep(1000);
                progressBar.CloseProgress();

                TaskDialog.Show("Detail Group Cleanup",
                    $"{explodedCount} detail groups ungrouped.\n{deletedCount} unplaced detail group types deleted.");

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
                "btnUngroupDeleteDetailGroups",
                "Ungroup and Delete all Detail Groups ",
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.UngroupDeleteDGroups32x32,
                Properties.Resources.UngroupDeleteDGroups16x16,
                "Ungroup and then delete all detail groups in the project.").Data;
        }
    }

}