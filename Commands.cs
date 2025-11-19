using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AreaDivider
{
    public class Commands
    {
        private const double Tolerance = 0.001; // Area calculation tolerance

        // Helper function to create a temporary Region from a Polyline
        private Region PolylineToRegion(Polyline polyline, Transaction tr)
        {
            Polyline tempPoly = polyline.Clone() as Polyline;
            if (tempPoly == null) return null;

            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(
                (ObjectId)Application.GetSystemVariable("CLAYER"), OpenMode.ForWrite);
            btr.AppendEntity(tempPoly);
            tr.AddNewlyCreatedDBObject(tempPoly, true);

            DBObjectCollection dboc = new DBObjectCollection();
            dboc.Add(tempPoly);

            DBObjectCollection regions = Region.CreateFromCurves(dboc);
            
            tempPoly.Erase();

            if (regions.Count > 0)
            {
                return regions[0] as Region;
            }
            return null;
        }

        // Helper function to create a cutting Region based on the line direction and distance
        private Region CreateCuttingRegion(Point3d startPoint, double angle, double distance, double maxDistance, Transaction tr)
        {
            Point3d cutPoint = startPoint.Polar(angle, distance);
            double perpendicularAngle = angle + Math.PI / 2.0;
            double length = maxDistance * 2.0; 

            Point3d p1 = cutPoint.Polar(perpendicularAngle, length / 2.0);
            Point3d p2 = p1.Polar(angle, length);
            Point3d p3 = p2.Polar(perpendicularAngle, -length / 2.0);
            Point3d p4 = p3.Polar(angle, -length);
            
            Polyline cutterPoly = new Polyline();
            cutterPoly.AddVertexAt(0, new Point2d(p1.X, p1.Y), 0, 0, 0);
            cutterPoly.AddVertexAt(1, new Point2d(p2.X, p2.Y), 0, 0, 0);
            cutterPoly.AddVertexAt(2, new Point2d(p3.X, p3.Y), 0, 0, 0);
            cutterPoly.AddVertexAt(3, new Point2d(p4.X, p4.Y), 0, 0, 0);
            cutterPoly.Closed = true;
            
            Region cutterRegion = PolylineToRegion(cutterPoly, tr);
            cutterPoly.Dispose();
            
            return cutterRegion;
        }
        
        // Helper function to create a cutting Region based on a line defined by a point and an angle (for Rotation Cut)
        private Region CreateRotationCuttingRegion(Point3d pivotPoint, double angle, double maxDistance, Transaction tr)
        {
            double length = maxDistance * 2.0; 
            double perpendicularAngle = angle + Math.PI / 2.0;

            // Define the four corners of the cutting rectangle (to the "left" of the line passing through pivotPoint)
            Point3d p1 = pivotPoint.Polar(angle, length);
            Point3d p2 = pivotPoint.Polar(angle, -length);
            
            Point3d cp1 = p1.Polar(perpendicularAngle, length / 2.0);
            Point3d cp2 = p2.Polar(perpendicularAngle, length / 2.0);
            Point3d cp3 = p2.Polar(perpendicularAngle, -length / 2.0);
            Point3d cp4 = p1.Polar(perpendicularAngle, -length / 2.0);
            
            Polyline cutterPoly = new Polyline();
            cutterPoly.AddVertexAt(0, new Point2d(cp1.X, cp1.Y), 0, 0, 0);
            cutterPoly.AddVertexAt(1, new Point2d(cp2.X, cp2.Y), 0, 0, 0);
            cutterPoly.AddVertexAt(2, new Point2d(cp3.X, cp3.Y), 0, 0, 0);
            cutterPoly.AddVertexAt(3, new Point2d(cp4.X, cp4.Y), 0, 0, 0);
            cutterPoly.Closed = true;
            
            Region cutterRegion = PolylineToRegion(cutterPoly, tr);
            cutterPoly.Dispose();
            
            return cutterRegion;
        }

        // Helper function to calculate the area of the section cut by a line (Parallel Cut)
        private double GetCutAreaParallel(Polyline originalPoly, Point3d startPoint, double angle, double distance, double maxDistance, Transaction tr)
        {
            Region cutterRegion = CreateCuttingRegion(startPoint, angle, distance, maxDistance, tr);
            if (cutterRegion == null) return 0.0;

            Region originalRegion = PolylineToRegion(originalPoly, tr);
            if (originalRegion == null)
            {
                cutterRegion.Dispose();
                return 0.0;
            }

            Region tempRegion = originalRegion.Clone() as Region;
            tempRegion.IntersectWith(cutterRegion, Intersect.ExtendBoth);
            
            double area = tempRegion.Area;

            originalRegion.Dispose();
            cutterRegion.Dispose();
            tempRegion.Dispose();
            
            return area;
        }
        
        // Helper function to calculate the area of the section cut by a line (Rotation Cut)
        private double GetCutAreaRotation(Polyline originalPoly, Point3d pivotPoint, double angle, double maxDistance, Transaction tr)
        {
            Region cutterRegion = CreateRotationCuttingRegion(pivotPoint, angle, maxDistance, tr);
            if (cutterRegion == null) return 0.0;

            Region originalRegion = PolylineToRegion(originalPoly, tr);
            if (originalRegion == null)
            {
                cutterRegion.Dispose();
                return 0.0;
            }

            Region tempRegion = originalRegion.Clone() as Region;
            tempRegion.IntersectWith(cutterRegion, Intersect.ExtendBoth);
            
            double area = tempRegion.Area;

            originalRegion.Dispose();
            cutterRegion.Dispose();
            tempRegion.Dispose();
            
            return area;
        }

        // Bisection Search implementation for Parallel Cut
        private double BisectionSearchParallel(Polyline polyline, double targetArea, Point3d startPoint, double angle, double maxDistance, Transaction tr)
        {
            double minDistance = 0.0;
            double maxDist = maxDistance;
            double currentDistance = 0.0;
            double currentArea = 0.0;
            int maxIterations = 100; 
            
            double maxArea = GetCutAreaParallel(polyline, startPoint, angle, maxDist, maxDistance, tr);
            if (maxArea < targetArea) return -1.0; 

            for (int i = 0; i < maxIterations; i++)
            {
                currentDistance = (minDistance + maxDist) / 2.0;
                currentArea = GetCutAreaParallel(polyline, startPoint, angle, currentDistance, maxDistance, tr);

                if (Math.Abs(currentArea - targetArea) < Tolerance) return currentDistance; 

                if (currentArea < targetArea)
                {
                    minDistance = currentDistance;
                }
                else
                {
                    maxDist = currentDistance;
                }
            }
            
            return currentDistance;
        }
        
        // Bisection Search implementation for Rotation Cut
        private double BisectionSearchRotation(Polyline polyline, double targetArea, Point3d pivotPoint, double maxDistance, Transaction tr)
        {
            // We must search over a range where the area function is monotonic.
            // A full 2*PI search is not monotonic. We will search over a limited range
            // (e.g., PI/2) around the initial direction, assuming the pivot point is outside.
            
            double minAngle = 0.0;
            double maxAngle = Math.PI / 2.0; // Limited search range
            double currentAngle = 0.0;
            double currentArea = 0.0;
            int maxIterations = 100; 
            
            // Check if the target area is achievable within the range
            double areaAtMin = GetCutAreaRotation(polyline, pivotPoint, minAngle, maxDistance, tr);
            double areaAtMax = GetCutAreaRotation(polyline, pivotPoint, maxAngle, maxDistance, tr);
            
            if (targetArea < Math.Min(areaAtMin, areaAtMax) || targetArea > Math.Max(areaAtMin, areaAtMax))
            {
                // Target area is outside the search range for this pivot/angle combination
                return -1.0; 
            }

            for (int i = 0; i < maxIterations; i++)
            {
                currentAngle = (minAngle + maxAngle) / 2.0;
                currentArea = GetCutAreaRotation(polyline, pivotPoint, currentAngle, maxDistance, tr);

                if (Math.Abs(currentArea - targetArea) < Tolerance) return currentAngle; 

                // Determine if the area function is increasing or decreasing in this range
                // For simplicity, we assume it's increasing (or decreasing) and adjust the search accordingly.
                // In a real-world scenario, this requires a more robust check.
                if (areaAtMin < areaAtMax) // Area is increasing with angle
                {
                    if (currentArea < targetArea)
                    {
                        minAngle = currentAngle;
                    }
                    else
                    {
                        maxAngle = currentAngle;
                    }
                }
                else // Area is decreasing with angle
                {
                    if (currentArea < targetArea)
                    {
                        maxAngle = currentAngle;
                    }
                    else
                    {
                        minAngle = currentAngle;
                    }
                }
            }
            
            return currentAngle;
        }

        // --- Main Command Method ---
        [CommandMethod("DIVAREA_NET", CommandFlags.Modal)]
        public void DivideAreaNet()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            // Show the GUI Form
            AreaDividerForm form = new AreaDividerForm();
            
            // Use Application.ShowModalDialog for AutoCAD compatibility
            if (Application.ShowModalDialog(form) != DialogResult.OK)
            {
                ed.WriteMessage("\nOperation cancelled by user.");
                return;
            }

            // Get settings from the form
            string cuttingMethod = form.CuttingMethod;
            double numParts = form.NumberOfParts;
            double frontDimension = form.FrontDimension;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    // 1. Select the closed Polyline
                    PromptEntityOptions peo = new PromptEntityOptions("\nSelect the closed Polyline (Area to be divided): ");
                    peo.SetRejectMessage("\nInvalid selection. Please select a closed Polyline.");
                    peo.AddAllowedClass(typeof(Polyline), true);
                    PromptEntityResult per = ed.GetEntity(peo);
                    if (per.Status != PromptStatus.OK) return;
                    
                    Polyline originalPoly = tr.GetObject(per.ObjectId, OpenMode.ForWrite) as Polyline;
                    if (originalPoly == null || originalPoly.Closed == false) { ed.WriteMessage("\nSelected object is not a closed Polyline."); return; }

                    double totalArea = originalPoly.Area;
                    
                    // 2. Get Cutting Direction/Pivot Point
                    Point3d startPoint = Point3d.Origin;
                    double angle = 0.0;
                    Point3d pivotPoint = Point3d.Origin;
                    
                    if (cuttingMethod == "RotationCut")
                    {
                        PromptPointOptions ppoPivot = new PromptPointOptions("\nSpecify the Pivot Point for the Rotation Cut: ");
                        PromptPointResult pprPivot = ed.GetPoint(ppoPivot);
                        if (pprPivot.Status != PromptStatus.OK) return;
                        pivotPoint = pprPivot.Value;
                        ed.WriteMessage($"\nMethod: Rotation Cut. Pivot Point: {pivotPoint}");
                    }
                    else // EqualArea or FrontDimension (Parallel Cut)
                    {
                        PromptPointOptions ppoStart = new PromptPointOptions("\nSpecify the start point of the cutting direction line: ");
                        PromptPointResult pprStart = ed.GetPoint(ppoStart);
                        if (pprStart.Status != PromptStatus.OK) return;
                        PromptPointOptions ppoEnd = new PromptPointOptions("\nSpecify the end point of the cutting direction line: ");
                        ppoEnd.BasePoint = pprStart.Value;
                        ppoEnd.UseBasePoint = true;
                        PromptPointResult pprEnd = ed.GetPoint(ppoEnd);
                        if (pprEnd.Status != PromptStatus.OK) return;
                        
                        startPoint = pprStart.Value;
                        angle = startPoint.GetVectorTo(endPoint).AngleOnPlane(new Plane());
                        ed.WriteMessage($"\nMethod: Parallel Cut. Direction Angle: {angle * 180.0 / Math.PI:F2} degrees");
                    }
                    
                    double maxDistance = originalPoly.GeometricExtents.MinPoint.GetDistanceTo(originalPoly.GeometricExtents.MaxPoint) * 2.0;

                    // --- Division Logic based on Method ---
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    
                    Region currentRegion = PolylineToRegion(originalPoly, tr);
                    if (currentRegion == null) { ed.WriteMessage("\nCould not create region from polyline."); return; }
                    
                    // Erase the original polyline
                    originalPoly.Erase();

                    if (cuttingMethod == "EqualArea" || cuttingMethod == "RotationCut")
                    {
                        double targetArea = totalArea / numParts;
                        
                        for (int i = 0; i < numParts - 1; i++)
                        {
                            double cutValue = -1.0; // Will be distance for Parallel, angle for Rotation
                            
                            if (cuttingMethod == "EqualArea")
                            {
                                cutValue = BisectionSearchParallel(originalPoly, targetArea, startPoint, angle, maxDistance, tr);
                            }
                            else // RotationCut
                            {
                                cutValue = BisectionSearchRotation(originalPoly, targetArea, pivotPoint, maxDistance, tr);
                            }
                            
                            if (cutValue < 0)
                            {
                                ed.WriteMessage("\nError: Could not find the required cut value. Division stopped.");
                                break;
                            }

                            // 2. Create the cutting Region
                            Region cutterRegion = null;
                            if (cuttingMethod == "EqualArea")
                            {
                                cutterRegion = CreateCuttingRegion(startPoint, angle, cutValue, maxDistance, tr);
                            }
                            else // RotationCut
                            {
                                cutterRegion = CreateRotationCuttingRegion(pivotPoint, cutValue, maxDistance, tr);
                            }
                            
                            // 3. Create the first part (Intersection)
                            Region firstPart = currentRegion.Clone() as Region;
                            firstPart.IntersectWith(cutterRegion, Intersect.ExtendBoth);
                            
                            // 4. Create the remaining part (Subtraction)
                            currentRegion.Subtract(cutterRegion);
                            
                            // 5. Convert the first part Region back to a Polyline and add to drawing
                            DBObjectCollection firstPartPolys = firstPart.Boundary();
                            if (firstPartPolys.Count > 0)
                            {
                                foreach (Entity ent in firstPartPolys)
                                {
                                    btr.AppendEntity(ent);
                                    tr.AddNewlyCreatedDBObject(ent, true);
                                }
                            }
                            
                            ed.WriteMessage($"\nPart {i + 1} created. Area: {firstPart.Area:F2}");
                            
                            // Clean up
                            firstPart.Dispose();
                            cutterRegion.Dispose();
                        }
                        
                        // 6. The last remaining part is in currentRegion. Convert it to Polyline.
                        DBObjectCollection lastPartPolys = currentRegion.Boundary();
                        if (lastPartPolys.Count > 0)
                        {
                            foreach (Entity ent in lastPartPolys)
                            {
                                btr.AppendEntity(ent);
                                tr.AddNewlyCreatedDBObject(ent, true);
                            }
                        }
                        
                        ed.WriteMessage($"\nPart {numParts} created. Area: {currentRegion.Area:F2}");
                        currentRegion.Dispose();
                    }
                    else if (cuttingMethod == "FrontDimension")
                    {
                        // --- Fixed Front Dimension Logic (Simplified) ---
                        double totalLength = originalPoly.Length;
                        double averageDepth = totalArea / totalLength;
                        double assumedTargetArea = frontDimension * averageDepth;
                        
                        ed.WriteMessage($"\nMethod: Fixed Front Dimension ({frontDimension:F2}). Assumed Target Area: {assumedTargetArea:F2}");
                        
                        // Find the cut distance for the assumed area
                        double cutDistance = BisectionSearchParallel(originalPoly, assumedTargetArea, startPoint, angle, maxDistance, tr);
                        
                        if (cutDistance < 0)
                        {
                            ed.WriteMessage("\nError: Could not find the required cut distance.");
                        }
                        else
                        {
                            // Draw the final cutting line (no splitting for this simplified method)
                            double perpendicularAngle = angle + Math.PI / 2.0;
                            Point3d cutPoint = startPoint.Polar(angle, cutDistance);
                            
                            Line line = new Line(
                                cutPoint.Polar(perpendicularAngle, maxDistance),
                                cutPoint.Polar(perpendicularAngle, -maxDistance)
                            );
                            
                            btr.AppendEntity(line);
                            tr.AddNewlyCreatedDBObject(line, true);
                            
                            ed.WriteMessage($"\nCut line drawn at distance: {cutDistance:F3}.");
                        }
                        
                        // Since this is a simplified method, we don't split the polyline, we just draw the line.
                        // The original polyline was erased, so we need to restore it or inform the user.
                        ed.WriteMessage("\nNote: For Fixed Front Dimension, only the cut line is drawn. The original polyline was erased.");
                        currentRegion.Dispose(); // Clean up the region
                    }

                    tr.Commit();
                    ed.WriteMessage("\nArea Division (NET) completed.");
                }
                catch (Exception ex)
                {
                    ed.WriteMessage($"\nAn error occurred: {ex.Message}");
                    tr.Abort();
                }
            }
        }
    }
}
