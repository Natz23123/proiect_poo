using System;
using System.Drawing;
using System.Windows.Forms;

namespace proiect_poo
{
    public class TerminalProgressBar : Control
    {
        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 0;

        public TerminalProgressBar()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(160, 20);
        }

        public int Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                if (_maximum < _minimum)
                {
                    _maximum = _minimum;
                }
                if (_value < _minimum)
                {
                    _value = _minimum;
                }
                Invalidate();
            }
        }

        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                if (_minimum > _maximum)
                {
                    _minimum = _maximum;
                }
                if (_value > _maximum)
                {
                    _value = _maximum;
                }
                Invalidate();
            }
        }

        public int Value
        {
            get => _value;
            set
            {
                int clamped = Math.Max(_minimum, Math.Min(_maximum, value));
                if (_value == clamped)
                {
                    return;
                }
                _value = clamped;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(Color.Black);

            const int segmentCount = 20;
            const int gap = 2;

            int totalGap = gap * (segmentCount - 1);
            int segmentWidth = (Width - totalGap) / segmentCount;
            if (segmentWidth < 1)
            {
                segmentWidth = 1;
            }

            double range = _maximum - _minimum;
            double progress = range <= 0 ? 0 : (double)(_value - _minimum) / range;
            int filledSegments = (int)Math.Floor(progress * segmentCount);
            filledSegments = Math.Max(0, Math.Min(segmentCount, filledSegments));

            using (Brush fillBrush = new SolidBrush(Color.White))
            using (Pen borderPen = new Pen(Color.White))
            {
                int x = 0;
                for (int i = 0; i < segmentCount; i++)
                {
                    Rectangle rect = new Rectangle(x, 0, segmentWidth, Height - 1);
                    if (i < filledSegments)
                    {
                        e.Graphics.FillRectangle(fillBrush, rect);
                    }
                    else
                    {
                        e.Graphics.DrawRectangle(borderPen, rect);
                    }

                    x += segmentWidth + gap;
                }
            }
        }
    }
}
