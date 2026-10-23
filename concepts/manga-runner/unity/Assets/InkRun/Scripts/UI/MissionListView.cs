using UnityEngine;
using TMPro;
using UnityEngine.UI;
using InkRun.Services;

namespace InkRun.UI
{
    /// <summary>Three rows: mission text, progress, bar. Row prefab has Text, Count, Bar (Image, filled).</summary>
    public class MissionListView : MonoBehaviour
    {
        public Transform Container;
        public GameObject RowPrefab;

        public void Refresh()
        {
            for (int i = Container.childCount - 1; i >= 0; i--) Destroy(Container.GetChild(i).gameObject);
            foreach (var m in Services.Services.Missions.Today())
            {
                var row = Instantiate(RowPrefab, Container);
                row.transform.Find("Text").GetComponent<TMP_Text>().text = m.Text + (m.Done ? " ✓" : "");
                row.transform.Find("Count").GetComponent<TMP_Text>().text = m.Done ? "+" + m.Reward : m.Progress + "/" + m.Target;
                var bar = row.transform.Find("Bar");
                bar.gameObject.SetActive(!m.Done);
                bar.GetComponent<Image>().fillAmount = (float)m.Progress / m.Target;
            }
        }
    }
}
