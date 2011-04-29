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
using SkinnedModel;

namespace ClassLibrary
{
    //kanske får modifiera denna senare som abstrakt? så man senare lätt kan ha underclasser som gun, rifle osv
    //lagt till den för fixa rekyl endast på vapnet
    public class Weapon
    {
        public float zRecoil;
        public const uint MAXROUNDS = 30;
        public const uint MAXCLIP = 4;
        public uint clip;
        public uint rounds;

        public float timer;
        public const uint ROUNDS_PER_MINUTE = 900;

        public Matrix[] bones;
        public AnimationPlayer player;
        public AnimationClip animationClip;
        public Model model;
        public Camera camera;

        public bool isShooting;
        public bool animation;

        public Weapon(Camera cam)
        {
            camera = cam;
            zRecoil = 0f;
            clip = MAXCLIP;
            rounds = MAXROUNDS;
            timer = 0.0f;
            isShooting = false;
            animation = false;

        }

        public void update(float amount)
        {
            float amplitudeZ = 6f;
            if (zRecoil < 0) zRecoil += amplitudeZ * amount * Math.Abs(zRecoil);
            if (zRecoil > 0) zRecoil = 0;

            checkIfPlayerFire(amount);
            handleAnimation();
        }

        public void addRecoilToWeapon()
        {
            Random random = new Random();
            float amplitude = 0.2f;
            zRecoil -= amplitude;
        }

        public void checkIfPlayerFire(float amount)
        {
            MouseState currentMouseState = Mouse.GetState();
            timer += amount;
            if (timer >= (60f / (float)ROUNDS_PER_MINUTE)) timer = 0;

            //Kolla om personen kan skjuta eller inte, om han kan så skall han lägga till en nya kula och även lägga recoil på kameran och vapnet
            if (currentMouseState.LeftButton == ButtonState.Pressed && timer == 0)
            {
                isShooting = true;

                //Bullets.add(new Bullet(camera.pos, camera.direction))
                Audio fireSound = new Audio("gunshot", camera.position);
                fireSound.play(Globals.audioManager);

                camera.addRecoilToCamera();
                addRecoilToWeapon();
            }
            else if (currentMouseState.LeftButton == ButtonState.Released)
            {
                isShooting = false;
            }
        }

        public void handleAnimation()
        {
            if (isShooting && !animation)
            {
                //Skjutanimation
                animation = true;
                Globals.clipPlayer.play(Globals.rifleClip, 106, 112, true);
            }
            if (!isShooting && animation)
            {
                //Sluta skjuta
                animation = false;
                Globals.clipPlayer.play(Globals.rifleClip, 116, 124, false);
            }
        }
    }
}
