using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TimetableUI.Controls
{
    public class GridEx : Grid
    {
        #region Properties
        public bool ShowCustomGridLines {
            get { return (bool)GetValue(ShowCustomGridLinesProperty); }
            set { SetValue(ShowCustomGridLinesProperty, value); }
        }

        public static readonly DependencyProperty ShowCustomGridLinesProperty =
            DependencyProperty.Register("ShowCustomGridLines", typeof(bool), typeof(GridEx), new UIPropertyMetadata(false));

        public Brush GridLineBrush {
            get { return (Brush)GetValue(GridLineBrushProperty); }
            set { SetValue(GridLineBrushProperty, value); }
        }

        public static readonly DependencyProperty GridLineBrushProperty =
            DependencyProperty.Register("GridLineBrush", typeof(Brush), typeof(GridEx), new UIPropertyMetadata(Brushes.Black));

        public double GridLineThickness {
            get { return (double)GetValue(GridLineThicknessProperty); }
            set { SetValue(GridLineThicknessProperty, value); }
        }

        public static readonly DependencyProperty GridLineThicknessProperty =
            DependencyProperty.Register("GridLineThickness", typeof(double), typeof(GridEx), new UIPropertyMetadata(1.0));
        #endregion

        protected override void OnRender(DrawingContext dc)
        {
            if (ShowCustomGridLines)
            {
                var pen = new Pen(GridLineBrush, GridLineThickness);
                var penWidth = pen.Thickness / 2;
                var guidelines = new GuidelineSet();

                // Setup guidelines
                //guidelines.GuidelinesX.Add(0 + penWidth);
                //guidelines.GuidelinesX.Add(ActualWidth + penWidth);
                //guidelines.GuidelinesY.Add(0 + penWidth);
                //guidelines.GuidelinesY.Add(ActualHeight + penWidth);

                foreach (var rowDefinition in RowDefinitions)
                {
                    guidelines.GuidelinesY.Add(rowDefinition.Offset + penWidth);
                }
                foreach (var columnDefinition in ColumnDefinitions)
                {
                    guidelines.GuidelinesX.Add(columnDefinition.Offset + penWidth);
                }
                dc.PushGuidelineSet(guidelines);

                // Draw gridlines
                foreach (var rowDefinition in RowDefinitions)
                {
                    dc.DrawLine(pen, new Point(0, rowDefinition.Offset), new Point(ActualWidth, rowDefinition.Offset));
                }
                foreach (var columnDefinition in ColumnDefinitions)
                {
                    dc.DrawLine(pen, new Point(columnDefinition.Offset, 0), new Point(columnDefinition.Offset, ActualHeight));
                }
                
                //dc.DrawRectangle(Brushes.Transparent, new Pen(GridLineBrush, GridLineThickness), new Rect(0, 0, ActualWidth, ActualHeight));
            }
            base.OnRender(dc);
        }
        static GridEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridEx), new FrameworkPropertyMetadata(typeof(GridEx)));
        }
    }
}
