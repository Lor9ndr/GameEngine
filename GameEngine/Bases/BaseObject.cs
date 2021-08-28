using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Bases
{
    public abstract class BaseObject
    {
        private List<IComponent> _components = new List<IComponent>();
        public IComponent GetComponent<IComponent>() => _components.OfType<IComponent>().First();
        protected void AddComponent(IComponent component) => _components.Add(component);
    }
}
