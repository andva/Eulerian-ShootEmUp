using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkinnedModel;

namespace ClassLibrary
{
    public class BloodList
    {
        public List<Blood> blood = new List<Blood>();
        public Texture2D[] bloodTextures;

        public BloodList(Texture2D[] textureArray)
        {
            bloodTextures = textureArray;
        }

        public void add(Vector3 pos)
        {
            if (blood.Count() > 0)
            {
                blood.RemoveAt(0);
            }

            blood.Add(new Blood(pos, bloodTextures));
        }

        public void update(float amount)
        {
            for (int i = 0; i < blood.Count(); i++)
            {
                blood.ElementAt(i).update();
                if (!blood.ElementAt(i).show) //Risk för krash här, ifall jag har tänkt fel
                {
                    blood.RemoveAt(i);
                    --i;
                }
            }
        }

        public void draw()
        {
            foreach (Blood b in blood)
            {
                b.draw();
            }
        }
    }
}
