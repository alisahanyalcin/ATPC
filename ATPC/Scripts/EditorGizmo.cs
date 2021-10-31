using UnityEngine;
using UnityEditor;

namespace alisahanyalcin
{
    public class EditorGizmo : MonoBehaviour
    {
        public enum GizmoShape { None, Cube, Sphere, Texture, WireCube, WireSphere }
        public GizmoShape shapes;

        [Range(0f, 10f)][Header("Gizmo Size")]
        public float gizmoSize;

        [Header("Gizmo Color")]
        public Color gizColor = new Color(1, 1, 1, 1);

        [Header("Gizmo Texture")]
        public Texture2D gizTexture;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(gizColor.r, gizColor.g, gizColor.b, gizColor.a);
            switch (shapes)
            {
                case GizmoShape.Cube:
                    Gizmos.DrawCube(transform.position, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                    break;
                case GizmoShape.Sphere:
                    Gizmos.DrawSphere(transform.position, gizmoSize);
                    break;
                case GizmoShape.Texture:
                    if (gizTexture != null)
                    {
                        Gizmos.DrawGUITexture(
                            new Rect(transform.position.x, transform.position.y, (float) gizmoSize, (float) gizmoSize),
                            gizTexture);
                    }
                    break;
                case GizmoShape.WireCube:
                    Gizmos.DrawWireCube(transform.position,new Vector3(gizmoSize, gizmoSize, gizmoSize));
                    break;
                case GizmoShape.WireSphere:
                    Gizmos.DrawWireSphere(transform.position, gizmoSize);
                    break;
            }
        }
    }
}