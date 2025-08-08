
using CommonModels.Client;
using DesktopWPFManagementApp.MVVM.ViewModel.Core;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static CommonModels.Client.Client;

namespace DesktopWPFManagementApp.Models
{
    public class BotModel : ViewModelBase
    {
        private bool isMarked = false;
        public bool IsMarked
        {
            get { return isMarked; }
            set
            {
                isMarked = value;
                OnPropertyChanged();
            }
        }
        public string ID { get; set; }
        public string SERVER_HOST { get; set; }
        public DateTime RegistrationDateTime { get; set; }
        public string RegistrationDateTimePreviewFormat { get; set; }
        public BotRole Role { get; set; }
        public string IP { get; set; }
        public Machine MACHINE { get; set; }
        private ClientStatus status;
        public ClientStatus Status
        {
            get { return status; }
            set 
            { 
                status = value;
                OnPropertyChanged();
            }
        }


        public BotModel(string ID, string SERVER_HOST, DateTime RegistrationDateTime, BotRole Role, string IP, Machine MACHINE, ClientStatus status)
        {
            this.ID = ID;
            this.SERVER_HOST = SERVER_HOST;
            this.RegistrationDateTime = RegistrationDateTime;
            this.RegistrationDateTimePreviewFormat = RegistrationDateTime.ToShortDateString() + " - " + RegistrationDateTime.ToShortTimeString();
            this.Role = Role;
            this.IP = IP;
            this.MACHINE = MACHINE;
            this.status = status;
        }
    }
}
