using System;
using UnityEngine;

public class DataManager
{
    public static DataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new DataManager();
            }
            return instance;
        }
    }
    private static DataManager instance;
}
