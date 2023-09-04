using System;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    public class ScoreField : StringField
    {
        public override bool SupportsType(Type type)
        {
            return typeof(Atom.ScoreData) == type;
        }

        protected override bool OnValueChanged(BoundInputField source, string input)
        {
            var value = base.OnValueChanged(source, input);
            var score = int.Parse(input);
            InvokeDataChange(score);
            return value;
        }

        public override void Refresh()
        {
            if (Value != null)
            {
                var value = (Atom.ScoreData)Value;
                input.Text = value.score.ToString();
            }
        }

        private void InvokeDataChange(int score)
        {
            var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            foreach (var selected in selections)
            {
                if (selected.TryGetComponent(out Collectible collectible))
                {
                    collectible.Score.score = score;
                    var isUpdatingScore = score != 0;
                    EditorOp.Resolve<SceneDataHandler>()?.UpdateScoreModifiersCount(isUpdatingScore, collectible.Score.instanceId);
                }
            }
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
        }
    }
}
