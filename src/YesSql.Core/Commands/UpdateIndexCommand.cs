﻿using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using YesSql.Core.Indexes;
using YesSql.Core.Sql;
using YesSql.Core.Collections;
using YesSql.Core.Services;

namespace YesSql.Core.Commands
{
    public class UpdateIndexCommand : IndexCommand
    {
        private readonly IEnumerable<int> _addedDocumentIds;
        private readonly IEnumerable<int> _deletedDocumentIds;

        public override int ExecutionOrder { get; } = 3;

        public UpdateIndexCommand(
            IIndex index,
            IEnumerable<int> addedDocumentIds,
            IEnumerable<int> deletedDocumentIds,
            string tablePrefix) : base(index, tablePrefix)
        {
            _addedDocumentIds = addedDocumentIds;
            _deletedDocumentIds = deletedDocumentIds;
        }

        public override async Task ExecuteAsync(DbConnection connection, DbTransaction transaction)
        {
            var type = Index.GetType();

            var sql = Updates(type);
            await connection.ExecuteAsync(sql, Index, transaction);

            // Update the documents list
            var reduceIndex = Index as ReduceIndex;
            if (reduceIndex != null)
            {
                var documentTable = CollectionHelper.Current.GetPrefixedName(Store.DocumentTable);
                var bridgeTableName = type.Name + "_" + documentTable;
                var columnList = $"[{type.Name}Id], [DocumentId]";
                var parameterList = $"@Id, @DocumentId";
                var bridgeSqlAdd = $"insert into [{_tablePrefix}{bridgeTableName}] ({columnList}) values ({parameterList});";
                var bridgeSqlRemove = $"delete from [{_tablePrefix}{bridgeTableName}] where DocumentId = @DocumentId and {type.Name}Id = @Id;";

                await connection.ExecuteAsync(bridgeSqlAdd, _addedDocumentIds.Select(x => new { DocumentId = x, Id = Index.Id }), transaction);
                await connection.ExecuteAsync(bridgeSqlRemove, _deletedDocumentIds.Select(x => new { DocumentId = x, Id = Index.Id }), transaction);
            }
        }

    }
}
