using UnityEngine;
using System.Collections.Generic;

//Add this to the character model, which should be a child of the overall character
public class CustomAnimController : MonoBehaviour
{
    public bool useAnimTranslation;
    public bool useAnimRotation;
    private Transform[] jointsInvolved;
    private int currentFrame;
    private CustomAnimCollection.CustomAnimation anim;

    public void initialize(string startingAnimation)
    {
        setAnimation(CustomAnimCollection.getAnimationByName(startingAnimation));
    }
    public void setAnimation(CustomAnimCollection.CustomAnimation anim)
    {
        this.anim = anim;
        //Reset transform to default local values and currentFrame to 0
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        currentFrame = 0;

        //Of all the joints involved in the animation, find the ones that are available and store
        //them in an array with the same indexes as the one in the animation. Use one queue search so as
        //not to take too much time
        jointsInvolved = new Transform[anim.jointsInvolved.Length];
        List<string> jointNames = new List<string>(anim.jointsInvolved);
        LinkedList<Transform> queue = new LinkedList<Transform>();
        queue.AddLast(transform);
        while (queue.Count > 0)
        {
            Transform check = queue.First.Value;
            for (int q = 0; q < check.childCount; q++)
            {
                Transform child = check.GetChild(q);
                if (child.GetComponent<Joint>() != null && jointNames.Contains(child.name))
                {
                    int idx = jointNames.IndexOf(child.name);
                    if (jointsInvolved[idx] != null)
                    {
                        Debug.LogError($"There are multiple joints called {child.name}, which could cause problems.");
                    }
                    jointsInvolved[idx] = child;
                }
                queue.AddLast(child);
            }
            queue.RemoveFirst();
        }
        //Decide whether you're using the fullbody transformations or not. Can be changed later, but
        //why would you? Maybe do this in another function, though
    }
    //We could have a general clock to notify all animators that it's time to go to the next
    //frame, thereby reducing Update methods and checks? The only reason this wouldn't work is if
    //people can move at different framerates
    public void nextAnimationFrame()
    {
        if (anim != null)
        {
            currentFrame = (currentFrame + 1) % anim.numFrames;
            if (useAnimRotation)
            {
                transform.rotation = anim.fullBodyRotation[currentFrame];
            }
            if (useAnimTranslation)
            {
                transform.position = anim.fullBodyTranslation[currentFrame];
            }
            for (int q = 0; q < jointsInvolved.Length; q++)
            {
                Transform j = jointsInvolved[q];
                if (j != null)
                {
                    j.rotation = anim.jointMovement[q][currentFrame];
                }
            }
        }
    }
}
