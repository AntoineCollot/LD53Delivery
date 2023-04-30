using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[ExecuteInEditMode]
public class MapBuilder : MonoBehaviour
{
#if UNITY_EDITOR
    const int CELL_SIZE = 1;
    const int MAP_LAYER = 6;
    public LayerMask mapMask = 1 << MAP_LAYER;

    public Transform cellParent;

    Camera cam;

    [Header("Blocks")]
    public List<Sprite> blockLibrary = new List<Sprite>();

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    private void OnEnable()
    {
        //EditorApplication.update += MapEditorUpdate;
    }

    private void OnDisable()
    {
        //EditorApplication.update -= MapEditorUpdate;
    }

    // Update is called once per frame
    void Update()
    {
        MapEditorUpdate();
    }

    void MapEditorUpdate()
    {
        //Create
        if (Input.GetMouseButtonDown(0))
        {
            WanderBlock(1);
        }
        if (Input.GetMouseButtonDown(1))
        {
            WanderBlock(-1);
        }
        //Delete
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Delete))
        {
            GameObject obj = GetObjectAtMousePos();
            if (obj != null)
                DestroyCell(obj);
        }

        //Rotate
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            RotateCell(-1);
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            RotateCell(1);
        }
        //Inverse
        if (Input.GetKeyDown(KeyCode.R))
        {
            InverseCell();
        }

        //Prefabs
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
           
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            
        }
        //Prefabs
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
           
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Vector2Int coords = GetMouseCoords();
            //Instantiate(leverBunchPrefab, new Vector3(coords.x, coords.y), Quaternion.identity, wallBunchParent);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            Vector2Int coords = GetMouseCoords();
            //Instantiate(obstaclePrefab, new Vector3(coords.x, coords.y), Quaternion.identity, otherParent);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            Vector2Int coords = GetMouseCoords();
            //Instantiate(furniturePrefab, new Vector3(coords.x, coords.y), Quaternion.identity, otherParent);
        }
    }

    void RotateCell(int dir)
    {
        SpriteRenderer renderer = GetRendererAtMousePos();
        if (renderer == null)
            return;

        if (dir > 0)
            renderer.transform.Rotate(Vector3.forward * -90);
        else
            renderer.transform.Rotate(Vector3.forward * 90);
    }

    void InverseCell()
    {
        GameObject obj = GetObjectAtMousePos();
        if (obj == null)
            return;

        Vector3 scale = obj.transform.localScale;
        scale.x *= -1;
        obj.transform.localScale = scale;
    }

    void WanderBlock(int dir)
    {
        SpriteRenderer renderer = GetRendererAtMousePos();

        //No object yet, create one
        if (renderer == null)
        {
            renderer = CreateSpriteCell(GetMouseCoords());
        }

        //Find the ID
        int id = GetSpiteID(renderer.sprite);

        if (dir > 0)
        {
            id++;
        }
        if (dir < 0)
        {
            if (id == -1)
                id = blockLibrary.Count;
            id--;
        }

        //If we reached the end, del block
        if (id >= blockLibrary.Count || id < 0)
        {
            DestroyCell(renderer.gameObject);
        }
        else
        {
            renderer.sprite = blockLibrary[id];
        }
    }

    int GetSpiteID(Sprite sprite)
    {
        if (sprite == null)
            return -1;

        if (blockLibrary.Contains(sprite))
            return blockLibrary.IndexOf(sprite);

        return -1;
    }

    void DestroyCell(GameObject go)
    {
        Destroy(go);
    }

    SpriteRenderer CreateSpriteCell(Vector2Int coords)
    {
        GameObject newCell = new GameObject("Cell_" + coords.ToString(), typeof(SpriteRenderer), typeof(BoxCollider2D));
        newCell.transform.SetParent(cellParent, false);
        newCell.transform.position = new Vector3(coords.x, coords.y);
        newCell.layer = MAP_LAYER;
        newCell.GetComponent<BoxCollider2D>().size = Vector2.one * CELL_SIZE;

        SpriteRenderer renderer = newCell.GetComponent<SpriteRenderer>();
        renderer.sortingLayerName = "LevelForeground";
        renderer.sortingOrder = 0;
        return renderer;
    }

    GameObject GetObjectAtMousePos()
    {
        Collider2D hitCollider = Physics2D.OverlapPoint(GetMouseCoords());
        if(hitCollider.gameObject.layer != MAP_LAYER) return null;
        if (hitCollider == null)
            return null;

        return hitCollider.gameObject;
    }

    SpriteRenderer GetRendererAtMousePos()
    {
        Collider2D hitCollider = Physics2D.OverlapPoint(GetMouseCoords(), mapMask);
        if (hitCollider == null)
            return null;

        SpriteRenderer renderer = hitCollider.GetComponent<SpriteRenderer>();
        return renderer;
    }

    Vector2Int CoordsFromPos(Vector2 position)
    {
        Vector2Int coords = new Vector2Int();
        coords.x = Mathf.RoundToInt(position.x / CELL_SIZE) * CELL_SIZE;
        coords.y = Mathf.RoundToInt(position.y / CELL_SIZE) * CELL_SIZE;
        return coords;
    }

    Vector2Int GetMouseCoords()
    {
        if (cam == null)
            cam = Camera.main;
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        return CoordsFromPos(mouseWorldPos);
    }

    private void OnDrawGizmos()
    {
        Color col = new Color(1, 1, 1, 0.05f);

        Vector2Int coords = GetMouseCoords();
        Vector3 pos = new Vector3(coords.x, coords.y, 0);
        Gizmos.DrawWireCube(pos, Vector3.one * CELL_SIZE);
    }
#endif
}
