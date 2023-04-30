using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BackgroundElements : MonoBehaviour
{
    const float MAX_DEPTH = 10;
    const float MIN_SLIDE = 0.5f;
    [Range(0, 1)] public float minSize = 0.65f;
    public float maxCamDist;
    Transform camT;
    float lastCamXPos;
    double lastUpdateTime;

    public float Depth => Mathf.Clamp01(transform.position.z / MAX_DEPTH);
    public float Slide => Mathf.Lerp(MIN_SLIDE, 1, Depth);

    // Start is called before the first frame update
    void Start()
    {
        UpdateRenderer();
        camT = Camera.main.transform;
        lastCamXPos = camT.position.x;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        UpdateRenderer();
    }
#endif

    private void LateUpdate()
    {
        transform.Translate((camT.position.x - lastCamXPos) * Slide, 0, 0);
        lastCamXPos = camT.position.x;

        float distToCam = camT.position.x - transform.position.x;
        if (Mathf.Abs(distToCam)>maxCamDist)
        {
            Vector3 pos = transform.position;
            pos.x = camT.position.x + maxCamDist * Mathf.Sign(distToCam);
            transform.position = pos;
        }
    }

    [ContextMenu("UpdateRenderer")]
    public void UpdateRenderer()
    {
#if UNITY_EDITOR
        if (EditorApplication.timeSinceStartup < lastUpdateTime + 0.01)
            return;
        lastUpdateTime = EditorApplication.timeSinceStartup;
#endif

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        LevelConstants constants = null;
        if (LevelManager.Instance != null)
            constants = LevelManager.Instance.levelConstants;
        else
            constants = FindObjectOfType<LevelManager>().levelConstants;

        renderer.sharedMaterial.SetColor("_BackgroundColor", constants.backgroundElementsColor);
        GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.black, Depth);
        transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * minSize, Depth);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(Camera.main.transform.position, Vector2.one * maxCamDist);
    }
#endif
}
