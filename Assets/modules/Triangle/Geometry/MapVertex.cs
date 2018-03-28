// -----------------------------------------------------------------------
// <copyright file="Vertex.cs" company="">
// Original Triangle code by Jonathan Richard Shewchuk, http://www.cs.cmu.edu/~quake/triangle.html
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet.Geometry {
  using System;
  using UnityEngine;
  using TriangleNet.Topology;

  /// <summary>
  /// The vertex data structure.
  /// </summary>
  public class MapVertex : Vertex {

    public Hex hex = null;
    public bool isRiver = false;
    /// <summary>
    /// Initializes a new instance of the <see cref="Vertex" /> class.
    /// </summary>
    public MapVertex()
        : base(0, 0, 0) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Vertex" /> class.
    /// </summary>
    /// <param name="x">The x coordinate of the vertex.</param>
    /// <param name="y">The y coordinate of the vertex.</param>
    public MapVertex(double x, double y)
        : base(x, y, 0) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Vertex" /> class.
    /// </summary>
    /// <param name="x">The x coordinate of the vertex.</param>
    /// <param name="y">The y coordinate of the vertex.</param>
    public MapVertex(double x, double y, Hex hex)
        : base(x, y, 0) {
      this.hex = hex;
    }

    public MapVertex(double x, double y, Hex hex, bool isRiver)
      : base(x, y, 0) {
      this.hex = hex;
      this.isRiver = isRiver;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Vertex" /> class.
    /// </summary>
    /// <param name="x">The x coordinate of the vertex.</param>
    /// <param name="y">The y coordinate of the vertex.</param>
    /// <param name="mark">The boundary mark.</param>
    public MapVertex(double x, double y, int mark)
        : base(x, y, mark) {
      this.type = VertexType.InputVertex;
    }

    public Vector3 toVector3() {
      return new Vector3((float)this.x, (float)this.y);
    }

    public override int GetHashCode() {
      return this.hash;
    }
  }
}
