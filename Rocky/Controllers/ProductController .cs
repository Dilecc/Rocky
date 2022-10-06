using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rocky.Data;
using Rocky.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Utilitu;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _proRepos;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductRepository proRepos, IWebHostEnvironment webHostEnvironment)
        {
            _proRepos = proRepos;
            _webHostEnvironment = webHostEnvironment;
        }

        //get
        public IActionResult Index()
        {
            //IEnumerable<Product> objList = _db.Product
            //        .Include(u => u.Category)
            //        .Include(u => u.ApplicationType);
            IEnumerable<Product> objList = _proRepos.GetAll(includeProperties: "Category,ApplicationType");
            return View(objList);
        }

        [HttpGet]
        public IActionResult Upsert(int? Id)
        {
            var productVm = new ProductVM
            {
                CategorySelectList = _proRepos.GetAllDropDawnList(WC.CategoryName),
                ApplicationTypeSelectList = _proRepos.GetAllDropDawnList(WC.ApplicationTypeName),
                Product = new Product()
            };


            if(Id == null)
            {
                // add new product
                return View(productVm);
            }
            else
            {
                productVm.Product = _proRepos.FirstOfDefault(u => u.Id == Id);
                if(productVm.Product == null)
                {
                    return NotFound();
                }
                return View(productVm);
            }
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM ProductVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;
                string upload = webRootPath + WC.ImgPath;
                string fileName = Guid.NewGuid().ToString();
                
                if (ProductVM.Product.Id == 0)
                {
                    //create
                    string extension = Path.GetExtension(files[0].FileName);
                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    };
                    ProductVM.Product.Imge = fileName + extension;
                    _proRepos.Add(ProductVM.Product);
                }
                else
                {
                    var obj = _proRepos.FirstOfDefault(u => u.Id == ProductVM.Product.Id, isTracking:false);

                    if (files.Count > 0) // если есть файл то добавляем ссылку на картинку и удаляем старый файл
                    {
                        if (obj.Imge != null)
                        {
                            var oLdFile = Path.Combine(upload, obj.Imge);

                            // Проверяем есть ли по этому пути картинка
                            if (System.IO.File.Exists(oLdFile))
                            {
                                System.IO.File.Delete(oLdFile);
                            }
                        }
                        string extension = Path.GetExtension(files[0].FileName);
                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        };
                        ProductVM.Product.Imge = fileName + extension;

                    }
                    else
                    {
                        ProductVM.Product.Imge = obj.Imge;
                    }
                    _proRepos.Update(ProductVM.Product);

                }

                _proRepos.Save();
                TempData[WC.Success] = "Продукт создан";

                return RedirectToAction("index");
            }
            // если не пройдена валидация вернет список категорий
            ProductVM.CategorySelectList = _proRepos.GetAllDropDawnList(WC.CategoryName);
            ProductVM.ApplicationTypeSelectList = _proRepos.GetAllDropDawnList(WC.ApplicationTypeName);

            return View(ProductVM);
        }


        //get
        public IActionResult Del(int Id)
        {
            if (ModelState.IsValid)
            {
                var obj = _proRepos.FirstOfDefault(includeProperties: "Category,ApplicationType",
                    filter: (u => u.Id == Id));

                return View(obj);
            }
            return View();
        }

        //post
        [HttpPost]
        public IActionResult Del(Product obj)
        {
            if (obj.Imge != null)
            {
                var webRootPath = _webHostEnvironment.WebRootPath;
                var upload = webRootPath + WC.ImgPath;
                var oLdFile = Path.Combine(upload, obj.Imge);

                // Проверяем есть ли по этому пути картинка
                if (System.IO.File.Exists(oLdFile))
                {
                    System.IO.File.Delete(oLdFile);
                }
            }
            _proRepos.Remove(obj);
            _proRepos.Save();
            TempData[WC.Success] = "Продукт удален";

            return RedirectToAction("index");
        }


    }
}
