using System;
using System.Collections.Generic;
using System.Reflection;
using RuntimeInspectorNamespace;
using UnityEngine;

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
            if (string.IsNullOrEmpty(input))
            {
                input = "0";
            }
            var score = int.Parse(input);
            UpdateOtherCompData(score);
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

        private void UpdateOtherCompData(int score)
        {
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            foreach (var obj in selectedObjects)
            {
                var allInstances = EditorOp.Resolve<Atom>().AllScores;
                foreach (Atom.ScoreData atom in allInstances)
                {
                    var isUpdatingScore = score != 0;
                    if (obj == atom.target)
                    {
                        atom.score = score;
                     
                        EditorOp.Resolve<SceneDataHandler>()?.UpdateScoreModifiersCount(isUpdatingScore, atom.instanceId);
                    }
                }
            }
        }
    }
}
