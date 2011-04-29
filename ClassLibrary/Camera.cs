using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ClassLibrary
{
    public class Camera
    {
        public Vector3 position;
        public Vector3 direction;
        public Matrix rotation;
        public Vector3 up;

        public float leftrightRot;
        public float updownRot;
        public const float rotationSpeed = 0.1f;

        public MouseState originalMouseState;

        public Matrix view;
        public Matrix projection;


        public Camera(GraphicsDevice device, Vector3 pos)
        {
            Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
            originalMouseState = Mouse.GetState();

            leftrightRot = MathHelper.PiOver2;
            updownRot = 0;

            position = pos;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 0.3f, 1000.0f);

            updateViewMatrix();
        }

        public void updateViewMatrix()
        {
            rotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);            
            Vector3 originalTarget = new Vector3(0, 0, -1);
            direction = Vector3.Transform(originalTarget, rotation);
            Vector3 finalTarget = position + direction;
            Vector3 originalUpVector = new Vector3(0, 1, 0);
            up = Vector3.Transform(originalUpVector, rotation);
            view = Matrix.CreateLookAt(position, finalTarget, up);
        }

        public void processMouseInput(float amount)
        {
            MouseState currentMouseState = Mouse.GetState();
            if (currentMouseState != originalMouseState)
            {
                float xDifference = currentMouseState.X - originalMouseState.X;
                float yDifference = currentMouseState.Y - originalMouseState.Y;
                leftrightRot -= rotationSpeed * xDifference * amount;
                updownRot -= rotationSpeed * yDifference * amount;
                //fixa så inte det går snurra helt varv "uppåt"
                float maxrot = (float)Math.PI * 0.48f;
                if (updownRot <= -maxrot)
                {
                    updownRot = -maxrot;
                }
                if (updownRot >= maxrot)
                {
                    updownRot = maxrot;
                }
                Mouse.SetPosition(originalMouseState.X, originalMouseState.Y);
                updateViewMatrix();
            }
        }

        public void addRecoilToCamera()
        {
            //lägg på rekyl på cameran
            Random random = new Random();
            float amplitude = 0.003f;
            float xRecoil = amplitude * (random.Next(100) - random.Next(100));
            float yRecoil = amplitude * random.Next(100);

            leftrightRot -= rotationSpeed * xRecoil;
            updownRot += rotationSpeed * yRecoil;
        }

        public void updateCamera(float amount)
        {
            processMouseInput(amount);
        }

        public float GetForwardDir()
        {
            return leftrightRot;
        }
    }
}