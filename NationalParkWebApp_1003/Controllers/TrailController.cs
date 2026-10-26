using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NationalParkWebApp_1003.Models.NationalParkVM;
using NationalParkWebApp_1003.Repository.IRepository;

namespace NationalParkWebApp_1003.Controllers
{
    public class TrailController : Controller
    {
        private readonly IunitOfWorkRepository _unitofwork;

        public TrailController(IunitOfWorkRepository unitofwork)
        {
            _unitofwork = unitofwork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            // Sahi repository use karein: NationalPark
            var nationalparkList = await _unitofwork.NationalPark.GetAllAsync(SD.NationalParkAPIPath);

            TrailVm trailVm = new TrailVm()
            {
                NationalParkList = nationalparkList.Select(np => new SelectListItem()
                {
                    Text = np.Name,
                    Value = np.Id.ToString()
                }),
                Trail = new Models.Trail()
            };

            if (id == null) return View(trailVm);

            trailVm.Trail = await _unitofwork.Trail.GetAsync(SD.TrailAPIPath, id.Value);
            if (trailVm.Trail == null) return NotFound();

            return View(trailVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(TrailVm trailVm)
        {
            if (trailVm == null) return BadRequest();

            if (ModelState.IsValid)
            {
                bool isSuccess;

                if (trailVm.Trail.Id == 0)
                    isSuccess = await _unitofwork.Trail.CreateAsync(SD.TrailAPIPath, trailVm.Trail);
                else
                    isSuccess = await _unitofwork.Trail.UpdateAsync(SD.TrailAPIPath, trailVm.Trail);

                if (isSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }

                // Agar API call fail ho gayi:
                ModelState.AddModelError("", "API ne data save karne se mana kar diya. Backend API console check karein.");
            }

            // Agar ModelState invalid hai ya API save fail hua, dropdown dobara load karein
            var nationalparkList = await _unitofwork.NationalPark.GetAllAsync(SD.NationalParkAPIPath);
            trailVm.NationalParkList = nationalparkList.Select(np => new SelectListItem()
            {
                Text = np.Name,
                Value = np.Id.ToString()
            });

            return View(trailVm);
        }

        #region APIs
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Json(new { data = await _unitofwork.Trail.GetAllAsync(SD.TrailAPIPath) });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var status = await _unitofwork.Trail.DeleteAsync(SD.TrailAPIPath, id);
            if (!status)
                return Json(new { success = false, message = "Unable to delete data" });

            return Json(new { success = true, message = "Data deleted successfully" });
        }
        #endregion
    }
}