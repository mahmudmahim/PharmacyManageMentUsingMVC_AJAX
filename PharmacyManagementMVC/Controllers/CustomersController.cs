using PharmacyManagementMVC.Models;
using PharmacyManagementMVC.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PharmacyManagementMVC.Controllers
{
    public class CustomersController : Controller
    {
        private PharmacyShopDb db = new PharmacyShopDb();
        // GET: Customers
        public ActionResult Index()
        {
            var customers = db.Customers.Include(c => c.PurchasingEntries.Select(x => x.Medicine)).OrderByDescending(c => c.CustomerId).ToList();
            return View(customers);
        }

        public ActionResult AddMedicine(int? id)
        {
            ViewBag.medicines = new SelectList(db.Medicines.ToList(), "MedicineId", "MedicineName", (id != null) ? id.ToString() : "");

            return PartialView("_addNewMedicine");
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]

        public ActionResult Create(CustomerVM customerVM, int[] medicineId)
        {
            if (ModelState.IsValid)
            {
                //Customer or Client Part

                Customer customer = new Customer()
                {
                    CustomerName = customerVM.CustomerName,
                    Age = customerVM.Age,
                    PurchaseDate = customerVM.PurchaseDate,
                    VisitedBefore = customerVM.VisitedBefore,
                };
                HttpPostedFileBase file = customerVM.PictureFile;
                if (file != null)
                {
                    string filePath = Path.Combine("/Images/", Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
                    file.SaveAs(Server.MapPath(filePath));
                    customer.Picture = filePath;
                }

                //Spot or Medicine Part

                foreach (var item in medicineId)
                {
                    PurchasingEntry purchasingEntry = new PurchasingEntry()
                    {
                        Customer = customer,
                        CustomerId = customer.CustomerId,
                        MedicineId = item
                    };
                    db.PurchasingEntries.Add(purchasingEntry);
                }
                db.SaveChanges();
                return PartialView("_success");
                //Here we will return partial view as we are using AJax
            }
            return PartialView("_error");
        }

        public ActionResult Edit(int? id)
        {
            Customer customer = db.Customers.First(x => x.CustomerId == id);
            var medicineEnt = db.PurchasingEntries.Where(x => x.CustomerId == id).ToList();

            CustomerVM customerVM = new CustomerVM()
            {
                CustomerId = customer.CustomerId,
                CustomerName = customer.CustomerName,
                Age = customer.Age,
                PurchaseDate = customer.PurchaseDate,
                Picture = customer.Picture,
                VisitedBefore = (bool)customer.VisitedBefore
            };

            Session["imgPath"] = customer.Picture;

            if (medicineEnt.Count() > 0)
            {
                foreach (var item in medicineEnt)
                {
                    customerVM.MedicineList.Add((int)item.MedicineId);
                }
            }

            return View(customerVM);
        }

        [HttpPost]

        public ActionResult Edit(CustomerVM customerVM, int[] medicineId)
        {
            if (ModelState.IsValid)
            {
                Customer customer = new Customer()
                {
                    CustomerId = customerVM.CustomerId,
                    CustomerName = customerVM.CustomerName,
                    Age = customerVM.Age,
                    PurchaseDate = customerVM.PurchaseDate,
                    VisitedBefore = customerVM.VisitedBefore
                };

                HttpPostedFileBase file = customerVM.PictureFile;
                if(file != null)
                {
                    string filePath = Path.Combine("/Images/", Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
                    file.SaveAs(Server.MapPath(filePath));
                    customer.Picture = filePath;
                }
                else
                {
                    customer.Picture = Session["imgPath"].ToString();
                }

                var medicineEnt = db.PurchasingEntries.Where(x => x.CustomerId == customer.CustomerId).ToList();
                foreach (var item in medicineEnt)
                {
                    db.PurchasingEntries.Remove(item);
                }

                foreach(var item in medicineId)
                {
                    PurchasingEntry purchasingEntry = new PurchasingEntry()
                    {
                        Customer = customer,
                        MedicineId = item
                    };
                    db.PurchasingEntries.Add(purchasingEntry);
                }
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                return PartialView("_success");
            }
            return PartialView("_error");
        }

        public ActionResult Delete(int? id)
        {
            Customer customer = db.Customers.First(x => x.CustomerId == id);

            var medicineEnt = db.PurchasingEntries.Where(x => x.CustomerId == id).ToList();
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest); 
            }
           
            if (customer == null)
            {
                return HttpNotFound();
            }

            CustomerVM customerVM = new CustomerVM()
            {
                CustomerId = customer.CustomerId,
                CustomerName = customer.CustomerName,
                Age = customer.Age,
                PurchaseDate = customer.PurchaseDate,
                Picture = customer.Picture,
                VisitedBefore = (bool)customer.VisitedBefore
            };

            if (medicineEnt.Count() > 0)
            {
                foreach (var item in medicineEnt)
                {
                    customerVM.MedicineList.Add((int)item.MedicineId);
                }
            }

            return View(customerVM);
        }

        [HttpPost,ValidateAntiForgeryToken]
        [ActionName("Delete")]

        public ActionResult DoDelete(int? id)
        {
            Customer customer = db.Customers.Find(id);
            PurchasingEntry purchasingEntry = db.PurchasingEntries.First(x=>x.CustomerId == customer.CustomerId);
            string file_Name = customer.Picture;
            string path = Server.MapPath(file_Name);
            FileInfo file = new FileInfo(path);
            if(file.Exists)
            {
                file.Delete();
            }
            else
            {
               
                db.PurchasingEntries.Remove(purchasingEntry);
                db.Customers.Remove(customer);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

    }
}
