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
        public Weapon rifle;
        public playerStatus status; //håller reda på isrunning mm
        
        public const float GRAVITY = 150f;
        public float speedY;
        public bool killingspree = false;

        public Player(Vector3 pos, GraphicsDevice device)
        {
            position = pos;
            camera = new Camera(device, getCameraPos());
            status = new playerStatus();
            rifle = new Weapon(camera, status);
            speedY = 0.0f;
        }
        public Player(float x, float y, float z, Int32 identity, Int32 charModel, GraphicsDevice device)
        {
            ChangePosition(x, y, z);
            id = identity;
            Console.WriteLine("id: " + id.ToString());
            Globals.player.id = identity;
            alive = true;
            model = charModel;
            camera = new Camera(device, getCameraPos());
            status = new playerStatus();
            rifle = new Weapon(camera, status);
            speedY = 0.0f;
            Globals.player.model = charModel;
        }
        public void HitOne(Int32 id)
        {
            Globals.players[id].hit = true;
        }
        public void processKeyboardInput(float amount)
        {
            Vector3 moveVector = new Vector3(0, 0, 0);
            KeyboardState keyState = Keyboard.GetState();
            status.isWalking = false;
            if (keyState.IsKeyDown(Keys.W))
            {
                moveVector += new Vector3(0, 0, -1);
                status.isWalking = true;
            }
            if (keyState.IsKeyDown(Keys.S))
            {
                moveVector += new Vector3(0, 0, 1);
                status.isWalking = true;
            }
            if (keyState.IsKeyDown(Keys.D))
            {
                moveVector += new Vector3(1, 0, 0);
                status.isWalking = true;
            }
            if (keyState.IsKeyDown(Keys.A))
            {
                moveVector += new Vector3(-1, 0, 0);
                status.isWalking = true;
            }

            if (keyState.IsKeyDown(Keys.LeftControl))
            {
                if (headPos > Constants.HEADMIN) headPos -= SITSPEAD*amount;

                if (keyState.IsKeyDown(Keys.W))
                {
                    activity = Constants.CROUCHWALKING;
                }
                else
                {
                    activity = Constants.CROUCHING;
                }
            }
            if (keyState.IsKeyUp(Keys.LeftControl))
            {
                if (headPos < Constants.HEADMAX) headPos += SITSPEAD * amount;
            }
            if (keyState.IsKeyDown(Keys.LeftShift) && !status.isRunning && !status.isReloading && keyState.IsKeyDown(Keys.W))//Springer han?
            {
                status.isRunning = true;
                moveSpeed = 75f;
                Globals.clipPlayer.play(Globals.rifleClip, 284, 308, false);
            }
            if ((keyState.IsKeyUp(Keys.LeftShift) || keyState.IsKeyUp(Keys.W)) && status.isRunning)//sluta springa
            {
                Globals.clipPlayer.play(Globals.rifleClip, 309, 340, false);
                status.isRunning = false;
                moveSpeed = 50f;
            }
            if (keyState.IsKeyDown(Keys.R) && !status.isRunning)
            {
                rifle.reload();
            }
            if (status.isWalking && !keyState.IsKeyDown(Keys.LeftControl))
            {
                activity = Constants.WALKING;
            }
            if (!status.isWalking && !status.isRunning && !keyState.IsKeyDown(Keys.LeftControl))
                activity = Constants.STANDING;

            if (status.isRunning && !keyState.IsKeyDown(Keys.LeftControl))
                activity = Constants.RUNNING;

            addToPosition(moveVector, amount);
        }
        public void resetPlayer()
        {
            alive = true;
            rifle.resetGun();
            rifle.clip = rifle.MAXCLIP;
            hp = 100;
            Globals.player = this;
        }
        public void addToPosition(Vector3 vectorToAdd, float amount)
        {
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, camera.rotation);
            Vector3 planeMove = new Vector3(rotatedVector.X, 0, rotatedVector.Z);
            if (planeMove != Vector3.Zero)
            {
                planeMove.Normalize();
            }

            Vector3 oldPos = new Vector3(position.X, position.Y, position.Z);
            Vector3 moveVector = moveSpeed * planeMove * amount;
            position += moveVector;
            checkPos(); //Fixar så man kan kolla heightmap utan krash

            //Fixar så man inte är under banan
            if (position.Y < Globals.level.heightData[(int)Math.Abs(position.X), (int)Math.Abs(position.Z)])
            {
                position.Y = Globals.level.heightData[(int)Math.Abs(position.X), (int)Math.Abs(position.Z)];
            }

            //Fixa så man inte kan gå upp för allt för branta objekt
            if (oldPos != Vector3.Zero //fulfix så man inte fastnar när man startar
                && position.Y - oldPos.Y > 4.5f)
            {
                position = oldPos;
            }


            handleJump(amount);

            //position += moveSpeed * rotatedVector * amount * 3; //NOclip läge kommentera koden över

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
            
            Globals.player.xr = camera.updownRot;
            Globals.player.yr = camera.leftrightRot;

            if (alive)
            {
                camera.updateCamera(amount);
                processKeyboardInput(amount);
            }
        }

        public void handleJump(float amount)
        {
            //Fixar hopp
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Space) && 
                (position.Y <= Globals.level.heightData[(int)Math.Abs(position.X), (int)Math.Abs(position.Z)] ||
                 position.Y - Globals.level.heightData[(int)Math.Abs(position.X), (int)Math.Abs(position.Z)] < 1.0f))//behöver ej stå exakt på marken
            {
                speedY = 65f;
            }
            position.Y += speedY * amount;
            if (position.Y > Globals.level.heightData[(int)Math.Abs(position.X), (int)Math.Abs(position.Z)])
            {
                speedY -= GRAVITY * amount;
            }
            else
            {
                speedY = 0.0f;
            }
        }

        public void checkPos()
        {
            //Gör så int hamnar utanför banan
            if (position.X < 0) position.X = 0;
            if (position.X >= Globals.level.width) position.X = Globals.level.width - 1;
            if (position.Z < 0) position.Z = 0;
            if (position.Z >= Globals.level.length) position.Z = Globals.level.length - 1;
        }
    }
}
