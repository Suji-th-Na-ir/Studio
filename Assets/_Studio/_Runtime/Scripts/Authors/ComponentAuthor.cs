using System;
using Newtonsoft.Json;
using System.Reflection;
using UnityEngine.Scripting;
using System.Collections.Generic;

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

        public static void Flush()
        {
            Author<ComponentAuthor>.Flush();
        }

        private class ComponentAuthor : BaseAuthor
        {
            private Dictionary<Type, MethodInfo> cachedMethodForTypes = new();

            public override void Generate(object data)
            {
                var generatedData = (ComponentGenerateData)data;
                var compData = generatedData.data;
                if (compData.Equals(default(EntityBasedComponent)))
                {
                    return;
                }
                var isDataPresent = SystemOp.Resolve<System>().SystemData.TryGetComponentAndSystemForType(generatedData.data.type, out var component, out var system);
                if (!isDataPresent)
                {
                    return;
                }
                var authorData = new ComponentAuthorData()
                {
                    entity = generatedData.entity,
                    type = generatedData.data.type,
                    compData = generatedData.data.data,
                    obj = generatedData.obj
                };
                var method = GetMethodForTypes(component, system);
                method.Invoke(this, new object[] { authorData });
            }

            [Preserve]
            private void Generate<T1, T2>(ComponentAuthorData authorData)
                                                        where T1 : struct, IBaseComponent
                                                        where T2 : BaseSystem<T1>
            {
                var component = JsonConvert.DeserializeObject<T1>(authorData.compData);
                var world = RuntimeOp.Resolve<RuntimeSystem>().World;
                var pool = world.GetPool<T1>();
                pool.Add(authorData.entity);
                ref var compRef = ref pool.Get(authorData.entity);
                ((IBaseComponent)component).Clone(component, ref compRef, authorData.obj);
                var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<T1>(typeof(T2));
                instance.Init(authorData.entity);
            }

            private MethodInfo GetMethodForTypes(Type componentType, Type systemType)
            {
                if (!cachedMethodForTypes.TryGetValue(componentType, out var method))
                {
                    method = typeof(ComponentAuthor).GetMethod(nameof(Generate), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(componentType, systemType);
                    cachedMethodForTypes.Add(componentType, method);
                }
                return method;
            }
        }
    }
}
