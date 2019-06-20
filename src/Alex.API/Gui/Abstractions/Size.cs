using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui
{
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Size : IEquatable<Size>
    {
        public static Size Zero { get; } = new Size(0, 0);
        public static Size One { get; } = new Size(1, 1);
        public static Size UnitWidth { get; } = new Size(1, 0);
        public static Size UnitHeight { get; } = new Size(0, 1);

        internal string DebugDisplayString => this.Width.ToString() + "x" + this.Height.ToString();
        
        [DataMember] public int Width;
        [DataMember] public int Height;

        public Size(Point point) : this(point.X, point.Y) { }
        public Size(Vector2 vector) : this((int)vector.X, (int)vector.Y) { }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Size(int value)
        {
            Width = value;
            Height = value;
        }

        public static Size Parse(string value)
        {
            if (value == null) return Zero;

            if (value.IndexOf(',') > 0)
            {
                var split = value.Split(',', 2);
                var w = int.Parse(split[0]);
                var h = int.Parse(split[1]);
                return new Size(w, h);
            }
            else
            {
                return new Size(int.Parse(value));
            }

        }

        public Point ToPoint()
        {
            return new Point(Width, Height);
        }
        public Vector2 ToVector2()
        {
            return new Vector2(Width, Height);
        }
        public override string ToString()
        {
            return "{Width:" + Width + " Height:" + Height + "}";
        }

        #region Static Helpers

        public static Size Max(Size a, Size b)
        {
            return new Size(Math.Max(a.Width, b.Width), Math.Max(a.Height, b.Height));
        }
        public static Size Min(Size a, Size b)
        {
            return new Size(Math.Min(a.Width, b.Width), Math.Min(a.Height, b.Height));
        }
        public static Size Clamp(Size value, Size minSize, Size maxSize)
        {
            return new Size(MathHelper.Clamp(value.Width, minSize.Width, maxSize.Width), MathHelper.Clamp(value.Height, minSize.Height, maxSize.Height));
        }

        #endregion

        #region Operators
        
        public static Size operator +(Size a, Size b)
        {
            return new Size(a.Width + b.Width, a.Height + b.Height);
        } 
        public static Size operator -(Size a, Size b)
        {
            return new Size(a.Width - b.Width, a.Height - b.Height);
        } 
        public static Size operator *(Size a, Size b)
        {
            return new Size(a.Width * b.Width, a.Height * b.Height);
        }    
        public static Size operator /(Size a, Size b)
        {
            return new Size(a.Width / b.Width, a.Height / b.Height);
        }
        public static Size operator %(Size a, Size b)
        {
            return new Size(a.Width % b.Width, a.Height % b.Height);
        }

        public static Size operator +(Size a, int b)
        {
            return new Size(a.Width + b, a.Height + b);
        } 
        public static Size operator -(Size a, int b)
        {
            return new Size(a.Width - b, a.Height - b);
        } 
        public static Size operator *(Size a, int b)
        {
            return new Size(a.Width * b, a.Height * b);
        } 
        public static Size operator /(Size a, int b)
        {
            return new Size(a.Width / b, a.Height / b);
        }
        public static Size operator %(Size a, int b)
        {
            return new Size(a.Width % b, a.Height % b);
        } 


        public static Size operator +(Size a, Point b)
        {
            return new Size(a.Width + b.X, a.Height + b.Y);
        } 
        public static Size operator -(Size a, Point b)
        {
            return new Size(a.Width - b.X, a.Height - b.Y);
        } 
        public static Point operator +(Point a, Size b)
        {
            return new Point(a.X + b.Width, a.Y + b.Height);
        } 
        public static Point operator -(Point a, Size b)
        {
            return new Point(a.X - b.Width, a.Y - b.Height);
        } 

        public static bool operator ==(Size a, Size b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(Size a, Size b)
        {
            return !a.Equals(b);
        }

        public static bool operator <(Size a, Size b)
        {
            return a.Width < b.Width && a.Height < b.Height;
        }
        public static bool operator >(Size a, Size b)
        {
            return a.Width > b.Width && a.Height > b.Height;
        }

        public static bool operator <=(Size a, Size b)
        {
            return a.Width <= b.Width && a.Height <= b.Height;
        }
        public static bool operator >=(Size a, Size b)
        {
            return a.Width >= b.Width && a.Height >= b.Height;
        }

        public static Rectangle operator +(Rectangle rectangle, Size size)
        {
            return new Rectangle(rectangle.Location, rectangle.Size + size);
        }
        public static Rectangle operator -(Rectangle rectangle, Size size)
        {
            return new Rectangle(rectangle.Location, rectangle.Size - size);
        }


        public static implicit operator Point(Size size)
        {
            return size.ToPoint();
        }
        public static implicit operator Size(Point size)
        {
            return new Size(size);
        }

        public static explicit operator Vector2(Size size)
        {
            return size.ToVector2();
        }
        public static explicit operator Size(Vector2 size)
        {
            return new Size(size);
        }
        
        #endregion
        
        #region Equality

        public bool Equals(Size other)
        {
            return Width == other.Width && Height == other.Height;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Size s && Equals(s);
        }
        public override int GetHashCode()
        {
            return (Width.GetHashCode() * 397) ^ Height.GetHashCode();
        }

        #endregion
    }
}
