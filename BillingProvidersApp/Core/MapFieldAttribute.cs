using System;

namespace BillingProvidersApp.Core
{
    /// <summary>
    /// Attribute for mapping database field to class property/field by DB field name.
    /// </summary>
    public class MapFieldAttribute : Attribute
    {
        string fieldName;     // DB field name
        bool isPrimaryKey;  // true if this field is primary key

        /// <summary>
        /// Maps property/field to a DB field with the same name.
        /// </summary>
        public MapFieldAttribute() : this(string.Empty, false) { }

        /// <summary>
        /// Maps property/field to a DB field with the same name.
        /// </summary>
        /// <param name="isPrimaryKey">True if this field is primary key.</param>
        public MapFieldAttribute(bool isPrimaryKey) : this(string.Empty, isPrimaryKey) { }

        /// <summary>
        /// Maps property/field to a DB field with <paramref name="fieldName" /> name.
        /// </summary>
        /// <param name="fieldName">DB field name to map.</param>
        public MapFieldAttribute(string fieldName) : this(fieldName, false) { }

        /// <summary>
        /// Maps property/field to a DB field with <paramref name="fieldName" /> name.
        /// </summary>
        /// <param name="fieldName">DB field name to map.</param>
        /// <param name="isPrimaryKey">True if this field is primary key.</param>
        public MapFieldAttribute(string fieldName, bool isPrimaryKey)
        {
            this.fieldName = fieldName;
            this.isPrimaryKey = isPrimaryKey;
        }


        /// <summary>
        /// Provides DB field name to which property/field is mapped.
        /// </summary>
        virtual public string FieldName
        {
            get { return fieldName; }
        }

        /// <summary>
        /// Returns true if this field is primary key.
        /// </summary>
        virtual public bool IsPrimaryKey
        {
            get { return isPrimaryKey; }
        }
    }
}
