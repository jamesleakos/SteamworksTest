
using UnityEngine;

namespace Errantastra
{
    /// <summary>
    /// Script that makes child objects non-destroyable on scene changes.
    /// Only keeps one instance (the same) across the whole game.
    /// </summary>
    public class DontDestroyManager : MonoBehaviour
    {
        //reference to this script instance
        private static DontDestroyManager instance;
        
        //set the whole gameobject to 'dont destroy',
        //or destroy the other one if there's a duplicate
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
                Destroy(gameObject);
        }
    }
}