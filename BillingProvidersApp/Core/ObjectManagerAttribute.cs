using System;

namespace BillingProvidersApp.Core
{
    /// <summary>
    /// BO manager attribute. Shows which items managed by the manager class.
    /// </summary>
    public class ObjectManagerAttribute : Attribute
    {
        Type itemType;

        /// <summary>
        /// Creates new attribute.
        /// </summary>
        /// <param name="itemType">Type of managed items.</param>
        public ObjectManagerAttribute(Type itemType)
        {
            this.itemType = itemType;
        }

        /// <summary>
        /// Type of managed items.
        /// </summary>
        public Type ItemType
        {
            get { return itemType; }
        }
    }
}
