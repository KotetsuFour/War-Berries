using UnityEngine;
using System.Collections.Generic;

public static class CustomAnimCollection
{
    private static Dictionary<string, CustomAnimation> animationCollection;
    public static void initialize()
    {
        animationCollection = new Dictionary<string, CustomAnimation>();
    }
    public static CustomAnimation getAnimationByName(string animName)
    {
        return animationCollection[animName];
    }
    public class CustomAnimation
    {
        public string name;
        public int numFrames;
        public string[] jointsInvolved;
        public Quaternion[][] jointMovement;
        public Vector3[] fullBodyTranslation;
        public Quaternion[] fullBodyRotation;
    }
}
