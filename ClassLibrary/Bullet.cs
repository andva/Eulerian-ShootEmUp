using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ClassLibrary
{
    public class Bullet
    {
        public Int32 id;
        public Vector3 dir;
        public Vector3 emitter;

        public Bullet(Vector3 emitterPos, Vector3 d, Int32 i)
        {
            id = i;
            dir = d;
        }

        public void CheckHits(Player player, OtherPlayer[] players)
        {
            if (players[id] != null)
            {

                Vector3 shooterPos = players[id].GetPosition();

                BoundingSphere b = new BoundingSphere(player.GetPosition(), Constants.BOLLRADIE);
                Ray r = new Ray(shooterPos, dir);
                if (r.Intersects(b) != null)
                {
                    player.ChangeLifeStatus(false);
                }
            }
        }
    }
}
