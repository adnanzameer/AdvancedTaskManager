﻿using System;
using System.Linq;
using AdvancedTask.Business.AdvancedTask.Command;

namespace AdvancedTask.Business.AdvancedTask
{
    public class ApprovalCommandRepositoryBase
    {
        private readonly ChangeApprovalDynamicDataStoreFactory _changeApprovalDynamicDataStoreFactory;

        public ApprovalCommandRepositoryBase(ChangeApprovalDynamicDataStoreFactory changeApprovalDynamicDataStoreFactory)
        {
            _changeApprovalDynamicDataStoreFactory = changeApprovalDynamicDataStoreFactory;
        }

        public ChangeApprovalCommandBase GetByCommandId<T>(Guid id, string type) where T : ChangeApprovalCommandBase
        {
            var store = _changeApprovalDynamicDataStoreFactory.GetStore(type);

            var obj = store?.Items<T>().FirstOrDefault(x => x.Id.ExternalId == id);

            return obj;
        }
    }
}