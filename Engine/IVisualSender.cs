using System;
using System.Collections;

/// <summary>
/// Members of this interface are allowed to update 1D visuals.
/// </summary>
public interface IVisualCaller
{
    public event EventHandler VisualEvent;
}