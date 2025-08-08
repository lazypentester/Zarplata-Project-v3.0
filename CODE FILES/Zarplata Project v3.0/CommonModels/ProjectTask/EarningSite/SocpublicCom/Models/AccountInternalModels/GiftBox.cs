using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels
{
    public class GiftBox
    {
        public bool? Received { get; set; } = null;
        public bool? Error { get; set; } = null;
        public GiftBoxType? GiftBoxType { get; set; } = null;
        public string? GiftBoxDescription { get; set; } = null;
        public string? GiftBoxValue { get; set; } = null;
    }

    public enum GiftBoxType
    {
        BronzeBox = 1,
        SilverBox = 2,
        GoldenBox = 3
    }
}
