using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem Instance;

    public GridLayout gridLayout;
    public Tilemap MainTailMap;
    public Tilemap TempTailMap;

    private static Dictionary<TileType, TileBase> tileBases = new Dictionary<TileType, TileBase>();

    private Building temp;
    private Vector3 prevPos;
    private BoundsInt prevArea;

    #region Unity Methods
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        string tilePath = @"Tiles\";
        tileBases.Add(TileType.Empty, null);
        tileBases.Add(TileType.White, Resources.Load<TileBase>(tilePath + "white"));
        tileBases.Add(TileType.Green, Resources.Load<TileBase>(tilePath + "green"));
        tileBases.Add(TileType.Red, Resources.Load<TileBase>(tilePath + "red"));

    }

    // Update is called once per frame
    void Update()
    {
        if (!temp)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject(0))
            {
                return;
            }
            if (!temp.Placed)
            {
                Debug.Log("49a girdi");
                Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPos = gridLayout.LocalToCell(touchPos);
                if (prevPos != cellPos)
                {
                    Debug.Log("56a girdi");
                    temp.transform.localPosition = gridLayout.CellToLocalInterpolated(cellPos + new Vector3(.5f, .5f, 0f));
                    prevPos = cellPos;
                    FollowBuilding();
                }
            }
        }
    }
    #endregion

    #region Tilemap Management

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;
        foreach (var v in area.allPositionsWithin)
        {
            Debug.Log(v);
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            array[counter] = tilemap.GetTile(pos);
            counter++;
        }
        return array;
    }
    private static void SetTilesBlock(BoundsInt area, TileType type, Tilemap tilemap)
    {
        int size = area.size.x * area.size.z;
        TileBase[] tileArray = new TileBase[size];
        FillTiles(tileArray, type);
        tilemap.SetTilesBlock(area, tileArray);
    }

    private static void FillTiles(TileBase[] arr, TileType type)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = tileBases[type];
        }
    }


    #endregion

    #region Building Placement

    public void InitializeWithBuilding(GameObject building)
    {
        temp = Instantiate(building, Vector3.zero, Quaternion.identity).GetComponent<Building>();
        FollowBuilding();
    }

    private void ClearArea()
    {
        TileBase[] toClear = new TileBase[prevArea.size.x * prevArea.size.y * prevArea.size.z];
        FillTiles(toClear, TileType.Empty);
        TempTailMap.SetTilesBlock(prevArea, toClear);
    }
    private void FollowBuilding()
    {
        ClearArea();
        temp.area.position = gridLayout.WorldToCell(temp.gameObject.transform.position);
        BoundsInt buildingArea = temp.area;

        TileBase[] baseArray = GetTilesBlock(buildingArea, MainTailMap);

        int size = baseArray.Length;
        TileBase[] tileArray = new TileBase[size];

        for (int i = 0; i < baseArray.Length; i++)
        {
            if (baseArray[i] == tileBases[TileType.White])
            {
                tileArray[i] = tileBases[TileType.Green];
            }
            else
            {
                FillTiles(tileArray, TileType.Red);
                break;
            }
        }
        TempTailMap.SetTilesBlock(buildingArea, tileArray);
        prevArea = buildingArea;
    }
    //private bool CanTakeArea()
    //{

    //}

    #endregion
}
public enum TileType
{
    Empty,
    White,
    Green,
    Red

}