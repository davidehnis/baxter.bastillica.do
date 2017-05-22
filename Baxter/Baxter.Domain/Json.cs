using Newtonsoft.Json;

namespace Baxter.Domain
{
    //<summary>Contains and facilitates communication via JSON</summary>
    public class Json : Construct
    {
        #region Protected Fields
        protected object _entity;
        protected string _string = string.Empty;
        #endregion Protected Fields

        #region Public Constructors
        public Json(object entity) : base(typeof(Json))
        {
            _entity = entity;
            _string = Serialize(entity);
        }

        public Json(string message) : base(typeof(Json))
        {
            _string = message;
        }

        public Json(object entity, string message) : this(entity)
        {
            _string = message;
        }
        #endregion Public Constructors

        #region Public Methods
        //<summary>Converts the entity into JSON format</summary>
        public static Json Convert(object entity)
        {
            return new Json(entity);
        }

        //<summary>Converts a Json entity into its object equivalent</summary>
        public static object Deserialize(Json entity)
        {
            return JsonConvert.DeserializeObject(entity.ToString());
        }

        //<summary>Converts the entity into JSON format</summary>
        public static string Serialize(object entity)
        {
            return JsonConvert.SerializeObject(entity);
        }

        //<summary>Converts this object into a string</summary>
        public override string ToString()
        {
            return _string;
        }
        #endregion Public Methods

        #region Private Methods
        private static object Deserialize()
        {
            return string.Empty;
        }
        #endregion Private Methods
    }
}