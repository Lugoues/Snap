﻿using System.Collections.Generic;
/*
Snap v1.0

Copyright (c) 2010 Tyler Brinks

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System.Linq;
using Castle.DynamicProxy;

namespace Snap {
    /// <summary>
    /// Creates sorting strategies.
    /// </summary>
    public static class SortOrderFactory {
        /// <summary>
        /// Gets the sort order strategy.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        /// <param name="interceptors">The interceptors.</param>
        /// <returns>Sort order strategy.</returns>
        public static ISortOrderStrategy GetSortOrderStrategy(IInvocation invocation, List<IAttributeInterceptor> interceptors) {
            var attributes = invocation.MethodInvocationTarget.GetCustomAttributes(false)
                .Where(a => a is IInterceptAttribute).Select(at => (IInterceptAttribute)at).ToList();

            var attributesAreIndexed = attributes.Any(a => a.Order > 0);
            var interceptorsAreIndexed = interceptors.Any(a => a.Order > 0);

            if(attributesAreIndexed) {
                return new AttributedSortOrderStrategy(attributes, interceptors);
            }

            if(interceptorsAreIndexed) {
                return new IndexSortOrderStrategy(interceptors);
            }

            return new DefaultSortOrderStrategy(interceptors);
        }
    }
}