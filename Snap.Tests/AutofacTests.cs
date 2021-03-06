﻿/*
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
using System;
using Autofac;
using NUnit.Framework;
using Snap.Autofac;
using SnapTests.Fakes;
using Snap.Tests.Interceptors;

namespace Snap.Tests
{
    [TestFixture]
    public class AutofacTests : TestBase
    {
        [Test]
        public void Autofac_Container_Supports_Method_Aspects()
        {
            var builder = new ContainerBuilder();

            SnapConfiguration.For(new AutofacAspectContainer(builder)).Configure(c =>
            {
                 c.IncludeNamespace("SnapTests.*");
                 c.Bind<HandleErrorInterceptor>().To<HandleErrorAttribute>();
            });

            builder.Register(r => new BadCode()).As<IBadCode>();

            using (var container = builder.Build())
            {
                var badCode = container.Resolve<IBadCode>();

                Assert.DoesNotThrow(badCode.GiddyUp);
                Assert.IsTrue(badCode.GetType().Name.Equals("IBadCodeProxy"));
            }
        }

        [Test]
        public void Autofac_Container_Supports_Multiple_Method_Aspects()
        {
            var builder = new ContainerBuilder();

            SnapConfiguration.For(new AutofacAspectContainer(builder)).Configure(c =>
            {
                c.IncludeNamespace("SnapTests.*");
                c.Bind<FirstInterceptor>().To<FirstAttribute>();
                c.Bind<SecondInterceptor>().To<SecondAttribute>();
            });

            builder.Register(r => new OrderedCode()).As<IOrderedCode>();

            using (var container = builder.Build())
            {
                var orderedCode = container.Resolve<IOrderedCode>();
                orderedCode.RunInOrder();

                Assert.AreEqual("First", OrderedCode.Actions[0]);
                Assert.AreEqual("Second", OrderedCode.Actions[1]);
            }
        }

        [Test]
        public void Autofac_Container_Ignores_Types_Without_Decoration()
        {
            var builder = new ContainerBuilder();

            SnapConfiguration.For(new AutofacAspectContainer(builder)).Configure(c =>
            {
                c.IncludeNamespace("SnapTests.*");
                c.Bind<HandleErrorInterceptor>().To<HandleErrorAttribute>();
            });

            builder.Register(r => new NotInterceptable()).As<INotInterceptable>();

            using (var container = builder.Build())
            {
                var code = container.Resolve<INotInterceptable>();

                Assert.IsFalse(code.GetType().Name.Equals("IBadCodeProxy"));
            }
        }

        [Test]
        public void Autofac_Container_Allow_Wildcard_Matching()
        {
            var builder = new ContainerBuilder();

            SnapConfiguration.For(new AutofacAspectContainer(builder)).Configure(c =>
            {
                c.IncludeNamespace("SnapTests*");
                c.Bind<HandleErrorInterceptor>().To<HandleErrorAttribute>();
            });

            builder.Register(r => new BadCode()).As<IBadCode>();

            using (var container = builder.Build())
            {
                Assert.DoesNotThrow(() => container.Resolve<IBadCode>());
            }
        }

        [Test]
        public void Autofac_Container_Allow_Strict_Matching()
        {
            var builder = new ContainerBuilder();

            SnapConfiguration.For(new AutofacAspectContainer(builder)).Configure(c =>
            {
                c.IncludeNamespace("SnapTests.Fakes.IBadCode");
                c.Bind<HandleErrorInterceptor>().To<HandleErrorAttribute>();
            });

            builder.Register(r => new BadCode()).As<IBadCode>();

            using (var container = builder.Build())
            {
                Assert.DoesNotThrow(() => container.Resolve<IBadCode>());
            }
        }

        [Test]
        public void Autofac_Container_Fails_Without_Match()
        {
            var builder = new ContainerBuilder();

            SnapConfiguration.For(new AutofacAspectContainer(builder)).Configure(c =>
            {
                c.IncludeNamespace("Does.Not.Work");
                c.Bind<HandleErrorInterceptor>().To<HandleErrorAttribute>();
            });

            builder.Register(r => new BadCode()).As<IBadCode>();

            using (var container = builder.Build())
            {
                Assert.Throws<NullReferenceException>(() => container.Resolve<IBadCode>());
            }
        }
    }
}
