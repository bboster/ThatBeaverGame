using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NetworkingSO/SceneTransitionData")]
public class SceneTransitionDataSO : ScriptableObject
{
    public string SceneName;
    public string TransitionAnimName;
    public float StayTime = 0.5f;
}
