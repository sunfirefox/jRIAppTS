﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Reflection;
using System.Collections;
using System.Transactions;
using System.Data.Linq.Mapping;
using RIAPP.DataService.Utils;

namespace RIAPP.DataService.LinqSql
{
    public abstract class LinqForSqlDomainService<TDB> : BaseDomainService
        where TDB : System.Data.Linq.DataContext
    {
        private TDB _db;
        private bool _ownsDb = false;
        
        public LinqForSqlDomainService(TDB db, IPrincipal principal)
            :base(principal)
        {
            this._db = db;
        }

        public LinqForSqlDomainService(IPrincipal principal)
            : this(null,principal)
        {
           
        }

        protected override DataHelper CreateDataHelper()
        {
            return new DataHelper(new LinqValueConverter());
        }

        #region Overridable Methods
        protected virtual TDB CreateDataContext() {
            return Activator.CreateInstance<TDB>();
        }

        protected override Metadata GetMetadata()
        {
            Metadata metadata = new Metadata();
            PropertyInfo[] dbsetPropList = this.DB.GetType().GetProperties().Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(System.Data.Linq.Table<>)).ToArray();
            Array.ForEach(dbsetPropList, (propInfo) =>
            {
                Type entityType = propInfo.PropertyType.GetGenericArguments().First();
                DbSetInfo dbSetInfo = new DbSetInfo() { dbSetName = entityType.Name, EntityType = entityType, insertDataMethod = "Insert{0}", updateDataMethod = "Update{0}", deleteDataMethod = "Delete{0}", refreshDataMethod = "", validateDataMethod = "" };
                metadata.DbSets.Add(dbSetInfo);
                PropertyInfo[] fieldPropList = entityType.GetProperties().Where(p => p.IsDefined(typeof(System.Data.Linq.Mapping.ColumnAttribute), false)).ToArray();
                short pkNum = 0;
                Array.ForEach(fieldPropList, (propInfo2) =>
                {
                    FieldInfo fieldInfo = new FieldInfo();
                    fieldInfo.fieldName = propInfo2.Name;
                    var colAttr = (System.Data.Linq.Mapping.ColumnAttribute)propInfo2.GetCustomAttributes(typeof(System.Data.Linq.Mapping.ColumnAttribute), false).First();
                    fieldInfo.isAutoGenerated = colAttr.IsDbGenerated;
                    if (colAttr.IsPrimaryKey)
                    {
                        fieldInfo.isPrimaryKey = ++pkNum;
                    }
                    bool isArray = false;
                    fieldInfo.dataType = this.dataHelper.DataTypeFromType(propInfo2.PropertyType, out isArray);
                    fieldInfo.isNullable = DataHelper.IsNullableType(propInfo2.PropertyType) || colAttr.CanBeNull;
                    fieldInfo.isRowTimeStamp = colAttr.IsVersion;
                    fieldInfo.isReadOnly = !propInfo2.CanWrite;
                    dbSetInfo.fieldInfos.Add(fieldInfo);
                });

                PropertyInfo[] navPropList = entityType.GetProperties().Where(p => p.IsDefined(typeof(System.Data.Linq.Mapping.AssociationAttribute), false)).ToArray();
                Array.ForEach(navPropList, (propInfo3) =>
                {
                    var attr = (System.Data.Linq.Mapping.AssociationAttribute)propInfo3.GetCustomAttributes(typeof(System.Data.Linq.Mapping.AssociationAttribute), false).First();

                    Association ass = metadata.Associations.Where(a => a.name == attr.Name).FirstOrDefault();
                    if (ass == null)
                    {
                        ass = new Association();
                        ass.name = attr.Name;
                        metadata.Associations.Add(ass);
                    }
                    FieldRel frel;
                    if (propInfo3.PropertyType.IsGenericType && propInfo3.PropertyType.GetGenericTypeDefinition() == typeof(System.Data.Linq.EntitySet<>))
                    {
                        frel = ass.fieldRels.Where(f => f.childField == attr.OtherKey && f.parentField == attr.ThisKey).FirstOrDefault();
                        if (frel == null)
                        {
                            frel = new FieldRel();
                            ass.fieldRels.Add(frel);
                        }
                        Type entityType3 = propInfo3.PropertyType.GetGenericArguments().First();
                        ass.childDbSetName =entityType3.Name;
                        ass.parentToChildrenName = propInfo3.Name;
                        frel.childField= attr.OtherKey;
                        frel.parentField = attr.ThisKey;
                    }
                    else
                    {
                        frel = ass.fieldRels.Where(f => f.childField == attr.ThisKey && f.parentField == attr.OtherKey).FirstOrDefault();
                        if (frel == null)
                        {
                            frel = new FieldRel();
                            ass.fieldRels.Add(frel);
                        }
                        ass.parentDbSetName = propInfo3.PropertyType.Name;
                        ass.childToParentName = propInfo3.Name;
                        frel.childField = attr.ThisKey;
                        frel.parentField = attr.OtherKey;
                    }
                });
            });

            return metadata;
        }

        protected override void ExecuteChangeSet()
        {
            using (TransactionScope transScope = new TransactionScope(TransactionScopeOption.RequiresNew, 
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.FromMinutes(1.0) }))
            {
                this.DB.SubmitChanges();
                
                transScope.Complete();
            }
        }

        #endregion

        protected TDB DB
        {
            get
            {
                if (this._db == null)
                {
                    this._db = this.CreateDataContext();
                    if (this._db != null)
                    {
                        this._ownsDb = true;
                    }
                }
                return this._db;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (this._db != null && this._ownsDb)
            {
                this._db.Dispose();
                this._db = null;
                this._ownsDb = false;
            }
        }
    }
}
