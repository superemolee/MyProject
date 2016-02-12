using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Firefighter.ToolStates
{

    // this enum track the state whether the tool is in use
    public enum ToolUseStates
    {
        beforeUse,
        inUse,
        afterUse
    }

    // this states is using for multiuser-need-same tool problem while picking up
    public enum ToolOwnerStates
    {
        noOwner,
        waitingForOwner,
        hasOwner
    }
    
    // this enum track the state of how the tool is picked up
    public enum ToolPickUpStates
    {
        rightHand,
        doubleHands,
        doublePerson
    }

    // mark the type of the tool ... TODO: add all
    [Serializable]
    public enum ToolType{
        General,
        StableWheel,
        StableSill,
        TapeDispenser,
        SpringCenterPunch
    }
}