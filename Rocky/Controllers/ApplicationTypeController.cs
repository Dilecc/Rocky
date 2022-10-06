using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Models;
using System.Collections;
using System.Collections.Generic;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Utilitu;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ApplicationTypeController : Controller
    {
        private readonly IApplicationTypeRepository _ApplTypRep;

        public ApplicationTypeController(IApplicationTypeRepository ApplTypRep)
        {
            _ApplTypRep = ApplTypRep;
        }

        //get
        public IActionResult Index()
        {
            IEnumerable<ApplicationType> objList = _ApplTypRep.GetAll();
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
        public IActionResult Create(ApplicationType obj)
        {
            if (ModelState.IsValid)
            {
                _ApplTypRep.Add(obj);
                _ApplTypRep.Save();
                TempData[WC.Success] = "Тип успешно создан";
                return RedirectToAction("index");
            }
            TempData[WC.Error] = "Ошибка создания Типа";

            return View(obj);
        }
    

        public IActionResult Edit(int? Id)
        {
            if (ModelState.IsValid)
            {
                var obj = _ApplTypRep.Find(Id.GetValueOrDefault());
                return View(obj);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationType obj)
        {
            _ApplTypRep.Update(obj);
            _ApplTypRep.Save();
            TempData[WC.Success] = "Тип успешно исправлен";

            return RedirectToAction("index");
        }
    }
}
