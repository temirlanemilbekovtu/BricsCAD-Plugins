using System.Collections.Generic;
using Bricscad.ApplicationServices;
using Bricscad.EditorInput;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;

namespace BricsCADPlugins
{
    public static class TransformationsHelper
    {
        [CommandMethod("StretchRelatively")]
        public static void StretchRelatively() {
            Document doc    = Application.DocumentManager.MdiActiveDocument;
            Database db     = doc.Database;
            Editor editor   = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction()) {
                var selRes = editor.GetSelection();
                if (selRes.Status != PromptStatus.OK) return;

                SelectionSet selection = selRes.Value;
                var entities = new List<Entity>();
                foreach (SelectedObject selObj in selection) {
                    if (selObj != null) {
                        Entity entity = tr.GetObject(selObj.ObjectId, OpenMode.ForWrite) as Entity;
                        if (entity != null) {
                            entities.Add(entity);
                        }
                    }
                }

                PromptIntegerOptions pIntOpt = new PromptIntegerOptions("\nEnter the axis (X=0, Y=1, Z=2): ");
                PromptIntegerResult pIntRes = editor.GetInteger(pIntOpt);
                if (pIntRes.Status != PromptStatus.OK) return;
                int axis = pIntRes.Value;
                
                PromptDoubleOptions pDoubleOps = new PromptDoubleOptions("\nEnter the multiplier: ");
                PromptDoubleResult pDoubleRes = editor.GetDouble(pDoubleOps);
                if (pDoubleRes.Status != PromptStatus.OK) return;
                double multiplier = pDoubleRes.Value;

                PromptPointOptions pPointOps = new PromptPointOptions("\nEnter the origin point: ");
                PromptPointResult pPointRes = editor.GetPoint(pPointOps);
                if (pPointRes.Status != PromptStatus.OK) return;
                Point3d originPoint = pPointRes.Value;

                foreach (Entity entity in entities) {
                    if (entity is Polyline polyline) {
                        for (int i = 0; i < polyline.NumberOfVertices; i++) {
                            var point = polyline.GetPoint3dAt(i);
                            var newPoint = TransformCoordinate(point, axis, originPoint, multiplier);
                            
                            polyline.SetPointAt(i, new Point2d(newPoint.X, newPoint.Y));
                        }
                    } else if (entity is Line line) {
                        var startPoint = line.StartPoint;
                        var endPoint = line.EndPoint;
                        
                        var newStartPoint = TransformCoordinate(startPoint, axis, originPoint, multiplier);
                        var newEndPoint = TransformCoordinate(endPoint, axis, originPoint, multiplier);

                        line.StartPoint = newStartPoint;
                        line.EndPoint = newEndPoint;
                    } else if (entity is Arc arc) {
                        var startPoint = arc.StartPoint;
                        var endPoint = arc.EndPoint;
                        
                        var newStartPoint = TransformCoordinate(startPoint, axis, originPoint, multiplier);
                        var newEndPoint = TransformCoordinate(endPoint, axis, originPoint, multiplier);
                        
                        arc.StartPoint = newStartPoint;
                        arc.EndPoint = newEndPoint;
                    }
                }
                tr.Commit();
            }
        }

        private static Point3d TransformCoordinate(Point3d sourcePoint, int axis, Point3d originPoint, double multiplier) {
            double sourceCoord = GetPointAtAxis(sourcePoint, axis);
            double originCoord = GetPointAtAxis(originPoint, axis);
            double resCoord = originCoord + (sourceCoord - originCoord) * multiplier;
            return GetModifiedAxis(sourcePoint, axis, resCoord);
        }
        
        private static double GetPointAtAxis(Point3d point, int axis) {
            switch (axis) {
                case 0: return point.X;
                case 1: return point.Y;
                case 2: return point.Z;
                default: return point.X;
            }
        }

        private static Point3d GetModifiedAxis(Point3d point, int axis, double newValue) {
            switch (axis) {
                case 0: return new Point3d(newValue, point.Y, point.Z);
                case 1: return new Point3d(point.X, newValue, point.Z);
                case 2: return new Point3d(point.X, point.Y, newValue);
                default: return point;
            }
        }
    }
}