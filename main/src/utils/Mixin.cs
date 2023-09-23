using System;

namespace Myriad.utils; 

/// <summary>
/// Helper attribute used within code to just give a quick way of getting to a modified target
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class Mixin : Attribute  {

    public Mixin(Type target) {
        this.Target = target;
    }
    
    public Type Target {
        set;
        get;
    }
}