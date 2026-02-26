using UnityEngine;

namespace srUtils.Unity
{
    public static class Utils
    {
	    public static LayerMask GetPhysicsLayerMask(int layer)
	    {
		    //TODO: caching
		    int output = 0;
		    for (int a = 0; a < 32; a++)
		    {
			    if (Physics.GetIgnoreLayerCollision(layer, a)) output |= 1 << a;
		    }
		    return output;
	    }
    }
}