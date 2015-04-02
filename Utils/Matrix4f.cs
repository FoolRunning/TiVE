#region --- License ---
/*
Copyright (c) 2006 - 2008 The Open Toolkit library.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */
#endregion

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ProdigalSoftware.Utils
{
    /// <summary>
    /// Represents a 4x4 matrix containing 3D rotation, scale, transform, and projection.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix4f : IEquatable<Matrix4f>
    {
        #region Fields

        /// <summary>
        /// Top row of the matrix.
        /// </summary>
        public float Row0X;

        /// <summary>
        /// Top row of the matrix.
        /// </summary>
        public float Row0Y;

        /// <summary>
        /// Top row of the matrix.
        /// </summary>
        public float Row0Z;

        /// <summary>
        /// Top row of the matrix.
        /// </summary>
        public float Row0W;

        /// <summary>
        /// 2nd row of the matrix.
        /// </summary>
        public float Row1X;

        /// <summary>
        /// 2nd row of the matrix.
        /// </summary>
        public float Row1Y;

        /// <summary>
        /// 2nd row of the matrix.
        /// </summary>
        public float Row1Z;

        /// <summary>
        /// 2nd row of the matrix.
        /// </summary>
        public float Row1W;

        /// <summary>
        /// 3rd row of the matrix.
        /// </summary>
        public float Row2X;

        /// <summary>
        /// 3rd row of the matrix.
        /// </summary>
        public float Row2Y;

        /// <summary>
        /// 3rd row of the matrix.
        /// </summary>
        public float Row2Z;

        /// <summary>
        /// 3rd row of the matrix.
        /// </summary>
        public float Row2W;

        /// <summary>
        /// Bottom row of the matrix.
        /// </summary>
        public float Row3X;

        /// <summary>
        /// Bottom row of the matrix.
        /// </summary>
        public float Row3Y;

        /// <summary>
        /// Bottom row of the matrix.
        /// </summary>
        public float Row3Z;

        /// <summary>
        /// Bottom row of the matrix.
        /// </summary>
        public float Row3W;

        /// <summary>
        /// The identity matrix.
        /// </summary>
        public static readonly Matrix4f Identity = new Matrix4f(Vector4f.UnitX, Vector4f.UnitY, Vector4f.UnitZ, Vector4f.UnitW);

        /// <summary>
        /// The zero matrix.
        /// </summary>
        public static readonly Matrix4f Zero = new Matrix4f(Vector4f.Zero, Vector4f.Zero, Vector4f.Zero, Vector4f.Zero);

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="row0">Top row of the matrix.</param>
        /// <param name="row1">Second row of the matrix.</param>
        /// <param name="row2">Third row of the matrix.</param>
        /// <param name="row3">Bottom row of the matrix.</param>
        public Matrix4f(Vector4f row0, Vector4f row1, Vector4f row2, Vector4f row3)
        {
            Row0X = row0.X;
            Row0Y = row0.Y;
            Row0Z = row0.Z;
            Row0W = row0.W;

            Row1X = row1.X;
            Row1Y = row1.Y;
            Row1Z = row1.Z;
            Row1W = row1.W;

            Row2X = row2.X;
            Row2Y = row2.Y;
            Row2Z = row2.Z;
            Row2W = row2.W;

            Row3X = row3.X;
            Row3Y = row3.Y;
            Row3Z = row3.Z;
            Row3W = row3.W;
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="m00">First item of the first row of the matrix.</param>
        /// <param name="m01">Second item of the first row of the matrix.</param>
        /// <param name="m02">Third item of the first row of the matrix.</param>
        /// <param name="m03">Fourth item of the first row of the matrix.</param>
        /// <param name="m10">First item of the second row of the matrix.</param>
        /// <param name="m11">Second item of the second row of the matrix.</param>
        /// <param name="m12">Third item of the second row of the matrix.</param>
        /// <param name="m13">Fourth item of the second row of the matrix.</param>
        /// <param name="m20">First item of the third row of the matrix.</param>
        /// <param name="m21">Second item of the third row of the matrix.</param>
        /// <param name="m22">Third item of the third row of the matrix.</param>
        /// <param name="m23">First item of the third row of the matrix.</param>
        /// <param name="m30">Fourth item of the fourth row of the matrix.</param>
        /// <param name="m31">Second item of the fourth row of the matrix.</param>
        /// <param name="m32">Third item of the fourth row of the matrix.</param>
        /// <param name="m33">Fourth item of the fourth row of the matrix.</param>
        public Matrix4f(
            float m00, float m01, float m02, float m03,
            float m10, float m11, float m12, float m13,
            float m20, float m21, float m22, float m23,
            float m30, float m31, float m32, float m33)
        {
            Row0X = m00;
            Row0Y = m01;
            Row0Z = m02;
            Row0W = m03;
            
            Row1X = m10;
            Row1Y = m11;
            Row1Z = m12;
            Row1W = m13;

            Row2X = m20;
            Row2Y = m21;
            Row2Z = m22;
            Row2W = m23;

            Row3X = m30;
            Row3Y = m31;
            Row3Z = m32;
            Row3W = m33;
        }
        
        #endregion

        #region Public Members

        #region Properties

        /// <summary>
        /// Gets the determinant of this matrix.
        /// </summary>
        public float Determinant
        {
            get
            {
                return
                    Row0X * Row1Y * Row2Z * Row3W - Row0X * Row1Y * Row2W * Row3Z + Row0X * Row1Z * Row2W * Row3Y - Row0X * Row1Z * Row2Y * Row3W
                  + Row0X * Row1W * Row2Y * Row3Z - Row0X * Row1W * Row2Z * Row3Y - Row0Y * Row1Z * Row2W * Row3X + Row0Y * Row1Z * Row2X * Row3W
                  - Row0Y * Row1W * Row2X * Row3Z + Row0Y * Row1W * Row2Z * Row3X - Row0Y * Row1X * Row2Z * Row3W + Row0Y * Row1X * Row2W * Row3Z
                  + Row0Z * Row1W * Row2X * Row3Y - Row0Z * Row1W * Row2Y * Row3X + Row0Z * Row1X * Row2Y * Row3W - Row0Z * Row1X * Row2W * Row3Y
                  + Row0Z * Row1Y * Row2W * Row3X - Row0Z * Row1Y * Row2X * Row3W - Row0W * Row1X * Row2Y * Row3Z + Row0W * Row1X * Row2Z * Row3Y
                  - Row0W * Row1Y * Row2Z * Row3X + Row0W * Row1Y * Row2X * Row3Z - Row0W * Row1Z * Row2X * Row3Y + Row0W * Row1Z * Row2Y * Row3X;
            }
        }

        /// <summary>
        /// Gets the first column of this matrix.
        /// </summary>
        public Vector4f Column0
        {
            get { return new Vector4f(Row0X, Row1X, Row2X, Row3X); }
            set { Row0X = value.X; Row1X = value.Y; Row2X = value.Z; Row3X = value.W; }
        }

        /// <summary>
        /// Gets the second column of this matrix.
        /// </summary>
        public Vector4f Column1
        {
            get { return new Vector4f(Row0Y, Row1Y, Row2Y, Row3Y); }
            set { Row0Y = value.X; Row1Y = value.Y; Row2Y = value.Z; Row3Y = value.W; }
        }

        /// <summary>
        /// Gets the third column of this matrix.
        /// </summary>
        public Vector4f Column2
        {
            get { return new Vector4f(Row0Z, Row1Z, Row2Z, Row3Z); }
            set { Row0Z = value.X; Row1Z = value.Y; Row2Z = value.Z; Row3Z = value.W; }
        }

        /// <summary>
        /// Gets the fourth column of this matrix.
        /// </summary>
        public Vector4f Column3
        {
            get { return new Vector4f(Row0W, Row1W, Row2W, Row3W); }
            set { Row0W = value.X; Row1W = value.Y; Row2W = value.Z; Row3W = value.W; }
        }

        /// <summary>
        /// Gets or sets the values along the main diagonal of the matrix.
        /// </summary>
        public Vector4f Diagonal
        {
            get
            {
                return new Vector4f(Row0X, Row1Y, Row2Z, Row3W);
            }
            set
            {
                Row0X = value.X;
                Row1Y = value.Y;
                Row2Z = value.Z;
                Row3W = value.W;
            }
        }

        /// <summary>
        /// Gets the trace of the matrix, the sum of the values along the diagonal.
        /// </summary>
        public float Trace { get { return Row0X + Row1Y + Row2Z + Row3W; } }

        #endregion

        #region Instance

        #region public void Invert()

        /// <summary>
        /// Converts this instance into its inverse.
        /// </summary>
        public void Invert()
        {
            this = Invert(this);
        }

        #endregion

        #region public void Transpose()

        /// <summary>
        /// Converts this instance into its transpose.
        /// </summary>
        public void Transpose()
        {
            this = Transpose(this);
        }

        #endregion

        /// <summary>
        /// Returns a normalised copy of this instance.
        /// </summary>
        public Matrix4f Normalized()
        {
            Matrix4f m = this;
            m.Normalize();
            return m;
        }

        /// <summary>
        /// Divides each element in the Matrix by the <see cref="Determinant"/>.
        /// </summary>
        public void Normalize()
        {
            var determinant = Determinant;
            Row0X /= determinant;
            Row0Y /= determinant;
            Row0Z /= determinant;
            Row0W /= determinant;

            Row1X /= determinant;
            Row1Y /= determinant;
            Row1Z /= determinant;
            Row1W /= determinant;

            Row2X /= determinant;
            Row2Y /= determinant;
            Row2Z /= determinant;
            Row2W /= determinant;

            Row3X /= determinant;
            Row3Y /= determinant;
            Row3Z /= determinant;
            Row3W /= determinant;
        }

        /// <summary>
        /// Returns an inverted copy of this instance.
        /// </summary>
        public Matrix4f Inverted()
        {
            Matrix4f m = this;
            if (m.Determinant != 0)
                m.Invert();
            return m;
        }

        /// <summary>
        /// Returns a copy of this Matrix4f without translation.
        /// </summary>
        public Matrix4f ClearTranslation()
        {
            Matrix4f m = this;
            m.Row3X = 0;
            m.Row3Y = 0;
            m.Row3Z = 0;
            return m;
        }
        ///// <summary>
        ///// Returns a copy of this Matrix4f without scale.
        ///// </summary>
        //public Matrix4f ClearScale()
        //{
        //    Matrix4f m = this;
        //    m.Row0.Xyz = m.Row0.Xyz.Normalized();
        //    m.Row1.Xyz = m.Row1.Xyz.Normalized();
        //    m.Row2.Xyz = m.Row2.Xyz.Normalized();
        //    return m;
        //}
        ///// <summary>
        ///// Returns a copy of this Matrix4f without rotation.
        ///// </summary>
        //public Matrix4f ClearRotation()
        //{
        //    Matrix4f m = this;
        //    m.Row0.Xyz = new Vector3f(m.Row0.Xyz.Length, 0, 0);
        //    m.Row1.Xyz = new Vector3f(0, m.Row1.Xyz.Length, 0);
        //    m.Row2.Xyz = new Vector3f(0, 0, m.Row2.Xyz.Length);
        //    return m;
        //}
        
        /// <summary>
        /// Returns a copy of this Matrix4f without projection.
        /// </summary>
        public Matrix4f ClearProjection()
        {
            Matrix4f m = this;
            m.Column3 = Vector4f.Zero;
            return m;
        }

        /// <summary>
        /// Returns the translation component of this instance.
        /// </summary>
        public Vector3f ExtractTranslation() { return new Vector3f(Row3X, Row3Y, Row3Z); }

        ///// <summary>
        ///// Returns the scale component of this instance.
        ///// </summary>
        //public Vector3f ExtractScale() { return new Vector3f(Row0.Xyz.Length, Row1.Xyz.Length, Row2.Xyz.Length); }

        /// <summary>
        /// Returns the projection component of this instance.
        /// </summary>
        public Vector4f ExtractProjection()
        {
            return Column3;
        }

        #endregion

        #region Static

        #region CreateFromAxisAngle

        /// <summary>
        /// Build a rotation matrix from the specified axis/angle rotation.
        /// </summary>
        /// <param name="axis">The axis to rotate about.</param>
        /// <param name="angle">Angle in radians to rotate counter-clockwise (looking in the direction of the given axis).</param>
        /// <param name="result">A matrix instance.</param>
        public static void CreateFromAxisAngle(Vector3f axis, float angle, out Matrix4f result)
        {
            // normalize and create a local copy of the vector.
            axis.Normalize();
            float axisX = axis.X, axisY = axis.Y, axisZ = axis.Z;

            // calculate angles
            float cos = (float)Math.Cos(-angle);
            float sin = (float)Math.Sin(-angle);
            float t = 1.0f - cos;

            // do the conversion math once
            float tXX = t * axisX * axisX,
                tXY = t * axisX * axisY,
                tXZ = t * axisX * axisZ,
                tYY = t * axisY * axisY,
                tYZ = t * axisY * axisZ,
                tZZ = t * axisZ * axisZ;

            float sinX = sin * axisX,
                sinY = sin * axisY,
                sinZ = sin * axisZ;

            result.Row0X = tXX + cos;
            result.Row0Y = tXY - sinZ;
            result.Row0Z = tXZ + sinY;
            result.Row0W = 0;
            result.Row1X = tXY + sinZ;
            result.Row1Y = tYY + cos;
            result.Row1Z = tYZ - sinX;
            result.Row1W = 0;
            result.Row2X = tXZ - sinY;
            result.Row2Y = tYZ + sinX;
            result.Row2Z = tZZ + cos;
            result.Row2W = 0;
            result.Row3X = 0;
            result.Row3Y = 0;
            result.Row3Z = 0;
            result.Row3W = 1;
        }

        /// <summary>
        /// Build a rotation matrix from the specified axis/angle rotation.
        /// </summary>
        /// <param name="axis">The axis to rotate about.</param>
        /// <param name="angle">Angle in radians to rotate counter-clockwise (looking in the direction of the given axis).</param>
        /// <returns>A matrix instance.</returns>
        public static Matrix4f CreateFromAxisAngle(Vector3f axis, float angle)
        {
            Matrix4f result;
            CreateFromAxisAngle(axis, angle, out result);
            return result;
        }

        #endregion

        #region CreateRotation[XYZ]

        /// <summary>
        /// Builds a rotation matrix for a rotation around the x-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <param name="result">The resulting Matrix4f instance.</param>
        public static void CreateRotationX(float angle, out Matrix4f result)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            result = Identity;
            result.Row1Y = cos;
            result.Row1Z = sin;
            result.Row2Y = -sin;
            result.Row2Z = cos;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the x-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <returns>The resulting Matrix4f instance.</returns>
        public static Matrix4f CreateRotationX(float angle)
        {
            Matrix4f result;
            CreateRotationX(angle, out result);
            return result;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the y-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <param name="result">The resulting Matrix4f instance.</param>
        public static void CreateRotationY(float angle, out Matrix4f result)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            result = Identity;
            result.Row0X = cos;
            result.Row0Z = -sin;
            result.Row2X = sin;
            result.Row2Z = cos;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the y-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <returns>The resulting Matrix4f instance.</returns>
        public static Matrix4f CreateRotationY(float angle)
        {
            Matrix4f result;
            CreateRotationY(angle, out result);
            return result;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the z-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <param name="result">The resulting Matrix4f instance.</param>
        public static void CreateRotationZ(float angle, out Matrix4f result)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            result = Identity;
            result.Row0X = cos;
            result.Row0Y = sin;
            result.Row1X = -sin;
            result.Row1Y = cos;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the z-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <returns>The resulting Matrix4f instance.</returns>
        public static Matrix4f CreateRotationZ(float angle)
        {
            Matrix4f result;
            CreateRotationZ(angle, out result);
            return result;
        }

        #endregion

        #region CreateTranslation

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="x">X translation.</param>
        /// <param name="y">Y translation.</param>
        /// <param name="z">Z translation.</param>
        /// <param name="result">The resulting Matrix4f instance.</param>
        public static void CreateTranslation(float x, float y, float z, out Matrix4f result)
        {
            result = Identity;
            result.Row3X = x;
            result.Row3Y = y;
            result.Row3Z = z;
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="vector">The translation vector.</param>
        /// <param name="result">The resulting Matrix4f instance.</param>
        public static void CreateTranslation(ref Vector3f vector, out Matrix4f result)
        {
            result = Identity;
            result.Row3X = vector.X;
            result.Row3Y = vector.Y;
            result.Row3Z = vector.Z;
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="x">X translation.</param>
        /// <param name="y">Y translation.</param>
        /// <param name="z">Z translation.</param>
        /// <returns>The resulting Matrix4f instance.</returns>
        public static Matrix4f CreateTranslation(float x, float y, float z)
        {
            Matrix4f result;
            CreateTranslation(x, y, z, out result);
            return result;
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="vector">The translation vector.</param>
        /// <returns>The resulting Matrix4f instance.</returns>
        public static Matrix4f CreateTranslation(Vector3f vector)
        {
            Matrix4f result;
            CreateTranslation(vector.X, vector.Y, vector.Z, out result);
            return result;
        }

        #endregion

        #region CreateScale

        /// <summary>
        /// Creates a scale matrix.
        /// </summary>
        /// <param name="scale">Single scale factor for the x, y, and z axes.</param>
        /// <returns>A scale matrix.</returns>
        public static Matrix4f CreateScale(float scale)
        {
            Matrix4f result;
            CreateScale(scale, out result);
            return result;
        }

        /// <summary>
        /// Creates a scale matrix.
        /// </summary>
        /// <param name="scale">Scale factors for the x, y, and z axes.</param>
        /// <returns>A scale matrix.</returns>
        public static Matrix4f CreateScale(Vector3f scale)
        {
            Matrix4f result;
            CreateScale(ref scale, out result);
            return result;
        }

        /// <summary>
        /// Creates a scale matrix.
        /// </summary>
        /// <param name="x">Scale factor for the x axis.</param>
        /// <param name="y">Scale factor for the y axis.</param>
        /// <param name="z">Scale factor for the z axis.</param>
        /// <returns>A scale matrix.</returns>
        public static Matrix4f CreateScale(float x, float y, float z)
        {
            Matrix4f result;
            CreateScale(x, y, z, out result);
            return result;
        }

        /// <summary>
        /// Creates a scale matrix.
        /// </summary>
        /// <param name="scale">Single scale factor for the x, y, and z axes.</param>
        /// <param name="result">A scale matrix.</param>
        public static void CreateScale(float scale, out Matrix4f result)
        {
            result = Identity;
            result.Row0X = scale;
            result.Row1Y = scale;
            result.Row2Z = scale;
        }

        /// <summary>
        /// Creates a scale matrix.
        /// </summary>
        /// <param name="scale">Scale factors for the x, y, and z axes.</param>
        /// <param name="result">A scale matrix.</param>
        public static void CreateScale(ref Vector3f scale, out Matrix4f result)
        {
            result = Identity;
            result.Row0X = scale.X;
            result.Row1Y = scale.Y;
            result.Row2Z = scale.Z;
        }

        /// <summary>
        /// Creates a scale matrix.
        /// </summary>
        /// <param name="x">Scale factor for the x axis.</param>
        /// <param name="y">Scale factor for the y axis.</param>
        /// <param name="z">Scale factor for the z axis.</param>
        /// <param name="result">A scale matrix.</param>
        public static void CreateScale(float x, float y, float z, out Matrix4f result)
        {
            result = Identity;
            result.Row0X = x;
            result.Row1Y = y;
            result.Row2Z = z;
        }

        #endregion

        #region CreateOrthographic

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="width">The width of the projection volume.</param>
        /// <param name="height">The height of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <param name="result">The resulting Matrix4f instance.</param>
        public static void CreateOrthographic(float width, float height, float zNear, float zFar, out Matrix4f result)
        {
            CreateOrthographicOffCenter(-width / 2, width / 2, -height / 2, height / 2, zNear, zFar, out result);
        }

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="width">The width of the projection volume.</param>
        /// <param name="height">The height of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <rereturns>The resulting Matrix4f instance.</rereturns>
        public static Matrix4f CreateOrthographic(float width, float height, float zNear, float zFar)
        {
            Matrix4f result;
            CreateOrthographicOffCenter(-width / 2, width / 2, -height / 2, height / 2, zNear, zFar, out result);
            return result;
        }

        #endregion

        #region CreateOrthographicOffCenter

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="left">The left edge of the projection volume.</param>
        /// <param name="right">The right edge of the projection volume.</param>
        /// <param name="bottom">The bottom edge of the projection volume.</param>
        /// <param name="top">The top edge of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <param name="result">The resulting Matrix4f instance.</param>
        public static void CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNear, float zFar, out Matrix4f result)
        {
            result = Identity;

            float invRL = 1.0f / (right - left);
            float invTB = 1.0f / (top - bottom);
            float invFN = 1.0f / (zFar - zNear);

            result.Row0X = 2 * invRL;
            result.Row1Y = 2 * invTB;
            result.Row2Z = -2 * invFN;

            result.Row3X = -(right + left) * invRL;
            result.Row3Y = -(top + bottom) * invTB;
            result.Row3Z = -(zFar + zNear) * invFN;
        }

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="left">The left edge of the projection volume.</param>
        /// <param name="right">The right edge of the projection volume.</param>
        /// <param name="bottom">The bottom edge of the projection volume.</param>
        /// <param name="top">The top edge of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <returns>The resulting Matrix4f instance.</returns>
        public static Matrix4f CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNear, float zFar)
        {
            Matrix4f result;
            CreateOrthographicOffCenter(left, right, bottom, top, zNear, zFar, out result);
            return result;
        }

        #endregion

        #region CreatePerspectiveFieldOfView

        /// <summary>
        /// Creates a perspective projection matrix.
        /// </summary>
        /// <param name="fovy">Angle of the field of view in the y direction (in radians)</param>
        /// <param name="aspect">Aspect ratio of the view (width / height)</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <param name="result">A projection matrix that transforms camera space to raster space</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>fovy is zero, less than zero or larger than Math.PI</item>
        /// <item>aspect is negative or zero</item>
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        public static void CreatePerspectiveFieldOfView(float fovy, float aspect, float zNear, float zFar, out Matrix4f result)
        {
            if (fovy <= 0 || fovy > Math.PI)
                throw new ArgumentOutOfRangeException("fovy");
            if (aspect <= 0)
                throw new ArgumentOutOfRangeException("aspect");
            if (zNear <= 0)
                throw new ArgumentOutOfRangeException("zNear");
            if (zFar <= 0)
                throw new ArgumentOutOfRangeException("zFar");

            float yMax = zNear * (float)Math.Tan(0.5f * fovy);
            float yMin = -yMax;
            float xMin = yMin * aspect;
            float xMax = yMax * aspect;

            CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar, out result);
        }

        /// <summary>
        /// Creates a perspective projection matrix.
        /// </summary>
        /// <param name="fovy">Angle of the field of view in the y direction (in radians)</param>
        /// <param name="aspect">Aspect ratio of the view (width / height)</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <returns>A projection matrix that transforms camera space to raster space</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>fovy is zero, less than zero or larger than Math.PI</item>
        /// <item>aspect is negative or zero</item>
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        public static Matrix4f CreatePerspectiveFieldOfView(float fovy, float aspect, float zNear, float zFar)
        {
            Matrix4f result;
            CreatePerspectiveFieldOfView(fovy, aspect, zNear, zFar, out result);
            return result;
        }

        #endregion

        #region CreatePerspectiveOffCenter

        /// <summary>
        /// Creates an perspective projection matrix.
        /// </summary>
        /// <param name="left">Left edge of the view frustum</param>
        /// <param name="right">Right edge of the view frustum</param>
        /// <param name="bottom">Bottom edge of the view frustum</param>
        /// <param name="top">Top edge of the view frustum</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <param name="result">A projection matrix that transforms camera space to raster space</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        public static void CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float zNear, float zFar, out Matrix4f result)
        {
            if (zNear <= 0)
                throw new ArgumentOutOfRangeException("zNear");
            if (zFar <= 0)
                throw new ArgumentOutOfRangeException("zFar");
            if (zNear >= zFar)
                throw new ArgumentOutOfRangeException("zNear");

            float x = (2.0f * zNear) / (right - left);
            float y = (2.0f * zNear) / (top - bottom);
            float a = (right + left) / (right - left);
            float b = (top + bottom) / (top - bottom);
            float c = -(zFar + zNear) / (zFar - zNear);
            float d = -(2.0f * zFar * zNear) / (zFar - zNear);

            result.Row0X = x;
            result.Row0Y = 0;
            result.Row0Z = 0;
            result.Row0W = 0;
            result.Row1X = 0;
            result.Row1Y = y;
            result.Row1Z = 0;
            result.Row1W = 0;
            result.Row2X = a;
            result.Row2Y = b;
            result.Row2Z = c;
            result.Row2W = -1;
            result.Row3X = 0;
            result.Row3Y = 0;
            result.Row3Z = d;
            result.Row3W = 0;
        }

        /// <summary>
        /// Creates an perspective projection matrix.
        /// </summary>
        /// <param name="left">Left edge of the view frustum</param>
        /// <param name="right">Right edge of the view frustum</param>
        /// <param name="bottom">Bottom edge of the view frustum</param>
        /// <param name="top">Top edge of the view frustum</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <returns>A projection matrix that transforms camera space to raster space</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        public static Matrix4f CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float zNear, float zFar)
        {
            Matrix4f result;
            CreatePerspectiveOffCenter(left, right, bottom, top, zNear, zFar, out result);
            return result;
        }

        #endregion

        #region Camera Helper Functions

        /// <summary>
        /// Build a world space to camera space matrix
        /// </summary>
        /// <param name="eye">Eye (camera) position in world space</param>
        /// <param name="target">Target position in world space</param>
        /// <param name="up">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <returns>A Matrix4f that transforms world space to camera space</returns>
        public static Matrix4f LookAt(Vector3f eye, Vector3f target, Vector3f up)
        {
            Matrix4f result;
            LookAt(eye, target, up, out result);
            return result;
        }

        /// <summary>
        /// Build a world space to camera space matrix
        /// </summary>
        /// <param name="eye">Eye (camera) position in world space</param>
        /// <param name="target">Target position in world space</param>
        /// <param name="up">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <param name="result">A matrix that transforms world space to camera space matrix</param>
        /// <returns>A Matrix4f that transforms world space to camera space</returns>
        public static void LookAt(Vector3f eye, Vector3f target, Vector3f up, out Matrix4f result)
        {
            Vector3f z = Vector3f.Normalize(eye - target);
            Vector3f x = Vector3f.Normalize(Vector3f.Cross(up, z));
            Vector3f y = Vector3f.Normalize(Vector3f.Cross(z, x));

            result.Row0X = x.X;
            result.Row0Y = y.X;
            result.Row0Z = z.X;
            result.Row0W = 0;
            result.Row1X = x.Y;
            result.Row1Y = y.Y;
            result.Row1Z = z.Y;
            result.Row1W = 0;
            result.Row2X = x.Z;
            result.Row2Y = y.Z;
            result.Row2Z = z.Z;
            result.Row2W = 0;
            result.Row3X = -((x.X * eye.X) + (x.Y * eye.Y) + (x.Z * eye.Z));
            result.Row3Y = -((y.X * eye.X) + (y.Y * eye.Y) + (y.Z * eye.Z));
            result.Row3Z = -((z.X * eye.X) + (z.Y * eye.Y) + (z.Z * eye.Z));
            result.Row3W = 1;
        }

        /// <summary>
        /// Build a world space to camera space matrix
        /// </summary>
        /// <param name="eyeX">Eye (camera) position in world space</param>
        /// <param name="eyeY">Eye (camera) position in world space</param>
        /// <param name="eyeZ">Eye (camera) position in world space</param>
        /// <param name="targetX">Target position in world space</param>
        /// <param name="targetY">Target position in world space</param>
        /// <param name="targetZ">Target position in world space</param>
        /// <param name="upX">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <param name="upY">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <param name="upZ">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <returns>A Matrix4f that transforms world space to camera space</returns>
        public static Matrix4f LookAt(float eyeX, float eyeY, float eyeZ, float targetX, float targetY, float targetZ, float upX, float upY, float upZ)
        {
            return LookAt(new Vector3f(eyeX, eyeY, eyeZ), new Vector3f(targetX, targetY, targetZ), new Vector3f(upX, upY, upZ));
        }

        #endregion

        #region Add Functions

        /// <summary>
        /// Adds two instances.
        /// </summary>
        /// <param name="left">The left operand of the addition.</param>
        /// <param name="right">The right operand of the addition.</param>
        /// <returns>A new instance that is the result of the addition.</returns>
        public static Matrix4f Add(Matrix4f left, Matrix4f right)
        {
            Matrix4f result;
            Add(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Adds two instances.
        /// </summary>
        /// <param name="left">The left operand of the addition.</param>
        /// <param name="right">The right operand of the addition.</param>
        /// <param name="result">A new instance that is the result of the addition.</param>
        public static void Add(ref Matrix4f left, ref Matrix4f right, out Matrix4f result)
        {
            result.Row0X = left.Row0X + right.Row0X;
            result.Row0Y = left.Row0Y + right.Row0Y;
            result.Row0Z = left.Row0Z + right.Row0Z;
            result.Row0W = left.Row0W + right.Row0W;

            result.Row1X = left.Row1X + right.Row1X;
            result.Row1Y = left.Row1Y + right.Row1Y;
            result.Row1Z = left.Row1Z + right.Row1Z;
            result.Row1W = left.Row1W + right.Row1W;

            result.Row2X = left.Row2X + right.Row2X;
            result.Row2Y = left.Row2Y + right.Row2Y;
            result.Row2Z = left.Row2Z + right.Row2Z;
            result.Row2W = left.Row2W + right.Row2W;

            result.Row3X = left.Row3X + right.Row3X;
            result.Row3Y = left.Row3Y + right.Row3Y;
            result.Row3Z = left.Row3Z + right.Row3Z;
            result.Row3W = left.Row3W + right.Row3W;
        }

        #endregion

        #region Subtract Functions

        /// <summary>
        /// Subtracts one instance from another.
        /// </summary>
        /// <param name="left">The left operand of the subraction.</param>
        /// <param name="right">The right operand of the subraction.</param>
        /// <returns>A new instance that is the result of the subraction.</returns>
        public static Matrix4f Subtract(Matrix4f left, Matrix4f right)
        {
            Matrix4f result;
            Subtract(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Subtracts one instance from another.
        /// </summary>
        /// <param name="left">The left operand of the subraction.</param>
        /// <param name="right">The right operand of the subraction.</param>
        /// <param name="result">A new instance that is the result of the subraction.</param>
        public static void Subtract(ref Matrix4f left, ref Matrix4f right, out Matrix4f result)
        {
            result.Row0X = left.Row0X - right.Row0X;
            result.Row0Y = left.Row0Y - right.Row0Y;
            result.Row0Z = left.Row0Z - right.Row0Z;
            result.Row0W = left.Row0W - right.Row0W;

            result.Row1X = left.Row1X - right.Row1X;
            result.Row1Y = left.Row1Y - right.Row1Y;
            result.Row1Z = left.Row1Z - right.Row1Z;
            result.Row1W = left.Row1W - right.Row1W;

            result.Row2X = left.Row2X - right.Row2X;
            result.Row2Y = left.Row2Y - right.Row2Y;
            result.Row2Z = left.Row2Z - right.Row2Z;
            result.Row2W = left.Row2W - right.Row2W;

            result.Row3X = left.Row3X - right.Row3X;
            result.Row3Y = left.Row3Y - right.Row3Y;
            result.Row3Z = left.Row3Z - right.Row3Z;
            result.Row3W = left.Row3W - right.Row3W;
        }

        #endregion

        #region Multiply Functions

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The left operand of the multiplication.</param>
        /// <param name="right">The right operand of the multiplication.</param>
        /// <returns>A new instance that is the result of the multiplication.</returns>
        public static Matrix4f Mult(Matrix4f left, Matrix4f right)
        {
            Matrix4f result;
            Mult(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The left operand of the multiplication.</param>
        /// <param name="right">The right operand of the multiplication.</param>
        /// <param name="result">A new instance that is the result of the multiplication.</param>
        public static void Mult(ref Matrix4f left, ref Matrix4f right, out Matrix4f result)
        {
            float lM11 = left.Row0X, lM12 = left.Row0Y, lM13 = left.Row0Z, lM14 = left.Row0W,
                lM21 = left.Row1X, lM22 = left.Row1Y, lM23 = left.Row1Z, lM24 = left.Row1W,
                lM31 = left.Row2X, lM32 = left.Row2Y, lM33 = left.Row2Z, lM34 = left.Row2W,
                lM41 = left.Row3X, lM42 = left.Row3Y, lM43 = left.Row3Z, lM44 = left.Row3W,
                rM11 = right.Row0X, rM12 = right.Row0Y, rM13 = right.Row0Z, rM14 = right.Row0W,
                rM21 = right.Row1X, rM22 = right.Row1Y, rM23 = right.Row1Z, rM24 = right.Row1W,
                rM31 = right.Row2X, rM32 = right.Row2Y, rM33 = right.Row2Z, rM34 = right.Row2W,
                rM41 = right.Row3X, rM42 = right.Row3Y, rM43 = right.Row3Z, rM44 = right.Row3W;

            result.Row0X = (((lM11 * rM11) + (lM12 * rM21)) + (lM13 * rM31)) + (lM14 * rM41);
            result.Row0Y = (((lM11 * rM12) + (lM12 * rM22)) + (lM13 * rM32)) + (lM14 * rM42);
            result.Row0Z = (((lM11 * rM13) + (lM12 * rM23)) + (lM13 * rM33)) + (lM14 * rM43);
            result.Row0W = (((lM11 * rM14) + (lM12 * rM24)) + (lM13 * rM34)) + (lM14 * rM44);
            result.Row1X = (((lM21 * rM11) + (lM22 * rM21)) + (lM23 * rM31)) + (lM24 * rM41);
            result.Row1Y = (((lM21 * rM12) + (lM22 * rM22)) + (lM23 * rM32)) + (lM24 * rM42);
            result.Row1Z = (((lM21 * rM13) + (lM22 * rM23)) + (lM23 * rM33)) + (lM24 * rM43);
            result.Row1W = (((lM21 * rM14) + (lM22 * rM24)) + (lM23 * rM34)) + (lM24 * rM44);
            result.Row2X = (((lM31 * rM11) + (lM32 * rM21)) + (lM33 * rM31)) + (lM34 * rM41);
            result.Row2Y = (((lM31 * rM12) + (lM32 * rM22)) + (lM33 * rM32)) + (lM34 * rM42);
            result.Row2Z = (((lM31 * rM13) + (lM32 * rM23)) + (lM33 * rM33)) + (lM34 * rM43);
            result.Row2W = (((lM31 * rM14) + (lM32 * rM24)) + (lM33 * rM34)) + (lM34 * rM44);
            result.Row3X = (((lM41 * rM11) + (lM42 * rM21)) + (lM43 * rM31)) + (lM44 * rM41);
            result.Row3Y = (((lM41 * rM12) + (lM42 * rM22)) + (lM43 * rM32)) + (lM44 * rM42);
            result.Row3Z = (((lM41 * rM13) + (lM42 * rM23)) + (lM43 * rM33)) + (lM44 * rM43);
            result.Row3W = (((lM41 * rM14) + (lM42 * rM24)) + (lM43 * rM34)) + (lM44 * rM44);
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="left">The left operand of the multiplication.</param>
        /// <param name="right">The right operand of the multiplication.</param>
        /// <returns>A new instance that is the result of the multiplication</returns>
        public static Matrix4f Mult(Matrix4f left, float right)
        {
            Matrix4f result;
            Mult(ref left, right, out result);
            return result;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="left">The left operand of the multiplication.</param>
        /// <param name="right">The right operand of the multiplication.</param>
        /// <param name="result">A new instance that is the result of the multiplication</param>
        public static void Mult(ref Matrix4f left, float right, out Matrix4f result)
        {
            result.Row0X = left.Row0X * right;
            result.Row0Y = left.Row0Y * right;
            result.Row0Z = left.Row0Z * right;
            result.Row0W = left.Row0W * right;

            result.Row1X = left.Row1X * right;
            result.Row1Y = left.Row1Y * right;
            result.Row1Z = left.Row1Z * right;
            result.Row1W = left.Row1W * right;

            result.Row2X = left.Row2X * right;
            result.Row2Y = left.Row2Y * right;
            result.Row2Z = left.Row2Z * right;
            result.Row2W = left.Row2W * right;

            result.Row3X = left.Row3X * right;
            result.Row3Y = left.Row3Y * right;
            result.Row3Z = left.Row3Z * right;
            result.Row3W = left.Row3W * right;
        }

        #endregion

        #region Invert Functions

        /// <summary>
        /// Calculate the inverse of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to invert</param>
        /// <param name="result">The inverse of the given matrix if it has one, or the input if it is singular</param>
        /// <exception cref="InvalidOperationException">Thrown if the Matrix4f is singular.</exception>
        public static void Invert(ref Matrix4f mat, out Matrix4f result)
        {
            int[] colIdx = { 0, 0, 0, 0 };
            int[] rowIdx = { 0, 0, 0, 0 };
            int[] pivotIdx = { -1, -1, -1, -1 };

            // convert the matrix to an array for easy looping
            float[,] inverse = {{mat.Row0X, mat.Row0Y, mat.Row0Z, mat.Row0W}, 
                                {mat.Row1X, mat.Row1Y, mat.Row1Z, mat.Row1W}, 
                                {mat.Row2X, mat.Row2Y, mat.Row2Z, mat.Row2W}, 
                                {mat.Row3X, mat.Row3Y, mat.Row3Z, mat.Row3W} };
            int icol = 0;
            int irow = 0;
            for (int i = 0; i < 4; i++)
            {
                // Find the largest pivot value
                float maxPivot = 0.0f;
                for (int j = 0; j < 4; j++)
                {
                    if (pivotIdx[j] != 0)
                    {
                        for (int k = 0; k < 4; ++k)
                        {
                            if (pivotIdx[k] == -1)
                            {
                                float absVal = Math.Abs(inverse[j, k]);
                                if (absVal > maxPivot)
                                {
                                    maxPivot = absVal;
                                    irow = j;
                                    icol = k;
                                }
                            }
                            else if (pivotIdx[k] > 0)
                            {
                                result = mat;
                                return;
                            }
                        }
                    }
                }

                ++(pivotIdx[icol]);

                // Swap rows over so pivot is on diagonal
                if (irow != icol)
                {
                    for (int k = 0; k < 4; ++k)
                    {
                        float f = inverse[irow, k];
                        inverse[irow, k] = inverse[icol, k];
                        inverse[icol, k] = f;
                    }
                }

                rowIdx[i] = irow;
                colIdx[i] = icol;

                float pivot = inverse[icol, icol];
                // check for singular matrix
                if (pivot == 0.0f)
                {
                    throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
                }

                // Scale row so it has a unit diagonal
                float oneOverPivot = 1.0f / pivot;
                inverse[icol, icol] = 1.0f;
                for (int k = 0; k < 4; ++k)
                    inverse[icol, k] *= oneOverPivot;

                // Do elimination of non-diagonal elements
                for (int j = 0; j < 4; ++j)
                {
                    // check this isn't on the diagonal
                    if (icol != j)
                    {
                        float f = inverse[j, icol];
                        inverse[j, icol] = 0.0f;
                        for (int k = 0; k < 4; ++k)
                            inverse[j, k] -= inverse[icol, k] * f;
                    }
                }
            }

            for (int j = 3; j >= 0; --j)
            {
                int ir = rowIdx[j];
                int ic = colIdx[j];
                for (int k = 0; k < 4; ++k)
                {
                    float f = inverse[k, ir];
                    inverse[k, ir] = inverse[k, ic];
                    inverse[k, ic] = f;
                }
            }

            result.Row0X = inverse[0, 0];
            result.Row0Y = inverse[0, 1];
            result.Row0Z = inverse[0, 2];
            result.Row0W = inverse[0, 3];
            result.Row1X = inverse[1, 0];
            result.Row1Y = inverse[1, 1];
            result.Row1Z = inverse[1, 2];
            result.Row1W = inverse[1, 3];
            result.Row2X = inverse[2, 0];
            result.Row2Y = inverse[2, 1];
            result.Row2Z = inverse[2, 2];
            result.Row2W = inverse[2, 3];
            result.Row3X = inverse[3, 0];
            result.Row3Y = inverse[3, 1];
            result.Row3Z = inverse[3, 2];
            result.Row3W = inverse[3, 3];
        }

        /// <summary>
        /// Calculate the inverse of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to invert</param>
        /// <returns>The inverse of the given matrix if it has one, or the input if it is singular</returns>
        /// <exception cref="InvalidOperationException">Thrown if the Matrix4f is singular.</exception>
        public static Matrix4f Invert(Matrix4f mat)
        {
            Matrix4f result;
            Invert(ref mat, out result);
            return result;
        }

        #endregion

        #region Transpose

        /// <summary>
        /// Calculate the transpose of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to transpose</param>
        /// <returns>The transpose of the given matrix</returns>
        public static Matrix4f Transpose(Matrix4f mat)
        {
            Matrix4f result;
            Transpose(ref mat, out result);
            return result;
        }


        /// <summary>
        /// Calculate the transpose of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to transpose</param>
        /// <param name="result">The result of the calculation</param>
        public static void Transpose(ref Matrix4f mat, out Matrix4f result)
        {
            result.Row0X = mat.Row0X;
            result.Row0Y = mat.Row1X;
            result.Row0Z = mat.Row2X;
            result.Row0W = mat.Row3X;

            result.Row1X = mat.Row0Y;
            result.Row1Y = mat.Row1Y;
            result.Row1Z = mat.Row2Y;
            result.Row1W = mat.Row3Y;

            result.Row2X = mat.Row0Z;
            result.Row2Y = mat.Row1Z;
            result.Row2Z = mat.Row2Z;
            result.Row2W = mat.Row3Z;

            result.Row3X = mat.Row0W;
            result.Row3Y = mat.Row1W;
            result.Row3Z = mat.Row2W;
            result.Row3W = mat.Row3W;
        }

        #endregion

        #endregion

        #region Operators

        /// <summary>
        /// Matrix multiplication
        /// </summary>
        /// <param name="left">left-hand operand</param>
        /// <param name="right">right-hand operand</param>
        /// <returns>A new Matrix4f which holds the result of the multiplication</returns>
        public static Matrix4f operator *(Matrix4f left, Matrix4f right)
        {
            return Mult(left, right);
        }

        /// <summary>
        /// Matrix-scalar multiplication
        /// </summary>
        /// <param name="left">left-hand operand</param>
        /// <param name="right">right-hand operand</param>
        /// <returns>A new Matrix4f which holds the result of the multiplication</returns>
        public static Matrix4f operator *(Matrix4f left, float right)
        {
            return Mult(left, right);
        }

        /// <summary>
        /// Matrix addition
        /// </summary>
        /// <param name="left">left-hand operand</param>
        /// <param name="right">right-hand operand</param>
        /// <returns>A new Matrix4f which holds the result of the addition</returns>
        public static Matrix4f operator +(Matrix4f left, Matrix4f right)
        {
            return Add(left, right);
        }

        /// <summary>
        /// Matrix subtraction
        /// </summary>
        /// <param name="left">left-hand operand</param>
        /// <param name="right">right-hand operand</param>
        /// <returns>A new Matrix4f which holds the result of the subtraction</returns>
        public static Matrix4f operator -(Matrix4f left, Matrix4f right)
        {
            return Subtract(left, right);
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left equals right; false otherwise.</returns>
        public static bool operator ==(Matrix4f left, Matrix4f right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left does not equal right; false otherwise.</returns>
        public static bool operator !=(Matrix4f left, Matrix4f right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a pointer to the first element of the specified instance.
        /// </summary>
        /// <param name="mat">The instance.</param>
        /// <returns>A pointer to the first element of mat.</returns>
        unsafe public static explicit operator float*(Matrix4f mat)
        {
            return &mat.Row0X;
        }

        #endregion

        #region Overrides

        #region public override string ToString()

        /// <summary>
        /// Returns a System.String that represents the current Matrix4f.
        /// </summary>
        /// <returns>The string representation of the matrix.</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('(').Append(Row0X).Append(", ").Append(Row0Y).Append(", ").Append(Row0Z).Append(", ").Append(Row0W).Append(")\n");
            builder.Append('(').Append(Row1X).Append(", ").Append(Row1Y).Append(", ").Append(Row1Z).Append(", ").Append(Row1W).Append(")\n");
            builder.Append('(').Append(Row2X).Append(", ").Append(Row2Y).Append(", ").Append(Row2Z).Append(", ").Append(Row2W).Append(")\n");
            builder.Append('(').Append(Row3X).Append(", ").Append(Row3Y).Append(", ").Append(Row3Z).Append(", ").Append(Row3W).Append(')');
            return builder.ToString();
        }

        #endregion

        #region public override int GetHashCode()

        /// <summary>
        /// Returns the hashcode for this instance.
        /// </summary>
        /// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
        public override int GetHashCode()
        {
            return (Row0X.GetHashCode() ^ Row0Y.GetHashCode() ^ Row0Z.GetHashCode() ^ Row0W.GetHashCode()) 
                ^ (Row1X.GetHashCode() ^ Row1Y.GetHashCode() ^ Row1Z.GetHashCode() ^ Row1W.GetHashCode())
                ^ (Row2X.GetHashCode() ^ Row2Y.GetHashCode() ^ Row2Z.GetHashCode() ^ Row2W.GetHashCode())
                ^ (Row3X.GetHashCode() ^ Row3Y.GetHashCode() ^ Row3Z.GetHashCode() ^ Row3W.GetHashCode());
        }

        #endregion

        #region public override bool Equals(object obj)

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare tresult.</param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Matrix4f))
                return false;

            return Equals((Matrix4f)obj);
        }

        #endregion

        #endregion

        #endregion

        #region IEquatable<Matrix4f> Members

        /// <summary>Indicates whether the current matrix is equal to another matrix.</summary>
        /// <param name="other">An matrix to compare with this matrix.</param>
        /// <returns>true if the current matrix is equal to the matrix parameter; otherwise, false.</returns>
        public bool Equals(Matrix4f other)
        {
            return
                Row0X == other.Row0X &&
                Row0Y == other.Row0Y &&
                Row0Z == other.Row0Z &&
                Row0W == other.Row0W &&
                Row1X == other.Row1X &&
                Row1Y == other.Row1Y &&
                Row1Z == other.Row1Z &&
                Row1W == other.Row1W &&
                Row2X == other.Row2X &&
                Row2Y == other.Row2Y &&
                Row2Z == other.Row2Z &&
                Row2W == other.Row2W &&
                Row3X == other.Row3X &&
                Row3Y == other.Row3Y &&
                Row3Z == other.Row3Z &&
                Row3W == other.Row3W;
        }

        #endregion
    }
}