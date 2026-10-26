using Microsoft.AspNetCore.Mvc;
using NationalParkWebApp_1003.Models;
using NationalParkWebApp_1003.Repository.IRepository;

namespace NationalParkWebApp_1003.Controllers
{
    public class NationalParkController : Controller
    {
        private readonly IunitOfWorkRepository _unitofwork;
        public NationalParkController(IunitOfWorkRepository unitOfWork)
        {
            _unitofwork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Upsert(int? id)
        {
            NationalPark nationalPark = new NationalPark();
            if (id == null) return View(nationalPark);
            nationalPark = await _unitofwork.NationalPark.GetAsync(
                SD.NationalParkAPIPath, id.GetValueOrDefault());
            if (nationalPark == null) return NotFound();
            return View(nationalPark);
        }
        [HttpPost]

        public async  Task<IActionResult> Upsert(NationalPark nationalPark)
        {
            if (nationalPark == null) return NotFound();
            if (!ModelState.IsValid) return View(nationalPark);
            //Image
            var files = HttpContext.Request.Form.Files;
            if(files.Count()>0)
            {
                byte[] p1 = null;
                using(var fs = files[0].OpenReadStream())
                {
                    using (var ms=new MemoryStream())
                    {
                        fs.CopyTo(ms);
                        p1 = ms.ToArray();
                    };

                };
                nationalPark.Picture = p1;
            }else
            {
                var nationalParkInDb = await _unitofwork.NationalPark.GetAsync(
                    SD.NationalParkAPIPath, nationalPark.Id);
                nationalPark.Picture = nationalParkInDb.Picture;
            }
            //****
            if (nationalPark.Id == 0)
                await _unitofwork.NationalPark.CreateAsync(SD.NationalParkAPIPath, nationalPark);
            else
                await _unitofwork.NationalPark.UpdateAsync(SD.NationalParkAPIPath, nationalPark);
            return View(nationalPark);
        }

        #region APIs
        [HttpGet]
        public async Task<IActionResult> GetALL()
        {
            return Json(new { data = await _unitofwork.NationalPark.
                GetAllAsync(SD.NationalParkAPIPath) });
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var status =await _unitofwork.NationalPark.DeleteAsync(SD.NationalParkAPIPath, id);
            if(status == false)
                return Json(new { success = false, message = "Unable to delete data " });
            return Json(new { success = true, message="message deleted successfully" });
        }
        #endregion
    }
}
