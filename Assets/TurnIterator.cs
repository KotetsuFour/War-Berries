using UnityEngine;

public class TurnIterator : MonoBehaviour
{
    private bool working;
    public void startIterating()
    {

    }
    // Update is called once per frame
    void Update()
    {
        if (working)
        {
            foreach (Affiliation aff in StaticData.affiliations)
            {
                float turnValue = Mathf.Abs(1000 - ((float)aff.getPopulation() / aff.getTotalTileCount()));
                //TODO
            }
        }
    }
}
