using System;

namespace MockProviderTests
{
    public class Bar<T>
    {
        public Bar(IServiceProvider sp)
        {
        }
    }

    public class Bar
    {
        public Bar(IServiceProvider sp)
        {
        }
    }
}
