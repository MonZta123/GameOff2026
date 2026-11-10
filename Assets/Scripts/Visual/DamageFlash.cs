using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Color flashColour = Color.white;
    [SerializeField] private float flashTime = 0.25f;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material spriteMaterial;

    private Coroutine damageFlashCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponentsInChildren<SpriteRenderer>();
        spriteMaterial = _spriteRenderer.material;

    }

    // Call when taking damage in player or enemy script
    public void CallDamageFlash()
    {
        damageFlashCoroutine = StartCoroutine(DamageFlasher());
    }

    private IEnumerator DamageFlasher()
    {
        SetFlashColour();

        float currentFlashAmount = 0f;
        float elapsedTime = 0f;

        while(elapsedTime < flashTime)
        {
            elapsedTime += Time.deltaTime;

            currentFlashAmount = Mathf.Lerp(1f, 0f, (elapsedTime/flashTime));
            spriteMaterial.SetFloat("_FlashAmount", currentFlashAmount);

            yield return null;
        }
    }

    private void SetFlashColour()
    {
        spriteMaterial.SetColor("_FlashColour", flashColour);
    }
}