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
    /// Endpoint used to interact with the Books in the book store's database.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator, Customer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public BooksController(IBookRepository bookRepository,
            ILoggerService logger, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all books from database.
        /// </summary>
        /// <returns>List of Authors</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetCotrollerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted call");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                _logger.LogInfo($"{location}: Successfully");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalServerError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Get a book by Id from database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An author record</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            var location = GetCotrollerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted call for Id: {id}");
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    _logger.LogWarn($"{location}: Id: {id} was not found");
                    return NotFound();
                }
                var response = _mapper.Map<BookDTO>(book);
                _logger.LogInfo($"{location}: Successfully get a record with Id: {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalServerError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Create a Book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetCotrollerActionNames();
            try
            {
                _logger.LogInfo($"{location}: submission attempted");
                if (bookDTO == null)
                {
                    _logger.LogWarn($"{location}: Empty request was submitted");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: data was incompleted");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Create(book);

                if (!isSuccess)
                {
                    return InternalServerError($"{location}: creation failed");
                }

                _logger.LogInfo($"{location}: created successfully.");
                return Created("Create", new { book });
            }
            catch (Exception e)
            {
                return InternalServerError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Updates a Book
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPatch("{id:int}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            var location = GetCotrollerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted to update Id: {id}");
                if (id < 1 || bookDTO == null || id != bookDTO.Id)
                {
                    _logger.LogWarn($"{location}: Update failed with bad data");
                    return BadRequest();
                }

                var isExist = await _bookRepository.IsExist(id);
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

                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Update(book);

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
        /// Remove a book by Id
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

                var isExist = await _bookRepository.IsExist(id);
                if (!isExist)
                {
                    _logger.LogWarn($"{location}: Record with Id: {id} was not found");
                    return NotFound();
                }

                var book = await _bookRepository.FindById(id);
                var isSuccess = await _bookRepository.Delete(book);
                if (isSuccess)
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