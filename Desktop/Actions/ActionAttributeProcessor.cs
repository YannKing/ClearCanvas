#region License

// Copyright (c) 2013, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This file is part of the ClearCanvas RIS/PACS open source project.
//
// The ClearCanvas RIS/PACS open source project is free software: you can
// redistribute it and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// The ClearCanvas RIS/PACS open source project is distributed in the hope that it
// will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General
// Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// the ClearCanvas RIS/PACS open source project.  If not, see
// <http://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;

namespace ClearCanvas.Desktop.Actions
{
    /// <summary>提供处理在给定目标上声明的操作属性集的方法对象，通常是一种工具。
    /// Provides methods for processing the set of action attributes declared on a given target
    /// object, which is typically a tool.
    /// </summary>
    internal static class ActionAttributeProcessor
    {
        /// <summary>处理在给定目标对象上声明的动作属性集以生成相应的<see cref ="IAction"/>对象集。
        /// Processes the set of action attributes declared on a given target object to generate the
        /// corresponding set of <see cref="IAction"/> objects.
        /// </summary>
        /// 
        /// <param name="actionTarget">声明属性的目标对象，通常是工具。
        /// The target object on which the attributes are declared, typically a tool.</param>
        /// <returns>生成的一组操作，其中每个操作都绑定到目标对象。
        /// The resulting set of actions, where each action is bound to the target object.</returns>
        internal static IAction[] Process(object actionTarget)
        {
            object[] attributes = actionTarget.GetType().GetCustomAttributes(typeof(ActionAttribute), true);

            // first pass - create an ActionBuilder for each initiator of the specified type
            // 第一次传递 - 为指定类型的每个启动器创建一个ActionBuilder
            List<ActionBuildingContext> actionBuilders = new List<ActionBuildingContext>();
            foreach (ActionAttribute a in attributes)
            {
                if (a is ActionInitiatorAttribute)
                {
                    ActionBuildingContext actionBuilder = new ActionBuildingContext(a.QualifiedActionID(actionTarget), actionTarget);
                    a.Apply(actionBuilder);
                    actionBuilders.Add(actionBuilder);
                }
            }

            // second pass - apply decorators to all ActionBuilders with same actionID
            // 第二遍 - 将装饰器应用于具有相同actionID的所有ActionBuilders
            foreach (ActionAttribute a in attributes)
            {
                if (a is ActionDecoratorAttribute)
                {
                    foreach (ActionBuildingContext actionBuilder in actionBuilders)
                    {
                        if (a.QualifiedActionID(actionTarget) == actionBuilder.ActionID)
                        {
                            a.Apply(actionBuilder);
                        }
                    }
                }
            }

            List<IAction> actions = new List<IAction>();
            foreach (ActionBuildingContext actionBuilder in actionBuilders)
            {
                actions.Add(actionBuilder.Action);
            }

            return actions.ToArray();
        }
    }
}
