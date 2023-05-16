using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FreeDraw
{
    // Helper methods used to set drawing settings
    public class DrawingSettings : MonoBehaviour
    {
        public static bool isCursorOverUI = false;
        public float Transparency = 1f;
        PhotonView pV;

        void Start() => pV = GetComponent<PhotonView>();

        // Changing pen settings is easy as changing the static properties Drawable.Pen_Colour and Drawable.Pen_Width
        public void SetMarkerColour(Color new_color)
        {
            Drawable.Pen_Colour = new_color;
        }
        // new_width is radius in pixels
        public void SetMarkerWidth(int new_width)
        {
            Drawable.Pen_Width = new_width;
        }
        public void SetMarkerWidth(float new_width)
        {
            SetMarkerWidth((int)new_width);
        }

        public void SetTransparency(float amount)
        {
            Transparency = amount;
            Color c = Drawable.Pen_Colour;
            c.a = amount;
            Drawable.Pen_Colour = c;
        }

        // Call these these to change the pen settings
        public void SetMarkerRed() => pV.RPC("SetMarkerRedRPC", RpcTarget.All);

        [PunRPC]
        void SetMarkerRedRPC()
        {
            Color c = Color.red;
            c.a = Transparency;
            SetMarkerColour(c);
            Drawable.drawable.SetPenBrush();
            Drawable.penColor = PenColor.RED;
        }

        public void SetMarkerGreen() => pV.RPC("SetMarkerGreenRPC", RpcTarget.All);

        [PunRPC]
        void SetMarkerGreenRPC()
        {
            Color c = Color.green;
            c.a = Transparency;
            SetMarkerColour(c);
            Drawable.drawable.SetPenBrush();
            Drawable.penColor = PenColor.GREEN;
        }

        public void SetMarkerBlue() => pV.RPC("SetMarkerBlueRPC", RpcTarget.All);

        [PunRPC]
        void SetMarkerBlueRPC()
        {
            Color c = Color.blue;
            c.a = Transparency;
            SetMarkerColour(c);
            Drawable.drawable.SetPenBrush();
            Drawable.penColor = PenColor.BLUE;
        }

        public void SetEraser()
        {
            SetMarkerColour(new Color(255f, 255f, 255f, 0f));
        }

        public void PartialSetEraser()
        {
            SetMarkerColour(new Color(255f, 255f, 255f, 0.5f));
        }
    }
}