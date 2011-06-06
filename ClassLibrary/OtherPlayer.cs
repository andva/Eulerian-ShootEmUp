using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkinnedModel;

namespace ClassLibrary
{
    public class OtherPlayer : AbsData
    {
        AnimationClip animationClip;
        ClipPlayer clipPlayer;
        Model mod;
        Boolean gotHit = false;
        Int16 lastAction = 0;

        public OtherPlayer(float x, float y, float z, Int32 identity, float xro, float yro, bool b)
        {
            position.X = x;
            position.Y = y;
            position.Z = z;
            id = identity;
            alive = true;
            xr = xro;
            yr = yro;
        }
        public OtherPlayer(float x, float y, float z, Int32 identity, Int16 mo, float xro, float yro)
        {
            model = mo;
            Console.WriteLine(mo);
            position.X = x;
            position.Y = y;
            position.Z = z;
            id = identity;
            alive = true;
            Console.WriteLine(mo.ToString());
            if (model == Constants.HAMPUS)
            {
                mod = Globals.hampus;
                clipPlayer = new ClipPlayer(Globals.hampusSkinningData, 60);
                animationClip = Globals.hampusSkinningData.AnimationClips["Take 001"];
            }
            else if (model == Constants.RASMUS)
            {
                mod = Globals.rasmus;
                clipPlayer = new ClipPlayer(Globals.rasmusSkinningData, 60);
                animationClip = Globals.rasmusSkinningData.AnimationClips["Take 001"];
            }
            else if (model == Constants.VALTER)
            {
                mod = Globals.valter;
                clipPlayer = new ClipPlayer(Globals.valterSkinningData, 60);
                animationClip = Globals.valterSkinningData.AnimationClips["Take 001"];
            }
            else if (model == Constants.AXEL)
            {
                mod = Globals.axel;
                clipPlayer = new ClipPlayer(Globals.axelSkinningData, 60);
                animationClip = Globals.axelSkinningData.AnimationClips["Take 001"];
            }

            Globals.s += identity;
            
            xr = xro;
            yr = yro;
            Globals.players[identity] = this;

            //idle
            clipPlayer.play(animationClip, 570, 600, true);


        }
        public void checkHit(Vector3 position, Vector3 dir)
        {
            Ray r = new Ray(position, dir);
            if (r.Intersects(this.boundingSphere) != null)
            {
                gotHit = true;
                Globals.players[id].hit = true;                

                float d = (position - Globals.players[id].position).Length();
                //Console.WriteLine(Globals.players[id].position + " " + (position + dir * d).ToString() + " " + d.ToString()); 
                Globals.blood.add(position + dir * d); //lägg till blod
            }
        }
        public void UpdateAnimations(GameTime gameTime)
        {
            Globals.players[id] = this;
            clipPlayer.update(gameTime.ElapsedGameTime, true, Matrix.Identity);
            if (lastAction != activity)
            {
                if (activity == Constants.WALKING)
                {
                    walk();
                }

                else if (activity == Constants.RUNNING)
                {
                    run();
                }

                else if (activity == Constants.STANDING)
                {
                    idle();
                    
                }
                else if (activity == Constants.CROUCHING)
                {
                    crouchIdle();
                }
                else if (activity == Constants.CROUCHWALKING)
                {
                    crouchwalk();
                }
                else if (activity == Constants.DEAD)
                {
                    if (lastAction == Constants.WALKING || lastAction == Constants.RUNNING || lastAction == Constants.STANDING)
                        dieStanding();
                    else
                        dieCrouching();
                }
                lastAction = activity;
                if (clipPlayer.inRange(629, 629))
                    deadFw();
                else if (clipPlayer.inRange(469, 469))
                    deadFc();
            }
        }
        private void idle()
        {
            clipPlayer.play(animationClip, 3, 200, true);
        }
        private void walk()
        {
            if (model == Constants.AXEL)
                clipPlayer.play(animationClip, 201, 278, true);
            else
                clipPlayer.play(animationClip, 201, 280, true);
        }
        private void run()
        {
            clipPlayer.play(animationClip, 500, 560, true);
        }
        private void crouchwalk()
        {
            clipPlayer.play(animationClip, 381, 440, true);
        }
        private void crouchIdle()
        {
            clipPlayer.play(animationClip, 381, 381, true);
        }
        private void crouchDown()
        {
            clipPlayer.play(animationClip, 355, 365, true);
        }
        private void crouchUp()
        {
            clipPlayer.play(animationClip, 365, 374, true);
        }
        private void dieStanding()
        {
            clipPlayer.play(animationClip, 610, 629, false);
        }
        private void dieCrouching()
        {
            clipPlayer.play(animationClip, 444, 469, false);
        }
        private void deadFw()
        {
            clipPlayer.play(animationClip, 630, 630, true);
        }
        private void deadFc()
        {
            clipPlayer.play(animationClip, 470, 470, true);
        }

        public void Draw()
        {

            Matrix world = Matrix.CreateRotationY(Globals.players[id].yr + (float)Math.PI) * Matrix.CreateTranslation(position);
            Matrix[] bones = clipPlayer.GetSkinTransforms();

            for(int i = 0; i < mod.Meshes.Count; i++)
            {
                foreach (SkinnedEffect effect in mod.Meshes[i].Effects)
                {
                    effect.SetBoneTransforms(bones);

                    effect.World = world;

                    effect.View = Globals.player.camera.view;
                    effect.Projection = Globals.player.camera.projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0f);
                    effect.SpecularPower = 16;
                }
                if(i != 0)
                    mod.Meshes[i].Draw();
            }
            mod.Meshes[0].Draw();
        }
    }
}
