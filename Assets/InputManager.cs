using System.Runtime.CompilerServices;
using UnityEngine;

namespace Ladder.Input
{

}
public sealed class inputManager : MonoBehaviour
{
    private static inputManager instance = null;

    inputManager() { }

    public static inputManager InputManager
    {
        get
        {
            if (instance == null)
            {
                instance = new inputManager();
            }
            return instance;
        }
    }
}
