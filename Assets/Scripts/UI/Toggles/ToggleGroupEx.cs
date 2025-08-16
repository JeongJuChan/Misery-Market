using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class ToggleGroupEx : ToggleGroup
{
    public List<Toggle> GetAllToggleRegistered()
    {
        return m_Toggles;
    }
}
