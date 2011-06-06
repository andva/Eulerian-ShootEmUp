using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;


namespace ClassLibrary
{

    public class Data
    {
        //private Vector3 
        public Boolean alive;
        public Int32 weapon;
        public Int32 id;
        public Model model { get; set; }
        public Vector3 position;
        public float forwardDirection = 0;
        public BoundingSphere boundingSphere;
        

        public Data()
        {
            alive = true;
            position = Vector3.Zero;
            weapon = 1;
        }
        public Data(Vector3 p, Int32 w, Int32 i)
        {
            ChangePosition(p);
            ChangeWeapon(w);
            id = i;
            alive = true;
        }
        public Data(float x, float y, float z, Int32 weapon, Int32 identity)
        {
            ChangePosition(x, y, z);
            ChangeWeapon(weapon);
            id = identity;
            alive = true;
        }
        public Data(Data d)
        {
            ChangePosition(d.GetPosition());
            ChangeWeapon(weapon);
            id = d.GetId();
            alive = true;
        }
        public void ChangeWeapon(Int32 w)
        {
            weapon = w;
        }
        public int GetId()
        {
            return id;
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
        public void ChangePositionX(float a)
        {
            position.X = a;
        }
        public void ChangePositionY(float a)
        {
            position.Y = a;
        }
        public void ChangePositionZ(float a)
        {
            position.Z = a;
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
