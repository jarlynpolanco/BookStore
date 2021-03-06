﻿using BookStore_API.Contracts;
using BookStore_API.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookStore_API.Services
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AuthorRepository(ApplicationDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Create(Author entity)
        {
            await _dbContext.Authors.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(Author entity)
        {
            _dbContext.Authors.Remove(entity);
            return await Save();
        }

        public async Task<IList<Author>> FindAll()
        {
            var authors = await _dbContext.Authors.Include(x => x.Books).ToListAsync();
            return authors;
        }

        public async Task<Author> FindById(int id)
        {
            var author = await _dbContext.Authors.Include(x => x.Books)
                .FirstOrDefaultAsync(x => x.Id == id);
            return author;
        }

        public async Task<bool> Update(Author entity)
        {
            _dbContext.Authors.Update(entity);
            return await Save();
        }

        public async Task<bool> Save()
        {
            var changes = await _dbContext.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> IsExist(int id) => await _dbContext.Authors.AnyAsync(x => x.Id == id);
    }
}
