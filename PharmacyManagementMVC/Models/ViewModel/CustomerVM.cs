using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace PharmacyManagementMVC.Models.ViewModel
{
    public class CustomerVM
    {
        public CustomerVM()
        {
            this.MedicineList = new List<int>();
        }

        public int CustomerId { get; set; }
        [Required,StringLength(50,ErrorMessage ="Customer Name is Required"),Display(Name ="Customer Name")]
        public string CustomerName { get; set; }
        [Required]
        public int Age { get; set; }
        [Required,Column(TypeName ="date"),DisplayFormat(DataFormatString ="{0:yyyy-MM-dd}",ApplyFormatInEditMode =true),DisplayName("Purchase Date")]
        public DateTime PurchaseDate { get; set; }
        public string Picture { get; set; }
        public HttpPostedFileBase PictureFile { get; set; }
        public bool VisitedBefore { get; set; }

        public List<int> MedicineList { get; set; }
    }
}

