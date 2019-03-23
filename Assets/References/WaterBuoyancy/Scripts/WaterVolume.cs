using UnityEngine;
using System.Collections.Generic;
using WaterBuoyancy.Collections;

namespace WaterBuoyancy
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(MeshFilter))]
    public class WaterVolume : MonoBehaviour
    {
        public const string TAG = "Water Volume";

        [SerializeField]
        private float density = 1f;

        [SerializeField]
        private int rows = 10;

        [SerializeField]
        private int columns = 10;

        [SerializeField]
        private float quadSegmentSize = 1f;

        private Mesh mesh;
        private Vector3[] meshLocalVertices;
        private Vector3[] meshWorldVertices;

        public float Density
        {
            get
            {
                return this.density;
            }
        }

        public int Rows
        {
            get
            {
                return this.rows;
            }
        }

        public int Columns
        {
            get
            {
                return this.columns;
            }
        }

        public float QuadSegmentSize
        {
            get
            {
                return this.quadSegmentSize;
            }
        }

        public Mesh Mesh
        {
            get
            {
                if (this.mesh == null)
                {
                    this.mesh = this.GetComponent<MeshFilter>().mesh;
                }

                return this.mesh;
            }
        }

        protected virtual void Awake()
        {
            this.CacheMeshVertices();
        }

        protected virtual void Update()
        {
            this.CacheMeshVertices();
        }


        public Vector3[] GetSurroundingTrianglePolygon(Vector3 worldPoint)
        {
            Vector3 localPoint = this.transform.InverseTransformPoint(worldPoint);
            int x = Mathf.CeilToInt(localPoint.x / this.QuadSegmentSize);
            int z = Mathf.CeilToInt(localPoint.z / this.QuadSegmentSize);
            if (x <= 0 || z <= 0 || x >= (this.Columns + 1) || z >= (this.Rows + 1))
            {
                return null;
            }

            Vector3[] trianglePolygon = new Vector3[3];
            if ((worldPoint - this.meshWorldVertices[this.GetIndex(z, x)]).sqrMagnitude <
                ((worldPoint - this.meshWorldVertices[this.GetIndex(z - 1, x - 1)]).sqrMagnitude))
            {
                trianglePolygon[0] = this.meshWorldVertices[this.GetIndex(z, x)];
            }
            else
            {
                trianglePolygon[0] = this.meshWorldVertices[this.GetIndex(z - 1, x - 1)];
            }

            trianglePolygon[1] = this.meshWorldVertices[this.GetIndex(z - 1, x)];
            trianglePolygon[2] = this.meshWorldVertices[this.GetIndex(z, x - 1)];

            return trianglePolygon;
        }


        public Vector3 GetSurfaceNormal(Vector3 worldPoint)
        {
            Vector3[] meshPolygon = this.GetSurroundingTrianglePolygon(worldPoint);
            if (meshPolygon != null)
            {
                Vector3 planeV1 = meshPolygon[1] - meshPolygon[0];
                Vector3 planeV2 = meshPolygon[2] - meshPolygon[0];
                Vector3 planeNormal = Vector3.Cross(planeV1, planeV2).normalized;
                if (planeNormal.y < 0f)
                {
                    planeNormal *= -1f;
                }

                return planeNormal;
            }

            return this.transform.up;
        }

        public float GetWaterLevel(Vector3 worldPoint)
        {
            Vector3[] meshPolygon = this.GetSurroundingTrianglePolygon(worldPoint);
            if (meshPolygon != null)
            {
                Vector3 planeV1 = meshPolygon[1] - meshPolygon[0];
                Vector3 planeV2 = meshPolygon[2] - meshPolygon[0];
                Vector3 planeNormal = Vector3.Cross(planeV1, planeV2).normalized;
                if (planeNormal.y < 0f)
                {
                    planeNormal *= -1f;
                }

                // Plane equation
                float yOnWaterSurface = (-(worldPoint.x * planeNormal.x) - (worldPoint.z * planeNormal.z) + Vector3.Dot(meshPolygon[0], planeNormal)) / planeNormal.y;

                // Vector3 pointOnWaterSurface = new Vector3(worldPoint.x, yOnWaterSurface, worldPoint.z);
                // DebugUtils.DrawPoint(pointOnWaterSurface, Color.magenta);

                return yOnWaterSurface;
            }

            return this.transform.position.y;
        }


        private int GetIndex(int row, int column)
        {
            return row * (this.Columns + 1) + column;
        }

        private void CacheMeshVertices()
        {
            this.meshLocalVertices = this.Mesh.vertices;
            this.meshWorldVertices = this.ConvertPointsToWorldSpace(meshLocalVertices);
        }

        private Vector3[] ConvertPointsToWorldSpace(Vector3[] localPoints)
        {
            Vector3[] worldPoints = new Vector3[localPoints.Length];
            for (int i = 0; i < localPoints.Length; i++)
            {
                worldPoints[i] = this.transform.TransformPoint(localPoints[i]);
            }

            return worldPoints;
        }
    }
}
