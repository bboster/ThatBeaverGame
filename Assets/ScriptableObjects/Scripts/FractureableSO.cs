using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractureableSO : ScriptableObject
{
    [Header("Custom Fracture Options")]
    [Tooltip("The force required for GnawAttack to cause fracture.")]
    public float minForceToTrigger;

    [Header("Base Fracture Options")]
    public TriggerOptions triggerOptions;
    public FractureOptions fractureOptions;
    public RefractureOptions refractureOptions;
    public CallbackOptions callbackOptions;
}
