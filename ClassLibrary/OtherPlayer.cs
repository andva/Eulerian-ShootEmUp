using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary
{
    public class OtherPlayer : AbsData
    {
        public OtherPlayer(float x, float y, float z, Int32 weapon, Int32 identity, float dir)
        {
            ChangePosition(x, y, z);
            ChangeWeapon(weapon);
            id = identity;
            alive = true;
            model = 0;
            forwardDir = dir;
        }
    }
}
