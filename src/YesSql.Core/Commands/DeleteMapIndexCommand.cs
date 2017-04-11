﻿using Dapper;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace YesSql.Core.Commands
{
    public class DeleteMapIndexCommand : IIndexCommand
    {
        private readonly int _documentId;
        private readonly Type _indexType;
        private readonly string _tablePrefix;

        public int ExecutionOrder { get; } = 1;

        public DeleteMapIndexCommand(Type indexType, int documentId, string tablePrefix)
        {
            _indexType = indexType;
            _documentId = documentId;
            _tablePrefix = tablePrefix;
        }

        public virtual Task ExecuteAsync(DbConnection connection, DbTransaction transaction)
        {
            return connection.ExecuteAsync($"delete from [{_tablePrefix}{_indexType.Name}] where DocumentId = @Id", new { Id = _documentId }, transaction);
        }
    }
}
