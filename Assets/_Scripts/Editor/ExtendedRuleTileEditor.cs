#if UNITY_EDITOR
using UnityEngine;

namespace UnityEditor {
    [CustomEditor(typeof(ExtendedRuleTile))]
    [CanEditMultipleObjects]
    public class ExtendedRuleTileEditor : RuleTileEditor {
        public Texture2D AnyIcon;
        public Texture2D SpecifiedIcon;
        public Texture2D NothingIcon;

        public override void RuleOnGUI(Rect rect, Vector3Int position, int neighbor) {
            switch (neighbor) {
                case ExtendedRuleTile.Neighbor.Any:
                    GUI.DrawTexture(rect, AnyIcon);
                    return;
                case ExtendedRuleTile.Neighbor.Specified:
                    GUI.DrawTexture(rect, SpecifiedIcon);
                    return;
                case ExtendedRuleTile.Neighbor.Nothing:
                    GUI.DrawTexture(rect, NothingIcon);
                    return;
            }

            base.RuleOnGUI(rect, position, neighbor);
        }
    }
}
#endif