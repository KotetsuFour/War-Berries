using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattlefieldCreator : MonoBehaviour
{
    [SerializeField] private LayerMask tileLayer;
    [SerializeField] private LayerMask wmTeamLayer;
    public const int STANDARD_POSITION_OFFSET = -1;
    public const float MY_COLLIDER_VERTICAL = 0.1f;
    public const int FINITE_HIGH_NUMBER = 1000;
    public Battle setup(WMTeam[] initialEngagement)
    {
        StaticData.pauseWM(null);
        int coreMinX = int.MaxValue;
        int coreMaxX = int.MinValue;
        int coreMinY = int.MaxValue;
        int coreMaxY = int.MinValue;
        float elevation = float.MaxValue;
        List<Tile> coreTiles = new List<Tile>();

        foreach (WMTeam team in initialEngagement)
        {
            Collider initialUnitCollider = team.GetComponent<Collider>();
            elevation = Mathf.Min(elevation,
                (initialUnitCollider.bounds.center.y - (initialUnitCollider.bounds.extents.y / 2)) + STANDARD_POSITION_OFFSET);
            RaycastHit[] iHits = Physics.BoxCastAll(initialUnitCollider.bounds.center,
                initialUnitCollider.bounds.extents / 2,
                Vector3.down, Quaternion.Euler(-90, 0, 0), FINITE_HIGH_NUMBER, tileLayer);
            foreach (RaycastHit hit in iHits)
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (coreTiles.Contains(tile))
                {
                    continue;
                }
                coreTiles.Add(tile);
                coreMinX = Mathf.Min(coreMinX, tile.x);
                coreMaxX = Mathf.Max(coreMaxX, tile.x);
                coreMinY = Mathf.Min(coreMinY, tile.y);
                coreMaxY = Mathf.Max(coreMaxY, tile.y);
            }
        }

        Battle ret = new Battle(coreMinX, coreMaxX, coreMinY, coreMaxY);
        float middleX = (((float)(coreMaxX - coreMinX)) / 2) + coreMinX;
        float middleY = (((float)(coreMaxY - coreMinY)) / 2) + coreMinY;
        transform.position = new Vector3(middleX, STANDARD_POSITION_OFFSET, middleY);

        BoxCollider myCollider = GetComponent<BoxCollider>();
        myCollider.size = new Vector3(coreMaxX - coreMinX, MY_COLLIDER_VERTICAL, coreMaxY - coreMinY);
        RaycastHit[] cHits = Physics.BoxCastAll(myCollider.center, myCollider.bounds.extents / 2,
            Vector3.up,
            Quaternion.Euler(90, 0, 0), FINITE_HIGH_NUMBER, wmTeamLayer);

        List<WMTeam> coreTeamObjects = new List<WMTeam>();
        List<WMTeam> allyTeamObjects = new List<WMTeam>();

        foreach (RaycastHit hit in cHits)
        {
            WMTeam team = hit.collider.GetComponent<WMTeam>();
            coreTeamObjects.Add(team);
            ret.addCoreTeam(team.getTeam(), team.transform.position, team.transform.rotation);
        }

        foreach (WMTeam team in coreTeamObjects)
        {
            Collider tileColliders = team.GetComponent<Collider>();
            Collider[] allies = Physics.OverlapSphere(tileColliders.bounds.center, WMTeam.SPACECRAFT_ASSIST_RANGE,
                wmTeamLayer);
            foreach (Collider aHit in allies)
            {
                WMTeam wCheck = aHit.GetComponent<WMTeam>();
                CharacterTeam tCheck = wCheck.getTeam();
                if (ret.completeTeamsList.Contains(tCheck))
                {
                    continue;
                }
                ret.addAllyTeam(tCheck, wCheck.transform.position, wCheck.transform.rotation);
            }
        }

        int periMinX = int.MaxValue;
        int periMaxX = int.MinValue;
        int periMinY = int.MaxValue;
        int periMaxY = int.MinValue;
        List<Tile> periTiles = new List<Tile>();
        foreach (WMTeam team in allyTeamObjects)
        {
            Collider allyColliders = team.GetComponent<Collider>();
            RaycastHit[] aHits = Physics.BoxCastAll(allyColliders.bounds.center, allyColliders.bounds.extents / 2,
                Vector3.down, Quaternion.Euler(-90, 0, 0), FINITE_HIGH_NUMBER, tileLayer);
            foreach (RaycastHit hit in aHits)
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (coreTiles.Contains(tile) || periTiles.Contains(tile))
                {
                    continue;
                }
                periTiles.Add(tile);
                periMinX = Mathf.Min(periMinX, tile.x);
                periMaxX = Mathf.Max(periMaxX, tile.x);
                periMinY = Mathf.Min(periMinY, tile.y);
                periMaxY = Mathf.Max(periMaxY, tile.y);
            }
        }

        middleX = (((float)(periMaxX - periMinX)) / 2) + periMinX;
        middleY = (((float)(periMaxY - periMinY)) / 2) + periMinY;
        transform.position = new Vector3(middleX, STANDARD_POSITION_OFFSET, middleY);

        ret.setPeripheral(periMinX, periMaxX, periMinY, periMaxY);

        return ret;
    }
}
