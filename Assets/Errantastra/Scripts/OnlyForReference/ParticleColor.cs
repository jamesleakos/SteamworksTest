
using UnityEngine;

namespace Errantastra
{
    /// <summary>
    /// Modifies the starting color of a particle system to the color passed in.
    /// This is used on the player death particles to match the player's team color.
    /// </summary>
    public class ParticleColor : MonoBehaviour
    {
        /// <summary>
        /// Array for particle systems that should be colored.
        /// </summary>
        public ParticleSystem[] particles;

        /// <summary>
        /// Iterates over all particles and assigns the color passed in,
        /// but ignoring the alpha value of the new color.
        /// </summary>
        public void SetColor(Color color)
        {
            for(int i = 0; i < particles.Length; i++)
            {
                var main = particles[i].main;
                color.a = main.startColor.color.a;
                main.startColor = color;
            }
        }
    }
}