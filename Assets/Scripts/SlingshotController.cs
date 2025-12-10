using UnityEngine;

public class SlingshotController : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;

    [Header("Anchor Points")]
    public Transform leftAnchor;
    public Transform rightAnchor;
    public Transform centerRest;

    [Header("Band Renderers")]
    public LineRenderer leftBand;
    public LineRenderer rightBand;

    [Header("Band Settings")]
    public float bandWidth = 0.1f;
    public Color bandColor = new Color(0.4f, 0.2f, 0.1f);
    public int bandSegments = 2;

    private Vector3 restPosition;
    private bool isStretched = false;
    private bool tensionTriggered = false;

    void Start()
    {
        if (centerRest != null)
            restPosition = centerRest.position;
        else
            restPosition = (leftAnchor.position + rightAnchor.position) / 2f;

        SetupBand(leftBand);
        SetupBand(rightBand);

        ResetBands();
        PlayIdleAnimation();
    }

    private void SetupBand(LineRenderer band)
    {
        if (band == null) return;

        band.positionCount = bandSegments;
        band.startWidth = bandWidth;
        band.endWidth = bandWidth;
        band.startColor = bandColor;
        band.endColor = bandColor;

        band.material = new Material(Shader.Find("Sprites/Default"));
        band.material.color = bandColor;
        band.useWorldSpace = true;
    }


    public void UpdateBands(Vector3 birdPosition)
    {
        isStretched = true;

        // Only trigger tension animation once when dragging starts
        if (!tensionTriggered)
        {
            PlayTensionAnimation();
            tensionTriggered = true;
        }

        if (leftBand)
        {
            leftBand.SetPosition(0, leftAnchor.position);
            leftBand.SetPosition(1, birdPosition);
        }

        if (rightBand)
        {
            rightBand.SetPosition(0, rightAnchor.position);
            rightBand.SetPosition(1, birdPosition);
        }
    }

    public void ResetBands()
    {
        isStretched = false;
        tensionTriggered = false; // Reset tension flag for next drag

        if (leftBand)
        {
            leftBand.SetPosition(0, leftAnchor.position);
            leftBand.SetPosition(1, restPosition);
        }

        if (rightBand)
        {
            rightBand.SetPosition(0, rightAnchor.position);
            rightBand.SetPosition(1, restPosition);
        }

        PlayIdleAnimation();
    }


    public void AnimateReset(float duration = 0.15f)
    {
        if (!isStretched) return;

        PlayReleaseAnimation();
        StartCoroutine(SmoothResetCoroutine(duration));
    }

    private System.Collections.IEnumerator SmoothResetCoroutine(float duration)
    {
        Vector3 startPos = leftBand.GetPosition(1);

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            Vector3 currentPos = Vector3.Lerp(startPos, restPosition, t);

            if (leftBand) leftBand.SetPosition(1, currentPos);
            if (rightBand) rightBand.SetPosition(1, currentPos);

            yield return null;
        }

        ResetBands();
    }



    void PlayIdleAnimation()
    {
        if (animator)
        {
            animator.SetTrigger("Idle");
            animator.Play("Idle");
            Debug.Log("SlingshotController: Playing Idle animation.");
        }
    }

    void PlayTensionAnimation()
    {
        if (animator)
        {
            animator.SetTrigger("Tension");
            animator.Play("Tension");
            Debug.Log("SlingshotController: Playing Tension animation.");
        }
    }

    void PlayReleaseAnimation()
    {
        if (animator)
        {
            animator.SetTrigger("Release");
            animator.Play("Release");
            Debug.Log("SlingshotController: Playing Release animation.");
        }
    }

    public void ShowBands(bool show)
    {
        if (leftBand) leftBand.enabled = show;
        if (rightBand) rightBand.enabled = show;
    }
}
