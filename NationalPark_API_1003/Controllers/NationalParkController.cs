using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NationalPark_API_1003.Models;
using NationalPark_API_1003.Models.DTOs;
using NationalPark_API_1003.Repository.IRepository;

namespace NationalPark_API_1003.Controllers
{
    [Route("api/NationalPark")]
    [ApiController]

    public class NationalParkController : Controller
    {
        private readonly INationalParkRepository _nationalparkRepository;
        private readonly IMapper _mapper;
        public NationalParkController(INationalParkRepository nationalParkRepository, IMapper mapper)
        {
            _nationalparkRepository = nationalParkRepository;
            _mapper = mapper;
        }
        [HttpGet]
        public IActionResult GetNationalParks()
        {
            var nationalParkDtoList = _nationalparkRepository.GetNationalParks().
                Select(_mapper.Map<NationalParkDto>);
            return Ok(nationalParkDtoList);
        }
        [HttpGet("{nationalParkId:int}",Name ="GetNationalPark")]
        public IActionResult GetNationalPark(int nationalParkId)
        {
            var nationalPark=_nationalparkRepository.GetNationalPark(nationalParkId);
            if (nationalPark == null) return NotFound();//404
            var nationalParkDto = _mapper.Map<NationalPark, NationalParkDto>(nationalPark);
            if(nationalParkDto==null)return NotFound ();
            return Ok(nationalParkDto);

        }
        [HttpPost]
        public IActionResult CreateNationalPark([FromBody]NationalParkDto nationalParkDto)
        {
            if (nationalParkDto == null) return BadRequest();
            if (!ModelState.IsValid) return BadRequest();
            var nationalPark = _mapper.Map<NationalParkDto, NationalPark>(nationalParkDto);
            if(_nationalparkRepository.NationalParkExists(nationalPark.Name))
            {
                ModelState.AddModelError("", "NationalPark in db!!!");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            if(!_nationalparkRepository.CreateNationalPark(nationalPark))
            {
                ModelState.AddModelError("", "Unable to Create National Park");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            //return Ok();
            return CreatedAtRoute("GetNationalPark", new { nationalParkId = nationalPark.Id }, nationalPark);
        }
        [HttpPut] //update ----[httppatch]
        public IActionResult UpdateNationalPark(
            [FromBody]NationalParkDto nationalParkDto)
        {
            if (nationalParkDto == null) return BadRequest();
            if (!ModelState.IsValid) return BadRequest();
            var nationalpark = _mapper.Map<NationalParkDto, NationalPark>(nationalParkDto);
            if(!_nationalparkRepository.UpdateNationalPark(nationalpark))
            {
                ModelState.AddModelError("", "unable to update national park");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return NoContent();

        }
        [HttpDelete("{nationalParkId:int}")]
        public IActionResult DeleteNationalPark(int nationalParkId)
        {

            if (!_nationalparkRepository.NationalParkExists(nationalParkId))
                return NotFound();
            var nationalpark = _nationalparkRepository.GetNationalPark(nationalParkId);
            if(nationalpark== null) return NotFound();
            if (!_nationalparkRepository.DeleteNationalPark(nationalpark))
            {
                ModelState.AddModelError("", "Unable to delete data");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok();
            
        }

    }
}
