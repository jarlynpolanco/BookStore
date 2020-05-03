using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the Authors in the book store's database.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator, Customer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public AuthorsController(IAuthorRepository authorRepository,
            ILoggerService logger, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all authors from database.
        /// </summary>
        /// <returns>List of Authors</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors() 
        {
            var location = GetCotrollerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted to call");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo($"{location}: Successfully");
                return Ok(response);
            }
            catch (Exception e) 
            {
                return InternalServerError($"{location}: {e.Message} - {e.InnerException}");
            }          
        }

        /// <summary>
        /// Get an author by Id from database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An author record</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            var location = GetCotrollerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted to call Id: {id}");
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _logger.LogWarn($"{location}: Record with Id: {id} was not found");
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _logger.LogInfo($"{location}: Successfully get an Id: {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalServerError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Create an Author
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO) 
        {
            var location = GetCotrollerActionNames();
            try
            {
                _logger.LogInfo($"{location}: submission attempted");
                if (authorDTO == null) 
                {
                    _logger.LogWarn($"{location}: Empty request was submitted");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid) 
                {
                    _logger.LogWarn($"{location}: data was incompleted");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Create(author);

                if (!isSuccess) 
                {
                    return InternalServerError($"{location}: creation failed");
                }

                _logger.LogInfo($"{location}: created successfully.");
                return Created("Create", new { author });                  
            }
            catch (Exception e) 
            {
                return InternalServerError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Updates an Author
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPatch("{id:int}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            var location = GetCotrollerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted to update Id: {id}");
                if (id < 1 || authorDTO == null || id != authorDTO.Id)
                {
                    _logger.LogWarn($"{location}: Update failed with bad data");
                    return BadRequest();
                }

                var isExist = await _authorRepository.IsExist(id);
                if (!isExist)
                {
                    _logger.LogWarn($"{location}: Record with Id: {id} was not found");
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Request data for update was incompleted");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Update(author);

                if (!isSuccess)
                {
                    return InternalServerError($"{location}: Update failed");
                }

                _logger.LogInfo($"{location} updated successfully.");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalServerError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Remove an author Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetCotrollerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Record with Id: {id} delete attempted");
                if (id < 1) 
                {
                    _logger.LogWarn($"{location}: Request failed with bad data");
                    return BadRequest();
                }

                var isExist = await _authorRepository.IsExist(id);
                if (!isExist)
                {
                    _logger.LogWarn($"{location}: Record with Id: {id} was not found");
                    return NotFound();
                }

                var author = await _authorRepository.FindById(id);
                var isSuccess = await _authorRepository.Delete(author);
                if (!isSuccess) 
                {
                    return InternalServerError($"{location}: Delete failed");
                }

                _logger.LogInfo($"{location}: Record with Id: {id} was deleted successfully");
                return NoContent();
            }
            catch (Exception e) 
            {
                return InternalServerError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        private string GetCotrollerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controller} - {action}";
        }

        private ObjectResult InternalServerError(string message) 
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the System Administrator.");
        }
    }
}