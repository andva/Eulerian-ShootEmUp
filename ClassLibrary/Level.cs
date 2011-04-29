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
    public class Level
    {
        public Model model;
        public Texture2D[] mapTexture;
        public Texture2D[] effectTextures;
        public Effect levelEffect;

        public float[,] heightData;
        Texture2D heightMap;

        public int width;
        public int length;

        public Level(Model m, Texture2D[] mT, Texture2D[] eT, Effect e, Texture2D hM)
        {
            model = m;
            mapTexture = mT;
            effectTextures = eT;
            levelEffect = e;
            heightMap = hM;

            loadHeightData();
        }

        public void loadHeightData()
        {
            float minimumHeight = float.MaxValue;
            float maximumHeight = float.MinValue;

            width = heightMap.Width;
            length = heightMap.Height;

            Color[] heightMapColors = new Color[width * length];
            heightMap.GetData(heightMapColors);


            heightData = new float[width, length];
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < length; z++)
                {
                    heightData[x, z] = heightMapColors[x + z * width].R;
                    if (heightData[x, z] < minimumHeight) minimumHeight = heightData[x, z];
                    if (heightData[x, z] > maximumHeight) maximumHeight = heightData[x, z];
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < length; z++)
                {
                    heightData[x, z] = (heightData[x, z] - minimumHeight) / (maximumHeight - minimumHeight) * 30.0f;
                }
            }      
        }

        public void DrawLevel()
        {
            Matrix world = Matrix.Identity;
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                ModelMesh mesh = model.Meshes.ElementAt(i);
                foreach (Effect effect in mesh.Effects)
                {
                    if (i == 0)
                    {
                        effect.CurrentTechnique = effect.Techniques["Bump"];
                        effect.Parameters["N_Texture"].SetValue(effectTextures[0]);
                    }
                    if (i == 1)
                    {
                        effect.CurrentTechnique = effect.Techniques["Basic"];
                    }

                    effect.Parameters["Texture"].SetValue(mapTexture[i]);
                    effect.Parameters["Projection"].SetValue(Globals.player.camera.projection);
                    effect.Parameters["View"].SetValue(Globals.player.camera.view);
                    effect.Parameters["lightDir1"].SetValue(new Vector3(0, 0, 5));
                }
                mesh.Draw();
            }
        }

    }
}
