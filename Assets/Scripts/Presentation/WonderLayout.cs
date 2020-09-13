using System;
using UnityEngine;
using UnityEngine.UI;

public class WonderLayout : MonoBehaviour
{
    // Raised when invalid game settings are defined.
    public class NotImplementedException : Exception
    {
        public NotImplementedException(string message) : base(message) { }
    }

    /// <summary>
    /// Setup the wonder steps layout.
    /// </summary>
    /// <param name="nbSteps">The number of buildable steps whithin the current wonder.</param>
    public void SetWonderLayout(int nbSteps)
    {
        HorizontalLayoutGroup layout = this.GetComponent<HorizontalLayoutGroup>();

        switch (nbSteps)
        {
            case 2: 
                layout.padding.left = 182;
                layout.spacing = 27.96F;
                break;
            case 3:
                layout.padding.left = 36;
                layout.spacing = 27.96F;
                break;
            case 4: 
                layout.padding.left = -11;
                layout.spacing = 12;
                break;
            default: 
                throw new NotImplementedException("Number of wonder steps is not matching existing layout");
        }
    }
}
