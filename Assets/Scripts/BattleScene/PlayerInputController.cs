using UnityEngine;

namespace BattleScene
{
    public class PlayerInputController : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;

        private void OnEnable()
        {
            if (battleManager == null)
            {
                battleManager = FindObjectOfType<BattleManager>();
            }
        }

        private void Update()
        {
            if (battleManager == null) return;
            if (!battleManager.IsDecisionWindow) return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                TrySelect(0);
            }
            else if (Input.GetKeyDown(KeyCode.J))
            {
                TrySelect(1);
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                TrySelect(2);
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                TrySelect(3);
            }
        }

        private void TrySelect(int index)
        {
            battleManager.TrySelectPlayerSkill(index, out _);
        }
    }
}
