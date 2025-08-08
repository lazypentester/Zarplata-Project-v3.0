using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class SocpublicAnketa
    {
        public string login;
        public string work_filter;
        public string family_filter;
        public string gender_filter;
        public string age_from;
        public string age_to;

        public SocpublicAnketa(string login, string work_filter, string family_filter, string gender_filter, string age_from, string age_to)
        {
            this.login = login;
            this.work_filter = work_filter;
            this.family_filter = family_filter;
            this.gender_filter = gender_filter;
            this.age_from = age_from;
            this.age_to = age_to;
        }
    }
}
