using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webSimulator
{
    public sealed class TaskManager
    {
        private static readonly TaskManager instance = new TaskManager();
        private TaskManager() { }
        public static TaskManager Instance { get { return instance; } }

        private const string _configFileName = @"task.json";
        public TaskInfo _taskInfo = new TaskInfo();

        public class TaskEnt
        {
            public string name;
            public string p1;
            public string p2;
        }

        public class TaskInfo
        {
            public IList<TaskEnt> task;
        }

        public void LoadFile()
        {
            try
            {
                //_taskInfo = JsonConvert.DeserializeObject<TaskInfo>(File.ReadAllText(_configFileName));

                _taskInfo.task = new List<TaskEnt>();

                JObject joRoot;
                joRoot = JObject.Parse(File.ReadAllText(_configFileName));

                var joTask = joRoot["task"];

                foreach (JObject entObj in joTask)
                {
                    TaskEnt ent = new TaskEnt();
                    ent.name = entObj["name"].ToString();

                    JToken v;
                    if(true == entObj.TryGetValue("p1", out v))
                    {
                        ent.p1 = v.ToString();
                    }
                    if (true == entObj.TryGetValue("p2", out v))
                    {
                        ent.p2 = v.ToString();
                    }

                    _taskInfo.task.Add(ent);
                }
            }
            catch (Exception /*e*/)
            {
                System.Windows.MessageBox.Show("not found task.json file");
                Environment.Exit(0);
            }
        }
    }
}
