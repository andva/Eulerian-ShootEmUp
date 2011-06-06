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

//Extremt lik klassen billboard men för flera frames
namespace ClassLibrary
{
    public class Blood
    {
        public Vector3 position;
        public Texture2D[] sprite;
        public int frame;
        public bool show;

        public Blood(Vector3 pos, Texture2D[] textureArray)
        {
            position = pos;
            sprite = textureArray;
            frame = 0;
            show = true;
        }

        public void update()
        {
            if (show)
            {
                frame++;
                if (frame == sprite.Length) show = false;
            }
        }

        public void draw()
        {
            if (show)
            {
                //Lägg spriten på en triangel
                VertexPositionTexture[] vertices = new VertexPositionTexture[6];
                //vertices[0] = new VertexPositionTexture(position, new Vector2(1, 1));
                vertices[0] = new VertexPositionTexture(position, new Vector2(1, 1));
                vertices[1] = new VertexPositionTexture(position, new Vector2(0, 0));
                vertices[2] = new VertexPositionTexture(position, new Vector2(1, 0));

                vertices[3] = new VertexPositionTexture(position, new Vector2(1, 1));
                vertices[4] = new VertexPositionTexture(position, new Vector2(0, 1));
                vertices[5] = new VertexPositionTexture(position, new Vector2(0, 0));

                Globals.effect.CurrentTechnique = Globals.effect.Techniques["PointSprites"];
                Globals.effect.Parameters["xWorld"].SetValue(Matrix.Identity);
                Globals.effect.Parameters["xProjection"].SetValue(Globals.player.camera.projection);
                Globals.effect.Parameters["xView"].SetValue(Globals.player.camera.view);
                Globals.effect.Parameters["xCamPos"].SetValue(Globals.player.camera.position);
                Globals.effect.Parameters["xTexture"].SetValue(sprite[frame]);
                Globals.effect.Parameters["xCamUp"].SetValue(Globals.player.camera.up);
                Globals.effect.Parameters["xPointSpriteSize"].SetValue(100f);

                if (frame >= sprite.Length - 40) Globals.device.BlendState = BlendState.Additive;

                foreach (EffectPass pass in Globals.effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Globals.device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 2);
                }

                if (frame >= sprite.Length - 40) Globals.device.BlendState = BlendState.Opaque;
            }
        }
    }
}
