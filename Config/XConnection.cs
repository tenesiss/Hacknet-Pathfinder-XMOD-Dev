using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMOD.Config
{
    public class XConnection {
        public string identifier;
        public string api_address;
        public int api_port;
        public string api_key;
        public Dictionary<string,string> attributes = new Dictionary<string,string>();
        public static HttpClientHandler httpHandler = new HttpClientHandler();

        public XConnection(string identifier, string api_address, int api_port, string api_key, Dictionary<string,string> attributes = null)
        {
            this.identifier = identifier;
            this.api_address = api_address;
            this.api_port = api_port;
            this.api_key = api_key;
            this.attributes = attributes;
        }

        async public Task<Dictionary<string, string>> GetData()
        {
            try
            {
                var client = new HttpClient(httpHandler);
                if (api_key.Length > 0)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", api_key);
                    client.DefaultRequestHeaders.Add("X-API-KEY", api_key);
                }
                var res_raw = await client.GetAsync(api_address);
                if (!res_raw.IsSuccessStatusCode) return null;

                var res_json = await res_raw.Content.ReadAsStringAsync();
                Console.WriteLine(res_json.ToString());
                var res = JsonConvert.DeserializeObject<Dictionary<string, object>>(res_json);
                Dictionary<string, string> finalRes = new Dictionary<string, string>();
                string dpart = "";
                bool darray = false;
                int arrLastIdx = -1;
                foreach (KeyValuePair<string, string> attribute in attributes)
                {
                    string[] parts = attribute.Value.Split('.');
                    Dictionary<string, object> currentLevel = res;
                    foreach (var part in parts)
                    {
                        Console.WriteLine($"{part}");
                        // Check if part accesses an array element
                        if (part.EndsWith("]"))
                        {
                            var arrayPart = part.Substring(0, part.IndexOf('['));
                            var indexPart = part.Substring(part.IndexOf('[')).Trim('[', ']');
                            if (currentLevel != null && currentLevel.ContainsKey(arrayPart))
                            {
                                var arr_raw = currentLevel[arrayPart];
                                if (!(arr_raw is Newtonsoft.Json.Linq.JArray))
                                {
                                    currentLevel = null;
                                    break;
                                }
                                var arr = arr_raw as Newtonsoft.Json.Linq.JArray;
                                int idx = indexPart == "*" ? 0 : int.Parse(indexPart);
                                if (arr.Count > idx)
                                {
                                    if (!(arr[idx] is Newtonsoft.Json.Linq.JObject))
                                    {
                                        dpart = arrayPart;
                                        darray = true;
                                        arrLastIdx = idx;
                                        break;
                                    }
                                    currentLevel = ((Newtonsoft.Json.Linq.JObject)arr[idx]).ToObject<Dictionary<string, object>>();
                                }
                                else
                                {
                                    currentLevel = null;
                                    break;
                                }
                            }
                            else
                            {
                                currentLevel = null;
                                break;
                            }
                        }
                        else
                        {
                            if (currentLevel != null && currentLevel.ContainsKey(part))
                            {
                                if (!(currentLevel[part] is Dictionary<string, object>))
                                {
                                    Console.WriteLine("si");
                                    dpart = part;
                                    break;
                                }
                                currentLevel = currentLevel[part] as Dictionary<string, object>;
                            }
                            else
                            {
                                currentLevel = null;
                                break;
                            }
                        }

                    }
                    finalRes[attribute.Key] = currentLevel != null ? (darray ? (currentLevel[dpart] as Newtonsoft.Json.Linq.JArray)[arrLastIdx] : currentLevel[dpart]) as string : null;
                }

                return finalRes;
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Nop");
                return null;
            }
            
        }
    }
}
