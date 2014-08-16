using System.Windows;
using System.Windows.Media;

namespace POESKillTree
{
    static class EXTHelper
    {
     public static void DrawArc(this DrawingContext drawingContext, Brush brush,
     Pen pen, Point start, Point end, Size radius)
        {
            // setup the geometry object
            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure();
            geometry.Figures.Add(figure);
            figure.StartPoint = start;

            // add the arc to the geometry
            figure.Segments.Add(new ArcSegment(end, radius,0, false, SweepDirection.Counterclockwise, true));
        
         
            // draw the arc
            drawingContext.DrawGeometry(brush, pen, geometry);
        }
    
    }
}
