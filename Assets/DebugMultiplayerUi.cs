using Ladder.PlayerMovement1Helpers;
using Ladder.PlayerMovementHelpers;
using UnityEngine;
using UnityEngine.UI;

namespace Ladder.DebugHelper
{
    public class DebugMultiplayerUi : MonoBehaviour
    {
        private static DebugMultiplayerUi instance;
        public static DebugMultiplayerUi Instance
        {
            get => instance;
        }

        [SerializeField] public Toggle InterpolationToggle;
        [SerializeField] public Toggle ExtrapolationToggle;
        [SerializeField] public Slider AheadTicksSlider;
        [SerializeField] public Slider RedundantTicksSlider;

        private void Awake()
        {
            instance = this;
        }
    }
}
