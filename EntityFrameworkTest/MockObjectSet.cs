﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;


namespace EntityFrameworkTest
{
/// <summary>
/// Testaus-objectset.
/// </summary>
/// <typeparam name="T"></typeparam>
public class MockObjectSet<T> : IObjectSet<T> where T : class
{
    readonly IList<T> _container = new List<T>();

    public void AddObject(T entity)
    {
        _container.Add(entity);
    }

    public void Attach(T entity)
    {
        _container.Add(entity);
    }

    public void DeleteObject(T entity)
    {
        _container.Remove(entity);
    }

    public void Detach(T entity)
    {
        _container.Remove(entity);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _container.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _container.GetEnumerator();
    }

    public Type ElementType
    {
        get { return typeof(T); }
    }

    public System.Linq.Expressions.Expression Expression
    {
        get { return _container.AsQueryable<T>().Expression; }
    }

    public IQueryProvider Provider
    {
        get { return _container.AsQueryable<T>().Provider; }
    }
}
}
