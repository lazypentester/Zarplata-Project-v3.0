using CommonModels.Client.Models;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.Client
{
    public class Machine : ModelMachine
    {
        public Machine()
        {
        }

        public enum Platform
        {
            Linux,
            Windows,
            MacOS,
            Other
        }
    }
}
