using System;
using System.Collections.Generic;

namespace Elastic.Apm.Mongo
{
    internal class CompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly object _lock = new object();

        private bool _isDisposed;

        public void Dispose()
        {
            if (_isDisposed) return;

            lock (_lock)
            {
                if (_isDisposed) return;

                _isDisposed = true;
                foreach (var d in _disposables) d.Dispose();
            }
        }

        public CompositeDisposable Add(IDisposable disposable)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(CompositeDisposable));

            _disposables.Add(disposable);
            return this;
        }
    }
}
