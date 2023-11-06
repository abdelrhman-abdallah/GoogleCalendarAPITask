using CalenderAPITask.DTO;
using CalenderAPITask.Service;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace CalenderAPITask.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IGoogleCalendarService _googleCalendarService;
        public EventController(IGoogleCalendarService googleCalendarService)

        {
            _googleCalendarService = googleCalendarService;
        }

        [HttpPost]
        [Route("/calendarevent")]
        public async Task<IActionResult> Add([FromBody] GoogleCalendarReqDTO googleCalenderReqDTO) 
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var jwtToken = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Substring("Bearer ".Length);

                    var eventId = await _googleCalendarService.AddEvent(googleCalenderReqDTO,jwtToken);

                    if (string.IsNullOrEmpty(eventId))
                    {
                        return BadRequest("Cannot add events in the past or during weekends.");
                    }
                    return CreatedAtAction(nameof(Add), new { id = eventId });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    return StatusCode(500, "Internal Server Error");
                }
            }
            else 
            {
                return BadRequest("Invalid request. Please check whether the request data is missing or incomplete.");
            }

        }

        [HttpGet]
        [Route("/calendarevent")]
        public async Task<IActionResult> Get([FromQuery] ListQueryParamsDTO listQueryParams) 
        {
            try
            {
                var jwtToken = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Substring("Bearer ".Length);

                bool isDateRangeProvided = listQueryParams.StartDate != null && listQueryParams.EndDate != null;
                bool isDateRangeValid = listQueryParams.EndDate > listQueryParams.StartDate;

                if (isDateRangeProvided && !isDateRangeValid)
                {
                    return BadRequest("EndDate Must Be Greater Than StartDate");
                }
                var response = await _googleCalendarService.GetEvents(jwtToken,listQueryParams.StartDate,listQueryParams.EndDate, listQueryParams.NextPageToken,listQueryParams.MaxResultSize ,listQueryParams.Query);

                return Ok(new {res = response });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return NotFound("No Events Where Found");
            }
        }

        [HttpDelete]
        [Route("/calendarevent/{eventId}")]
        public async Task<IActionResult> Delete(string eventId)
        {
            try
            {
                var jwtToken = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Substring("Bearer ".Length);

                var deletedEvent = await _googleCalendarService.DeleteEvent(eventId,jwtToken);
                return NoContent();
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(404, "Not Found");
            }
        }
       
    }
}
