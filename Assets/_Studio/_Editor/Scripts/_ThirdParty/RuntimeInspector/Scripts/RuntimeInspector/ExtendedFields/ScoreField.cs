using System;
using System.Reflection;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    public class ScoreField : StringField
    {
        public override bool SupportsType(Type type)
        {
            return typeof(Atom.ScoreData) == type;
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            lastSubmittedValue = ((Atom.ScoreData)lastSubmittedValue).score.ToString();
        }

        protected override bool OnValueChanged(BoundInputField source, string input)
        {
            var value = base.OnValueChanged(source, input);
            if (input == "")
                input = "0";
            var score = int.Parse(input);
            InvokeDataChange(score);
            return value;
        }

        protected override bool OnValueSubmitted(BoundInputField source, string input)
        {
            if (Value != lastSubmittedValue)
            {
                EditorOp.Resolve<IURCommand>().Record(
                    lastSubmittedValue, input,
                    $"String changed to: {input}",
                    (value) =>
                    {
                        OnValueChanged(source, (string)value);
                        lastSubmittedValue = value;
                    });
                lastSubmittedValue = input;
            }
            Inspector.RefreshDelayed();
            return true;
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
    }
}
