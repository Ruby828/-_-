using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class CreatOrder
    {
        public Guid FormOid { get; set; }
        // public virtual ICollection<Product> Pid { get; set; }
        public Guid Pid { get; set; }
        //商品購買數量
        public int FormNum { get; set; }


    }
}