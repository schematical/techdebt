using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class UIAttentionIcon : MonoBehaviour, IPointerClickHandler
    {


        public SpriteRenderer spriteRenderer;
        public Image uiImage;
        protected Transform targetTransform;
        protected UnityAction onClick;
        public TextMeshProUGUI text;
        protected bool isOffScreen;
        private Camera _cam;
        private Vector2 baseTextAnchoredPosition;
        private bool _hasCapturedBaseTextPos;


        public void Show(Transform _transform, Color color, UnityAction _onClick, string _text = null)
        {
            transform.SetAsFirstSibling();
            targetTransform = _transform;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(color.r, color.g, color.b, 0.5f);
            }
            if (uiImage != null)
            {
                uiImage.color = new Color(color.r, color.g, color.b, 0.5f);
            }
            onClick = _onClick;
            
            if (GameManager.Instance.UIManager.attentionIconBoarderPanel != null)
            {
                Canvas canvas = GameManager.Instance.UIManager.attentionIconBoarderPanel.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    _cam = canvas.worldCamera;
                }
            }
            if (_cam == null) _cam = Camera.main;
            if (_text != null)
            {
                text.text = _text;
            }
            else
            {
                text.text = "xxx";
            }

            if (uiImage != null)
            {
                text.color = uiImage.color;
            }

            if (text != null && !_hasCapturedBaseTextPos)
            {
                baseTextAnchoredPosition = text.rectTransform.anchoredPosition;
                _hasCapturedBaseTextPos = true;
            }

        }

        void Update()
        {
            if (targetTransform == null)
            {
                gameObject.SetActive(false);
                return;
            }
            
            if (transform.GetSiblingIndex() != 0)
            {
                transform.SetAsFirstSibling();
            }

            if (_cam == null) _cam = Camera.main;

            // Check if any Map Panel is currently open/opening via UIManager
            bool isMapOpen = GameManager.Instance.UIManager.IsAnyMapOpen();

            if (isMapOpen)
            {
                transform.localScale = Vector3.zero;
                return; // Skip drawing, positioning, and rotation while hidden
            }

            if (_cam != null && _cam.orthographic)
            {
                float referenceZoom = 5f;
                float scaleValue = referenceZoom / _cam.orthographicSize;
                scaleValue = Mathf.Clamp(scaleValue, 0.4f, 2.0f);
                transform.localScale = new Vector3(scaleValue, scaleValue, 1f);
            }
            else
            {
                transform.localScale = Vector3.one;
            }

            float padding = 50f;
            Vector3 targetPosition = targetTransform.position + new Vector3(0f, 2f, -1f);
            Vector3 screenPos = _cam.WorldToScreenPoint(targetPosition);

            float minX, maxX, minY, maxY;


            Vector3[] corners = new Vector3[4];
            GameManager.Instance.UIManager.attentionIconBoarderPanel.GetWorldCorners(corners);
            Vector3 bottomLeftScreen = RectTransformUtility.WorldToScreenPoint(_cam, corners[0]);
            Vector3 topRightScreen = RectTransformUtility.WorldToScreenPoint(_cam, corners[2]);

            minX = bottomLeftScreen.x;
            maxX = topRightScreen.x;
            minY = bottomLeftScreen.y;
            maxY = topRightScreen.y;

            isOffScreen = screenPos.x <= minX + padding || screenPos.x >= maxX - padding ||
                               screenPos.y <= minY + padding || screenPos.y >= maxY - padding || screenPos.z < 0;
         
            Canvas canvas = GetComponentInParent<Canvas>();
            Camera uiCam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
            RectTransform parentRect = transform.parent as RectTransform;

            Vector2 targetScreenPos;
            if (!isOffScreen)
            {
                targetScreenPos = new Vector2(screenPos.x, screenPos.y);
            }
            else
            {
                if (screenPos.z < 0)
                {
                    screenPos.x = Screen.width - screenPos.x;
                    screenPos.y = Screen.height - screenPos.y;
                }

                float clampedX = Mathf.Clamp(screenPos.x, minX + padding, maxX - padding);
                float clampedY = Mathf.Clamp(screenPos.y, minY + padding, maxY - padding);
                targetScreenPos = new Vector2(clampedX, clampedY);
            }

            // Calculate the parabolic float offset in screen space (10 pixels height, 1s period)
            Vector2 floatDirection = Vector2.up;
            if (isOffScreen)
            {
                Vector2 diff = (Vector2)screenPos - targetScreenPos;
                if (diff.sqrMagnitude > 0.001f)
                {
                    floatDirection = diff.normalized;
                }
            }

            float cycleTime = Time.time % 1f;
            float parabolicFactor = 4f * cycleTime * (1f - cycleTime); // perfect parabola 0 -> 1 -> 0
            float offsetPixels = parabolicFactor * 10f;
            Vector2 finalScreenPos = targetScreenPos + (floatDirection * offsetPixels);

            if (parentRect != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, finalScreenPos, uiCam, out Vector3 worldPoint))
            {
                transform.position = worldPoint;
            }
            else
            {
                if (!isOffScreen)
                {
                    transform.position = targetPosition + new Vector3(0f, offsetPixels * 0.02f, 0f);
                }
                else
                {
                    Vector3 clampedScreenPos = new Vector3(finalScreenPos.x, finalScreenPos.y, screenPos.z);
                    clampedScreenPos.z = _cam.nearClipPlane + 0.1f;
                    transform.position = _cam.ScreenToWorldPoint(clampedScreenPos);
                }
            }

            if (!isOffScreen)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.transform.localRotation = Quaternion.identity;
                }
                if (uiImage != null)
                {
                    uiImage.transform.localRotation = Quaternion.identity;
                }
                if (text != null)
                {
                    text.rectTransform.anchoredPosition = baseTextAnchoredPosition;
                    text.transform.localRotation = Quaternion.identity;
                }
            }
            else
            {
                Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
                float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg + 90;
                if (spriteRenderer != null)
                {
                    spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, angle);
                }
                if (uiImage != null)
                {
                    uiImage.transform.localRotation = Quaternion.Euler(0, 0, angle);
                }
                if (text != null)
                {
                    Vector3 rotatedPos = Quaternion.Euler(0, 0, angle) * (Vector3)baseTextAnchoredPosition;
                    text.rectTransform.anchoredPosition = new Vector2(rotatedPos.x, rotatedPos.y);
                    text.transform.localRotation = Quaternion.identity;
                }
            }
        }

        public bool IsOffScreen()
        {
            return isOffScreen;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                onClick.Invoke();
            }
           
        }
    }

}