using System;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using MudBlazor.Services;
using MudBlazor.UnitTests.TestComponents;
using NSubstitute;
using NUnit.Framework;

#nullable enable
namespace MudBlazor.UnitTests.Components
{
    [TestFixture]
    public class HiddenTests : BunitTest
    {
        [Test]
        [TestCase(false, false, false)]
        [TestCase(false, true, true)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        public void Content_Visible(bool mediaResult, bool invert, bool isHidden)
        {
            BrowserWindowSize GetBrowserSize()
            {
                return mediaResult
                    ? new BrowserWindowSize { Height = 720, Width = 1280 } //Lg
                    : new BrowserWindowSize { Height = 1080, Width = 1920 }; //Xl
            }

            var jsRuntimeMock = Substitute.For<IJSRuntime>();
            var browserViewportService = new BrowserViewportService(NullLogger<BrowserViewportService>.Instance, jsRuntimeMock);

            jsRuntimeMock.InvokeAsync<BrowserWindowSize>("mudResizeListener.getBrowserWindowSize", Arg.Any<object[]>())
                .Returns(GetBrowserSize());

            Context.Services.AddSingleton<IBrowserViewportService>(browserViewportService);

            var comp = Context.RenderComponent<SimpleMudHiddenTest>(parameterBuilder =>
            {
                parameterBuilder.Add(parameter => parameter.Breakpoint, Breakpoint.Lg);
                parameterBuilder.Add(parameter => parameter.Invert, invert);
            });

            if (isHidden)
            {
                Assert.Throws<ElementNotFoundException>(() => comp.Find("p"));
            }
            else
            {
                comp.Find("p").TextContent.Should().Be("MudHidden content");
            }

            jsRuntimeMock.Received();
        }

        [Test]
        public async Task SizeChanged()
        {
            BrowserWindowSize GetBrowserSize()
            {
                return new BrowserWindowSize { Height = 1080, Width = 1920 }; //Xl
            }
            var jsRuntimeMock = Substitute.For<IJSRuntime>();
            var browserViewportService = new BrowserViewportService(NullLogger<BrowserViewportService>.Instance, jsRuntimeMock);

            jsRuntimeMock.InvokeAsync<BrowserWindowSize>("mudResizeListener.getBrowserWindowSize", Arg.Any<object[]>())
                .Returns(GetBrowserSize());

            Context.Services.AddSingleton<IBrowserViewportService>(browserViewportService);

            var component = Context.RenderComponent<SimpleMudHiddenTest>(parameterBuilder =>
            {
                parameterBuilder.Add(parameter => parameter.Breakpoint, Breakpoint.Lg);
                parameterBuilder.Add(parameter => parameter.Invert, false);
            });
            var mudHiddenComponent = component.FindComponent<MudHidden>();

            component.Find("p").TextContent.Should().Be("MudHidden content");
            var subscription = browserViewportService.GetInternalSubscription(mudHiddenComponent.Instance)!;

            await component.InvokeAsync(async () => await browserViewportService.RaiseOnResized(new BrowserWindowSize { Height = 720, Width = 1280 }, Breakpoint.Lg, subscription.JavaScriptListenerId));

            Assert.Throws<ElementNotFoundException>(() => component.Find("p"));

            await component.InvokeAsync(async () => await browserViewportService.RaiseOnResized(new BrowserWindowSize { Height = 1080, Width = 1920 }, Breakpoint.Xl, subscription.JavaScriptListenerId));
            component.Find("p").TextContent.Should().Be("MudHidden content");

            component.Instance.HiddenChangedHistory.Should().HaveCount(3).And.BeEquivalentTo(new[] { false, true, false });

            jsRuntimeMock.Received();
        }

        [Test]
        public void InvertChangedAfterInitializing()
        {
            BrowserWindowSize GetBrowserSize()
            {
                return new BrowserWindowSize { Height = 640, Width = 960 }; //Md
            }
            var jsRuntimeMock = Substitute.For<IJSRuntime>();
            var browserViewportService = new BrowserViewportService(NullLogger<BrowserViewportService>.Instance, jsRuntimeMock);

            jsRuntimeMock.InvokeAsync<BrowserWindowSize>("mudResizeListener.getBrowserWindowSize", Arg.Any<object[]>())
                .Returns(GetBrowserSize());

            Context.Services.AddSingleton<IBrowserViewportService>(browserViewportService);

            var comp = Context.RenderComponent<SimpleMudHiddenTest>(p =>
            {
                p.Add(x => x.Breakpoint, Breakpoint.Lg);
                p.Add(x => x.Invert, false);
            });

            comp.Find("p").TextContent.Should().Be("MudHidden content");

            comp.SetParametersAndRender(p => p.Add(x => x.Invert, true));

            Assert.Throws<ElementNotFoundException>(() => comp.Find("p"));

            jsRuntimeMock.Received();
        }

        [Test]
        public void ReferenceBreakpointChangedAfterInitializing()
        {
            BrowserWindowSize GetBrowserSize()
            {
                return new BrowserWindowSize { Height = 640, Width = 960 }; //Md
            }

            var jsRuntimeMock = Substitute.For<IJSRuntime>();
            var browserViewportService = new BrowserViewportService(NullLogger<BrowserViewportService>.Instance, jsRuntimeMock);

            jsRuntimeMock.InvokeAsync<BrowserWindowSize>("mudResizeListener.getBrowserWindowSize", Arg.Any<object[]>())
                .Returns(GetBrowserSize());

            Context.Services.AddSingleton<IBrowserViewportService>(browserViewportService);

            var component = Context.RenderComponent<SimpleMudHiddenTest>(parameterBuilder =>
            {
                parameterBuilder.Add(parameter => parameter.Breakpoint, Breakpoint.Lg);
                parameterBuilder.Add(parameter => parameter.Invert, false);
            });

            component.Find("p").TextContent.Should().Be("MudHidden content");

            component.SetParametersAndRender(parameter => parameter.Add(x => x.Breakpoint, Breakpoint.Md));

            Assert.Throws<ElementNotFoundException>(() => component.Find("p"));

            jsRuntimeMock.Received();
        }

        [Test]
        public async Task SizeChangedToNone()
        {
            BrowserWindowSize GetBrowserSize()
            {
                return new BrowserWindowSize { Height = 640, Width = 960 }; //Md
            }

            var jsRuntimeMock = Substitute.For<IJSRuntime>();
            var browserViewportService = new BrowserViewportService(NullLogger<BrowserViewportService>.Instance, jsRuntimeMock);

            jsRuntimeMock.InvokeAsync<BrowserWindowSize>("mudResizeListener.getBrowserWindowSize", Arg.Any<object[]>())
                .Returns(GetBrowserSize());

            Context.Services.AddSingleton<IBrowserViewportService>(browserViewportService);

            var component = Context.RenderComponent<SimpleMudHiddenTest>(parameterBuilder =>
            {
                parameterBuilder.Add(parameter => parameter.Breakpoint, Breakpoint.Lg);
                parameterBuilder.Add(parameter => parameter.Invert, false);
            });

            component.Find("p").TextContent.Should().Be("MudHidden content");

            await component.InvokeAsync(async () => await browserViewportService.RaiseOnResized(new BrowserWindowSize(), Breakpoint.None, Guid.Empty));
            component.Find("p").TextContent.Should().Be("MudHidden content");

            component.Instance.HiddenChangedHistory.Should().ContainSingle().And.BeEquivalentTo(new[] { false });

            jsRuntimeMock.Received();
        }

        [Test]
        public void WithinMudBreakpointProvider()
        {
            var jsRuntimeMock = Substitute.For<IJSRuntime>();
            var browserViewportService = new BrowserViewportService(NullLogger<BrowserViewportService>.Instance, jsRuntimeMock);

            Context.Services.AddSingleton<IBrowserViewportService>(browserViewportService);

            var component = Context.RenderComponent<BreakpointProviderWithMudHiddenTest>(parameterBuilder =>
            {
                parameterBuilder.Add(parameter => parameter.Breakpoint, Breakpoint.Lg);
            });

            var items = component.FindAll("p");

            items.Should().HaveCount(4);

            for (var i = 0; i < 4; i++)
            {
                var item = items[i];
                item.TextContent.Should().Be($"MudHidden content {i + 1}");
            }

            jsRuntimeMock.Received();
        }

        [Test]
        public async Task WithinMudBreakpointProvider_UpdateBreakpointValue()
        {
            BrowserWindowSize GetBrowserSize()
            {
                return new BrowserWindowSize { Height = 640, Width = 960 }; //Md
            }
            var jsRuntimeMock = Substitute.For<IJSRuntime>();
            var browserViewportService = new BrowserViewportService(NullLogger<BrowserViewportService>.Instance, jsRuntimeMock);

            jsRuntimeMock.InvokeAsync<BrowserWindowSize>("mudResizeListener.getBrowserWindowSize", Arg.Any<object[]>())
                .Returns(GetBrowserSize());
            jsRuntimeMock.InvokeAsync<IJSVoidResult>("mudResizeListenerFactory.listenForResize", Arg.Any<object[]>())
                .Returns(Substitute.For< IJSVoidResult>());

            Context.Services.AddSingleton<IBrowserViewportService>(browserViewportService);

            var component = Context.RenderComponent<BreakpointProviderWithMudHiddenTest>(parameterBuilder =>
            {
                parameterBuilder.Add(parameter => parameter.Breakpoint, Breakpoint.Lg);
            });

            var items = component.FindAll("p");

            items.Should().HaveCount(4);

            for (var i = 0; i < 4; i++)
            {
                var item = items[i];
                item.TextContent.Should().Be($"MudHidden content {i + 1}");
            }

            var mudBreakpointProviderComponent = component.FindComponent<MudBreakpointProvider>();
            var subscription = browserViewportService.GetInternalSubscription(mudBreakpointProviderComponent.Instance)!;

            await component.InvokeAsync(async () => await browserViewportService.RaiseOnResized(new BrowserWindowSize { Height = 720, Width = 1280 }, Breakpoint.Lg, subscription.JavaScriptListenerId));
            items = component.FindAll("p");
            items.Should().BeEmpty();

            await component.InvokeAsync(async () => await browserViewportService.RaiseOnResized(new BrowserWindowSize { Height = 400, Width = 600 }, Breakpoint.Sm, subscription.JavaScriptListenerId));
            items = component.FindAll("p");

            items.Should().HaveCount(4);

            for (var i = 0; i < 4; i++)
            {
                var item = items[i];
                item.TextContent.Should().Be($"MudHidden content {i + 1}");
            }

            component.Instance.BreakpointChangedHistory.Should().HaveCount(3).And.BeEquivalentTo(new[] { Breakpoint.Md, Breakpoint.Lg, Breakpoint.Sm });

            jsRuntimeMock.Received();
        }

        [Test]
        public void TestSemaphore_RenderInParallel()
        {
            var jsRuntimeMock = Substitute.For<IJSRuntime>();
            var browserViewportService = new BrowserViewportService(NullLogger<BrowserViewportService>.Instance, jsRuntimeMock);

            jsRuntimeMock.InvokeAsync<BrowserWindowSize>("mudResizeListener.getBrowserWindowSize", Arg.Any<object[]>())
                .Returns(new BrowserWindowSize { Height = 1080, Width = 1920 });
            jsRuntimeMock.InvokeAsync<IJSVoidResult>("mudResizeListenerFactory.listenForResize", Arg.Any<object[]>())
                .Returns(Substitute.For<IJSVoidResult>());
            jsRuntimeMock.InvokeAsync<IJSVoidResult>("mudResizeListenerFactory.cancelListeners", Arg.Any<object[]>())
                .Returns(Substitute.For<IJSVoidResult>());

            Context.Services.AddSingleton<IBrowserViewportService>(browserViewportService);

            var component = Context.RenderComponent<RenderMultipleHiddenInParallel>();

            component.WaitForAssertion(() => component.FindAll(".xl").Should().HaveCount(10), TimeSpan.FromSeconds(1));
            component.WaitForAssertion(() => component.FindAll(".lg-and-up").Should().HaveCount(10), TimeSpan.FromSeconds(1));
            component.WaitForAssertion(() => component.FindAll(".md-and-up").Should().HaveCount(10), TimeSpan.FromSeconds(1));
            component.WaitForAssertion(() => component.FindAll(".sm-and-up").Should().HaveCount(10), TimeSpan.FromSeconds(1));
        }
    }
}
