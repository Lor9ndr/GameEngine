﻿using GameEngine.Bases.Components;
using System;
using System.Collections.Generic;
using System.Linq;


namespace GameEngine.Bases
{
    public abstract class BaseObject:IDisposable
    {
        private List<IComponent> _components = new List<IComponent>();

        public virtual void Dispose()
        {
        }

        public IComponent GetComponent<IComponent>() => _components.OfType<IComponent>().First();
        protected void AddComponent(IComponent component) => _components.Add(component);
    }
}
