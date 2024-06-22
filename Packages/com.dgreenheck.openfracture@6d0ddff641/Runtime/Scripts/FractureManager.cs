using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FractureManager : MonoBehaviour
{
    public static FractureManager Instance;

    public uint FractureCount { get; private set; } = 0;

    Dictionary<uint, Fracture> fractureMap = new();
    Dictionary<Fracture, uint> idMap = new();

    private void Awake()
    {
        Instance = this;
    }

    public Fracture GetFracture(uint id)
    {
        return fractureMap.TryGetValue(id, out Fracture fracture) ? fracture : null;
    }

    public uint GetId(Fracture fracture)
    {
        return idMap.TryGetValue(fracture, out uint id) ? id : 0;
    }

    public void AddFracture(Fracture fracture)
    {
        fractureMap.Add(FractureCount, fracture);
        idMap.Add(fracture, FractureCount);

        FractureCount++;
    }
}
