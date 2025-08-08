using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.Client.Models.DeleteBotsModels
{
    public class DeleteBots
    {
        public List<ModelClient> Bots { get; set; }
        public bool BlockBotsMachines { get; set; }

        public DeleteBots(List<ModelClient> bots, bool blockBotsMachines)
        {
            Bots = bots;
            BlockBotsMachines = blockBotsMachines;
        }
    }
}
