using GameEngine.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GameEngine.Attribute
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ComponentAttribute :System.Attribute
    {
        public List<IComponent> Components { get; private set; }
        public ComponentAttribute(IComponent value) => Components.Add(value);
    }
}
