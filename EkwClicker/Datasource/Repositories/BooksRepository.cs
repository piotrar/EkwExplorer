﻿using System;
using System.Threading.Tasks;
using Dapper;
using EkwClicker.Core;
using EkwClicker.Datasource.Entities;
using EkwClicker.Datasource.Mappers;
using EkwClicker.Models;

namespace EkwClicker.Datasource.Repositories
{
    internal class BooksRepository : IBooksRepository
    {
        private readonly DbConnection _connection;

        public BooksRepository(DbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task AddAsync(BookInfo bookInfo)
        {
            var query = SqlQueries.AddBook;
            var entity = new BookToEntityMapper(bookInfo).MapBook();
            
            await _connection.Db.ExecuteAsync(query, entity);
        }

        public async Task<BookInfo> GetRandomNotFilledBookAsync()
        {
            var query = SqlQueries.GetRandomNotFilledBook;
            
            var entity = await _connection.Db.QuerySingleOrDefaultAsync<BookEntity>(query);
            
            if (entity == null)
            {
                throw new InvalidOperationException(
                    "there are no empty books");
            }
            
            var model = new BookToModelMapper()
                .Map(entity)
                .Finish();
            
            return model;
        }
    }
}