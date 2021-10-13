using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reflex
{
    public Boat boatSC;
    public GameObject reflex;
    public GameObject boat;
    public SpriteRenderer renderer;

    public Coroutine coroutine;
    public Reflex(GameObject _obj, GameObject _boat, SpriteRenderer _render, Boat _boatSC)
    {
        reflex = _obj; boat = _boat; renderer = _render; boatSC = _boatSC;
    }
    public void Gameover()
    {
        reflex.SetActive(false);
        ChangeCoroutine(DisappearCoroutine());
    }
    public void GameReset()
    {
        reflex.SetActive(true);
        ChangeCoroutine(RePositionCoroutine());
    }
    public void ChangeCoroutine(IEnumerator rout)
    {
        if(coroutine!=null)
        boatSC.StopCoroutine(coroutine);
        coroutine = boatSC.StartCoroutine(rout);
    }
    public IEnumerator DisappearCoroutine()
    {
        Color color;
        while (renderer.color.a>0)
        {
            color = renderer.color;
            color.a -= 0.1f;
            renderer.color = color;
            reflex.transform.position = new Vector3(boat.transform.position.x, 0, 0);

            yield return new WaitForSeconds(0.05f);
        }

    }
    public IEnumerator RePositionCoroutine()
    {
        Color color;
        while (true)
        {
            color = renderer.color;
            color.a = Random.Range(0.45f, 0.55f);
            renderer.color = color;
            reflex.transform.position = new Vector3(boat.transform.position.x, 0, 0);

            yield return new WaitForSeconds(0.05f);
        }
    }
}
public class Boat : MonoBehaviour
{
    public Rigidbody2D rigid;
    public SpriteRenderer render;
    public Collider2D colli;
    public float boatHealth = 10;
    public Slider healthSlider;
    public bool wet = false;
    public bool keep = false;

    public Coroutine continuousDamage;

    public Vector3 resetPosition;
    public Quaternion resetRotation;

    public GameObject reflex;
    public Reflex _reflex;

    private void Awake()
    {
        GameManager.gameManager.GameStart += GameStart;
        GameManager.gameManager.GameReset += GameReset;

        GameManager.gameManager.Goal += GoalHealing;

        #region Reflex
        _reflex = new Reflex(reflex, gameObject, reflex.GetComponent<SpriteRenderer>(), this);
        GameManager.gameManager.GameOver += _reflex.Gameover;
        GameManager.gameManager.GameReset += _reflex.GameReset;
        _reflex.coroutine = StartCoroutine(_reflex.RePositionCoroutine());
        #endregion
    }
    public void GameStart()
    {
        continuousDamage = StartCoroutine(InGameBoatCoroutine());
    }
    public IEnumerator InGameBoatCoroutine()
    {
        while (boatHealth > 0)
        {
            StartCoroutine(Wetboat("continuous"));


            yield return new WaitForSeconds(0.5f);
        }
    }
    public void WaterHit()
    {
        if (wet == false)
        {
            wet = true;
            StartCoroutine(Wetboat("pour"));
        }
    }
    public bool resurQustion = false;
    public IEnumerator Wetboat(string whatIsDamage)
    {
        if (resurQustion)
            yield break;

        if (boatHealth > 0)
        {
            switch (whatIsDamage)
            {
                case "pour":
                    HealthSlider(-1);
                    break;
                case "continuous":
                    HealthSlider(0);
                    break;
            }
        }
        if(boatHealth <= 0)
        {
            if (!GameManager.gameManager.Resurrected) //이미 부활 했다면 죽음------------------------------------ 광고 넣고나서 ! 떼기 광고 없을 땐 !
                BoatDead();

            else // 부활한적이 없다면 부활 질문
            {
                //물끄기
                GameManager.gameManager.pour.waterParticle.SetActive(false);
                GameManager.gameManager.bucket.gameObject.SetActive(false);
                StopCoroutine(GameManager.gameManager.backGround.BucketPour());

                resurQustion = true;
                StopCoroutine(continuousDamage);
                GameManager.gameManager.adsCoroutine = StartCoroutine(GameManager.gameManager.QResurrect());
            }
        }
        yield return new WaitForSeconds(0.5f);
        wet = false;
    }
    public void GoalHealing()
    {
        var time = Time.realtimeSinceStartup - GameManager.gameManager.lastGoaltime;
        if (time < 6)
        {
            HealthSlider(1 * (6-time));
        }
    }
    public void HealthSlider(float health) 
    {
        boatHealth += health;
        healthSlider.value += health;
        if (boatHealth > healthSlider.maxValue)
        {
            boatHealth = healthSlider.maxValue;
            healthSlider.value = healthSlider.maxValue;
        }
        
        //healthSlider.value = healthSlider.maxValue - boatHealth;
    }
    public void BoatDead()
    {
        rigid.constraints = RigidbodyConstraints2D.None;
        rigid.mass = 1.3f;
        StopCoroutine(continuousDamage);
        GameManager.gameManager.GameOver.Invoke();
    }
    public void GameReset()
    {
        rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.position = resetPosition;
        transform.rotation = resetRotation;
        rigid.velocity = Vector2.zero;
        rigid.mass = 1;
        wet = false;

        resetPosition = transform.position;
        resetRotation = transform.rotation;

        int paperBoatNumber = GameManager.gameManager.store.paperBoats.FindIndex(x => x == GameManager.gameManager.currentItem.paperBoat);
        boatHealth = 10 + paperBoatNumber;
        healthSlider.maxValue = boatHealth;
        healthSlider.value = healthSlider.maxValue;
    }
}
