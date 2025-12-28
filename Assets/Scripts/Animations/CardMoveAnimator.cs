using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class CardMoveAnimator : MonoBehaviour
{
    private static Transform animationContainer;

    public static void AnimateCardMovement(GameObject originalCard, Vector3 targetPosition, float duration, bool destroyOriginal, Action onComplete = null)
    {
        if (originalCard == null)
        {
            Debug.LogWarning("[CardMoveAnimator] Objeto original é nulo.");
            onComplete?.Invoke();
            return;
        }

        // Cria container de animação se necessário
        if (animationContainer == null)
        {
            GameObject containerObj = new GameObject("CardAnimationContainer", typeof(RectTransform));
            Canvas canvas = containerObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            CanvasScaler scaler = containerObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            containerObj.AddComponent<GraphicRaycaster>();

            animationContainer = containerObj.transform;
            DontDestroyOnLoad(containerObj);
        }

        // Instancia a cópia visual fora do LayoutGroup
        GameObject visualCopy = Instantiate(originalCard, animationContainer);

        // Preserva escala, tamanho e posição
        RectTransform originalRect = originalCard.GetComponent<RectTransform>();
        RectTransform copyRect = visualCopy.GetComponent<RectTransform>();

        if (originalRect != null && copyRect != null)
        {
            // Copia os parâmetros visuais
            copyRect.anchorMin = new Vector2(0.5f, 0.5f);
            copyRect.anchorMax = new Vector2(0.5f, 0.5f);
            copyRect.pivot = originalRect.pivot;

            // Força escala e tamanho
            float sizeFloat = 15f;
            copyRect.localScale = new Vector3(sizeFloat, sizeFloat, sizeFloat);
            copyRect.sizeDelta = originalRect.rect.size;

            // Define posição exata no mundo
            Vector3 worldPosition = originalRect.position;
            copyRect.position = worldPosition;
        }
        else
        {
            visualCopy.transform.position = originalCard.transform.position;
            visualCopy.transform.localScale = originalCard.transform.localScale;
        }

        visualCopy.transform.SetAsLastSibling();

        // Remove componentes lógicos
        foreach (var comp in visualCopy.GetComponents<MonoBehaviour>())
        {
            comp.enabled = false;
        }

        CardMoveAnimator animator = visualCopy.AddComponent<CardMoveAnimator>();
        animator.StartAnimation(originalCard, visualCopy, targetPosition, duration, destroyOriginal, onComplete);
    }

    private GameObject originalCard;
    private GameObject visualCard;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float duration;
    private bool destroyOriginal;
    private Action onComplete;

    private void StartAnimation(GameObject original, GameObject visual, Vector3 target, float time, bool destroy, Action callback)
    {
        originalCard = original;
        visualCard = visual;
        startPos = original.transform.position;
        targetPos = target;
        duration = time;
        destroyOriginal = destroy;
        onComplete = callback;

        visualCard.transform.position = startPos;
        StartCoroutine(MoveToTarget());
    }

    private System.Collections.IEnumerator MoveToTarget()
    {
        

        while (Vector3.Distance(visualCard.transform.position, targetPos) > 0.01f)
        {
            float step = Vector3.Distance(startPos, targetPos) / duration * Time.deltaTime;
            visualCard.transform.position = Vector3.MoveTowards(visualCard.transform.position, targetPos, step);
            yield return null;
        }

        visualCard.transform.position = targetPos;

        Destroy(visualCard);
        onComplete?.Invoke();
    }
}
