using Microsoft.AspNetCore.Mvc.Rendering;

namespace NationalParkWebApp_1003.Models.NationalParkVM
{
    public class TrailVm
    {
        public IEnumerable<SelectListItem> NationalParkList { get; set; }
        public Trail Trail { get; set; }
    }
}
