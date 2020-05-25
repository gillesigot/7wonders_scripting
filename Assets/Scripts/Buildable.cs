using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buildable : MonoBehaviour
{
    // TODO buildable should actually be playable -> build is a separate logic concept
    // Used to define all different building types.
    public enum BuildType 
    { 
        RESOURCE = 0, 
        WAR = 1, 
        CIVIL = 2, 
        COMMERCIAL = 3, 
        SCIENCE = 4, 
        GUILD = 5, 
    };
    // Used to defint the current building type.
    public BuildType buildType = BuildType.CIVIL;

    /// <summary>
    /// Build the building:
    /// - Set it in the right build zone.
    /// </summary>
    /// <returns>True if the building has been built.</returns>
    public bool Build()
    {
        Transform buildZone = this.GetBuildZone();
        Draggable drag = this.GetComponent<Draggable>();

        drag.parentToReturnTo = buildZone;

        return true;
    }

    /// <summary>
    /// Get the corresponding building zone to the current buliding type.
    /// </summary>
    /// <returns>The buliding zone.</returns>
    public Transform GetBuildZone()
    {
        return GameObject.Find("build_zones").transform.GetChild((int)this.buildType);
    }
}
