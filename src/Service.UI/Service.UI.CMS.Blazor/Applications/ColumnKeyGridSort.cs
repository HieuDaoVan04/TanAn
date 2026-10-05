using System;
using System.Linq.Expressions;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Service.Shared.Commons.Querys.Grid
{
    public class ColumnKeyGridSort<T>
    {
        public GridSort<T> GridSort { get; }
        public string Key { get; }

        public ColumnKeyGridSort(string key)
        {
            Key = key;
            try
            {
                var param = Expression.Parameter(typeof(T), "x");
                var prop = Expression.PropertyOrField(param, key);
                var conv = Expression.Convert(prop, typeof(object));
                var lambda = Expression.Lambda<Func<T, object>>(conv, param);
                GridSort = GridSort<T>.ByAscending(lambda);
            }
            catch
            {
                GridSort = GridSort<T>.ByAscending(x => x != null ? x.ToString() : string.Empty);
            }
        }

        public static implicit operator GridSort<T>(ColumnKeyGridSort<T> sort) => sort.GridSort;
    }
}
