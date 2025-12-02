using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_BM_18_XiHatLoCao
    {
        [Key]
        public int ID { get; set; }

        public string Ca { get; set; }           
        public string Kip { get; set; }        

        public int ID_LoCao { get; set; }

        public string Ten_NVL { get; set; }      
        public string DVT { get; set; }          
        public string Lo { get; set; }            

        public decimal HeSo { get; set; }         

        public decimal KL_Gang_Giao { get; set; } 
        public decimal KL_Xi_Giao { get; set; }   
        public decimal KL_Gang_Nhan { get; set; } 
        public decimal KL_Xi_Nhan { get; set; }  

        public string GhiChu { get; set; }     
    }
}
