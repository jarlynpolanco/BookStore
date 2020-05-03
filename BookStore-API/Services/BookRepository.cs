using BookStore_API.Contracts;
using BookStore_API.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookStore_API.Services
{
    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public BookRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Create(Book entity)
        {
            await _dbContext.Books.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(Book entity)
        {
            _dbContext.Books.Remove(entity);
            return await Save();
        }

        public async Task<IList<Book>> FindAll()
        {
            var books = await _dbContext.Books.Include(x => x.Author).ToListAsync();
            return books;
        }

        public async Task<Book> FindById(int id)
        {
            var book = await _dbContext.Books.Include(x => x.Author)
                .FirstOrDefaultAsync(x => x.Id == id);
            return book;
        }

        public async Task<bool> Update(Book entity)
        {
            _dbContext.Books.Update(entity);
            return await Save();
        }

        public async Task<bool> Save()
        {
            var changes = await _dbContext.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> IsExist(int id) => await _dbContext.Books.AnyAsync(x => x.Id == id);
    }
}
