using System;
using UnityEngine;

namespace IronMountain.Quests.Editor
{
    public abstract class StyledInspector : UnityEditor.Editor
    {
        protected GUIStyle HeaderValid;
        protected GUIStyle HeaderInvalid;
        protected GUIStyle H1Valid;
        protected GUIStyle H1Invalid;
        protected GUIStyle NotTracking;
        protected GUIStyle Tracking;
        protected GUIStyle Completed;

        protected virtual void OnEnable()
        {
            Texture2D headerValidTexture = new Texture2D(1, 1);
            headerValidTexture.SetPixel(0, 0, new Color(0.12f, 0.12f, 0.12f));
            headerValidTexture.Apply();
            HeaderValid = new GUIStyle
            {
                padding = new RectOffset(5, 5, 5, 5),
                normal = new GUIStyleState
                {
                    background = headerValidTexture
                }
            };

            Texture2D headerInvalidTexture = new Texture2D(1, 1);
            headerInvalidTexture.SetPixel(0, 0, new Color(0.66f, 0.17f, 0.18f));
            headerInvalidTexture.Apply();
            HeaderInvalid = new GUIStyle
            {
                padding = new RectOffset(5, 5, 5, 5),
                normal = new GUIStyleState
                {
                    background = headerInvalidTexture
                }
            };

            H1Valid = new GUIStyle
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = new GUIStyleState
                {
                    textColor = new Color(0.36f, 0.36f, 0.36f)
                }
            };

            H1Invalid = new GUIStyle
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }
            };

            Texture2D notTrackingTexture = new Texture2D(1, 1);
            notTrackingTexture.SetPixel(0, 0, new Color(0.58f, 0.58f, 0.58f));
            notTrackingTexture.Apply();
            NotTracking = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(3, 3, 3, 3),
                padding = new RectOffset(1, 1, 1, 1),
                normal = new GUIStyleState
                {
                    background = notTrackingTexture,
                    textColor = Color.white
                }
            };

            Texture2D trackingTexture = new Texture2D(1, 1);
            trackingTexture.SetPixel(0, 0, new Color(0.99f, 0.95f, 0.15f));
            trackingTexture.Apply();
            Tracking = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(3, 3, 3, 3),
                padding = new RectOffset(1, 1, 1, 1),
                normal = new GUIStyleState
                {
                    background = trackingTexture,
                    textColor = new Color(0.18f, 0.18f, 0.18f)
                }
            };

            Texture2D completedTexture = new Texture2D(1, 1);
            completedTexture.SetPixel(0, 0, new Color(0f, 0.76f, 0.08f));
            completedTexture.Apply();
            Completed = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(3, 3, 3, 3),
                padding = new RectOffset(1, 1, 1, 1),
                normal = new GUIStyleState
                {
                    background = completedTexture,
                    textColor = Color.white
                }
            };
        }
    }
}
