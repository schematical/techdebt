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

            if (_cam != null && _cam.orthographic)
            {
                float referenceZoom = 5f;
                float scaleValue = referenceZoom / _cam.orthographicSize;
                scaleValue = Mathf.Clamp(scaleValue, 0.4f, 2.0f);
                transform.localScale = new Vector3(scaleValue, scaleValue, 1f);
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

            if (parentRect != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, targetScreenPos, uiCam, out Vector3 worldPoint))
            {
                transform.position = worldPoint;
            }
            else
            {
                if (!isOffScreen)
                {
                    transform.position = targetPosition;
                }
                else
                {
                    Vector3 clampedScreenPos = new Vector3(targetScreenPos.x, targetScreenPos.y, screenPos.z);
                    clampedScreenPos.z = _cam.nearClipPlane + 0.1f;
                    transform.position = _cam.ScreenToWorldPoint(clampedScreenPos);
                }
            }

            if (!isOffScreen)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.transform.rotation = Quaternion.identity;
                }
                if (uiImage != null)
                {
                    uiImage.transform.rotation = Quaternion.identity;
                }
            }
            else
            {
                Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
                float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg + 90;
                if (spriteRenderer != null)
                {
                    spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);
                }
                if (uiImage != null)
                {
                    uiImage.transform.rotation = Quaternion.Euler(0, 0, angle);
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