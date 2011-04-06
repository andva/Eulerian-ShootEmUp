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

        public Matrix[] bones;
        public AnimationPlayer player;
        public AnimationClip clip;
        public Model model;

        public Weapon()
        {
            zRecoil = 0f;
            

        }

        public void update(float amount)
        {
            float amplitudeZ = 6f;
            if (zRecoil < 0) zRecoil += amplitudeZ * amount * Math.Abs(zRecoil);
            if (zRecoil > 0) zRecoil = 0;
        }

        public void addRecoilToWeapon()
        {
            Random random = new Random();
            float amplitude = 0.2f;
            zRecoil -= amplitude;
        }
    }
}
