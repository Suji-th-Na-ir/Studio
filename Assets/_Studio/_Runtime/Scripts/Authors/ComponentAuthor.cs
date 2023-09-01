using UnityEngine;

namespace Terra.Studio
{
    public class ComponentAuthorOp
    {
        public static IAuthor Author => Author<ComponentAuthor>.Current;

        public static void Generate()
        {
            Author.Generate();
        }

        public static void Generate(object data)
        {
            Author.Generate(data);
        }

        public static void Degenerate(int entityID)
        {
            Author.Degenerate(entityID);
        }

        public static void Flush()
        {
            Author<ComponentAuthor>.Flush();
        }

        private class ComponentAuthor : BaseAuthor
        {
            public override void Generate(object data)
            {
                var tuple = (ComponentGenerateData)data;
                var compData = tuple.data;
                if (compData.Equals(default(EntityBasedComponent)))
                {
                    return;
                }
                var indivCompAuthor = RuntimeOp.Resolve<ComponentsData>().GetAuthorForType(compData.type);
                indivCompAuthor?.Generate(new ComponentAuthorData()
                {
                    entity = tuple.entity,
                    type = tuple.data.type,
                    compData = compData.data,
                    obj = tuple.obj
                });
            }
        }
    }
}
