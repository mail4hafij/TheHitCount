
namespace Admin.Models
{
    public class JsonDictionary
    {
        private Dictionary<string, string> _data;

        public JsonDictionary()
        {
            _data = new Dictionary<string, string>();
        }

        public void Add(string key, string val)
        {
            if(_data.ContainsKey(key))
            {
                _data[key] = val;
            }
            else
            {
                _data.Add(key, val);
            }
        }

        public Dictionary<string, string> GetResponse()
        {
            return _data;
        }
    }
}
