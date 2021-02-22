namespace QAToolKit.Engine.HttpTester.Test.Fixtures
{
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class note
    {
        private string toField;

        private string fromField;

        private string headingField;

        private string bodyField;

        /// <remarks/>
        public string to
        {
            get { return this.toField; }
            set { this.toField = value; }
        }

        /// <remarks/>
        public string from
        {
            get { return this.fromField; }
            set { this.fromField = value; }
        }

        /// <remarks/>
        public string heading
        {
            get { return this.headingField; }
            set { this.headingField = value; }
        }

        /// <remarks/>
        public string body
        {
            get { return this.bodyField; }
            set { this.bodyField = value; }
        }
    }
}