using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassLibrary
{
    //Abstrakt klass för alla som spelar
    public abstract class AbsData
    {
        public Vector3 position;
        public Int32 id;
        public Int32 weapon;
        public Boolean alive;
        public Int32 model;
        public float xr, yr;
        public BoundingSphere boundingSphere;
        public BoundingBox boundingBox;
        //public String name;
        public float moveSpeed = 50f;
        public Int16 hp = 100;
        public Boolean hit = false;
        public Boolean hitMe = false;
        public Int16 activity = 0;
        public const float SITSPEAD = 30f;
        public float headPos = Constants.HEADMAX;
        public int killer;

        public AbsData()
        {
            alive = true;
            position = Vector3.Zero;
            weapon = 1;
            Vector3 boundingPos = this.GetPosition();
            boundingPos.Y += Convert.ToInt16(headPos/2);
            boundingSphere = new BoundingSphere(boundingPos, Constants.BOLLRADIE);
        }
        public AbsData(Vector3 p, Int32 w, Int32 i)
        {
            ChangePosition(p);
            ChangeWeapon(w);
            id = i;
            alive = true;
            boundingSphere = new BoundingSphere(this.GetPosition(), Constants.BOLLRADIE);
        }
        public AbsData(Player d)
        {
            ChangePosition(d.GetPosition());
            ChangeWeapon(weapon);
            id = d.id;
            alive = true;
            boundingSphere = new BoundingSphere(this.GetPosition(), Constants.BOLLRADIE);
        }
        public AbsData(Vector3 pos, GraphicsDevice device)
        {
            position = pos;
            boundingSphere = new BoundingSphere(this.GetPosition(), Constants.BOLLRADIE);
        }

        public void ChangeWeapon(Int32 w)
        {
            weapon = w;
        }
        public void ChangePosition(Vector3 newPosition)
        {
            this.position.X = newPosition.X;
            position.Y = newPosition.Y;
            position.Z = newPosition.Z;
        }
        public void ChangePosition(float x,float y,float z)
        {
            position.X = x;
            position.Y = y;
            position.Z = z;
        }

        public void GotHit(Int16 hpw, Int32 shtr)
        {
            if (hp != 0)
            {
                hp -= hpw;
                hitMe = true;
            }
            if (hp <= 0)
            {
                if (hp + 10 > 0)
                    killer = shtr;
                activity = Constants.DEAD;
                alive = false;
                
            }
        }
        public void ChangeLifeStatus(Boolean l)
        {
            alive = l;
        }
        public void ChangeForwardDir(float xro, float yro)
        {
            xr = xro;
            yr = yro;
        }
        public Vector3 GetPosition()
        {
            return position;
        }
        public float GetXpos()
        {
            return position.X;
        }
        public float GetYpos()
        {
            return position.Y;
        }
        public float GetZpos()
        {
            return position.Z;
        }

        public string IdToString()
        {
            return id.ToString();
        }
    }
}
