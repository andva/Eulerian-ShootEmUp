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
        public uint MAXROUNDS = 30; //bör vara konstanta men då gick det inte nå dom från hud
        public uint MAXCLIP = 4;
        public uint clip;
        public uint rounds;

        public float timer;
        public float reloadTimer;
        public const uint ROUNDS_PER_MINUTE = 900;

        public Matrix[] bones;
        public AnimationPlayer player;
        public AnimationClip animationClip;
        public Model model;
        public Camera camera;

        public bool animation;
        public bool empty;

        public playerStatus status;

        Vector3 gunPos = new Vector3(0.3f, 0.1f, 2f);

        public Weapon(Camera cam, playerStatus stat)
        {
            camera = cam;
            zRecoil = 0f;
            clip = MAXCLIP;
            rounds = MAXROUNDS;
            timer = 0.0f;
            animation = false;
            empty = false;
            reloadTimer = 0.0f;
            status = stat;
        }

        public void update(float amount)
        {
            float amplitudeZ = 6f;
            if (zRecoil < 0) zRecoil += amplitudeZ * amount * Math.Abs(zRecoil);
            if (zRecoil > 0) zRecoil = 0;

            checkIfPlayerFire(amount);
            handleAnimation();
            
            //Fixar så man inte kan ladda för snabbt (dvs ladda ett magasin när man redan håller på)
            if (status.isReloading) reloadTimer += amount;
            if (reloadTimer > 2.85f)
            {
                status.isReloading = false;
                reloadTimer = 0.0f;
                empty = false;
                rounds = MAXROUNDS;
            }
        }
        public void resetGun()
        {
            clip = MAXCLIP;
            rounds = 30;
            empty = false;
            status.isReloading = false;
            status.isShooting = false;
        }
        private bool canShoot()
        {
            if (status.isReloading) return false;
            if (empty) return false;
            if (status.isRunning) return false;
            
            return true;
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
            if (currentMouseState.LeftButton == ButtonState.Pressed && timer == 0 && canShoot() && Globals.player.alive)
            {
                status.isShooting = true;

                //Bullets.add(new Bullet(camera.pos, camera.direction))
                Audio fireSound = new Audio("gunshot", camera.position);
                fireSound.play(Globals.audioManager);

                camera.addRecoilToCamera();
                addRecoilToWeapon();
                Shoot();
                rounds--;
                if(rounds == 0)
                {
                    empty = true;
                }
                Globals.muzzleflash.activate();
            }
            else if (empty || currentMouseState.LeftButton == ButtonState.Released)
            {
                status.isShooting = false;
            }
        }

        private void Shoot()
        {
            
            if (Globals.players.Length != 0)
            {
                foreach (OtherPlayer op in Globals.players)
                {
                    if (op != null)
                        op.checkHit(camera.position, camera.direction);
                }
            }
        }

        public void handleAnimation()
        {
            if (!status.isRunning && !status.isReloading)
            {
                //animation när spelaren skjuter
                if (status.isShooting && !animation)
                {
                    //Skjutanimation
                    animation = true;
                    Globals.clipPlayer.play(Globals.rifleClip, 106, 112, true);
                }
                if (!status.isShooting && animation)
                {
                    //Sluta skjuta
                    animation = false;
                    Globals.clipPlayer.play(Globals.rifleClip, 116, 124, false);
                }
            }
        }

        public void reload()
        {
            //Laddar om vapnet
            if (clip > 0 && !status.isReloading)
            {
                Globals.clipPlayer.play(Globals.rifleClip, 125, 283, false);
                
                clip--;
                
                status.isReloading = true;
            }
        }

        public void DrawGun(float time)
        {
            

            model = Globals.rifle;
            KeyboardState keyState = Keyboard.GetState();
            Matrix world = Matrix.Identity;
            int amplitude = 0;
            float cosDisp, sinDisp;
            float xT, yT, zT;
            if (status.isRunning || status.isWalking)
            {
                amplitude = 7;
                cosDisp = amplitude * (float)Math.Cos(time * amplitude) / 30;//cossinus displacement
                sinDisp = amplitude * (float)Math.Sin(time * amplitude) / 30;//sinus displacement
                yT = -12.2f - Math.Abs(sinDisp);
            }
            else
            {
                amplitude = 2;
                cosDisp = 0.0f;
                sinDisp = amplitude * (float)Math.Sin(time * amplitude) / 100;//sinus displacement
                yT = -12.2f - sinDisp * 0.5f;
            }
            xT = 0.15f + cosDisp / 2;
            zT = -20 + cosDisp / 3;
            
            //Fulkod! :)
            world = world * Matrix.CreateTranslation(gunPos) *
            Matrix.CreateTranslation(new Vector3(xT, yT, zT)) *
            Matrix.CreateTranslation(new Vector3(0f, 0f, Globals.player.rifle.zRecoil)) *
            Matrix.CreateScale(0.5f, 0.5f, 0.5f) *
            Matrix.CreateRotationY(MathHelper.Pi) / 2 *
            Globals.player.camera.rotation *
            Matrix.CreateTranslation(Globals.player.camera.position + 10 * Globals.player.camera.direction);
            Matrix[] bones = Globals.clipPlayer.GetSkinTransforms();

            //Allt som behövs för kameran...p.
            //world = Matrix.CreateTranslation(Globals.player.camera.position) * Globals.player.camera.rotation;
            //Console.WriteLine(Globals.player.camera.position);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);

                    effect.World = world;

                    effect.View = Globals.player.camera.view;
                    effect.Projection = Globals.player.camera.projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh.Draw();
            }
        }
    }
}
