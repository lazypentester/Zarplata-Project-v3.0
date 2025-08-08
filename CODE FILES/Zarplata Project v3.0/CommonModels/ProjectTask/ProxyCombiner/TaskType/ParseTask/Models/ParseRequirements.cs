using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models
{
    public class ParseRequirements
    {
        public int countOfParsedProxy { get; set; }

        public ParseRequirements(int countOfParsedProxy)
        {
            this.countOfParsedProxy = countOfParsedProxy;
        }
    }
}
