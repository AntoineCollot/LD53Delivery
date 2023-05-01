using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIScore : MonoBehaviour
{
    [Header("Components")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI bloodText;
    public TextMeshProUGUI bloodValueText;
    public TextMeshProUGUI scoreText;
    public Image mailImage;

    public Sprite[] mailSprites;

    public GameObject[] afterAnim;

    [Header("Settings")]
    const float TIME_SCROLL_DURATION = 2;
    const float PAUSE_TIMES = 0.75f;

    // Start is called before the first frame update
    void OnEnable()
    {
        timeText.gameObject.SetActive(false);
        bloodText.gameObject.SetActive(false);
        bloodValueText.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
        mailImage.gameObject.SetActive(false);

        foreach (GameObject go in afterAnim)
        {
            go.SetActive(false);
        }

        StartCoroutine(Anim());
    }

    IEnumerator Anim()
    {
        float t = 0;

        timeText.gameObject.SetActive(true);
        double finalTime = (double)(new decimal(ScoreSystem.Instance.levelTime));
        while (t<1)
        {
            t += Time.deltaTime / TIME_SCROLL_DURATION;

            TimeSpan span = TimeSpan.FromSeconds(finalTime * (double)t);
            timeText.text = "Time : "+span.ToString(@"mm\:ss\:ff") + " +";

            yield return null;
        }

        yield return new WaitForSeconds(PAUSE_TIMES);

        bloodText.gameObject.SetActive(true);

        yield return new WaitForSeconds(PAUSE_TIMES);

        mailImage.gameObject.SetActive(true);

        int finalBloodId = Mathf.RoundToInt(ScoreSystem.Instance.BloodLevel * 6) + 7;
        for (int i = 0; i < finalBloodId; i++)
        {
            mailImage.sprite = mailSprites[i];
            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(PAUSE_TIMES);

        bloodValueText.text = ScoreSystem.Instance.Bloodiness;
        bloodValueText.gameObject.SetActive(true);

        yield return new WaitForSeconds(PAUSE_TIMES);

        scoreText.gameObject.SetActive(true);
        scoreText.text = "= "+ScoreSystem.Instance.Score.ToString("N0");

        yield return new WaitForSeconds(PAUSE_TIMES);

        foreach(GameObject go in afterAnim)
        {
            go.SetActive(true);
        }
    }
}
