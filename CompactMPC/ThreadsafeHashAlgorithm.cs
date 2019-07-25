using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace CompactMPC
{
    public abstract class ThreadsafeHashAlgorithm : IDisposable
    {
        private readonly object _hashAlgorithmLock;
        private HashAlgorithm _hashAlgorithm;

        public ThreadsafeHashAlgorithm()
        {
            _hashAlgorithm = CreateHashAlgorithm();
            _hashAlgorithmLock = new object();
        }

        ~ThreadsafeHashAlgorithm()
        {
            Dispose();
        }

        /// <summary>
        /// Implementing subclasses need to return a HashAlgorithm object here.
        /// This guarantees that the HashAlgorithm object is never accessible outside of the
        /// class, which is not the case when it is given as argument to the constructor.
        /// </summary>
        /// <returns></returns>
        protected abstract HashAlgorithm CreateHashAlgorithm();

        public byte[] ComputeHash(byte[] buffer)
        {
            lock (_hashAlgorithmLock)
            {
                return _hashAlgorithm.ComputeHash(buffer);
            }
        }

        public byte[] ComputeHash(System.IO.Stream inputStream)
        {
            lock (_hashAlgorithmLock)
            {
                return _hashAlgorithm.ComputeHash(inputStream);
            }
        }

        public byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            lock (_hashAlgorithmLock)
            {
                return _hashAlgorithm.ComputeHash(buffer, offset, count);
            }
        }

        public void Dispose()
        {
            _hashAlgorithm.Dispose();
        }
    }
}
