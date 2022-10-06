using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Models;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Utilitu;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _cartRepo;

        public CategoryController(ICategoryRepository db)
        {
            _cartRepo = db;
        }

        //get
        public IActionResult Index()
        {
            IEnumerable<Category> objList = _cartRepo.GetAll();
            return View(objList);
        }

        //post
        public IActionResult Create()
        {
            return View();
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                _cartRepo.Add(obj);
                _cartRepo.Save();
                TempData[WC.Success] = "Категория создана";
                return RedirectToAction("index");
            }

            TempData[WC.Error] = "Ошибка создания категории";
            return View(obj);
        }

        //get
        public IActionResult Edit(int? Id)
        {
            if (ModelState.IsValid)
            {
                var obj = _cartRepo.Find(Id.GetValueOrDefault());
                return View(obj);
            }
            return View();
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _cartRepo.Update(obj);
                _cartRepo.Save();
                TempData[WC.Success] = "Категория успешно исправлена";
                return RedirectToAction("index");
            }
            TempData[WC.Error] = "Ошибка! категория не исправлена";

            return View();
        }

        //get
        public IActionResult Del(int? Id)
        {
            if (ModelState.IsValid)
            {
                var obj = _cartRepo.Find(Id.GetValueOrDefault());
                return View(obj);
            }
            return View();
        }

        //post
        [HttpPost]
        public IActionResult Del(Category obj)
        {
            _cartRepo.Remove(obj);
            _cartRepo.Save();
            TempData[WC.Success] = "Категория успешно удалена";
            return RedirectToAction("index");
        }
    }
}
