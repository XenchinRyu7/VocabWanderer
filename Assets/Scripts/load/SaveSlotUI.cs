using UnityEngine;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
    public GameObject indexObj,
        infoObj,
        dateObj;
    private SaveMenuUI menuUI;
    private int slotIndex;
    private bool isNew;

    public void SetData(SaveData data, bool isNew, SaveMenuUI menuUI, int slotIndex)
    {
        this.menuUI = menuUI;
        this.slotIndex = slotIndex;
        this.isNew = isNew;
        SetText(
            indexObj,
            isNew ? (SaveManager.Instance.saves.Count + 1).ToString() : data.index.ToString()
        );
        SetText(infoObj, isNew ? "New Save" : data.info);
        SetText(dateObj, isNew ? "" : data.dateSaved);
        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => menuUI.OnSlotSelected(slotIndex, isNew));
        }
    }

    private void SetText(GameObject obj, string value)
    {
        var tmp = obj.GetComponent<TMPro.TMP_Text>();
        if (tmp != null)
        {
            tmp.text = value;
            return;
        }
        var txt = obj.GetComponent<UnityEngine.UI.Text>();
        if (txt != null)
        {
            txt.text = value;
            return;
        }
    }
}
