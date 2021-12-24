using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Auralyte.Configuration {

    /// <summary>
    /// A property set of an <see cref="Aura"/>, containing a list of <see cref="Property"/>s and <see cref="Condition"/>s.
    /// </summary>
    public class PropertySet {
        [JsonProperty("id")]            [DefaultValue(-1)]      public int id = -1;
        [JsonProperty("properties")]    [DefaultValue(null)]    public List<Property> properties = new();
        [JsonProperty("conditions")]    [DefaultValue(null)]    public List<Condition> conditions = new();

        public void Reindex() {
            for(int i = 0; i < properties.Count; i++) {
                properties[i].id = i + 1;
            }

            for(int i = 0; i < conditions.Count; i++) {
                conditions[i].id = i + 1;
            }
        }

        public PropertySet Clone() {
            PropertySet clone = new PropertySet();
            clone.id = id;
            clone.properties = properties.Select((Property property) => { return property.Clone(); }).ToList();
            clone.conditions = conditions.Select((Condition condition) => { return condition.Clone(); }).ToList();

            return clone;
        }
    }
}
