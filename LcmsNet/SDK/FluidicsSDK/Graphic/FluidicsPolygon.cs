﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FluidicsSDK.Graphic
{
    public class FluidicsPolygon:GraphicsPrimitive
    {

        #region members
        private List<Point> vertices;
        Rectangle BoundingBox;
        private float m_scale;
        #endregion

        #region methods

        public FluidicsPolygon()
        {
            vertices = new List<Point>();
            BoundingBox = new Rectangle(0, 0, 0, 0);
            m_scale = 1;
        }


        public void AddPoint(int x, int y)
        {
            vertices.Add(new Point(x, y));
            UpdateBoundingBox();
        }

        private void UpdateBoundingBox()
        {
            var minX = vertices.Min(x => x.X);
            var maxX = vertices.Max(x => x.X);
            var minY = vertices.Min(x => x.Y);
            var maxY = vertices.Max(x => x.Y);
            var start = new Point(minX, minY);
            var sz = new Size(maxX - minX, maxY - minY);
            BoundingBox = new Rectangle(start, sz);
        }

        public void AddPoint(Point p)
        {
            vertices.Add(p);
            UpdateBoundingBox();
        }

        public void DeletePoint(int x, int y)
        {
            //remove from the end of the list, so we remove in reverse order of the points being added.
            vertices.Remove(vertices.Last(point => (point.X == x && point.Y == y)));
            UpdateBoundingBox();
        }

        public void DeletePoint(Point p)
        {
            vertices.Remove(p);
            UpdateBoundingBox();
        }

        public override void Render(Graphics g, int alpha, float scale, bool selected, bool error)
        {

            // alter the color of the circle to be proper alpha, EXCEPT for error which should always be solid.
            m_scale = scale;
            Color = Color.FromArgb(alpha, Color.R, Color.G, Color.B);
            Highlight = Color.FromArgb(alpha, Highlight.R, Highlight.G, Highlight.B);
            //Color fillColor = Color.FromArgb(alpha, base.FillColor.R, base.FillColor.G, base.FillColor.B);
            Pen drawingPen;
            var scaledVertices = new Point[vertices.Count];
            for (var i = 0; i < vertices.Count; i++)
            {
                scaledVertices[i].X = (int)(vertices[i].X * scale);
                scaledVertices[i].Y = (int)(vertices[i].Y * scale);

            }
            if (selected)
            {
                drawingPen = Highlighter;
            }
            else if (error)
            {
                drawingPen = ErrorPen;
            }
            else
            {
                drawingPen = Pen;
            }
            if (Fill)
            {
                g.FillPolygon(FillBrush, scaledVertices.ToArray());

            }
            g.DrawPolygon(drawingPen, scaledVertices.ToArray());
            //g.ResetTransform();

        }

        public override void Render(Graphics g, int alpha, float scale, Point moveby, bool highlight, bool error)
        {
            g.ScaleTransform(scale / 2, scale / 2);
            g.TranslateTransform(scale / 2, scale / 2);
            Render(g, alpha, scale, highlight, error);
        }

        private Point scalePoint(Point p)
        {
            return new Point((int)(p.X* m_scale), (int)(p.Y * m_scale));
        }

        public override void MoveBy(Point relativeValues)
        {
            for (var i = 0; i < vertices.Count; i++)
            {
                var p = new Point
                {
                    X = vertices[i].X + relativeValues.X,
                    Y = vertices[i].Y + relativeValues.Y
                };
                vertices[i] = p;
            }
            UpdateBoundingBox();
        }

        public override bool Contains(Point point, int max_variance)
        {
            return BoundingBox.Contains(point);
        }
        #endregion

        #region Properties

        public List<Point> Points
        {
          get
          {
                return vertices;
          }
          set
          {
              vertices = value;
          }
        }


        public override Point Loc
        {
            get
            {
                return BoundingBox.Location;
            }
            set
            {
                throw new FluidicsGraphicsError("To change the location of a fluidics polygon, use moveby method");
            }
        }
        public override Size Size
        {
            get
            {
                return BoundingBox.Size;
            }
            set
            {
                throw new InvalidOperationException("Cannot change the size of a polygon directly. Modify it's points");
            }
        }
        #endregion
    }
}
