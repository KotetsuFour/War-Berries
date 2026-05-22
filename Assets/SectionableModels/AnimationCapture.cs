using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class AnimationCapture : MonoBehaviour
{
    [SerializeField] private Transform subject; //Initial subject should not be a Joint
    [SerializeField] private List<string> exclude;
    [SerializeField] private string animationName;
    private List<Transform> joints;
    private List<string> jointNames;
    private List<List<Quaternion>> jointRotations;
    private List<Quaternion> fullBodyRotations;
    private List<Vector3> fullBodyTranslations;
    [SerializeField] private int startFrame;
    [SerializeField] private int numFramesToReview;
    [SerializeField] private bool loop;
    [SerializeField] private bool animating;
    [SerializeField] private int framesPerSecond;
    private float currentAnimationTime;
    private int currentFrame;
    [SerializeField] private Image framePrefab;
    [SerializeField] private Transform timelineContent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        joints = new List<Transform>();
        jointNames = new List<string>();
        jointRotations = new List<List<Quaternion>>();
        fullBodyRotations = new List<Quaternion>();
        fullBodyTranslations = new List<Vector3>();
        LinkedList<Transform> queue = new LinkedList<Transform>();
        queue.AddLast(subject);
        while (queue.Count > 0)
        {
            Transform check = queue.First.Value;
            for (int q = 0; q < check.childCount; q++)
            {
                Transform child = check.GetChild(q);
                if (child.GetComponent<SectionJoint>() != null && !exclude.Contains(child.name))
                {
                    if (jointNames.Contains(child.name))
                    {
                        Debug.LogError($"There are multiple joints called {child.name}, which could cause problems.");
                    }
                    joints.Add(child);
                    jointNames.Add(child.name);
                    jointRotations.Add(new List<Quaternion>());
                }
                queue.AddLast(child);
            }
            queue.RemoveFirst();
        }
    }
    public void captureFrameAtEnd()
    {
        int frame = currentNumberOfFrames() + 1;
        if (joints.Count <= 0)
        {
            Debug.LogError("There is no skeleton to capture!");
            return;
        }
        if (frame <= 0)
        {
            Debug.LogError("Frame number must be 1 or higher");
            return;
        }

        //Catch up on frames
        while (currentNumberOfFrames() < frame)
        {
            for (int q = 0; q < joints.Count; q++)
            {
                jointRotations[q].Add(joints[q].localRotation);
            }
            fullBodyRotations.Add(subject.localRotation);
            fullBodyTranslations.Add(subject.position);
            createFrameDisplay();
        }
        //Adjust to normal indexing
        frame--;

        //Modify the necessary frame
        for (int q = 0; q < joints.Count; q++)
        {
            jointRotations[q][frame] = joints[q].localRotation;
        }
        fullBodyRotations[frame] = subject.localRotation;
        fullBodyTranslations[frame] = subject.position;

        //Notify user
        Debug.Log($"Captured frame #{frame + 1}/{currentNumberOfFrames()}!");
        timelineContent.GetChild(currentFrame).GetComponent<Image>()
            .color = Color.white;
        currentFrame = frame;
        timelineContent.GetChild(frame).GetComponent<Image>()
            .color = Color.cyan;
    }
    public void createFrameDisplay()
    {
        int frame = currentNumberOfFrames();
        Image newFrame = Instantiate(framePrefab, timelineContent);
        Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
        click.AddListener(delegate { revisitFrame(frame); });
        newFrame.GetComponent<Button>().onClick = click;
        StaticData.findDeepChild(newFrame.transform, "FrameNumber")
            .GetComponent<TextMeshProUGUI>().text = $"{currentNumberOfFrames()}";
    }
    public void revisitFrame(int frame /*Starting with frame #1*/)
    {
        if (animating)
        {
            Debug.Log("Can't revisit frame during playback");
            return;
        }

        //Index adjustment
        frame--;

        if (joints.Count < 0 || frame >= currentNumberOfFrames())
        {
            Debug.Log("That frame doesn't exist");
        }
        subject.localRotation = fullBodyRotations[frame];
        subject.position = fullBodyTranslations[frame];
        for (int q = 0; q < joints.Count; q++)
        {
            joints[q].localRotation = jointRotations[q][frame];
        }

        //Notify user
        timelineContent.GetChild(currentFrame).GetComponent<Image>()
            .color = Color.white;
        currentFrame = frame;
        timelineContent.GetChild(frame).GetComponent<Image>()
            .color = Color.cyan;

        Debug.Log($"Now viewing frame #{frame + 1}/{currentNumberOfFrames()}.");
    }
    public void back()
    {
        int toFrame = currentFrame - 1;
        revisitFrame(toFrame == 0 ? currentNumberOfFrames() : toFrame);
    }
    public int currentNumberOfFrames()
    {
        return fullBodyRotations.Count;
    }
    public void next()
    {
        revisitFrame((currentFrame % currentNumberOfFrames()) + 1);
    }
    public void startReplay()
    {
        animating = true;
        currentFrame = startFrame;
        currentAnimationTime = 0;
        revisitFrame(currentFrame);
    }
    public void stopReplay()
    {
        animating = false;
    }
    public void printAnimation()
    {
        string construct = $"CustomAnimation {animationName} = new CustomAnimation();";
        construct += $"\n{animationName}.numFrames = {currentNumberOfFrames()};";

        construct += $"\n{animationName}.jointsInvolved = new string[] {{";
        for (int q = 0; q < jointNames.Count; q++)
        {
            if (q % 4 == 0)
            {
                construct += "\n";
            }
            construct += $"\"{jointNames[q]}\",";
        }
        construct += "\n};";

        construct += $"\n{animationName}.jointMovement = new Quaternion[] {{";
        for (int q = 0; q < jointRotations.Count; q++)
        {
            construct += "\n new Quaternion[] {";
            for (int w = 0; w < jointRotations[q].Count; w++)
            {
                if (w % 4 == 0)
                {
                    construct += "\n";
                }
                Vector3 eul = jointRotations[q][w].eulerAngles;
                construct += $"Quaternion.Euler({eul.x}, {eul.y}, {eul.z}),";
            }
            construct += "\n}";
        }
        construct += "\n}";

        construct += $"\n{animationName}.fullBodyTranslation = new Vector3[] {{";
        for (int q = 0; q < fullBodyTranslations.Count; q++)
        {
            if (q % 4 == 0)
            {
                construct += "\n";
            }
            Vector3 now = fullBodyTranslations[q];
            construct += $"new Vector3({now.x}, {now.y}, {now.z}),";
        }
        construct += "\n};";

        construct += $"\n{animationName}.fullBodyRotation = new Quaternion[] {{";
        for (int q = 0; q < fullBodyRotations.Count; q++)
        {
            if (q % 4 == 0)
            {
                construct += "\n";
            }
            Vector3 eul = fullBodyRotations[q].eulerAngles;
            construct += $"Quaternion.Euler({eul.x}, {eul.y}, {eul.z}),";
        }
        construct += "\n};\n";

        StreamWriter write = new StreamWriter(File.Create($"Assets/{animationName}.txt"));
        write.Write(construct);
        write.Close();
        Debug.Log($"The construction statements for the animation were printed in {animationName}.txt");
    }

    void Update()
    {
        if (animating)
        {
            currentAnimationTime += Time.deltaTime;
            currentFrame = Mathf.FloorToInt(currentAnimationTime * framesPerSecond);
            if (currentFrame > numFramesToReview)
            {
                currentFrame = startFrame;
            }
            else
            {
                currentFrame += startFrame;
            }
        }
    }
}
