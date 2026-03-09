using System;
using System.Collections;
using Battle.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation
{
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private Image hpFill;

        private Action<UnitView> _onClicked;
        private bool _isTargetable;

        public UnitState State { get; private set; }

        public void Initialize(UnitState state)
        {
            State = state;
            nameLabel.text = state.Definition.UnitName;
            UpdateHp(state.CurrentHp, state.Definition.BaseHp);

            if (state.Definition.Icon != null)
                spriteRenderer.sprite = state.Definition.Icon;
        }

        public void UpdateHp(int current, int max)
        {
            hpFill.fillAmount = Mathf.Clamp01((float)current / max);
        }

        public void SetTargetable(bool targetable, Action<UnitView> onClick = null)
        {
            _isTargetable = targetable;
            _onClicked = onClick;
            spriteRenderer.color = targetable ? new Color(1f, 0.85f, 0.1f) : Color.white;
        }

        public void PlayDeathAnimation()
        {
            StartCoroutine(DeathRoutine());
        }

        private void OnMouseDown()
        {
            if (_isTargetable)
                _onClicked?.Invoke(this);
        }

        private IEnumerator DeathRoutine()
        {
            float elapsed = 0f;
            float duration = 0.5f;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
