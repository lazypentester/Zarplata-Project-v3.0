using CommonModels.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ClientInitialization.Classes.Public.InitDelegates;

namespace ClientInitialization.Interfaces.Public
{
    public interface IInitializable
    {
        public Task Initialize(string[] args, Client client, PrintMessage printMessageMethods);
    }
}
