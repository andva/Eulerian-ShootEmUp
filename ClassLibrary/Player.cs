using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ClassLibrary
{
    public class Player : AbsData
    {
        public Camera camera;

        public float bulletTimer = 0; // bara simulera en delay på skotten
        public Weapon rifle;

        public Player(Vector3 pos, GraphicsDevice device)
        {
            position = pos;
            camera = new Camera(device, getCameraPos());
            model = 0;
            
            rifle = new Weapon(camera);
        }
        public Player(float x, float y, float z, Int32 weapon, Int32 identity, GraphicsDevice device)
        {
            ChangePosition(x, y, z);
            ChangeWeapon(weapon);
            id = identity;
            alive = true;
            camera = new Camera(device, getCameraPos());
            model = 0;

            rifle = new Weapon(camera);
        }

        public void processKeyboardInput(float amount)
        {
            Vector3 moveVector = new Vector3(0, 0, 0);
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))
                moveVector += new Vector3(0, 0, -1);
            if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))
                moveVector += new Vector3(0, 0, 1);
            if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))
                moveVector += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A))
                moveVector += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.Q))
                moveVector += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.Z))
                moveVector += new Vector3(0, -1, 0);

            if (keyState.IsKeyDown(Keys.LeftControl))
            {
                if (headPos > HEADMIN) headPos -= SITSPEAD*amount;
            }
            if (keyState.IsKeyUp(Keys.LeftControl))
            {
                if (headPos < HEADMAX) headPos += SITSPEAD * amount;
            }
            if (keyState.IsKeyDown(Keys.LeftShift))
            {
                moveSpeed = 1.5f;
            }
            if (keyState.IsKeyUp(Keys.LeftShift))
            {
                moveSpeed = 1f;
            }

            addToPosition(moveVector * amount);
            ChangeForwardDir(camera.GetForwardDir());
        }

        public void addToPosition(Vector3 vectorToAdd)
        {
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, camera.rotation);
            Vector3 planeMove = new Vector3(rotatedVector.X, 0, rotatedVector.Z);
            if (planeMove != Vector3.Zero)
            {
                planeMove.Normalize();
            }

            //fixa så cameran följer med
            position += moveSpeed * planeMove;
            if (position.X >= 0 && position.X < Globals.level.width && position.Z >= 0 && position.Z < Globals.level.length)//Fulfix för följa heightmap
            {
                position.Y = Globals.level.heightData[(int)Math.Abs(position.X), (int)Math.Abs(position.Z)];
            }
            camera.position = getCameraPos();
            camera.updateViewMatrix();
        }

        public Vector3 getCameraPos()
        {
            return position + new Vector3(0, headPos, 0);
        }

        public void updatePlayer(float amount)
        {
            rifle.update(amount);

            camera.updateCamera(amount);
            processKeyboardInput(amount);
        }

    }
}
