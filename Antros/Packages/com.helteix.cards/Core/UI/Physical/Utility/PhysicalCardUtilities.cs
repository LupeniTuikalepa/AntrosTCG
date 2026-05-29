using UnityEngine;

namespace Helteix.Cards.UI.Utility
{
    public static class PhysicalCardUtilities
    {
        private static readonly Vector3[] CornersA = new Vector3[4];
        private static readonly Vector3[] CornersB = new Vector3[4];

        public static Vector3 ConstraintScreenPosInsideRect(Vector3 position, RectTransform constraint, Canvas canvas = null)
        {
            if(constraint == null)
                return position;

            Camera camera = canvas == null ? Camera.main : canvas.worldCamera;

            if (camera == null)
                return position;

            RenderMode renderMode = canvas == null ? RenderMode.WorldSpace : canvas.renderMode;
            constraint.GetWorldCorners(CornersA);
            Rect rect = new Rect()
            {
                min = renderMode == RenderMode.ScreenSpaceOverlay ? CornersA[0] : camera.WorldToScreenPoint(CornersA[0]),
                max = renderMode == RenderMode.ScreenSpaceOverlay ? CornersA[2] : camera.WorldToScreenPoint(CornersA[2]),
            };

            position.x = Mathf.Clamp(position.x, rect.xMin, rect.xMax);
            position.y = Mathf.Clamp(position.y, rect.yMin, rect.yMax);
            return position;

        }

        public static Vector3 ConstraintWorldPosInsideRect(Vector3 position, RectTransform constraint)
        {
            constraint.GetWorldCorners(CornersA);
            Bounds bounds = GeometryUtility.CalculateBounds(CornersA, Matrix4x4.identity);
            if (!bounds.Contains(position))
                position = bounds.ClosestPoint(position);

            return position;
        }

        public static void ConstraintRectInsideAnother(RectTransform target, RectTransform constraint)
        {
            constraint.GetWorldCorners(CornersA);
            target.GetWorldCorners(CornersB);

            Vector3 offset = Vector3.zero;

            if (CornersB[0].x < CornersA[0].x) // gauche
                offset.x = CornersA[0].x - CornersB[0].x;
            if (CornersB[2].x > CornersA[2].x) // droite
                offset.x = CornersA[2].x - CornersB[2].x;
            if (CornersB[0].y < CornersA[0].y) // bas
                offset.y = CornersA[0].y - CornersB[0].y;
            if (CornersB[1].y > CornersA[1].y) // haut
                offset.y = CornersA[1].y - CornersB[1].y;

            target.position += offset;
        }
    }
}