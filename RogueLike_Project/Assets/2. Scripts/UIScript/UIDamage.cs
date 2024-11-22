using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class UIDamage : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] Transform player;
    [SerializeField] float initialScale = 1f;
    public float damage = 0f;

    // Start is called before the first frame update
    void Start()
    {
        //text = GetComponentInChildren<TMP_Text>();
        text.text = damage.ToString();
        Destroy(gameObject, 1f);
        player = FindObjectOfType<PlayerControl>().gameObject.transform;
    }
    private void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        // 크기 조정
        float scale = Mathf.Max(0.7f, distance * initialScale);
        transform.localScale = new Vector3(scale, scale, scale);
    }

}
