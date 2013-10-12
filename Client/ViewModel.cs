using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Client
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        private readonly Dictionary<string, PropertyChangedEventArgs> _argsCache = new Dictionary<string, PropertyChangedEventArgs>(); 

        protected virtual void NotifyChange<T>(Expression<Func<T>> memberExpression)
        {
            var pName = GetMemberName(memberExpression);
            if (!string.IsNullOrEmpty(pName))
                NotifyChange(pName);
        }

        protected virtual void NotifyChange(string propertyName)
        {
            if (_argsCache == null)
                return;

            if (!_argsCache.ContainsKey(propertyName))
                _argsCache[propertyName] = new PropertyChangedEventArgs(propertyName);

            NotifyChange(_argsCache[propertyName]);
        }

        private void NotifyChange(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        private static string GetMemberName<T>(Expression<Func<T>> memberExpression)
        {
            if (memberExpression == null)
                throw new ArgumentException("Invalid member expression");

            var body = memberExpression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("Lambda must return a property");

            return body.Member.Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
