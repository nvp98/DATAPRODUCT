﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class Tbl_BM_16_LoSanXuat
    {
        [Key]
        public int ID { get; set; }
        public int MaLo  { get; set; }
        public string TenLo { get; set; }
        public int ID_BoPhan { get; set; }  
        public bool IsActived { get; set; }
    }
}
