namespace ToolStates
{
    // this enum track the state whether the tool is in use
    public enum ToolUseStates
    {
        beforeUse,
        inUse,
        afterUse
    }
    
    // this enum track the state of how the tool is picked up
    public enum ToolPickUpStates
    {
        rightHand,
        doubleHands,
        doublePerson
    }

    // mark the type of the tool ... TODO: add all
    public enum ToolType{
        General,
        StableWheel,
        StableSill,
        TapeDispenser,
        SpringCenterPunch
    }
}