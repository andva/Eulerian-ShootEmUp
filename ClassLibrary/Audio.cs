#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace ClassLibrary
{
    /// <summary>
    /// Base class for game entities that are displayed as billboard sprites,
    /// and which can emit 3D sounds. The Cat and Dog classes both derive from this.
    /// </summary>
    public class Audio : IAudioEmitter
    {
        #region Properties (MicrosoftKod)


        /// <summary>
        /// Gets or sets the 3D position of the entity.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        Vector3 position;


        /// <summary>
        /// Gets or sets which way the entity is facing.
        /// </summary>
        public Vector3 Forward
        {
            get { return forward; }
            set { forward = value; }
        }

        Vector3 forward;


        /// <summary>
        /// Gets or sets the orientation of this entity.
        /// </summary>
        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }

        Vector3 up;

        
        /// <summary>
        /// Gets or sets how fast this entity is moving.
        /// </summary>
        public Vector3 Velocity
        {
            get { return velocity; }
            protected set { velocity = value; }
        }

        Vector3 velocity;

        #endregion

        string filename;

        //Sparar ljudfilens namn för få rätt ljud för respektive
        //Sparar för tillfället bara positionen och resten av forward,up,velocity är standard det går att ändra för att få "Bättre" ljud
        public Audio(string filename0, Vector3 pos)
        {
            filename = filename0;

            Position = pos;
            Forward = Vector3.Forward;
            Up = Vector3.Up;
            Velocity = Vector3.Zero;
        }

        //skickar ljudet till audiomanager som hanterar uppspelningen av ljudet
        /// <summary>
        /// Updates the position of the entity, and allows it to play sounds.
        /// </summary>
        public void play(AudioManager audioManager)
        {
            audioManager.Play3DSound(filename, false, this);
        }
    }
}
