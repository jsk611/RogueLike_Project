using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RankEntryUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Image starIcon;      // ����
    [SerializeField] private Image background;    // �� ���(���̶���Ʈ/��Ʈ������)

    [Header("Style")]
    [SerializeField] private Color normalRow = Color.white;
    [SerializeField] private Color altRow = new Color(0.94f, 0.96f, 1f);
    [SerializeField] private Color selfHighlight = new Color(0.95f, 1f, 0.85f);
    [SerializeField] private Color rank1Color = new Color(1f, 0.84f, 0.2f); // gold
    [SerializeField] private Color rank2Color = new Color(0.75f, 0.75f, 0.8f);
    [SerializeField] private Color rank3Color = new Color(0.8f, 0.55f, 0.3f);

    public void Bind(int rank, string playerName, int score, bool isSelf, bool useAltRow)
    {
        // �ؽ�Ʈ
        rankText.text = rank.ToString();
        nameText.text = TrimName(playerName);
        scoreText.text = score.ToString("N0"); // õ���� �޸�

        // �� ������: �������� ���� �ܺο��� �ѵ� �ǰ� �״�� �ֵ� ��
        if (starIcon) starIcon.enabled = true;

        // ���/���̶���Ʈ
        if (background)
            background.color = isSelf ? selfHighlight : (useAltRow ? altRow : normalRow);

        // ���� ���� ����
        var c = Color.white;
        if (rank == 1) c = rank1Color;
        else if (rank == 2) c = rank2Color;
        else if (rank == 3) c = rank3Color;

        rankText.color = c;
        // 1~3���� �̸��� ��¦ ����
        nameText.color = (rank <= 3) ? c : Color.white;
    }

    private string TrimName(string s, int max = 18)
    {
        if (string.IsNullOrEmpty(s)) return "Unknown";
        if (s.Length <= max) return s;
        return s.Substring(0, Mathf.Max(0, max - 1)) + "��";
    }
}
