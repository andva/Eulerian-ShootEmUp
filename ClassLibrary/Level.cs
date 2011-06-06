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
        public Effect levelEffect;

        public float[,] heightData;
        Texture2D heightMap;

        public int width;
        public int length;

        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        //Texture2D grassTexture;

        VertexPositionNormalTexture[] vertices;
        int[] indices;

        GraphicsDevice device;

        public Level(GraphicsDevice d, Model m, Effect e, Texture2D hM)
        {
            model = m;
            levelEffect = e;
            heightMap = hM;


            //Onödigt senare, används för kolla heightmap
            device = d;

            loadHeightData();
            SetUpTerrainVertices();
            SetUpTerrainIndices();
            CalculateNormals();
            CopyToTerrainBuffers();
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
                    heightData[x, z] = (heightData[x, z] - minimumHeight) / (maximumHeight - minimumHeight) * 50f;
                   // heightData[x, z] = heightData[x, z] - minimumHeight;
                }
            } 
        }
        public void DrawLevel()
        {
            //DrawTerrain(); // RITA UT HEIGHTMAP

            //Manuell justering av banana så den hamnar på heightmappen
            float skala = 9.40f;
            Matrix world = Matrix.CreateScale(1.695f * skala, 1.125f * skala, 1.695f * skala) * Matrix.CreateTranslation(new Vector3(255, 0, 248)); 

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = world;

                    effect.View = Globals.player.camera.view;
                    effect.Projection = Globals.player.camera.projection;
                }
                mesh.Draw();
            }

        }
        /*public void DrawLevel1()
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
        }*/

        private void SetUpTerrainVertices()
        {
            vertices = new VertexPositionNormalTexture[width * length];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < length; z++)
                {
                    vertices[x + z * width].Position = new Vector3(x, heightData[x, z], z);
                    vertices[x + z * width].TextureCoordinate.X = (float)x / 30.0f;
                    vertices[x + z * width].TextureCoordinate.Y = (float)z / 30.0f;
                }
            }
        }

        private void SetUpTerrainIndices()
        {
            indices = new int[(width - 1) * (length - 1) * 6];
            int counter = 0;
            for (int y = 0; y < length - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int lowerLeft = x + y * width;
                    int lowerRight = (x + 1) + y * width;
                    int topLeft = x + (y + 1) * width;
                    int topRight = (x + 1) + (y + 1) * width;

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }
        }

        private void CalculateNormals()
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();
        }

        public void CopyToTerrainBuffers()
        {
            vertexBuffer = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            indexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        public void DrawTerrain()
        {
            Globals.effect.CurrentTechnique = Globals.effect.Techniques["Textured"];
            Matrix worldMatrix = Matrix.Identity;
            Globals.effect.Parameters["xWorld"].SetValue(worldMatrix);
            Globals.effect.Parameters["xView"].SetValue(Globals.player.camera.view);
            Globals.effect.Parameters["xProjection"].SetValue(Globals.player.camera.projection);
            Globals.effect.Parameters["xTexture"].SetValue(heightMap);
            Globals.effect.Parameters["xEnableLighting"].SetValue(true);
            Globals.effect.Parameters["xAmbient"].SetValue(0.4f);
            Globals.effect.Parameters["xLightDirection"].SetValue(new Vector3(-0.5f, -1, -0.5f));

            foreach (EffectPass pass in Globals.effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.Indices = indexBuffer;
                device.SetVertexBuffer(vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);
            }
        }
    }
}
