using System;
using System.Collections.Generic;
using System.IO;
using Bricscad.ApplicationServices;
using Bricscad.EditorInput;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Application = Bricscad.ApplicationServices.Application;
using DialogResult = System.Windows.Forms.DialogResult;
using Exception = System.Exception;
using OpenFileDialog = Bricscad.Windows.OpenFileDialog;

namespace BricsCADPlugins
{
    public class AirfoilsHelper
    {
        [CommandMethod("AirfoilsDatToSpline")]
        public static void AirfoilsDatToSpline() {
            OpenFileDialog fileDialog = new OpenFileDialog("Select a .dat file", "", "dat", "", 0);

            if (fileDialog.ShowDialog() != DialogResult.OK) return;

            Document    doc = Application.DocumentManager.MdiActiveDocument;
            Database    db = doc.Database;
            Editor      ed = doc.Editor;

            string          filePath = fileDialog.Filename;
            List<Point3d>   points;
            
            try {
                points = ParseDatFile(filePath);
            } catch (Exception exception) {
                ed.WriteMessage($"\nError while parsing the file: {exception.Message}");
                return;
            }
            
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                PromptPointOptions  pPointOps = new PromptPointOptions("\nEnter the origin point: ");
                PromptPointResult   pPointRes = ed.GetPoint(pPointOps);
                if (pPointRes.Status != PromptStatus.OK) return;
                Point3d originPoint = pPointRes.Value;

                PromptDoubleOptions pDoubleOps = new PromptDoubleOptions("\nEnter the scale factor: ");
                PromptDoubleResult  pDoubleRes = ed.GetDouble(pDoubleOps);
                if (pDoubleRes.Status != PromptStatus.OK) return;
                double scaleFactor = pDoubleRes.Value;

                PromptAngleOptions  pAngleOpt = new PromptAngleOptions("\nEnter the rotation angle: ");
                                    pDoubleRes = ed.GetAngle(pAngleOpt);
                if (pDoubleRes.Status != PromptStatus.OK) return;
                double rotationAngle = pDoubleRes.Value;
                
                BlockTable          bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord    btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                for (int i = 0; i < points.Count; ++i) {
                    points[i] = points[i] * scaleFactor + originPoint.GetAsVector();
                    points[i] = points[i].RotateBy(rotationAngle, Vector3d.ZAxis, originPoint);
                }
                
                Point3dCollection pointCollection = new Point3dCollection(points.ToArray());

                Spline spline = new Spline(pointCollection, 3, 0.0);

                btr.AppendEntity(spline);
                tr.AddNewlyCreatedDBObject(spline, true);

                tr.Commit();
            }
        }
        
        private static List<Point3d> ParseDatFile(string filePath) {
            var points = new List<Point3d>();
            string[] lines = File.ReadAllLines(filePath);

            for (int i = 1; i < lines.Length; ++i) {
                if (lines[i] == "") {
                    continue;
                }
                
                string[] values = lines[i].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                double x = Convert.ToDouble(values[0]);
                double y = Convert.ToDouble(values[1]);

                if (-0.1 <= x && x <= 1.1 && -1.0 <= y && y <= 1.0) {
                    points.Add(new Point3d(x, y, 0d));
                }
            }

            return points;
        }
    }
}