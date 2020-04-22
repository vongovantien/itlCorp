using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class TariffModel
    {
        public SetTariffModel setTariff { get; set; }
        public List<SetTariffDetailModel> setTariffDetails { get; set; }
    }
}
