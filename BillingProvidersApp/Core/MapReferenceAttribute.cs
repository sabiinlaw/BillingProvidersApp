using System;

namespace BillingProvidersApp.Core
{
    /// <summary>
    /// Attribute for mapping reference to some business object to class property/field.
    /// </summary>
    public class MapReferenceAttribute : Attribute
    {
        Type referencedType;
        string referencePropertyName;

        /// <summary>
        /// Maps property/field to other object.
        /// </summary>
        /// <param name="referencedType">Referenced type.</param>
        public MapReferenceAttribute(Type referencedType) : this(referencedType, string.Empty) { }

        /// <summary>
        /// Maps property/field to other object.
        /// </summary>
        /// <param name="referencedType">Referenced type.</param>
        /// <param name="referencePropertyName">Reference property name.</param>
        public MapReferenceAttribute(Type referencedType, string referencePropertyName)
        {
            this.referencedType = referencedType;
            this.referencePropertyName = referencePropertyName;
        }


        /// <summary>
        /// Referenced type.
        /// </summary>
        virtual public Type ReferencedType
        {
            get { return referencedType; }
        }

        /// <summary>
        /// Reference property name.
        /// </summary>
        virtual public string ReferencePropertyName
        {
            get { return referencePropertyName; }
        }
    }
}
