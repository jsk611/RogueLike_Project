using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Prototype : MonoBehaviour
{
    [SerializeField] GameObject tile;
    [SerializeField] GameObject laser;
    [SerializeField] TMP_Text text;
    GameObject[] tiles;
    
    // 타일을 2*4*2 크기로 4*4개를 배치
    void Start()
    {
        tiles = new GameObject[8 * 8];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                tiles[i * 8 + j] = Instantiate(tile, new Vector3(i * 2, 0, j * 2), Quaternion.identity);
            }
        }

        StartCoroutine(StartSearch());
    }
    //SearchTile, SearchTileBinary, SearchTileRandom 코루틴을 무작위로 실행하는 걸 반복하는 코루틴
    IEnumerator StartSearch()
    {
        while (true)
        {
            int randomIndex = Random.Range(0, tiles.Length);
            int randomMethod = Random.Range(0, 3);
            switch (randomMethod)
            {
                case 0:
                    yield return StartCoroutine(SearchTile(randomIndex));
                    break;
                case 1:
                    yield return StartCoroutine(SearchTileBinary(randomIndex));
                    break;
                case 2:
                    yield return StartCoroutine(SearchTileRandom(randomIndex));
                    break;
            }
        }
    }


    //tiles 배열을 반전시키고 모든 타일을 0.5초동안 회색으로 바꾼뒤 흰색으로 변경하는 코루틴
    IEnumerator ReverseTiles()
    {
        text.text = "Reverse Tiles";
        // tiles 배열을 반전
        for (int i = 0; i < tiles.Length / 2; i++)
        {
            GameObject temp = tiles[i];
            tiles[i] = tiles[tiles.Length - 1 - i];
            tiles[tiles.Length - 1 - i] = temp;
        }
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.gray);
        }
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
        }
        yield return new WaitForSeconds(0.5f);
    }

    //타일의 랜덤 위치를 선형 탐색하는 과정을 시각적으로 보여주는 과정을 코루틴으로 작성
    IEnumerator SearchTile(int index)
    {
        //50% 확률로 tiles 배열을 반전
        if (Random.Range(0, 2) == 0)
        {
            yield return StartCoroutine(ReverseTiles());
        }
        text.text = "Linear Search";

        for (int i = 0; i < tiles.Length; i++)
        {
            if (i == index)
            {
                tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.green);
                yield return new WaitForSeconds(0.1f);
                tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
                break;
            }
            else
            {
                tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.red);
                Destroy(Instantiate(laser, tiles[i].transform.position + new Vector3(0, 1, 0), Quaternion.identity), 2f);
                yield return new WaitForSeconds(0.1f);
                tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
            }

        }
        yield return new WaitForSeconds(1f);
        //StartCoroutine(SearchTileBinary(Random.Range(1, tiles.Length)));
    }
    //이번엔 이분 탐색으로 타일을 찾는 과정을 코루틴으로 작성. 타일을 분할할 때 목표 타일이 포함된 부분 배열의 모든 타일을 빨간색으로 바꾸고 나머지는 흰색으로 바꿔서 시각적으로 보여줌. 각 과정마다 빨간색은 0.5초만 표시하고 흰색으로 변경
    IEnumerator SearchTileBinary(int index)
    {
        //50% 확률로 tiles 배열을 반전
        if (Random.Range(0, 2) == 0)
        {
            yield return StartCoroutine(ReverseTiles());
        }
        text.text = "Binary Search";
        int left = 0;
        int right = tiles.Length - 1;

        while (left <= right)
        {
            int mid = (left + right) / 2;

            // 강조: 현재 탐색 범위만 빨간색
            if(right - left < tiles.Length - 1)
            {
                for (int i = left; i <= right; i++)
                {
                    tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.red);
                    Destroy(Instantiate(laser, tiles[i].transform.position + new Vector3(0, 1, 0), Quaternion.identity), 2f);
                }
            }
            

            // 0.5초 동안 강조 유지
            yield return new WaitForSeconds(0.5f);

            // 범위 복구: 강조했던 타일만 원래대로
            for (int i = left; i <= right; i++)
            {
                tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
            }
            yield return new WaitForSeconds(0.5f);
            // 이분 탐색 진행
            if (mid == index)
            {
                tiles[mid].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.green);
                yield return new WaitForSeconds(0.3f);
                tiles[mid].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
                break;
            }
            else if (mid < index)
            {
                left = mid + 1;
            }
            else
            {
                right = mid;
            }
        }
        yield return new WaitForSeconds(1f);
        //StartCoroutine(SearchTile(Random.Range(1, tiles.Length)));
    }
    //20회 랜덤 탐색하는 걸 시각화하는 코루틴
    IEnumerator SearchTileRandom(int index)
    {
        //50% 확률로 tiles 배열을 반전
        if (Random.Range(0, 2) == 0)
        {
            yield return StartCoroutine(ReverseTiles());
        }
        text.text = "Random Search";
        for (int i = 0; i < 20; i++)
        {
            int randomIndex = Random.Range(0, tiles.Length);
            //해당 타일이 목표 타일이면 초록색으로 바꾸고 루프 종료
            if (randomIndex == index)
            {
                tiles[randomIndex].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.green);
                yield return new WaitForSeconds(0.3f);
                tiles[randomIndex].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
                break;
            }
            //아니면 빨간색으로 바꾸기
            else
            {
                tiles[randomIndex].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.red);
                Destroy(Instantiate(laser, tiles[randomIndex].transform.position + new Vector3(0, 1, 0), Quaternion.identity), 2f);
                yield return new WaitForSeconds(0.1f);
                tiles[randomIndex].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
            }


          
        }
    }





}
