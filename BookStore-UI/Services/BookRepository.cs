using Blazored.LocalStorage;
using BookStore_UI.Contracts;
using BookStore_UI.Models;
using System.Net.Http;

namespace BookStore_UI.Services
{
    public class BookRepository : BaseRepository<Book>, IBookRepository
    {
        public BookRepository(IHttpClientFactory client, 
            ILocalStorageService localStorage) : base(client, localStorage) 
        {

        }
    }
}
